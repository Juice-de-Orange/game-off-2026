---
name: sync
description: Team sync at session start or on request - pull latest changes, show open questions addressed to the current person, summarize the other's latest handover and current claims. Use at the start of every work session, or when the user says "was gibt's Neues", "sync", "hol den Stand".
---

# /sync — Team-Stand holen

1. Determine who you work with: `git config user.name` (`Juice-de-Orange` = max, `iMaterix` = martin; see CLAUDE.md).
2. `git status --short`. If there are uncommitted changes, tell the person and ask (multiple choice) whether to
   commit them first or keep them local before pulling.
3. `git pull`. If it fails, explain why in plain German and resolve with the person (never force anything).
4. Open questions **for me**: files in `docs/team/questions/` with `an: <me>` and `status: offen`.
   For each: show title, who asked, whether it blocks them (`blockiert`), and the options.
   Offer to answer now — help the person decide, write the answer into `## Antwort`, set `status: beantwortet`,
   commit + push right away (`Answer question: <slug>`).
5. Answered questions **I asked**: `von: <me>` and `status: beantwortet`. Show the answer, apply it to the work
   if needed, set `status: erledigt`, commit with the next push.
6. Latest handover by the other in `docs/team/handovers/` (newest file name with the other's name):
   summarize in 2–4 lines — what changed, anything unfinished, anything I must watch out for.
   If there are several new handovers since my last one, summarize them all briefly.
7. `docs/team/BOARD.md` → "In Arbeit": list what the other has claimed (don't touch) and what I still hold.
8. End with a suggestion of what to work on next (from my last handover's "Nächste Schritte" and the backlog),
   as a multiple-choice question.

Keep the whole briefing short and in German. Don't paste whole files.
