# Main menu 4K study

## Clean landscape redraw

The user confirmed a landscape game and requested a full-screen image with smooth edges, no distortion and no empty space. FAL produced a clean redraw of the saved-scene reference while retaining the overall layout and the `DEEP & DARKNESS` title. It is a new interpretation, not pixel-identical artwork or an approved final.

- [Unity preview asset](../../../DeepAndDarkness/Assets/_Project/Art/UI/Generated/FAL_MainMenu_Landscape_4K_Preview.png): 3840 x 2160 PNG, exact 16:9. Imported as a Sprite with max size 4096, bilinear filtering, no mipmaps and uncompressed default platform settings. Standalone, WebGL and Android overrides were disabled at verification time.
- [4K file](menu-fal-clean-16x9-4k.png), [1920 x 1080 preview](menu-fal-clean-16x9-preview.jpg), [exact-ratio master](menu-fal-clean-16x9-master.png), and [original FAL result](menu-fal-clean-landscape-original.png).
- The FAL result was 5504 x 3072 despite the 16:9 request. To preserve every source pixel without squeezing or cropping, 12 mirrored pixels were added to the top and bottom, producing a 5504 x 3096 master. The center was verified pixel-for-pixel against the original. The master was then resized to 3840 x 2160. The added edge strips are reflected image content, not blank bars.
- The original FAL result is saved in the `05 Main Menu - 4K Study` collection and associated with the existing scene entity. The local exact-ratio master and 4K PNG are derivatives, not the original FAL output.
- The new Sprite is available for review in Unity, but is **not assigned to the MainMenu scene**. It is a flat still containing the title and scenery; assigning it without adapting the existing layers would duplicate artwork and lose the independently moving parallax. The saved scene hash was unchanged after import. The 16:9 asset fills a 16:9 viewport without distortion or empty bars. Wider or taller screens need adapted edges or cropping; rendering in the live game has not been tested.

## Earlier upscale study

The earlier request was to preserve the existing menu image and title while increasing its resolution and reducing visible pixelation. The selected earlier upscale is [menu-fal-upscaled-4k.png](menu-fal-upscaled-4k.png), a **3840 x 2160** PNG produced through FAL. Its preview is [menu-4k-preview.jpg](menu-4k-preview.jpg); the preview itself is 1920 x 1080.

## Source and limitations

The running Editor had an older loaded MainMenu view while the scene and artwork were updated externally. The newer saved `MainMenu.unity` uses a background plus nine separate artwork layers: bats, moon, ruins, seabed, corals and logo. Those layers participate in the current menu's parallax behavior. `reconstruct_reference.py` assembled a 1920 x 1080 **static approximation** of their saved positions, tint and sibling order, excluding the interactive buttons. The source-scene and layer SHA256 hashes are recorded in [reference-provenance.json](reference-provenance.json) and matched the files after processing.

The reference was sent to FAL as a full-resolution, **lossy WebP** proxy. The reference is not a pixel-exact Unity render and does not capture runtime motion. The result is a single flattened image: it cannot directly replace the layered menu while retaining independently animated parallax or interactive UI. No Unity scene, prefab, texture or import settings were changed by this work.

## Results and review

- **Text Refine candidate:** the title `DEEP & DARKNESS`, moon, bats, seabed and main composition remain recognizable and closely positioned. This is the more faithful of the two results, selected by visual comparison.
- **CGI comparison:** a single targeted repair was run to try to improve the pixelated edges. It produced [menu-fal-cgi-4k.png](menu-fal-cgi-4k.png), but still retained the pixel-art appearance and sharpened some edges more strongly.
- Both outputs decode as 3840 x 2160 RGB PNGs. The first candidate has a mean absolute RGB difference of approximately 1.60/255 after downscaling against the local reference; this is only a coarse diagnostic, not proof of identical details or aesthetics.
- **Visible pixelation remains.** Increasing image resolution alone did not turn the pixel-art source into a smooth high-resolution illustration. A closer non-pixelated result would require deliberate redrawing or a different style interpretation, which may change fine details.
- [comparison-three-qa.png](comparison-three-qa.png) compares the reference and both outputs. [qa.json](qa.json) records decoder and inspection results. Neither output has user approval or has been integrated in Unity.

FAL contains a `05 Main Menu - 4K Study` collection and the scene entity `DAD | Main Menu - 4K Static Study`; the more faithful Text Refine image is its defining reference. The lossy source proxy, CGI comparison and candidate are retained for traceability. Exact original-account job receipts remain in a private journal outside the repository.
