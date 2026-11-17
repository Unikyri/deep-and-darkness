"""Render a game-scale inspection board from existing FAL outputs; no generation."""

import hashlib
import json
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]
DIR = ROOT / "docs/art/concepts/moon-pearl-relic-v1"


def main():
    original = Image.open(DIR / "concept-original.png").convert("RGB")
    cutout = Image.open(DIR / "concept-transparent.png").convert("RGBA")
    alpha = cutout.getchannel("A")
    bounds = alpha.point(lambda a: 255 if a > 16 else 0).getbbox()
    trimmed = cutout.crop(bounds)
    histogram = alpha.histogram()
    count = cutout.width * cutout.height
    facts = {
        "width": cutout.width, "height": cutout.height, "format": "PNG", "mode": cutout.mode,
        "alpha_extrema": list(alpha.getextrema()), "has_real_transparency": histogram[0] > 0 and histogram[255] > 0,
        "transparent_pixel_fraction": histogram[0] / count,
        "partial_alpha_pixel_fraction": sum(histogram[1:255]) / count,
        "visible_bounds_alpha_gt_16": list(bounds),
        "normalized_margins": [bounds[0] / cutout.width, bounds[1] / cutout.height,
                               (cutout.width - bounds[2]) / cutout.width, (cutout.height - bounds[3]) / cutout.height],
        "requested_minimum_margin": 0.15,
        "minimum_margin_passed": min(bounds[0], bounds[1], cutout.width - bounds[2], cutout.height - bounds[3]) / cutout.width >= 0.15,
        "checks": {"single_item": "passed_host_visual", "full_silhouette_visible": "passed_host_visual",
                   "pearl_crescent_shell_survive_cutout": "passed_host_visual",
                   "dark_background_edge_cleanup": "needs_review_thin_pale_fringe_visible",
                   "source_outline_weight_match": "failed_thicker_outline_than_source",
                   "user_art_direction_approval": "unverified", "animation_alignment": "not_applicable_single_concept",
                   "in_engine_readability": "unverified_not_integrated"},
        "sha256": hashlib.sha256((DIR / "concept-transparent.png").read_bytes()).hexdigest(),
        "sampling_limits": "Local PNG decoder and static composite inspection. No Unity gameplay, animation, or user approval check.",
    }
    board = Image.new("RGB", (1280, 900), "#171C29")
    draw = ImageDraw.Draw(board)
    title = ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf", 28)
    font = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 18)
    draw.text((32, 20), "MOON PEARL RELIC / CONCEPT 01", font=title, fill="#F0E5C9")
    draw.text((32, 60), "FAL-generated proposal - pending user review - no Unity asset replacement", font=font, fill="#AAB6CF")
    for i, (background, label) in enumerate((("#112336", "Dark gameplay background"), ("#E4E6E7", "Light background"))):
        x = 32 + i * 624
        panel = Image.new("RGB", (592, 490), background)
        large = trimmed.copy()
        large.thumbnail((490, 440), Image.Resampling.LANCZOS)
        panel.paste(large, ((592 - large.width) // 2, (490 - large.height) // 2), large)
        board.paste(panel, (x, 112))
        draw.text((x + 12, 614), label, font=font, fill="#D0D7E5")
        for j, size in enumerate((32, 48, 64, 96)):
            sprite = trimmed.copy()
            sprite.thumbnail((size, size), Image.Resampling.LANCZOS)
            bx = x + 12 + j * 143
            draw.rectangle((bx, 657, bx + 122, 783), fill=background)
            board.paste(sprite, (bx + (122 - sprite.width) // 2, 657 + (126 - sprite.height) // 2), sprite)
            draw.text((bx + 29, 794), f"{size} px", font=font, fill="#D0D7E5")
    draw.text((32, 841), "Review: thicker outline than source sprites; thin pale edge fringe; static preview only.", font=font, fill="#E4BA8A")
    draw.text((32, 868), "Margin is below requested 15%. Art direction and Unity integration remain pending.", font=font, fill="#E4BA8A")
    board.save(DIR / "review-board.png", optimize=True)
    (DIR / "qa.json").write_text(json.dumps(facts, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(facts))


if __name__ == "__main__":
    main()
