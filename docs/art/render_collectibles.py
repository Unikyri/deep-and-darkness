"""Inspect and package existing collectible PNGs; does not generate or edit art."""

import hashlib
import json
import zipfile
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]
DIR = ROOT / "docs/art/collectibles-six"


def main():
    manifest = json.loads((DIR / "set.json").read_text(encoding="utf-8"))
    board = Image.new("RGB", (1356, 1100), "#151C29")
    draw = ImageDraw.Draw(board)
    title = ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf", 30)
    name_font = ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf", 21)
    small = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 16)
    draw.text((36, 24), "DEEP AND DARKNESS / COLLECTIBLES", font=title, fill="#F1E4C9")
    draw.text((36, 70), "Six distinct object icons / one shared gothic and underwater style", font=small, fill="#9FB3C9")
    results = []
    for index, item in enumerate(manifest["items"]):
        transparent = DIR / item["transparent"]
        path = transparent if transparent.exists() else DIR / item["raw"]
        with Image.open(path) as decoded:
            mode, fmt = decoded.mode, decoded.format
            image = decoded.convert("RGBA")
        alpha = image.getchannel("A")
        histogram = alpha.histogram()
        bounds = alpha.point(lambda value: 255 if value > 16 else 0).getbbox()
        if bounds is None:
            raise ValueError(f"Empty image: {item['slug']}")
        crop = image.crop(bounds)
        real_alpha = "A" in mode and histogram[0] > 0 and histogram[255] > 0
        entry = {"number": item["number"], "slug": item["slug"], "name": item["name"],
                 "path": path.relative_to(ROOT).as_posix(), "format": fmt, "mode": mode,
                 "width": image.width, "height": image.height,
                 "has_real_alpha": real_alpha, "alpha_extrema": list(alpha.getextrema()),
                 "transparent_pixel_fraction": histogram[0] / (image.width * image.height),
                 "visible_bounds_alpha_gt_16": list(bounds),
                 "border_clear": bounds[0] > 0 and bounds[1] > 0 and bounds[2] < image.width and bounds[3] < image.height,
                 "sha256": hashlib.sha256(path.read_bytes()).hexdigest(),
                 "visual_review": item.get("visualReview", "pending"),
                 "in_engine_readability": "unverified_not_integrated"}
        results.append(entry)
        x, y = 24 + index % 3 * 444, 122 + index // 3 * 456
        card = Image.new("RGB", (420, 428), "#203144" if real_alpha else "#FFFFFF")
        large = crop.copy()
        large.thumbnail((336, 292), Image.Resampling.LANCZOS)
        card.paste(large, ((420 - large.width) // 2, 12 + (292 - large.height) // 2), large)
        board.paste(card, (x, y))
        draw.text((x + 18, y + 309), f'{item["number"]:02d}  {item["name"]}', font=name_font,
                  fill="#EBDABD" if real_alpha else "#203144")
        for j, size in enumerate((48, 64)):
            thumb = crop.copy()
            thumb.thumbnail((size, size), Image.Resampling.LANCZOS)
            tx = x + 38 + j * 182
            board.paste(thumb, (tx, y + 350 + (64 - thumb.height) // 2), thumb)
            draw.text((tx + 77, y + 371), f"{size} px", font=small,
                      fill="#93AABD" if real_alpha else "#36536E")
    all_ready = len(results) == 6 and all(x["has_real_alpha"] for x in results)
    draw.text((36, 1054), "FAL-generated art. First design approved; five new designs supplied for review. Static icons.", font=small, fill="#9FB3C9")
    board.save(DIR / "collection-preview.png", optimize=True)
    qa = {"item_count": len(results), "all_six_have_real_alpha": all_ready, "items": results,
          "evidence": "Pillow decoded actual local PNGs; host inspected rendered original/cutout comparisons.",
          "sampling_limits": "Static 48/64px previews; no gameplay, animation or collider checks."}
    (DIR / "qa.json").write_text(json.dumps(qa, indent=2) + "\n", encoding="utf-8")
    if all_ready:
        with zipfile.ZipFile(DIR / "deep-and-darkness-collectibles-six.zip", "w", zipfile.ZIP_DEFLATED) as archive:
            packaged_items = []
            for item in manifest["items"]:
                filename = f'{item["number"]:02d}-{item["slug"]}.png'
                archive.write(DIR / item["transparent"], filename)
                packaged_items.append({key: value for key, value in item.items() if key not in ("raw", "transparent")}
                                      | {"transparent": filename})
            archive.writestr("set.json", json.dumps(manifest | {"items": packaged_items}, indent=2) + "\n")
            for name in ("qa.json", "collection-preview.png", "README.md"):
                if not (DIR / name).exists():
                    continue
                archive.write(DIR / name, name)
    print(json.dumps({"items": len(results), "transparent_pngs": sum(x["has_real_alpha"] for x in results),
                      "all_six_have_real_alpha": all_ready, "packaged": all_ready}))


if __name__ == "__main__":
    main()
