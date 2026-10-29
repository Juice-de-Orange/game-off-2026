---
name: handover
description: End-of-session handover - write a handover file for the teammate, update the board, release claims, commit and push everything. Use when the user ends a session ("ich hör auf", "Feierabend", "bis morgen", "handover", "übergabe") or before a long break.
---

# /handover — Übergabe schreiben und alles pushen

1. Determine me (see CLAUDE.md). Get the current time (`date "+%Y-%m-%d %H:%M"` or PowerShell `Get-Date -Format "yyyy-MM-dd HH:mm"`).
2. Collect what happened this session: `git log --oneline` since my previous handover, `git status`, `git diff --stat`,
   and the conversation. Ask the person (multiple choice where possible) only for what you cannot infer —
   e.g. whether unfinished work should stay claimed.
3. If Unity scenes/prefabs are modified: remind the person to **save in Unity** (Ctrl+S) before committing,
   then re-check `git status`.
4. Commit outstanding work in sensible commits (English messages). Never commit `Library/`, `Temp/`, builds.
   If work is half-done and would break the game for the other (compile errors!), ask the person:
   finish/stub it, or keep it uncommitted locally. **Never push code that doesn't compile.**
5. Write `docs/team/handovers/<YYYY-MM-DD>_<HHMM>_<me>.md` from `docs/team/templates/handover.md`. In German, concise.
   Focus on what the other needs: changed interfaces, renamed/moved files, new tuning values, known bugs,
   and anything unfinished.
6. Update `docs/team/BOARD.md`: release claims I no longer need, move finished tasks to "Erledigt",
   update milestone status if one was reached.
7. Append real decisions made this session to `docs/DECISIONS.md`.
8. `git pull`, then commit (`Handover <me> <date>`) and push. Verify with `git status` that nothing is left
   and the branch is not ahead of origin.
9. Tell the person in 2–3 lines what the other will see, and that everything is pushed.
