# Three-layer 4K menu parallax

The three final PNG sprites are under `Assets/_Project/Art/UI/MenuParallax/Parallax3/`:

1. `01-Far.png`: opaque distant background, moon, bats and the title already baked into the source image.
2. `02-Middle.png`: transparent ruins and small vegetation.
3. `03-Foreground.png`: transparent rocks, seabed and coral.

Every image is 3840 x 2160. Unity imported them as single sprites with maximum texture size 4096, bilinear filtering, no mipmaps and uncompressed default settings. [assembly-qa.json](assembly-qa.json) contains source and output hashes, alpha checks and the measured recomposition difference. The combined still differs from the source by approximately 0.167/255 mean absolute RGB; this is a coarse diagnostic, not a claim of pixel identity.

## Production

The source is the user's `MenuParallax/Background 1.jpg`, SHA-256 `b07e3ea57efb466ccf66d3adab951511c2e9e79f9df2b29ded5f854bf9e77a62`. A 2560 x 1440 lossy WebP proxy was uploaded to FAL for alignment, together with a previously saved high-resolution reference of the same composition. GPT Image 2.6 Sunburst Edit generated the far reconstruction and transparent layer studies. The initial middle study shifted ruins and was rejected; a mask-guided repair was generated.

Segmentation masks were inspected and used to preserve the actual source pixels and positions in the visible moving layers. GPT's far reconstruction fills concealed areas, and GPT middle and foreground outputs provide material under the original cutouts. The final sprites are locally assembled derivatives, not the unmodified raw FAL outputs. Mask resolution was 2560 x 1440, resampled to the 4K canvas. The original JPEG was not overwritten.

The FAL collection `06 Main Menu - Three Parallax Layers` and scene entity `DAD | Main Menu - Three-Layer Parallax` contain the source proxy and three raw GPT studies. The final composited PNGs are local Unity assets, not uploaded FAL originals.

## Prefab and scene

`MenuParallax3.prefab` contains exactly three UI Image layers and reuses the existing `MenuParallaxAnimation` component. Far, middle and foreground have vertical amplitudes 0.4, 3 and 6 and speeds 0.18, 0.35 and 0.5, respectively. All images have raycast disabled. The title is part of the far image and moves with it.

The prefab was created in a preview scene and verified structurally. **It has not been added to `MainMenu.unity`**: the user's Editor was in Play mode, and the running session was left undisturbed. The existing scene and buttons were not edited by this task. Runtime playback and aspect ratios beyond the 16:9 reference have not been verified.

After leaving Play mode, insert the prefab as the first child of `MainMenuPanel/Background`, before the existing buttons. The original `Background 1` image remains beneath it and covers any subpixel edges; keep the old individual art layers and old `MenuParallaxAnimation` component disabled. Confirm the button ordering and visually review the movement before saving the scene.
