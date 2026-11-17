"""Build a static reference from the saved menu's artwork layers, without UI buttons.

The Unity Editor currently has an earlier loaded scene while the saved scene was
updated externally. This reproduces the saved scene at its 1920x1080 design
resolution. It is a visual reference, not a Unity scene render or a replacement
for the separate animated layers. The original source files are never changed.
"""

from __future__ import annotations

import hashlib
import io
import json
import re
from pathlib import Path

from PIL import Image, ImageChops


ROOT = Path(__file__).resolve().parents[3]
PROJECT = ROOT / "DeepAndDarkness"
SCENE = PROJECT / "Assets/_Project/Scenes/MainMenu.unity"
OUT = ROOT / "docs/art/menu-upscale"
SIZE = (1920, 1080)
EXPECTED_LAYERS = {"FarBats", "Moon", "Ruins", "SeaFloor", "CloseBats", "CloseBats (1)", "Corals", "Corals2", "Logo"}


def hash_file(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def field(body: str, name: str) -> str:
    match = re.search(r"^  " + re.escape(name) + r": (.*)$", body, re.M)
    if not match:
        raise ValueError(f"Missing serialized field {name}")
    return match.group(1)


def vector(value: str) -> dict[str, float]:
    return {k: float(v) for k, v in re.findall(r"([xyzwrgba]):\s*(-?[0-9.eE]+)", value)}


def make_proxy(image: Image.Image, path: Path) -> dict:
    """Make an honestly recorded <=64 KiB full-size WebP source proxy for FAL."""
    for width in (1920, 1600, 1376, 1280, 1152, 1024):
        height = round(width * SIZE[1] / SIZE[0])
        im = image.convert("RGB")
        if width != SIZE[0]:
            im = im.resize((width, height), Image.Resampling.LANCZOS)
        for quality in (95, 90, 85, 80, 75, 70, 65, 60, 55, 50, 45, 40):
            buf = io.BytesIO()
            im.save(buf, format="WEBP", quality=quality, method=6)
            if len(buf.getvalue()) <= 64 * 1024:
                path.write_bytes(buf.getvalue())
                return {"path": path.relative_to(ROOT).as_posix(), "width": width, "height": height,
                        "bytes": len(buf.getvalue()), "format": "webp", "quality": quality,
                        "source": "rendered saved-scene reference; lossy proxy for upload", "sha256": hash_file(path)}
    raise ValueError("Could not create an upload reference within the 64 KiB input limit")


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    data = SCENE.read_text(encoding="utf-8")
    chunks = re.split(r"^--- !u!(\d+) &(-?\d+)(?: stripped)?\s*$", data, flags=re.M)
    docs = [(int(chunks[i]), chunks[i + 1], chunks[i + 2]) for i in range(1, len(chunks), 3)]
    objects = {fid: body for typ, fid, body in docs if typ == 1}
    transforms = {fid: body for typ, fid, body in docs if typ == 224}
    image_components = {}
    for typ, _, body in docs:
        if typ != 114 or "UnityEngine.UI::UnityEngine.UI.Image" not in body:
            continue
        go = re.search(r"^  m_GameObject: \{fileID: (-?\d+)\}", body, re.M)
        sprite = re.search(r"^  m_Sprite: \{fileID: (-?\d+), guid: ([0-9a-f]{32})", body, re.M)
        if go and sprite:
            image_components[go.group(1)] = {"guid": sprite.group(2), "file_id": sprite.group(1),
                                             "preserve_aspect": field(body, "m_PreserveAspect") == "1",
                                             "enabled": field(body, "m_Enabled") == "1",
                                             "color": vector(field(body, "m_Color"))}
    guid_paths = {}
    folder = PROJECT / "Assets/_Project/Art/UI/MenuParallax"
    for path in folder.glob("*.meta"):
        match = re.search(r"^guid:\s*([0-9a-f]{32})", path.read_text(encoding="utf-8"), re.M)
        if match:
            guid_paths[match.group(1)] = path.with_suffix("")
    bg_id = "793548692"
    base = transforms[bg_id]
    if vector(field(base, "m_AnchorMin")) != {"x": 0.0, "y": 0.0} or vector(field(base, "m_AnchorMax")) != {"x": 1.0, "y": 1.0}:
        raise ValueError("Menu background anchoring changed; re-inspect scene")
    bg_go = re.search(r"^  m_GameObject: \{fileID: (-?\d+)\}", base, re.M).group(1)
    bg_image = image_components[bg_go]
    bg_path = guid_paths[bg_image["guid"]]
    with Image.open(bg_path) as source:
        board = source.convert("RGBA").resize(SIZE, Image.Resampling.LANCZOS)
    parent = re.search(r"  m_Children:\s*\n((?:  - \{fileID: -?\d+\}\s*\n)+)", base).group(1)
    layer_info = []
    logo_layer = Image.new("RGBA", SIZE, (0, 0, 0, 0))
    for tid in re.findall(r"fileID: (-?\d+)", parent):
        if tid not in transforms:
            continue
        transform = transforms[tid]
        go_match = re.search(r"^  m_GameObject: \{fileID: (-?\d+)\}", transform, re.M)
        if not go_match or go_match.group(1) not in objects:
            continue
        go_id = go_match.group(1)
        go = objects[go_id]
        name = field(go, "m_Name")
        if name not in EXPECTED_LAYERS:
            continue
        if field(go, "m_IsActive") != "1" or go_id not in image_components or not image_components[go_id]["enabled"]:
            raise ValueError(f"Expected artwork layer inactive: {name}")
        image_info = image_components[go_id]
        if image_info["file_id"] != "21300000":
            raise ValueError(f"Layer uses a sliced sprite; handle its rect explicitly: {name}")
        if vector(field(transform, "m_AnchorMin")) != {"x": 0.5, "y": 0.5} or vector(field(transform, "m_AnchorMax")) != {"x": 0.5, "y": 0.5}:
            raise ValueError(f"Expected center anchors changed: {name}")
        size = vector(field(transform, "m_SizeDelta"))
        position = vector(field(transform, "m_AnchoredPosition"))
        scale = vector(field(transform, "m_LocalScale"))
        if scale.get("x") != 1 or scale.get("y") != 1:
            raise ValueError(f"Expected unit transform scale changed: {name}")
        path = guid_paths[image_info["guid"]]
        with Image.open(path) as img:
            sprite = img.convert("RGBA")
        w, h = size["x"], size["y"]
        if image_info["preserve_aspect"]:
            factor = min(w / sprite.width, h / sprite.height)
            w, h = sprite.width * factor, sprite.height * factor
        sprite = sprite.resize((round(w), round(h)), Image.Resampling.BILINEAR)
        color = image_info["color"]
        if any(abs(color.get(c, 1) - 1) > 0.00001 for c in "rgba"):
            tint = Image.new("RGBA", sprite.size, tuple(round(color.get(c, 1) * 255) for c in "rgba"))
            sprite = ImageChops.multiply(sprite, tint)
        x = round(SIZE[0] / 2 + position["x"] - sprite.width / 2)
        y = round(SIZE[1] / 2 - position["y"] - sprite.height / 2)
        if name == "Logo":
            background_path = OUT / "reference-background-without-logo.png"
            board.convert("RGB").save(background_path, optimize=True)
            logo_layer.alpha_composite(sprite, (x, y))
            logo_layer.save(OUT / "reference-logo-layer.png", optimize=True)
        board.alpha_composite(sprite, (x, y))
        layer_info.append({"name": name, "unity_guid": image_info["guid"], "source": path.relative_to(ROOT).as_posix(),
                           "source_sha256": hash_file(path), "center_anchored_position": position,
                           "declared_rect": size, "drawn_size": [sprite.width, sprite.height], "drawn_top_left": [x, y],
                           "preserve_aspect": image_info["preserve_aspect"], "tint": color})
    if {item["name"] for item in layer_info} != EXPECTED_LAYERS:
        raise ValueError("Saved-scene artwork layers changed; reference is incomplete")
    final_path = OUT / "saved-menu-reference-1920.png"
    board.convert("RGB").save(final_path, optimize=True)
    proxy = make_proxy(board, OUT / "saved-menu-reference-proxy.webp")
    report = {"source_scene": SCENE.relative_to(ROOT).as_posix(), "source_scene_sha256": hash_file(SCENE),
              "resolution": SIZE, "canvas_assumption": "1920 x 1080 reference resolution and 16:9 viewport",
              "sampling_limits": "Static serialized anchored positions and sibling order, without runtime parallax movement or interactive controls. Pillow interpolation/tint may differ from Unity rendering.",
              "base_image": bg_path.relative_to(ROOT).as_posix(), "base_sha256": hash_file(bg_path),
              "layers": layer_info, "full_reference": final_path.relative_to(ROOT).as_posix(), "upload_proxy": proxy}
    (OUT / "reference-provenance.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"layers": len(layer_info), "reference": str(final_path), "proxy": proxy}))


if __name__ == "__main__":
    main()
