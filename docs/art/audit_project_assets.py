"""Inventory project-owned assets and build visual indexes without touching Assets/.

The scene-dependency input is a read-only Unity AssetDatabase query of saved scenes.
Run from the repository root with:
    uv run --with pillow python docs/art/audit_project_assets.py
"""

from __future__ import annotations

import hashlib
import json
import re
import zipfile
from collections import Counter, defaultdict
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[2]
UNITY_PROJECT = ROOT / "DeepAndDarkness"
ASSETS = UNITY_PROJECT / "Assets/_Project"
ART = ASSETS / "Art"
OUT = ROOT / "docs/art/project-asset-discovery"
GRAPHICS = ASSETS / "ThirdParty/AssetsBase/Graphics"
TMP = ASSETS / "ThirdParty/TextMesh Pro"
IMAGE_EXTS = {".png", ".jpg", ".jpeg", ".webp", ".tga", ".psd"}
AUDIO_EXTS = {".wav", ".mp3", ".ogg", ".aif", ".aiff", ".flac", ".m4a", ".xm", ".mod", ".it", ".s3m"}
MAJOR_UI = {
    "BackgtoundMenu.png", "FondoNoTitle.jpg", "HR_Castle of Lost Shadows.png", "Logo1.png",
    "StoryImage1.png", "StoryImage2.png", "Role2Image.jpeg", "Role3Image.jpeg", "Role4Image.jpeg",
}

DESCRIPTIONS = {
    "LowerWorldBGFar.png": "Opaque blue underwater distance with tiled ruined arches and plants.",
    "LowerWorldBGNear.png": "Blue seabed, reef shapes and sea plants; underwater middle layer.",
    "LowerWorldFG.png": "Underwater rock formation, vegetation, statue and broken column; foreground.",
    "UpperWorldBGFar.png": "Night sky and large orange moon; graveyard far layer.",
    "UpperWorldBGNear.png": "Blue hills, ruined chapel, bare trees and hanging lanterns; graveyard middle layer.",
    "UpperWorldFG.png": "Graveyard fence, tombstones, pumpkins, trees and orange ground; foreground.",
    "BackgtoundMenu.png": "Composite menu artwork with the Deep & Darkness logo on an orange moon.",
    "FondoNoTitle.jpg": "Menu background with orange moon, bats and aquatic details, without the title.",
    "HR_Castle of Lost Shadows.png": "Blue pixel-art castle under a moonlit sky.",
    "Logo1.png": "Standalone Deep & Darkness logo with moon, bats and ocean details.",
    "StoryImage1.png": "Story artwork showing a vampire above water and a mermaid below a moonlit landscape.",
    "StoryImage2.png": "Story artwork showing the vampire and mermaid linked by glowing objects across the water.",
    "Role2Image.jpeg": "Photograph of a black dog wearing a bow tie.",
    "Role3Image.jpeg": "Photograph of a white-and-brown cat wearing a collar.",
    "Role4Image.jpeg": "Photograph of a cat lying on its back.",
    "BannerExp.png": "Wide rectangular dark banner with a fine border.",
    "ESC.png": "Dark square key or button backing with a purple border.",
    "Espace.png": "Wide dark key or button backing with a purple border.",
    "Space.png": "Wide dark key or button backing with a purple border.",
    "Group 1.png": "Six square UI accents arranged around a horizontal orange line.",
    "Panel.png": "Dark rectangular interface panel with a colored ornamental border.",
}


def description(path: Path) -> str:
    if path.name in DESCRIPTIONS:
        return DESCRIPTIONS[path.name]
    action = {"arrow_left": "left arrow", "arrow_right": "right arrow", "home": "home",
              "play1": "play", "reset": "reset", "settings1": "settings",
              "trophy": "trophy", "volume": "volume"}
    for prefix, label in action.items():
        if path.stem.startswith(prefix + "_"):
            state = "pressed" if path.stem.endswith("_pressed") else "normal"
            return f"32 x 32 pixel-art {label} button, {state} state."
    return "Project art asset; inspect the visual index."


def project_path(path: Path) -> str:
    return path.relative_to(UNITY_PROJECT).as_posix()


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def metadata(path: Path) -> dict:
    meta_path = Path(str(path) + ".meta")
    if not meta_path.is_file():
        return {"present": False}
    data = meta_path.read_text(encoding="utf-8", errors="replace")
    result = {"present": True}
    for name, pattern in {
        "guid": r"^guid:\s*([0-9a-f]{32})",
        "sprite_mode": r"^\s*spriteMode:\s*(\d+)",
        "pixels_per_unit": r"^\s*spritePixelsToUnits:\s*([\d.]+)",
        "filter_mode": r"^\s*filterMode:\s*(-?\d+)",
        "alpha_is_transparency": r"^\s*alphaIsTransparency:\s*(\d+)",
    }.items():
        match = re.search(pattern, data, re.MULTILINE)
        if match:
            result[name] = match.group(1)
    return result


def record(path: Path, scene_deps: dict[str, set[str]], fal_sources: dict[str, list[str]]) -> dict:
    u_path = project_path(path)
    checksum = sha256(path)
    item = {"unity_path": u_path, "repository_path": path.relative_to(ROOT).as_posix(),
            "bytes": path.stat().st_size, "sha256": checksum, "unity_metadata": metadata(path),
            "referenced_by_saved_scenes": [name for name, deps in scene_deps.items() if u_path in deps],
            "fal_asset_records_with_identical_original_sha256": fal_sources.get(checksum, [])}
    if path.suffix.lower() in IMAGE_EXTS:
        with Image.open(path) as image:
            alpha = image.convert("RGBA").getchannel("A")
            item.update(width=image.width, height=image.height, mode=image.mode,
                        has_alpha_channel="A" in image.getbands() or "transparency" in image.info,
                        alpha_extrema=list(alpha.getextrema()))
    return item


def draw_board(items: list[dict], name: str, title: str, columns: int, cell_width: int, cell_height: int) -> None:
    rows = (len(items) + columns - 1) // columns
    board = Image.new("RGB", (columns * cell_width, 70 + rows * cell_height), "#171D28")
    draw = ImageDraw.Draw(board)
    title_font = ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf", 24)
    font = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 15)
    draw.text((16, 14), title, font=title_font, fill="#E8E4DA")
    for i, item in enumerate(items):
        x, y = (i % columns) * cell_width, 70 + (i // columns) * cell_height
        label = item["unity_path"].removeprefix("Assets/_Project/Art/")
        draw.text((x + 8, y + 3), label, font=font, fill="#F3F5F7")
        draw.text((x + 8, y + 25), f'{item["width"]} x {item["height"]}', font=font, fill="#AAB7C9")
        bounds = (x + 8, y + 50, x + cell_width - 8, y + cell_height - 8)
        for py in range(bounds[1], bounds[3], 18):
            for px in range(bounds[0], bounds[2], 18):
                fill = "#414A59" if ((px - bounds[0]) // 18 + (py - bounds[1]) // 18) % 2 == 0 else "#526073"
                draw.rectangle((px, py, min(px + 17, bounds[2] - 1), min(py + 17, bounds[3] - 1)), fill=fill)
        with Image.open(ROOT / item["repository_path"]) as source:
            image = source.convert("RGBA")
            scale = min((bounds[2] - bounds[0]) / image.width, (bounds[3] - bounds[1]) / image.height, 6)
            new_size = (max(1, round(image.width * scale)), max(1, round(image.height * scale)))
            method = Image.Resampling.NEAREST if image.width <= 128 and image.height <= 128 else Image.Resampling.LANCZOS
            image = image.resize(new_size, method)
            target = (bounds[0] + (bounds[2] - bounds[0] - image.width) // 2,
                      bounds[1] + (bounds[3] - bounds[1] - image.height) // 2)
            board.paste(image, target, image)
    board.save(OUT / f"{name}.png", optimize=True)


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    scene_file = OUT / "unity-scene-dependencies.json"
    recorded_scenes = json.loads(scene_file.read_text(encoding="utf-8"))
    scene_deps = {s["scene"]: set(s["dependencies"]) for s in recorded_scenes["scenes"]}
    audio_clip_snapshot = recorded_scenes["audio_clip_snapshot"]
    fal_catalog = json.loads((ROOT / "docs/art/fal-catalog.json").read_text(encoding="utf-8"))
    fal_sources: dict[str, list[str]] = defaultdict(list)
    for source in fal_catalog["sourceAssets"]:
        if source.get("sourceSha256"):
            fal_sources[source["sourceSha256"]].append(source["assetRecordId"])

    images = sorted(p for p in ART.rglob("*") if p.is_file() and p.suffix.lower() in IMAGE_EXTS)
    image_records = [record(path, scene_deps, fal_sources) for path in images]
    for item in image_records:
        item["family"] = "background" if "/Backgrounds/" in item["unity_path"] else "ui"
        item["description"] = description(ROOT / item["repository_path"])
        if item["family"] == "background":
            item["provenance_observation"] = ("Visually matches the previously inventoried KATO graveyard source family; composition/export is not byte-identical, so exact derivation was not independently verified."
                                               if "/UpperWorld" in item["unity_path"] else
                                               "Visually matches the previously inventoried Ansimuz underwater source family; composition/export is not byte-identical, so exact derivation was not independently verified.")
    duplicates: dict[str, list[dict]] = defaultdict(list)
    for item in image_records:
        duplicates[item["sha256"]].append(item)
    duplicate_groups = [{"sha256": digest,
                         "members": [{"unity_path": member["unity_path"],
                                      "guid": member["unity_metadata"].get("guid"),
                                      "referenced_by_saved_scenes": member["referenced_by_saved_scenes"]}
                                     for member in group]}
                        for digest, group in duplicates.items() if len(group) > 1]

    scenes = [ASSETS / "Scenes" / f"{name}.unity" for name in ("MainMenu", "Gameplay", "PausaScene", "GameplayLevel2")]
    font_files = sorted(p for p in (ART / "Fonts").glob("*") if p.is_file() and p.suffix.lower() in {".ttf", ".asset"})
    controller_files = sorted((ASSETS / "Animations/UI").glob("*.controller"))
    controllers = []
    for path in controller_files:
        item = record(path, scene_deps, fal_sources)
        text = path.read_text(encoding="utf-8", errors="replace")
        item["embedded_animation_clip_count"] = len(re.findall(r"^--- !u!74\s", text, re.MULTILINE))
        item["animator_state_count"] = len(re.findall(r"^AnimatorState:", text, re.MULTILINE))
        controllers.append(item)
    prefabs = [record(path, scene_deps, fal_sources) for path in sorted((ASSETS / "Prefabs").rglob("*.prefab"))]
    audio_clips = sorted(project_path(path) for path in ASSETS.rglob("*") if path.is_file() and path.suffix.lower() in AUDIO_EXTS)
    mixer = record(ASSETS / "Audio/GameAudioMixer.mixer", scene_deps, fal_sources)
    tmp_files = [p for p in TMP.rglob("*") if p.is_file() and not p.name.endswith(".meta") and p.name != ".DS_Store"]
    source_files = [p for p in GRAPHICS.rglob("*") if p.is_file() and not p.name.endswith(".meta")]
    archive = ASSETS / "ThirdParty/AssetsBase/Graphics-20261117T052917Z-1-001.zip"
    with zipfile.ZipFile(archive) as zf:
        archive_members = []
        for member in zf.infolist():
            if member.is_dir():
                continue
            path = GRAPHICS.joinpath(*Path(member.filename).parts[1:])
            match = path.is_file() and hashlib.sha256(zf.read(member)).digest() == hashlib.sha256(path.read_bytes()).digest()
            archive_members.append({"name": member.filename, "unpacked_exists": path.is_file(), "sha256_equal": match})
    generated_manifest = ROOT / "docs/art/collectibles-six/set.json"
    generated_set = json.loads(generated_manifest.read_text(encoding="utf-8")) if generated_manifest.exists() else {"items": []}
    all_files = sorted(p for p in ASSETS.rglob("*") if p.is_file() and not p.name.endswith(".meta") and p.name not in {".DS_Store", ".gitkeep"})
    file_index = []
    for path in all_files:
        if path.is_relative_to(GRAPHICS):
            family = "previously_audited_graphics"
        elif path.is_relative_to(TMP):
            family = "textmesh_pro_support"
        elif path.is_relative_to(ART):
            family = "project_art"
        elif path.is_relative_to(ASSETS / "Settings"):
            family = "project_configuration"
        elif path.is_relative_to(ASSETS / "Scripts"):
            family = "project_scripts"
        else:
            family = path.relative_to(ASSETS).parts[0]
        u_path = project_path(path)
        file_index.append({"unity_path": u_path, "family": family, "extension": path.suffix.lower(),
                           "bytes": path.stat().st_size, "guid": metadata(path).get("guid"),
                           "referenced_by_saved_scenes": [name for name, deps in scene_deps.items() if u_path in deps]})
    inventory = {
        "project": "Deep and Darkness", "scope": "Assets/_Project excluding prior Graphics pack and TMP package from the new-art image count",
        "scene_dependency_evidence": recorded_scenes["evidence"],
        "limitations": ["Scene dependency results describe saved assets, include transitive and inactive references, and do not prove on-screen visibility or dynamic loading.",
                        "A missing serialized reference does not establish that an asset is unused.",
                        "FAL presence matches the previous local catalog by original SHA256; external library records lacking this provenance are not ruled out.",
                        "AssetDatabase query returned zero AudioClip assets in Assets/_Project; file scan covers common audio extensions.",
                        "Editor may contain unsaved scene changes; this inventory does not modify or save them."],
        "counts": {"new_art_images": len(images), "ui_images": sum(x["family"] == "ui" for x in image_records),
                   "background_images": sum(x["family"] == "background" for x in image_records),
                   "unique_image_contents": len(duplicates), "exact_duplicate_groups": len(duplicate_groups),
                   "project_font_files": len(font_files), "ui_controllers": len(controllers),
                   "embedded_ui_animation_clips": sum(x["embedded_animation_clip_count"] for x in controllers),
                   "prefabs": len(prefabs), "build_scenes": len(scenes), "audio_mixers": 1,
                   "source_audio_files": len(audio_clips), "unity_audio_clip_assets_at_inspection": audio_clip_snapshot["count"],
                   "previous_graphics_pngs": sum(p.suffix.lower() == ".png" for p in source_files),
                   "tmp_support_files": len(tmp_files), "generated_collectibles_outside_assets": len(generated_set["items"]),
                   "archive_files": len(archive_members), "archive_files_identical_to_unpacked": sum(m["sha256_equal"] for m in archive_members),
                   "asset_files_under_project_excluding_meta_and_os_metadata": len(file_index)},
        "images": image_records, "duplicate_image_groups": duplicate_groups,
        "fonts": [record(p, scene_deps, fal_sources) for p in font_files], "controllers": controllers,
        "prefabs": prefabs, "scenes": [record(p, scene_deps, fal_sources) for p in scenes],
        "audio": {"mixer": mixer, "source_files": audio_clips, "unity_audio_clip_snapshot": audio_clip_snapshot},
        "tmp_support": {"directory": project_path(TMP), "files_by_extension": dict(Counter(p.suffix.lower() or "[no extension]" for p in tmp_files)),
                        "note": "Support package includes a LiberationSans font, EmojiOne texture, shaders, materials and TMP configuration; not extra game-specific art."},
        "archive": {"path": project_path(archive), "members": archive_members},
        "previous_catalog": "docs/art/audit-2026-11-17/inventory.json",
        "generated_collectibles": "docs/art/collectibles-six/set.json", "project_asset_files": file_index,
    }
    (OUT / "inventory.json").write_text(json.dumps(inventory, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    world = [item for item in image_records if item["family"] == "background"]
    major = [item for item in image_records if item["family"] == "ui" and Path(item["unity_path"]).name in MAJOR_UI and "/AnimationSprites/" not in item["unity_path"]]
    controls = [item for item in image_records if item["family"] == "ui" and item not in major]
    draw_board(world, "backgrounds", "Level backgrounds / six 1800 x 505 layers", 2, 700, 270)
    draw_board(major, "ui-art", "Menu, logo, story and credit imagery", 3, 466, 310)
    draw_board(controls, "ui-controls", "Interface controls and panel source images", 4, 350, 210)
    lines = ["# Additional Unity project assets", "", "Read-only review of the saved project and its four build scenes. A scene dependency can be transitive or inactive. Files without a dependency may still be loaded dynamically.", "",
             "## Summary", "", f'- {inventory["counts"]["background_images"]} background images and {inventory["counts"]["ui_images"]} UI images, including {len(duplicate_groups)} exact duplicate pairs.',
             f'- {len(font_files)} project font files; {len(controllers)} UI Animator Controllers with {inventory["counts"]["embedded_ui_animation_clips"]} embedded clips; {len(prefabs)} prefabs; {len(scenes)} build scenes.',
             f'- One audio mixer; {len(audio_clips)} audio files found and {audio_clip_snapshot["count"]} AudioClip assets returned by the Unity AssetDatabase snapshot in `Assets/_Project`.',
             f'- The source ZIP contains {len(archive_members)} files, all {sum(m["sha256_equal"] for m in archive_members)} byte-identical to the unpacked Graphics folder.' if all(m["sha256_equal"] for m in archive_members) else '- The source ZIP and unpacked folder have differences; see inventory.json.',
             f'- {len(tmp_files)} TextMesh Pro support files are inventoried separately. The six previously generated collectibles remain under `docs/art/collectibles-six`, outside Unity `Assets`.',
             f'- The machine-readable index covers {len(file_index)} files under `Assets/_Project`, including code and rendering/input configuration, without `.meta`, `.gitkeep` or operating-system metadata files.',
             '', '## Visual review and provenance', '',
             '- Main menu art includes an orange-moon background, standalone logo, two illustrated story panels, a castle and three animal photographs. The project also has normal/pressed 32 x 32 button variants and panel elements.',
             '- Both gameplay scenes list all six background layers as dependencies. The upper graveyard layers visually match the KATO pack and the lower ocean layers visually match the Ansimuz pack from the earlier audit; exact derivation is not established by these visual comparisons.',
             '- The two story illustrations, logo and 32 x 32 UI elements create a useful reference for the game’s actual menu art direction. Five UI image pairs are byte-identical but have distinct GUIDs.',
             '- The prior FAL catalog was compared using source SHA256; no matching originals were found for these 41 additional art images in that catalog. This does not rule out other unindexed FAL media.',
             '', '## Image inventory', '', '| Unity path | Dimensions | Observed content | Saved-scene dependencies |', '| --- | --- | --- | --- |']
    for item in image_records:
        scenes_text = ", ".join(item["referenced_by_saved_scenes"]) or "None found in saved-scene dependencies"
        lines.append(f'| `{item["unity_path"]}` | {item["width"]} x {item["height"]} | {item["description"]} | {scenes_text} |')
    lines.extend(["", "## Byte-identical image pairs", "", "The copies have different Unity GUIDs; do not delete either solely because the pixels match.", ""])
    for group in duplicate_groups:
        lines.append("- " + " / ".join(f'`{m["unity_path"]}`' for m in group["members"]))
    lines.extend(["", "## Other authored asset files", "", "| Category | Unity path | Saved-scene dependencies |", "| --- | --- | --- |"])
    for category, group in (("Font", inventory["fonts"]), ("UI controller", controllers), ("Prefab", prefabs), ("Audio mixer", [mixer])):
        for item in group:
            names = ", ".join(item["referenced_by_saved_scenes"]) or "None found in saved-scene dependencies"
            lines.append(f'| {category} | `{item["unity_path"]}` | {names} |')
    lines.extend(["", "## Visual indexes", "", "- [World backgrounds](backgrounds.png)", "- [Menu, logo, story and credit images](ui-art.png)",
                  "- [Interface controls and panels](ui-controls.png)", "- [Machine-readable inventory](inventory.json)",
                  "- [Actual Unity AssetDatabase scene dependencies](unity-scene-dependencies.json)",
                  "", "No scene, prefab, asset, import setting or project source was modified by this audit."])
    (OUT / "README.md").write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(json.dumps(inventory["counts"]))


if __name__ == "__main__":
    main()
