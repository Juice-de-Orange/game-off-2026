# Game Off 2026

Our entry for [GitHub Game Off 2026](https://itch.io/jam/game-off-2026).

- **Theme:** Shift
- **Play the current build:** https://kestrel9.maxlworks.com (work in progress, updated from `main`)
- **Concept:** *Kestrel-9*, a first-person customs inspector on a small, run-down space station — check travelers'
  papers, let them pass, arrest them or open the airlock, and keep your family back home alive.
  See [docs/DESIGN.md](docs/DESIGN.md).
- **Engine:** Unity 6000.3.23f1 (exact version — do not upgrade)
- **Target:** WebGL (playable in the browser on itch.io)
- **Deadline:** December 1st, 2026, 13:37 PST (22:37 CET)

## Getting started

1. Install Unity Hub and Unity **6000.3.23f1** with the **Web Build Support** module.
2. Clone this repo, then open the folder via Unity Hub → *Add project from disk*.

## Team workflow

- `git pull` before you start working, commit and push small steps often.
- Work is split into scoped packages: [docs/team/PAKETE.md](docs/team/PAKETE.md). Take one, push the take, build it.
- Scenes/prefabs/shared assets are claimed before editing (`/claim`), each person tests in their own sandbox scene.
- Assets: rules in [docs/ASSETS.md](docs/ASSETS.md), style in [docs/STYLE.md](docs/STYLE.md),
  requests in [docs/ASSET-REQUESTS.md](docs/ASSET-REQUESTS.md) (delivered via pull requests, approved by Max).
- Builds never go into git: they go to itch.io and to the live version ([docs/DEPLOY.md](docs/DEPLOY.md)).
- Every external asset (art, sound, font, music) goes into [CREDITS.md](CREDITS.md) with its license.
- Coordination happens in [docs/team/](docs/team/BOARD.md) via the Claude Code commands
  `/sync`, `/claim`, `/ask`, `/handover` — rules in [CLAUDE.md](CLAUDE.md).

## Team

- Juice-de-Orange (Max) — art & assets
- iMaterix (Martin) — start screen & menu
- Everything else: open packages, whoever has time

## License

- **Code** (everything under `Assets/_Project/Scripts/`, `tools/`, `.claude/`): [MIT](LICENSE).
- **Our own art, audio, music, texts and voice lines**: © 2026 Juice-de-Orange & iMaterix, all rights reserved.
- **Third-party assets** keep their own licenses, listed in [CREDITS.md](CREDITS.md).

## AI disclosure

AI tools are used in this project (as allowed by the Game Off rules): Claude Code for programming and planning,
an AI-assisted pipeline for 3D models/textures, and ElevenLabs for voice transformation of our own recordings.
