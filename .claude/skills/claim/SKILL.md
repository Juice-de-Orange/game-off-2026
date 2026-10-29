---
name: claim
description: Claim (lock) a Unity scene, prefab, ScriptableObject or shared script before editing it, so the teammate doesn't edit it at the same time; also releases claims. Use before modifying any .unity/.prefab/.asset or a script the other works on, or when the user says "claim", "sperr", "ich arbeite an", "freigeben".
---

# /claim — Datei reservieren oder freigeben

Arguments: the files/assets to claim, or `release <files>` / `release all`.

## Claim
1. `git pull` (must be up to date — a stale board is worse than none).
2. Read `docs/team/BOARD.md` → "In Arbeit".
   - If **the other** holds any of the requested files: stop. Tell the person who holds it, since when,
     and offer options (multiple choice): work on something else / `/ask` the other to release it / wait.
     Never claim over the other's claim.
3. Add or extend my row: `| <me> | <short task> | <file paths> | <DD.MM. HH:MM> |`.
4. Commit only `docs/team/BOARD.md` with message `Claim: <files>` and **push immediately**.
   If the push is rejected: pull, re-check step 2, push again.
5. Confirm to the person in one line.

## Release
1. Remove the files from my row (remove the row if empty). Do this in the same commit as the finished work
   where possible, otherwise as its own commit `Release: <files>`.
2. Push.

Claims are cheap — prefer claiming too much over a silent conflict. But release as soon as you're done:
an unreleased claim blocks the other.
