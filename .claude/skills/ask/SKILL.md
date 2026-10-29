---
name: ask
description: Ask the teammate a question asynchronously via a question file in docs/team/questions/ - use when something the other built is unclear, an interface/behaviour is ambiguous, or a design decision needs both people. Also when the user says "frag Martin", "frag Max", "ask".
---

# /ask — Frage an den anderen stellen

1. Determine me and the other (see CLAUDE.md, `git config user.name`).
2. Before asking, check whether the answer is already findable: the other's handovers, `docs/DESIGN.md`,
   `docs/DECISIONS.md`, code comments, git log of the relevant files (`git log -p -- <file>`).
   If yes, use that and tell the person instead of asking.
3. Formulate the question together with the person. Good questions:
   - name the concrete files/classes/prefabs,
   - offer answer options (A/B/C) so the other can answer in one line,
   - state an "Annahme bis zur Antwort" so work can continue.
4. Create `docs/team/questions/<YYYY-MM-DD>_<me>-an-<other>_<short-slug>.md` from `docs/team/templates/question.md`
   with `status: offen` and `blockiert: ja|nein`.
5. `git pull`, commit (`Question to <other>: <slug>`), push immediately.
6. Tell the person: the question is pushed; the other's Claude will show it at their next session start.
   If it's urgent/blocking, suggest they also ping the other in chat.
7. Continue with unblocked work (or work under the stated assumption).
