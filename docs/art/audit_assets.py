"""Inspect source art without changing Unity assets; build a traceable FAL catalog.

Run from the repository root:
    uv run --with pillow --with pypdf python docs/art/audit_assets.py
"""

from __future__ import annotations

import hashlib
import json
import struct
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont
from pypdf import PdfReader


ROOT = Path(__file__).resolve().parents[2]
GRAPHICS = ROOT / "DeepAndDarkness/Assets/_Project/ThirdParty/AssetsBase/Graphics"
OUT = ROOT / "docs/art/audit-2026-11-17"
REFS = OUT / "references"
LIMIT = 64 * 1024

SHEETS = {
    "Collectible.png": ("collectible-legacy", "prop", "Four violet crystal rotation poses; existing visual reference, redesign candidate."),
    "HazardSprite.png": ("spectral-hazard", "prop", "Three left-facing cyan spectral skull poses with trailing wisps."),
    "LifeIcons.png": ("life-icons", "ui", "Six icons: three blood-vial states and three shell/pearl states. The checkerboard is baked into RGB pixels; no alpha channel."),
    "Mermaid Swimming Animation Sprite Sheet.png": ("mermaid", "character", "Right-facing pale mermaid with long silver hair and teal tail; six swimming poses and a final stars/hurt pose, by visual inspection."),
    "Seven-Frame Gothic Bat Sprite Sheet.png": ("gothic-bat", "character", "Right-facing dark bat with rust-brown wing membranes; six flight poses and a final stars/hurt pose, by visual inspection."),
    "Fantasy Graveyard and Underwater Obstacle Sprite Sheet.png": ("obstacle-props", "prop", "Upper row: gate, two tombstones, spikes, dead tree, thorns, cross, lantern, gargoyle. Lower row: kelp, purple coral, tube coral, column, spear, anchor, mine, jellyfish, wreck."),
}

LAYER_NOTES = {
    "01_ground_grass.png": "Nearest ground strip with dark soil, grass and orange highlights.",
    "02_graveyard_props.png": "Graves, fence, pumpkins and large lantern tree; near environmental layer.",
    "03_lantern_trees.png": "Blue midground terrain, bare trees and warm lanterns.",
    "04_hills_chapel.png": "Distant blue hills and chapel silhouette.",
    "05_sky_moon.png": "Sky backdrop with orange moon; check wrap seams before tiling.",
    "halloween_grass_ground_800x200.png": "Exact duplicate of the 800x200 ground layer; retain source path, avoid a second production copy.",
    "far.png": "Opaque blue underwater backdrop and distant ruined arch; 256x192 pixel art.",
    "foreground-1.png": "Near reef, rock stack and plants; transparent 256x192 layer.",
    "foreground-2.png": "Ruin statue, column, rock and plants; transparent 256x192 layer.",
    "foregound-merged.png": "Merged 512x192 foreground arrangement; preserve the source filename typo in provenance.",
    "sand.png": "Blue seabed/sand layer, rocks and vegetation; transparent 256x192 image.",
    "underwater-fantasy-preview.png": "768x192 composite pack preview; reference only, not a separate parallax layer.",
}


def write_json(path: Path, data: object) -> None:
    path.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def proxy(image: Image.Image, path: Path) -> dict:
    """Create a bounded reference, never a claimed production replacement."""
    for width in (1024, 768, 640, 512, 384, 256):
        scaled = image.convert("RGBA")
        scaled.thumbnail((width, width), Image.Resampling.LANCZOS)
        for colors in (256, 128, 64):
            reduced = scaled.quantize(colors=colors, method=Image.Quantize.FASTOCTREE)
            reduced.save(path, optimize=True)
            if path.stat().st_size <= LIMIT:
                return {"path": path.relative_to(ROOT).as_posix(), "width": reduced.width,
                        "height": reduced.height, "bytes": path.stat().st_size,
                        "kind": "reduced_palette_reference_proxy", "palette_colors": colors,
                        "production_asset": False}
    raise RuntimeError(f"Cannot create bounded reference: {path.name}")


def first_pose(image: Image.Image) -> tuple[Image.Image, list[int]]:
    alpha = image.convert("RGBA").getchannel("A")
    columns = [alpha.crop((x, 0, x + 1, alpha.height)).getextrema()[1] > 16 for x in range(alpha.width)]
    start = next(i for i, value in enumerate(columns) if value)
    end = start + 1
    while end < len(columns) and columns[end]:
        end += 1
    crop = image.crop((start, 0, end, image.height)).convert("RGBA")
    bounds = crop.getchannel("A").point(lambda value: 255 if value > 16 else 0).getbbox()
    if bounds is None:
        raise RuntimeError("Empty first pose")
    box = [start + bounds[0], bounds[1], start + bounds[2], bounds[3]]
    return image.crop(tuple(box)), box


def contact_sheet(records: list[dict], name: str, cell_height: int) -> None:
    width, cell_width = 1400, 700
    canvas = Image.new("RGB", (width, ((len(records) + 1) // 2) * cell_height), "#1E222C")
    draw = ImageDraw.Draw(canvas)
    font = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 17)
    for i, item in enumerate(records):
        x, y = (i % 2) * cell_width, (i // 2) * cell_height
        rel = Path(item["graphics_relative_path"])
        label = rel.as_posix()
        for j, line in enumerate([label[k:k + 73] for k in range(0, len(label), 73)]):
            draw.text((x + 10, y + 4 + j * 20), line, fill="white", font=font)
        draw.text((x + 10, y + 45), f'{item["width"]} x {item["height"]}', fill="#CBD0DB", font=font)
        box = (x + 10, y + 72, x + 690, y + cell_height - 10)
        for sy in range(box[1], box[3], 20):
            for sx in range(box[0], box[2], 20):
                c = "#414650" if ((sx - box[0]) // 20 + (sy - box[1]) // 20) % 2 == 0 else "#555A64"
                draw.rectangle((sx, sy, min(sx + 19, box[2] - 1), min(sy + 19, box[3] - 1)), fill=c)
        with Image.open(ROOT / item["path"]) as source:
            source = source.convert("RGBA")
            source.thumbnail((box[2] - box[0], box[3] - box[1]), Image.Resampling.NEAREST)
            canvas.paste(source, (box[0] + (box[2] - box[0] - source.width) // 2,
                                  box[1] + (box[3] - box[1] - source.height) // 2), source)
    canvas.save(OUT / f"{name}.png", optimize=True)


def main() -> None:
    REFS.mkdir(parents=True, exist_ok=True)
    records, auxiliary, upload_queue = [], [], []
    hashes: dict[str, str] = {}
    for path in sorted(GRAPHICS.rglob("*")):
        if not path.is_file() or path.suffix == ".meta":
            continue
        rel = path.relative_to(GRAPHICS)
        common = {"path": path.relative_to(ROOT).as_posix(), "graphics_relative_path": rel.as_posix(),
                  "bytes": path.stat().st_size, "sha256": hashlib.sha256(path.read_bytes()).hexdigest()}
        if path.suffix.lower() != ".png":
            common["role"] = "system_metadata" if path.name == ".DS_Store" else "editable_source" if path.suffix.lower() in (".psd", ".aseprite") else "license" if "license" in path.name.lower() else "pack_marketing_document"
            if path.suffix.lower() == ".psd":
                with Image.open(path) as source:
                    common["container"] = {"width": source.width, "height": source.height,
                                           "layers": [layer[0] for layer in getattr(source, "layers", [])]}
            elif path.suffix.lower() == ".aseprite":
                data = path.read_bytes()
                _, magic, frames, width, height, depth = struct.unpack_from("<I5H", data)
                if magic != 0xA5E0:
                    raise ValueError("Invalid Aseprite source header")
                offset, layers = 128, []
                for _ in range(frames):
                    size, _, old_count = struct.unpack_from("<IHH", data, offset)
                    new_count = struct.unpack_from("<I", data, offset + 12)[0]
                    chunk = offset + 16
                    for _ in range(new_count or old_count):
                        chunk_size, chunk_type = struct.unpack_from("<IH", data, chunk)
                        if chunk_type == 0x2004:
                            length = struct.unpack_from("<H", data, chunk + 22)[0]
                            layers.append(data[chunk + 24:chunk + 24 + length].decode("utf-8"))
                        chunk += chunk_size
                    offset += size
                common["container"] = {"width": width, "height": height, "frames": frames,
                                       "color_depth": depth, "layers": layers}
            auxiliary.append(common)
            continue
        family = "graveyard" if rel.parts[0] == "HalloweenGrassGround_Free_v1.0" else "underwater" if rel.parts[0] == "underwater-fantasy-files" else "characters-props"
        slug, category, note = SHEETS.get(path.name, ("-".join(rel.with_suffix("").parts).lower().replace("_", "-"), "environment", LAYER_NOTES.get(path.name, "Source art")))
        with Image.open(path) as image:
            alpha = image.convert("RGBA").getchannel("A")
            common.update(id=slug, family=family, category=category, description=note,
                          width=image.width, height=image.height, mode=image.mode,
                          has_alpha_channel="A" in image.getbands() or "transparency" in image.info,
                          alpha_extrema=list(alpha.getextrema()), visible_bounds=alpha.getbbox(),
                          source_meta_present=Path(str(path) + ".meta").exists(),
                          exact_duplicate_of=hashes.get(common["sha256"]),
                          provenance="KATO, Hallowmoon Graveyard" if family == "graveyard" else "Luis Zuno / Ansimuz, Underwater Fantasy" if family == "underwater" else "User-supplied team asset; individual authorship not independently verified",
                          license="Custom local license: game use/edit allowed; standalone redistribution and AI training prohibited" if family == "graveyard" else "CC0, as declared in the included public-license.pdf" if family == "underwater" else "Not supplied in this folder",
                          cloud_reference_status="local_catalog_only_due_to_standalone_redistribution_restriction" if family == "graveyard" else "prepared")
            hashes.setdefault(common["sha256"], common["path"])
            if family != "graveyard":
                if path.stat().st_size <= LIMIT:
                    reference = {"path": common["path"], "width": image.width, "height": image.height,
                                 "bytes": path.stat().st_size, "kind": "original_file", "production_asset": False}
                else:
                    reference = proxy(image, REFS / f"{slug}.png")
                common["fal_reference"] = reference
                upload_queue.append({"key": slug, "source_path": common["path"], "source_sha256": common["sha256"],
                                     "description": note, "category": category, "reference": reference})
            records.append(common)
    for slug, source_name in (("gothic-bat-anchor", "Seven-Frame Gothic Bat Sprite Sheet.png"),
                              ("mermaid-anchor", "Mermaid Swimming Animation Sprite Sheet.png"),
                              ("spectral-hazard-anchor", "HazardSprite.png")):
        with Image.open(GRAPHICS / source_name) as source:
            pose, bounds = first_pose(source)
            ref = proxy(pose, REFS / f"{slug}.png")
        upload_queue.append({"key": slug, "source_path": (GRAPHICS / source_name).relative_to(ROOT).as_posix(),
                             "description": "First visible source pose, cropped as an entity anchor; not an approved redesign.",
                             "category": "anchor", "crop_box": bounds, "reference": ref})
    with Image.open(GRAPHICS / "Fantasy Graveyard and Underwater Obstacle Sprite Sheet.png") as source:
        for key, box in (("graveyard-prop-vocabulary", (0, 0, source.width, source.height // 2)),
                         ("underwater-prop-vocabulary", (0, source.height // 2, source.width, source.height))):
            ref = proxy(source.crop(box), REFS / f"{key}.png")
            upload_queue.append({"key": key, "source_path": (GRAPHICS / "Fantasy Graveyard and Underwater Obstacle Sprite Sheet.png").relative_to(ROOT).as_posix(),
                                 "description": "User-supplied obstacle sheet row cropped as prop vocabulary; the separate KATO background pack is not included.",
                                 "category": "anchor", "crop_box": list(box), "reference": ref})
    license_text = "\n\n".join(page.extract_text() for page in PdfReader(GRAPHICS / "underwater-fantasy-files/public-license.pdf").pages)
    (OUT / "underwater-license-extracted.txt").write_text(license_text, encoding="utf-8")
    write_json(OUT / "inventory.json", {"date": "2026-11-17", "image_count": len(records),
               "auxiliary_file_count": len(auxiliary), "images": records, "auxiliary_files": auxiliary})
    write_json(OUT / "upload-plan.json", upload_queue)
    for family, height in (("characters-props", 350), ("graveyard", 270), ("underwater", 400)):
        contact_sheet([r for r in records if r["family"] == family], family, height)
    lines = ["# Source asset inventory", "", "All original files are unchanged. FAL references are identified separately from production assets.", "", "| Asset | Size | Role and observed content |", "| --- | --- | --- |"]
    for record in records:
        lines.append(f'| `{record["graphics_relative_path"]}` | {record["width"]} x {record["height"]} | {record["description"]} |')
    (OUT / "inventory.md").write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(json.dumps({"images": len(records), "auxiliary_files": len(auxiliary),
                      "prepared_cloud_references": len(upload_queue), "output": str(OUT)}))


if __name__ == "__main__":
    main()
