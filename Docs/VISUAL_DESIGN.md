# Coral Club visual refresh

The integrated game uses a miniature coral lagoon inside a mint enamel water toy. Warm coral and mango controls, champagne trim, rounded cabinet corners, glass reflections, drifting bubbles and a dark teal atmospheric backdrop establish the palette. The home screen displays a smaller collectible toy with decorative floating rings; gameplay rings share their orientation with hollow physical colliders. Peg positions and controls follow the authored campaign.

## Art and typography

- `Assets/_Game/Art/Resources/CoralLagoon.png`: original generated aquarium artwork, created with the built-in ImageGen tool. The illustration is sampled by `LagoonWater.shader`, with subtle UV refraction and animated caustics. Keep the central water clear enough to read gameplay silhouettes.
- `Assets/_Game/Art/Resources/OceanBackdrop.shader`: procedural gradient, slow wave ribbons and restrained grain. No external render pipeline or post-processing package is required.
- Lilita One headings and Lato interface text are bundled from the Google Fonts repository. Their SIL Open Font License files are in `Assets/_Game/Art/Fonts`.
- All runtime-loaded artwork, fonts and the backdrop shader live in Resources so they are included in standalone builds. Runtime-created meshes and materials are disposed by the presentation.

Reduced motion freezes refraction, background waves and ambient bubble drift, and keeps the existing reduced-effects behavior. Showcase rings are decorative and appear only on Home.

## Artwork generation prompt

Use case: stylized-concept. Asset type: production background texture inside a water ring toss game's glass aquarium. Create a stunning polished miniature underwater coral lagoon illustration, portrait 4:5 composition. Luminous clear turquoise water at top, deep teal blue at bottom, beautiful sunbeams entering from upper left, softly rippling caustic light. A charming sculptural coral garden hugs ONLY the bottom 18 percent and outer left/right 12 percent: peach and pink branching coral, rounded apricot sea anemones, lavender fan coral, seafoam seaweed, tiny smooth cream pebbles and a little golden starfish bottom right, layered with subtle atmospheric depth. The central 70 percent is OPEN QUIET WATER with restrained detail and smooth blue teal gradients, for brightly coloured interactive rings and pegs to be rendered in front. Art direction: premium cozy casual game, tactile clay-like 3D illustration, beautiful rounded organic forms, clean silhouettes, soft subsurface scattering, inviting tropical light, exquisite color harmony. Straight-on front view, no perspective frame, no aquarium borders, no UI, no text, no letters, no rings, no pegs, no fish, no logos, no watermark. Full bleed rectangular art.

## Visual checks

`Tools/Run-Checks.ps1` runs the existing EditMode and PlayMode suites. The PlayMode journey captures Home, gameplay, all menus and a 360 × 800 tall portrait to `Logs/Previews`. Review these images after presentation changes. The Windows build can also run the isolated `-pocketToysAudit` documented in the root README.
