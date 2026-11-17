# Deep and Darkness art catalog and FAL workflow

**Collectibles:** the user approved the Moon Pearl visual direction and requested five additional object icons. The complete six-item collection is available in [collectibles-six](collectibles-six/README.md), with a [comparison board](collectibles-six/collection-preview.png) and [downloadable transparent PNG pack](collectibles-six/deep-and-darkness-collectibles-six.zip).

**Project-wide asset review:** the [additional Unity asset inventory](project-asset-discovery/README.md) catalogs 41 further images, four font files, nine UI controllers, seven prefabs and the audio mixer as of that audit. The saved MainMenu scene and its parallax layers were changed externally after the inventory snapshot, so the newer menu sources are documented in the following study. The [visual index](project-asset-discovery/ui-art.png) shows the earlier menu, story art and photographs; [background layers](project-asset-discovery/backgrounds.png) and [UI controls](project-asset-discovery/ui-controls.png) are indexed separately.

**Main menu study:** the [4K menu candidate](menu-upscale/README.md) is a static approximation built from the newer saved parallax artwork and processed through FAL. The title and overall composition remain recognizable, but the pixel-art appearance has not been eliminated. The current project FAL catalog records 18 source references, 14 generated images and 16 entities.

The source art was inspected on 2026-11-17. The requested workflow uses the connected FAL plugin for reference organization, generation and extraction. Existing Unity source files remain the source of truth. All art described below has been inspected; author identity for the six loose sheets is based on the user's description, not independently verified.

## Deliverables

- [Full inventory](audit-2026-11-17/inventory.md): all 23 PNGs, their dimensions and observed purpose.
- [Machine-readable inventory](audit-2026-11-17/inventory.json): hashes, alpha facts, duplicates, provenance and editable-source metadata.
- [Character and prop board](audit-2026-11-17/characters-props.png), [graveyard layers](audit-2026-11-17/graveyard.png), [underwater layers](audit-2026-11-17/underwater.png).
- [FAL catalog manifest](fal-catalog.json): confirmed library asset, collection and entity IDs, source paths and reference roles.
- [Collectible review board](concepts/moon-pearl-relic-v1/review-board.png), [original generation](concepts/moon-pearl-relic-v1/concept-original.png), [transparent extraction](concepts/moon-pearl-relic-v1/concept-transparent.png), [mask](concepts/moon-pearl-relic-v1/cutout-mask.png), [decoder/visual checks](concepts/moon-pearl-relic-v1/qa.json).

## What each source family contains

| Family | Observed content | Production implications |
| --- | --- | --- |
| Gothic bat | 2152x731 sheet: six flight poses and a final stars/hurt pose | Split the hurt pose from the flight loop; frame timing and alignment still need authoring. |
| Mermaid | 2120x742 sheet: six swimming poses and a final stars/hurt pose | Preserve silver hair, teal tail, right-facing direction and consistent body placement. |
| Spectral hazard | 743x162 sheet: three cyan skull/wisp poses facing left | Keep its cool, threatening silhouette distinct from reward shapes. |
| Legacy collectible | 2172x724 sheet: four violet crystal rotation poses | Retained as source history while a replacement direction is reviewed. |
| Life icons | 1774x887 RGB sheet: three blood-vial and three pearl/shell states | The checkerboard is painted into the image. There is no alpha channel; it is not a ready transparent UI sheet. |
| Obstacle sheet | 1536x1024: nine graveyard props and nine underwater props | Contains both scenery and potential hazards. Visual presence alone does not define a damaging collider. |
| Graveyard pack | Five layer families in 800x200 and 4067x1017 versions; an extra ground PNG duplicates the 800x200 ground exactly | Select one resolution family for production. Check horizontal seams before claiming repeatability. |
| Underwater pack | 256x192 far, sand and two foreground layers; 512x192 merged foreground; 768x192 composite preview | The preview is a reference, not another parallax layer. Effective pixel density differs substantially from the characters. |
| Editable sources | Graveyard Aseprite: 800x200, one frame, five generically named layers. Underwater PSD: 512x192 with back, sand and foreground layers | Preserve editable containers and derive exports deliberately. |

The four `.DS_Store` files are operating-system metadata. The underwater marketing PDF is pack documentation. Neither is game art.

## Provenance and permissions recorded from the supplied files

The graveyard package identifies **KATO** as its author. Its local `License.txt.txt` permits use and modification in games, prohibits standalone asset redistribution and AI training, and requests optional credit. Its 11 PNGs and editable source are cataloged locally; no KATO reference pixels were uploaded to FAL. The graveyard prop entity uses the separate user-supplied obstacle sheet.

The underwater package identifies **Luis Zuno / Ansimuz**. Its supplied `public-license.pdf` declares CC0. The six underwater PNG references are saved in FAL with that provenance. This is a record of the included license, not an independent ownership investigation.

The loose character, collectible, hazard, obstacle and UI sheets have no accompanying license in this folder. They are recorded as user-supplied team art. No model training was requested or performed.

## Initial FAL organization

The `Deep and Darkness` collection has three children:

| Collection | Confirmed image assets | Meaning |
| --- | ---: | --- |
| 01 Source References | 17 | Six loose-sheet proxies, six underwater originals, three cropped character/hazard anchors and two obstacle-row references. |
| 02 Generated Concepts | 2 | Original Moon Pearl generation and its transparent extraction. |
| 03 Approved Production | 0 | Reserved for exact outputs explicitly approved by the user and checked for integration. |

The source collection has eleven reduced palette proxies (including the five cropped anchors) and six original underwater files, with the exact kind and dimensions in the manifest. Full-resolution originals remain local. A proxy establishes visual vocabulary; it does not establish exact source pixels or production quality.

Ten entities were created and read back from FAL: Gothic Bat, Silver-haired Mermaid, Spectral Skull Hazard, Violet Crystal Legacy, Graveyard Prop Vocabulary, Underwater Prop Vocabulary, Blood and Pearl Life Icons, Underwater Ruins, Existing Character and Prop Style, and Moon Pearl Relic Concept 01. FAL's entity types include character, prop, environment, style and scene. A sprite sheet is a media asset linked to an appropriate entity; there is no separate sprite entity type in the exposed tools.

Defining anchors are separate from gallery sheets. New variants should use the defining references through `resolve_model_inputs`, then inspect the returned result before dependent animation work. Generated candidates must not silently become approved style references.

## First generation and checks

`google/nano-banana-2.1/edit` was selected by FAL's live recommendation for a one-image edit using the existing bat and mermaid as style references. It produced a single Moon Pearl Relic concept: pale gold pearl, metal crescent with bat-wing tips, teal shell base. Seed: `947162`. `bria/extract-object` then produced the transparent image and mask. Exactly two model jobs were run; no animation, training or further regeneration was submitted.

The original and extracted images decode at **1024x1024**. The extraction is RGBA with actual alpha values from 0 to 255, and the pearl, shell and crescent remain present. A static board compares the cutout on light and dark backgrounds at 32, 48, 64 and 96 pixels.

At initial delivery this was an **unapproved concept**. The user subsequently approved its visual direction as the reference for five more collectible objects. Its technical review items remain recorded:

- Its outline is thicker and its shapes more graphic than the supplied character sprites.
- A thin pale fringe is visible against the dark comparison background.
- Its outer margins are approximately 8-13%, below the requested 15%; the silhouette is fully visible.
- Animation alignment, runtime readability, world scale and gameplay integration remain unverified.

FAL's lineage read currently exposes only the extraction job and its output. It does not expose the parent concept or character references as ancestors. The local recipe journal records the actual submitted relationships; the catalog does not claim those missing links were captured by FAL.

The advertised `inspect_images` tool returned `Tool inspect_images not found`. Actual PNG files were downloaded once and inspected with host vision and Pillow instead. A technical feedback report was sent through the explicitly requested FAL plugin, without private media URLs or personal data.

## Current Unity integration boundary

No gameplay scripts, prefabs, scenes, import settings or source images were edited by this art task. The 23 PNGs in this incoming `Graphics` directory had no per-file `.meta` files at inspection time. Existing scene background references point to separately exported images in `Assets/_Project/Art/Backgrounds`.

Both gameplay scenes use the package's `Circle.png` GUID (`a86470a33a6bf42c4b3595704624658b`) for the character visuals, with tinting. `Collectible.prefab` and `Hazard.prefab` still use built-in sprite references. Replacing those placeholders is a separate integration step after visual direction is chosen.

## Next production step

The first collectible's direction is now approved and the six static object icons have been delivered. Review the five new designs, clean any pale edge fringe and verify their runtime scale before Unity integration. Animation, if requested later, needs aligned frames. For existing characters, slice the six movement poses separately from the hurt pose. For UI, generate or extract each state with verified alpha. Bring approved exports into project-owned art folders and preserve `.meta` GUIDs on subsequent revisions.

Use FAL for reference-driven variants, batching independent poses, cutouts and provenance. Use deterministic Unity authoring for slicing, pivots, pixels per unit, clips, colliders and atlases. A generative sheet still needs frame-count, alignment and loop checks.

## Reproduce the local inspection

From the repository root:

```powershell
uv run --with pillow --with pypdf python docs/art/audit_assets.py
uv run --with pillow python docs/art/review_concept.py
```

These scripts read originals and render local inventory/inspection artifacts. They do not upload files, run FAL jobs or change Unity assets. Actual generation recipes and original-account job receipts are retained in a private journal under the user's Codex directory, outside the repository.

## Six collectible object expansion

The five new objects are Tide Lantern, Abyssal Key, Bottled Message, Nautilus Compass and Tidebound Grimoire. Each was generated from the exact approved Moon Pearl output using the retained original endpoint and settings, with a new object prompt and seed. Five independent edit jobs and five extraction jobs were submitted in two bounded batches. No rejected or missing job was regenerated, and no automatic creative repair was run.

All six icons decode as 1024x1024 PNGs with real alpha and complete silhouettes. The comparison board includes 48px and 64px previews. Painted glass, lantern flame, key teeth, bottle parchment, compass needle and book page block remain visible after extraction. Fine pale edge fringes remain a production cleanup consideration. The new five designs have been supplied for user review; the user's earlier approval applies to the first design and shared direction.

The additional `04 Collectibles - Six Objects` collection was read back with exactly six image members. Each new collectible also has a separate prop entity, and its original generation is linked as gallery media. The main catalog records 15 total entities, 17 source references and 12 generated images. The six PNGs, normalized archive manifest, QA record and preview are packaged together. Unity scenes and prefabs were not changed by this pass.
