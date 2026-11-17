# Seedance menu animation study

The user selected the FAL-generated clean menu image and requested bubbles rising and bats flying. The output was generated with `fal-ai/bytedance/seedance/v1/pro/image-to-video`, using the selected image as both the start and end reference, a fixed camera, and a six-second request.

The [refined preview](main-menu-seedance-refined-original.mp4) contains rising bubbles and a flying bat. The bat is much larger than requested in the production prompt and covers the logo for several seconds. The initial take, [first preview](main-menu-seedance-original.mp4), also has oversized foreground bats obscuring the title. The refined preview is the recommended one for user review, not an approved production asset.

Both original FAL files decode as H.264, 1920 x 1088, 24 fps, duration 6.041667 seconds, with no audio stream. The dimensions are close to, but not exactly, 16:9. The first and last decoded frames are not pixel-identical, so a seamless loop is not verified. Visual inspection sampled 24 frames at quarter-second intervals; this is not proof of every-frame behavior.

The initial take received one targeted automatic repair, which did not resolve the foreground-bat defect. No further generation or automatic repair was performed. This animation work made no scene changes; neither video was imported into or tested in Unity. The saved scene changed separately during the work and must be rechecked before integration.

Both originals are stored in the existing FAL menu collection and associated with the main-menu scene entity. The screenshot montage [refined-motion-contact.png](refined-motion-contact.png) and [qa.json](qa.json) document the inspection.
