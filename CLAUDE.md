# Project: Game Off 2026 entry

Two-person team building a Unity game for GitHub Game Off 2026 (November 1st – December 1st, 2026).

## Hard constraints
- Unity version is pinned to **6000.3.23f1**. Never upgrade the project or open it with another version.
- Primary build target is **WebGL** for itch.io. Player Settings → Publishing Settings:
  keep **Decompression Fallback** enabled (or Compression Format = Disabled), otherwise the build fails to load on itch.io.
- Repo must be public by the deadline: December 1st, 2026, 13:37 PST (22:37 CET). Aim to submit November 30th.
- Never commit builds, `Library/`, `Temp/`, `Logs/`, `UserSettings/`.

## Collaboration rules
- Two people push to `main`. Always `git pull` before starting and before pushing.
- Scenes (`.unity`) and prefabs are hard to merge: before editing a shared scene/prefab, make sure the
  teammate is not editing it at the same time. Prefer working in own scenes/prefabs.
- Commit small and often, with English commit messages.
- Every third-party asset goes into `CREDITS.md` with source and license.

## Conventions
- Code, identifiers, comments, commit messages: English.
- Scripts in `Assets/Scripts/`, scenes in `Assets/Scenes/`, prefabs in `Assets/Prefabs/`, art in `Assets/Art/`, audio in `Assets/Audio/`.
- Explanations to the team: German.
