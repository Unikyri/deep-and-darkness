"""Assemble three 4K parallax layers from the original art and FAL outputs."""

from __future__ import annotations

import hashlib
import json
from pathlib import Path

from PIL import Image, ImageChops, ImageFilter, ImageStat


ROOT = Path(__file__).resolve().parents[3]
WORK = Path(__file__).resolve().parent
SOURCE = ROOT / "DeepAndDarkness/Assets/_Project/Art/UI/MenuParallax/Background 1.jpg"
SOURCE_SHA256 = "b07e3ea57efb466ccf66d3adab951511c2e9e79f9df2b29ded5f854bf9e77a62"
DEST = WORK / "final"
SIZE = (3840, 2160)


def load_mask(name: str) -> Image.Image:
    return Image.open(WORK / name).convert("L").resize(SIZE, Image.Resampling.LANCZOS)


def main() -> None:
    source_hash = hashlib.sha256(SOURCE.read_bytes()).hexdigest()
    if source_hash != SOURCE_SHA256:
        raise RuntimeError("The source image changed; rebuild the masks before assembling.")
    source = Image.open(SOURCE).convert("RGB")
    if source.size != SIZE:
        raise RuntimeError(f"Unexpected source resolution: {source.size}")

    gpt_far = Image.open(WORK / "gpt26-far-original.png").convert("RGB")
    gpt_mid = Image.open(WORK / "gpt26-mid-repaired-original.png").convert("RGBA")
    gpt_front = Image.open(WORK / "gpt26-front-original.png").convert("RGBA")
    if any(image.size != SIZE for image in (gpt_far, gpt_mid, gpt_front)):
        raise RuntimeError("A GPT source layer does not have the required 4K canvas.")

    middle_mask = load_mask("middle-mask.png")
    foreground_mask = load_mask("foreground-mask.png")
    inpaint_mask = load_mask("far-inpaint-mask.png")

    # Preserve the user's source exactly outside the masked reconstruction area.
    far = Image.composite(gpt_far, source, inpaint_mask).convert("RGBA")

    # Keep visible ruins from the source; use GPT details only underneath the
    # foreground, close to an actual ruin, where those details can be revealed.
    small_mask = Image.open(WORK / "middle-mask.png").convert("L")
    nearby_ruins = small_mask.filter(ImageFilter.MaxFilter(31)).resize(
        SIZE, Image.Resampling.LANCZOS
    )
    hidden_mask = ImageChops.multiply(foreground_mask, nearby_ruins)
    gpt_mid.putalpha(ImageChops.multiply(gpt_mid.getchannel("A"), hidden_mask))
    source_mid = source.convert("RGBA")
    source_mid.putalpha(middle_mask)
    middle = Image.alpha_composite(gpt_mid, source_mid)

    # The generated foreground fills edge pixels beneath the exact source
    # artwork. Source pixels and the segmentation mask control visible content.
    gpt_front.putalpha(ImageChops.multiply(gpt_front.getchannel("A"), foreground_mask))
    source_front = source.convert("RGBA")
    source_front.putalpha(foreground_mask)
    front = Image.alpha_composite(gpt_front, source_front)

    DEST.mkdir(parents=True, exist_ok=True)
    layers = [
        ("01-Far.png", far),
        ("02-Middle.png", middle),
        ("03-Foreground.png", front),
    ]
    records = []
    for name, image in layers:
        path = DEST / name
        image.save(path, format="PNG", optimize=True)
        alpha = image.getchannel("A")
        records.append(
            {
                "file": str(path.relative_to(ROOT)).replace("\\", "/"),
                "size": list(image.size),
                "alpha_extrema": list(alpha.getextrema()),
                "alpha_bbox": list(alpha.getbbox()) if alpha.getbbox() else None,
                "sha256": hashlib.sha256(path.read_bytes()).hexdigest(),
            }
        )

    combined = Image.alpha_composite(Image.alpha_composite(far, middle), front).convert("RGB")
    diff = ImageChops.difference(source, combined)
    mean = sum(ImageStat.Stat(diff).mean) / 3
    preview = combined.copy()
    preview.thumbnail((1600, 900), Image.Resampling.LANCZOS)
    preview.save(WORK / "assembled-preview.jpg", quality=93, optimize=True)

    metadata = {
        "source": str(SOURCE.relative_to(ROOT)).replace("\\", "/"),
        "source_sha256": source_hash,
        "canvas": list(SIZE),
        "layers": records,
        "composite_mean_absolute_rgb_difference_0_255": round(mean, 4),
        "notes": "Visible cutout colors come from the user's source; GPT Image 2.6 supplies the masked far reconstruction and hidden depth details. Masks were generated and inspected at 2560x1440 and resampled to 4K.",
    }
    (WORK / "assembly-qa.json").write_text(
        json.dumps(metadata, indent=2) + "\n", encoding="utf-8"
    )
    print(json.dumps(metadata, indent=2))


if __name__ == "__main__":
    main()
