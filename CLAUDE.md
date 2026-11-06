# Project: Game Off 2026 entry

Two-person team building a Unity game for GitHub Game Off 2026 (November 1st – December 1st, 2026).
Each person works with their own Claude Code. The Claudes never talk directly — they coordinate
**only through files in this repo** (`docs/team/`). Keeping those files current is part of every task.

## Team
| Person | GitHub (`git config user.name`) | Short name in files |
|---|---|---|
| Max    | `Juice-de-Orange` | `max`    |
| Martin | `iMaterix`        | `martin` |

Determine who you are working with from `git config user.name`. "The other" is the other person.
Talk to your person in **German**. Team docs (handovers, questions, board) are written in German.
Code, identifiers, comments, commit messages: English.

## Hard constraints
- Unity version is pinned to **6000.3.23f1**. Never upgrade the project or open it with another version.
- Primary build target is **WebGL** for itch.io. Player Settings → Publishing Settings:
  keep **Decompression Fallback** enabled (or Compression Format = Disabled), otherwise the build fails to load on itch.io.
- Repo must be public by the deadline: December 1st, 2026, 13:37 PST (22:37 CET). Team submits November 30th.
- Never commit builds, `Library/`, `Temp/`, `Logs/`, `UserSettings/`.
- Never rewrite shared history: no force-push, no rebase of pushed commits, no `git reset --hard` on shared work.

## Session rhythm (mandatory)

### At session start → `/sync`
A SessionStart hook prints a team briefing (behind/ahead, open questions for you, latest handover
from the other, current board). Act on it:
1. `git pull` (ask first if there are uncommitted local changes).
2. If there are **open questions addressed to your person**: tell them in German and help answer them
   (see "Questions" below). Answering the other is higher priority than new work — they may be blocked.
3. Read the other's latest handover in `docs/team/handovers/` and summarize what changed in 2–4 lines.
4. Check `docs/team/BOARD.md` → "In Arbeit" for files the other has claimed. Don't touch those.
5. Check `docs/PIPELINE.md` for messages from the asset pipeline (`an: team`, `status: offen`) and handle them first.

### Picking work → packages (`docs/team/PAKETE.md`)
All work is split into clearly scoped packages. This is the team's central development concept.
- Take an open package: `git pull`, set its status to `in Arbeit (<name>, <date>)`, commit + push immediately (`Take: Pxx`).
- One package per person at a time. Build only what is in scope; anything else becomes a new package.
- Rework of a finished package = a new follow-up package (`Folgt auf: Pxx`), never reopen.
- Done = acceptance criteria met, no console errors, pushed, status `fertig`, short "Ergebnis" note.
- Mechanics overview: `docs/MECHANICS.md`. When planning a package, check it for conflicts with existing mechanics; when a finished
  package adds or changes a mechanic, update it in the same push (incl. "Lücken / passt noch nicht").
- Need an asset? Order it yourself: add an entry to `docs/ASSET-REQUESTS.md`, run `python tools/check_asset_requests.py`, push to `main` (that is the order, see "Asset bestellen" in `docs/ASSETS.md`), create a placeholder at the final path, keep building. Only large orders or style changes need Max first.
- Max's asset pipeline delivers on `assets/<package>` branches (never pushes to `main`). It can't open pull requests itself (M002); its lead sends Max the link, and the session briefing and `/sync` list branches not yet on `main`. **Only Max approves and merges** deliveries, then integrates them in Unity (steps in `docs/ASSETS.md`).
- Want an asset changed? Both people may request it: add a dated line to the entry's `feedback` and set `status: rework` in `docs/ASSET-REQUESTS.md`, push to `main`. Never edit pipeline-delivered files by hand.

### Before editing a scene, prefab or shared asset → `/claim`
Unity scenes and prefabs cannot be merged reliably. Before modifying any `.unity`, `.prefab`,
ScriptableObject `.asset`, or a script the other is actively working on:
1. `git pull`, check BOARD "In Arbeit" — if the other holds it, stop and ask your person what to do
   (wait, work elsewhere, or `/ask` the other).
2. Otherwise add your claim to BOARD and **commit + push immediately** (commit message `Claim: <files>`),
   so the other's Claude sees it before they start.
3. Release the claim (remove the row) when done, in the same push as the work.

New files you create yourself need no claim.

### When unclear about something the other built → `/ask`
Don't guess about the other's intent when the answer changes what you build. Write a question file
(see below), push it, tell your person, and continue with something unblocked. If a guess is
reasonable and cheap to undo, make it, note it in the question as "Annahme", and continue.

### At session end (or when your person says they stop) → `/handover`
Write a handover file, update BOARD, release claims you no longer need, commit + push.
Never end a session with unpushed work unless your person explicitly says so.
If your person seems to be wrapping up ("ich hör auf", "bis morgen", "gute Nacht"), offer `/handover`.

## Team files (`docs/team/`)
| File | Purpose | Edited by |
|---|---|---|
| `BOARD.md` | Milestones, who works on what right now, claimed files, backlog, done | both — keep edits small, pull first |
| `PAKETE.md` | Development packages (scope, status, who took it) — the central work unit | both — pull first, push a take immediately |
| `../ASSET-REQUESTS.md` | Needed assets: file name, placeholder, final look — processed by Max's asset pipeline | both append; status by Max |
| `../PIPELINE.md` | Messages between team and asset pipeline (broken entries, questions, blockers) | both + pipeline (via PR) |
| `../ASSETS.md` | Asset rules: paths, naming, specs, placeholder → final swap | both, after agreement |
| `handovers/YYYY-MM-DD_HHMM_<name>.md` | One file per session end: state, unfinished work, hints for the other | author only |
| `questions/YYYY-MM-DD_<from>-an-<to>_<slug>.md` | One file per question; the other answers in the same file | both |
| `templates/` | Templates for handovers and questions | rarely |
| `../DESIGN.md` | Game design: theme, core loop, scope (must/should/could) | both, after agreement |
| `../DECISIONS.md` | Append-only log of decisions with reason | both, append only |
| `../DEPLOY.md` | Live version on kestrel9.maxlworks.com: build, nightly update, rollback, secrets | both |
| `../MECHANICS.md` | Overview of all game mechanics: how they connect, built vs. planned, gaps | both, with every package that changes a mechanic |

One file per handover/question means the two people never edit the same file at the same time → no merge conflicts.

### Questions
Frontmatter fields `von`, `an`, `status` (`offen` → `beantwortet` → `erledigt`). Use `templates/question.md`.
- The asker creates it with `status: offen`.
- The other's Claude shows it to their person at session start, writes the answer into `## Antwort`,
  sets `status: beantwortet`, pushes.
- The asker's Claude, on reading the answer, applies it and sets `status: erledigt`.
- If the answer is a real decision (affects design or architecture), also append it to `docs/DECISIONS.md`.

## Git workflow
- Everyone works on `main` (small team, short jam). Pull before starting, before claiming, before pushing.
- Commit small and often. Push at least at every handover.
- On a merge conflict in a `.md` file: resolve it, keeping both sides' content.
  On a conflict in a `.unity`/`.prefab`/`.asset` file: **stop**, explain to your person, don't pick a side alone.

## Unity project structure (created November 1st)
The Unity project lives at the repo root. All our own content goes under `Assets/_Project/`:
```
Assets/
  _Project/
    Scenes/          Boot.unity, Menu.unity, Main.unity (claim before editing!)
    Scenes/Sandbox/  Max_*.unity, Martin_*.unity — personal test scenes, never claimed, never touched by the other
    Scripts/         C# by feature: Core/, Player/, Counter/, Documents/, Travelers/, Day/, Economy/, Shops/, UI/, Audio/ ...
    Prefabs/         Prefabs by feature, same folders as Scripts
    Art/             Models/, Textures/, Materials/, Animations/, UI/  (each with area subfolders: Station, Bar, Hangar ...)
    Audio/           Music/, SFX/, Voice/
    Fonts/           TTF/OTF + TextMeshPro font assets
    Data/            ScriptableObjects (tuning values, travelers, day rules, prices)
  ThirdParty/        imported external assets, unmodified (credit in CREDITS.md)
ArtSource/           .blend/.psd/WAV working files, outside Assets/ (Git LFS)
deploy/              Live version: Dockerfile, nginx, server templates (`docs/DEPLOY.md`)
```
Asset naming, specs and the placeholder → final swap are defined in `docs/ASSETS.md` and `docs/STYLE.md`.
- Build features as **prefabs** in your own sandbox scene, then integrate into `Main.unity` with a claim.
  This keeps work on the shared scene short.
- **Generated content — don't hand-edit:** the station (`PF_Station_Graybox`), the exterior (`PF_Exterior`, `PF_SkyRig`), the
  document prefabs, the counter wiring and the front end (`Boot.unity`, `Menu.unity`, `PF_PauseMenu`) are built by editor tools
  (menu `GameOff/`: Build Station Graybox, Build Sky & Exterior, Build Documents, Build Test Day, Integrate Asset Deliveries,
  Setup Main Lighting + Bake, Build Menu Scenes, Build Traffic into Main, Build Ambience into Main, Build Travelers, Build Sound Set). Change the builder code in `Scripts/Editor/`, run the menu, rebake; manual edits
  in those prefabs are lost on the next run. **Occlusion:** Build Station clears Main's occlusion data, Setup Main Lighting + Bake bakes it again, Build Menu Scenes bakes the menu's; afterwards GameOff/Check Occlusion Bake. The menu shows the station as background: after rebuilding station or exterior,
  run Build Menu Scenes too. After asset deliveries, **GameOff/Rebuild All After Deliveries** runs all of them in order; bake after it.
- **Unity changes files by itself** when opening, playing or building: `ShareTechMono SDF.asset`, `Orbitron SDF.asset`, `ProjectSettings.asset`, `QualitySettings.asset`, `UniversalRenderPipelineGlobalSettings.asset`, `M_Ext_NavGreen.mat`, `M_Ext_NavRed.mat`, `Assets/Resources/`. Restore them before committing unless you changed them on purpose.
- **Counter rules are tested:** after changing travelers, days or rules run `GameOff/Validate Travelers` and the EditMode tests
  (Test Runner, or `unity command test run --mode EditMode`). Every traveler's computed verdict must match its intended one.
- **Performance is measured, not guessed:** `node tools/measure-webgl.mjs <build> <out> [full|quick]` (`K9_CPU=4` for a weak device), `tools/drive-webgl.mjs` plays a build from a script, `GameOff/Audit Main Scene` counts what the scene costs, `GameOff/List Placeholders` lists what is not final art. Numbers from P57 are the baseline.
- Tuning values (speed, damage, timings) go into ScriptableObjects in `Data/`, not hard-coded —
  both people can tweak them without touching scripts or scenes.

## C# conventions
- Namespace `GameOff` (sub-namespaces per feature folder optional, e.g. `GameOff.Player`).
- One public class per file, file name = class name. PascalCase types/methods, camelCase locals.
- Inspector fields: `[SerializeField] private` instead of public fields.
- Use the new Input System (Unity 6 default).
- Avoid `GameObject.Find` / string lookups; wire references in the inspector or via events.
- Keep it simple: jam code, not framework code. No speculative abstractions.
- Every external asset (art, sound, font, music) goes into `CREDITS.md` with source and license.
