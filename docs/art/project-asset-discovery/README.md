# Additional Unity project assets

> Historical snapshot: the saved MainMenu scene and its parallax art changed after this audit; some menu image paths and scene dependencies below are stale. See the newer [main menu study](../menu-upscale/README.md).

Read-only review of the saved project and its four build scenes. A scene dependency can be transitive or inactive. Files without a dependency may still be loaded dynamically.

## Summary

- 6 background images and 35 UI images, including 5 exact duplicate pairs.
- 4 project font files; 9 UI Animator Controllers with 45 embedded clips; 7 prefabs; 4 build scenes.
- One audio mixer; 0 audio files found and 0 AudioClip assets returned by the Unity AssetDatabase snapshot in `Assets/_Project`.
- The source ZIP contains 32 files, all 32 byte-identical to the unpacked Graphics folder.
- 37 TextMesh Pro support files are inventoried separately. The six previously generated collectibles remain under `docs/art/collectibles-six`, outside Unity `Assets`.
- The machine-readable index covers 157 files under `Assets/_Project`, including code and rendering/input configuration, without `.meta`, `.gitkeep` or operating-system metadata files.

## Visual review and provenance

- Main menu art includes an orange-moon background, standalone logo, two illustrated story panels, a castle and three animal photographs. The project also has normal/pressed 32 x 32 button variants and panel elements.
- Both gameplay scenes list all six background layers as dependencies. The upper graveyard layers visually match the KATO pack and the lower ocean layers visually match the Ansimuz pack from the earlier audit; exact derivation is not established by these visual comparisons.
- The two story illustrations, logo and 32 x 32 UI elements create a useful reference for the game’s actual menu art direction. Five UI image pairs are byte-identical but have distinct GUIDs.
- The prior FAL catalog was compared using source SHA256; no matching originals were found for these 41 additional art images in that catalog. This does not rule out other unindexed FAL media.

## Image inventory

| Unity path | Dimensions | Observed content | Saved-scene dependencies |
| --- | --- | --- | --- |
| `Assets/_Project/Art/Backgrounds/LowerWorldBGFar.png` | 1800 x 505 | Opaque blue underwater distance with tiled ruined arches and plants. | Gameplay, GameplayLevel2 |
| `Assets/_Project/Art/Backgrounds/LowerWorldBGNear.png` | 1800 x 505 | Blue seabed, reef shapes and sea plants; underwater middle layer. | Gameplay, GameplayLevel2 |
| `Assets/_Project/Art/Backgrounds/LowerWorldFG.png` | 1800 x 505 | Underwater rock formation, vegetation, statue and broken column; foreground. | Gameplay, GameplayLevel2 |
| `Assets/_Project/Art/Backgrounds/UpperWorldBGFar.png` | 1800 x 505 | Night sky and large orange moon; graveyard far layer. | Gameplay, GameplayLevel2 |
| `Assets/_Project/Art/Backgrounds/UpperWorldBGNear.png` | 1800 x 505 | Blue hills, ruined chapel, bare trees and hanging lanterns; graveyard middle layer. | Gameplay, GameplayLevel2 |
| `Assets/_Project/Art/Backgrounds/UpperWorldFG.png` | 1800 x 505 | Graveyard fence, tombstones, pumpkins, trees and orange ground; foreground. | Gameplay, GameplayLevel2 |
| `Assets/_Project/Art/UI/AnimationSprites/BannerExp.png` | 993 x 150 | Wide rectangular dark banner with a fine border. | None found in saved-scene dependencies |
| `Assets/_Project/Art/UI/AnimationSprites/ESC.png` | 82 x 82 | Dark square key or button backing with a purple border. | None found in saved-scene dependencies |
| `Assets/_Project/Art/UI/AnimationSprites/Espace.png` | 207 x 50 | Wide dark key or button backing with a purple border. | None found in saved-scene dependencies |
| `Assets/_Project/Art/UI/AnimationSprites/Group 1.png` | 207 x 96 | Six square UI accents arranged around a horizontal orange line. | None found in saved-scene dependencies |
| `Assets/_Project/Art/UI/AnimationSprites/Panel.png` | 1483 x 742 | Dark rectangular interface panel with a colored ornamental border. | None found in saved-scene dependencies |
| `Assets/_Project/Art/UI/arrow_left_normal.png` | 32 x 32 | 32 x 32 pixel-art left arrow button, normal state. | MainMenu, PausaScene |
| `Assets/_Project/Art/UI/arrow_left_pressed.png` | 32 x 32 | 32 x 32 pixel-art left arrow button, pressed state. | MainMenu, PausaScene |
| `Assets/_Project/Art/UI/arrow_right_normal.png` | 32 x 32 | 32 x 32 pixel-art right arrow button, normal state. | MainMenu |
| `Assets/_Project/Art/UI/arrow_right_pressed.png` | 32 x 32 | 32 x 32 pixel-art right arrow button, pressed state. | MainMenu |
| `Assets/_Project/Art/UI/BackgtoundMenu.png` | 1376 x 768 | Composite menu artwork with the Deep & Darkness logo on an orange moon. | None found in saved-scene dependencies |
| `Assets/_Project/Art/UI/BannerExp.png` | 993 x 150 | Wide rectangular dark banner with a fine border. | MainMenu, PausaScene |
| `Assets/_Project/Art/UI/ESC.png` | 82 x 82 | Dark square key or button backing with a purple border. | MainMenu, PausaScene |
| `Assets/_Project/Art/UI/FondoNoTitle.jpg` | 1376 x 768 | Menu background with orange moon, bats and aquatic details, without the title. | MainMenu |
| `Assets/_Project/Art/UI/Group 1.png` | 207 x 96 | Six square UI accents arranged around a horizontal orange line. | MainMenu, PausaScene |
| `Assets/_Project/Art/UI/home_normal.png` | 32 x 32 | 32 x 32 pixel-art home button, normal state. | PausaScene |
| `Assets/_Project/Art/UI/home_pressed.png` | 32 x 32 | 32 x 32 pixel-art home button, pressed state. | PausaScene |
| `Assets/_Project/Art/UI/HR_Castle of Lost Shadows.png` | 1920 x 1080 | Blue pixel-art castle under a moonlit sky. | None found in saved-scene dependencies |
| `Assets/_Project/Art/UI/Logo1.png` | 488 x 404 | Standalone Deep & Darkness logo with moon, bats and ocean details. | MainMenu, PausaScene |
| `Assets/_Project/Art/UI/Panel.png` | 1483 x 742 | Dark rectangular interface panel with a colored ornamental border. | MainMenu, PausaScene |
| `Assets/_Project/Art/UI/play1_normal.png` | 32 x 32 | 32 x 32 pixel-art play button, normal state. | MainMenu, PausaScene |
| `Assets/_Project/Art/UI/play1_pressed.png` | 32 x 32 | 32 x 32 pixel-art play button, pressed state. | MainMenu, PausaScene |
| `Assets/_Project/Art/UI/reset_normal.png` | 32 x 32 | 32 x 32 pixel-art reset button, normal state. | PausaScene |
| `Assets/_Project/Art/UI/reset_pressed.png` | 32 x 32 | 32 x 32 pixel-art reset button, pressed state. | PausaScene |
| `Assets/_Project/Art/UI/Role2Image.jpeg` | 672 x 871 | Photograph of a black dog wearing a bow tie. | MainMenu |
| `Assets/_Project/Art/UI/Role3Image.jpeg` | 1200 x 1600 | Photograph of a white-and-brown cat wearing a collar. | MainMenu |
| `Assets/_Project/Art/UI/Role4Image.jpeg` | 756 x 967 | Photograph of a cat lying on its back. | MainMenu |
| `Assets/_Project/Art/UI/settings1_normal.png` | 32 x 32 | 32 x 32 pixel-art settings button, normal state. | MainMenu, PausaScene |
| `Assets/_Project/Art/UI/settings1_pressed.png` | 32 x 32 | 32 x 32 pixel-art settings button, pressed state. | MainMenu, PausaScene |
| `Assets/_Project/Art/UI/Space.png` | 207 x 50 | Wide dark key or button backing with a purple border. | MainMenu, PausaScene |
| `Assets/_Project/Art/UI/StoryImage1.png` | 1836 x 856 | Story artwork showing a vampire above water and a mermaid below a moonlit landscape. | MainMenu |
| `Assets/_Project/Art/UI/StoryImage2.png` | 1836 x 856 | Story artwork showing the vampire and mermaid linked by glowing objects across the water. | MainMenu |
| `Assets/_Project/Art/UI/trophy_normal.png` | 32 x 32 | 32 x 32 pixel-art trophy button, normal state. | MainMenu |
| `Assets/_Project/Art/UI/trophy_pressed.png` | 32 x 32 | 32 x 32 pixel-art trophy button, pressed state. | MainMenu |
| `Assets/_Project/Art/UI/volume_normal.png` | 32 x 32 | 32 x 32 pixel-art volume button, normal state. | MainMenu, PausaScene |
| `Assets/_Project/Art/UI/volume_pressed.png` | 32 x 32 | 32 x 32 pixel-art volume button, pressed state. | MainMenu, PausaScene |

## Byte-identical image pairs

The copies have different Unity GUIDs; do not delete either solely because the pixels match.

- `Assets/_Project/Art/UI/AnimationSprites/BannerExp.png` / `Assets/_Project/Art/UI/BannerExp.png`
- `Assets/_Project/Art/UI/AnimationSprites/ESC.png` / `Assets/_Project/Art/UI/ESC.png`
- `Assets/_Project/Art/UI/AnimationSprites/Espace.png` / `Assets/_Project/Art/UI/Space.png`
- `Assets/_Project/Art/UI/AnimationSprites/Group 1.png` / `Assets/_Project/Art/UI/Group 1.png`
- `Assets/_Project/Art/UI/AnimationSprites/Panel.png` / `Assets/_Project/Art/UI/Panel.png`

## Other authored asset files

| Category | Unity path | Saved-scene dependencies |
| --- | --- | --- |
| Font | `Assets/_Project/Art/Fonts/DotGothic16-Regular SDF.asset` | MainMenu, PausaScene |
| Font | `Assets/_Project/Art/Fonts/DotGothic16-Regular.ttf` | MainMenu, PausaScene |
| Font | `Assets/_Project/Art/Fonts/PixelifySans-VariableFont_wght SDF.asset` | None found in saved-scene dependencies |
| Font | `Assets/_Project/Art/Fonts/PixelifySans-VariableFont_wght.ttf` | None found in saved-scene dependencies |
| UI controller | `Assets/_Project/Animations/UI/Continue.controller` | MainMenu |
| UI controller | `Assets/_Project/Animations/UI/ContinueGame.controller` | PausaScene |
| UI controller | `Assets/_Project/Animations/UI/Controls.controller` | MainMenu, PausaScene |
| UI controller | `Assets/_Project/Animations/UI/Credicts.controller` | MainMenu |
| UI controller | `Assets/_Project/Animations/UI/MainMenu.controller` | PausaScene |
| UI controller | `Assets/_Project/Animations/UI/Play.controller` | MainMenu |
| UI controller | `Assets/_Project/Animations/UI/Restart.controller` | PausaScene |
| UI controller | `Assets/_Project/Animations/UI/Sound.controller` | MainMenu, PausaScene |
| UI controller | `Assets/_Project/Animations/UI/Volver.controller` | MainMenu, PausaScene |
| Prefab | `Assets/_Project/Prefabs/Collectible.prefab` | Gameplay, GameplayLevel2 |
| Prefab | `Assets/_Project/Prefabs/Hazard.prefab` | Gameplay, GameplayLevel2 |
| Prefab | `Assets/_Project/Prefabs/UI/Controls.prefab` | MainMenu, PausaScene |
| Prefab | `Assets/_Project/Prefabs/UI/Credits.prefab` | MainMenu |
| Prefab | `Assets/_Project/Prefabs/UI/Play.prefab` | MainMenu |
| Prefab | `Assets/_Project/Prefabs/UI/Sound.prefab` | MainMenu |
| Prefab | `Assets/_Project/Prefabs/UI/Volver.prefab` | PausaScene |
| Audio mixer | `Assets/_Project/Audio/GameAudioMixer.mixer` | MainMenu, PausaScene |

## Visual indexes

- [World backgrounds](backgrounds.png)
- [Menu, logo, story and credit images](ui-art.png)
- [Interface controls and panels](ui-controls.png)
- [Machine-readable inventory](inventory.json)
- [Actual Unity AssetDatabase scene dependencies](unity-scene-dependencies.json)

No scene, prefab, asset, import setting or project source was modified by this audit.
