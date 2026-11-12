# Credits

**Team:** Martin (iMaterix), Max (Juice-de-Orange). **Lead Playtester:** Pep (SuperGoat).
The in-game credits come from `MenuSceneBuilder` (team) and from the table below (assets).

Every third-party asset used in the game, with source and license.
Only use assets whose license allows use in a public, open-source game jam entry
(CC0 is easiest; CC-BY requires attribution here).

| Asset | Type | Author | Source (URL) | License |
|-------|------|--------|--------------|---------|
| Liberation Sans (fallback for missing characters) | Font | Red Hat / Liberation Fonts project, shipped with Unity TextMeshPro | https://github.com/liberationfonts/liberation-fonts | SIL OFL 1.1 (license file next to the font: `Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt`) |
| Share Tech Mono (terminals, notes, help) | Font | Carrois Type Design, Ralph du Carrois | https://fonts.google.com/specimen/Share+Tech+Mono | SIL OFL 1.1 (license file next to the font: `Assets/_Project/Fonts/ShareTechMono-OFL.txt`) |
| Orbitron (titles, menus, documents) | Font | Matt McInerney / The Orbitron Project Authors | https://fonts.google.com/specimen/Orbitron | SIL OFL 1.1 (license file next to the font: `Assets/_Project/Fonts/Orbitron-OFL.txt`) |

Checked for the release (P146, 12.11.): these three fonts are the only third-party files in the build.
- **Music and sound effects** (`Assets/_Project/Audio/`, every file as of 12.11.): synthesised by our asset pipeline with its own code, no
  samples and no recordings (see the `assets(...)` delivery commits). Our own work, no row needed. The Freesound loop from P67
  (Jovica, CC BY 4.0) was replaced on 06.11. and is no longer in the project.
- **Models, textures, UI graphics, animations** (`Assets/_Project/Art/`): made by our AI-assisted asset pipeline, no third-party files.
  `Assets/ThirdParty/` is empty. The TextMeshPro emoji sprites were removed in P57.
- **Voice:** no voice recordings in the game.
- **Engine and packages:** Unity 6 and Unity's own packages (URP, TextMeshPro/uGUI, Input System, AI Navigation, Cinemachine,
  ProBuilder, Timeline) under Unity's licenses; the in-game credits end with "Made with Unity".

A new third-party asset gets a row in the table, then `GameOff/Build Menu Scenes` (or Rebuild All) puts it into the in-game credits.

## Useful sources

- https://kenney.nl/assets — art, UI, sounds (CC0)
- https://opengameart.org — art, music (check per asset)
- https://freesound.org — sound effects (check per sound: CC0 / CC-BY)
- https://sfxr.me — generate retro sound effects (yours, no license issues)
- https://fonts.google.com — fonts (OFL)
