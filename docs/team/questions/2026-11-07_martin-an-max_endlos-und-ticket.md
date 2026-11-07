---
von: martin
an: max
status: erledigt
erstellt: 2026-11-07 14:00
blockiert: nein
---

# Wird das Spiel endlos, mit einem teuren Ticket nach Hause als Ziel?

## Kontext
Beim Balancing (P71) kam heraus: Nach 3 Tagen hat man mit guter Arbeit 126–182 CR übrig, und Geld hat jenseits der Grundversorgung
keinen Zweck. Martin sagt, ihr habt ein Ticket nach Hause schon besprochen, es ist aber nirgends geplant. Martins Festlegung (07.11.):
- **Endlos statt 3 Tage:** erst die geschriebenen Story-Tage (P15), danach eine Schleife aus Pools (Reisende, Regeln, Marktereignisse,
  Bar-Wünsche), gezogen mit festem Startwert → neues Paket **P73**.
- **Ziel ist ein teures Ticket nach Hause:** abends im Hangar kaufbar, dann ins Schiff und abfliegen (man kann auch absichtlich aus der Map
  fliegen) – „das Ende der Game Jam“ → neues Paket **P72**. Mit guter Arbeit und versorgter Familie ca. 7 Tage.
- **P71** rechnet schon jetzt pro Tag über ~12 Tage und legt den Ticketpreis fest. Jede Geldanpassung braucht danach ein neues Rebalancing.

## Was das für deine Teile bedeutet
- `DESIGN.md` (heute 3 Tage, ~15 Reisende) muss angepasst werden; P15 „Fälle für 3 Tage“ wird zu „Story-Tage + Pools“.
- P12 Enden: das Ticket ist das Hauptende; andere Enden (Familie zerbrochen, gefeuert) bleiben.
- P72 braucht ein Schiff zum Einsteigen und einen Abflug aus dem Hangar (Modelle sind da, `PF_Ship_*`).

## Frage
- A) Ja, so machen wir es. Dein Claude passt `DESIGN.md` an und setzt P72/P73 auf `offen`; den Bau verteilen wir.
- B) Ja, mit Änderungen (bitte beschreiben).
- C) Nein, bleibt bei 3 Tagen.

## Annahme bis zur Antwort
P71 rechnet mit dem endlosen Spiel und dem Ticket als Sparziel; nur Werte und Tests ändern sich, keine Szenen. DESIGN.md bleibt unverändert.

## Antwort
**A** (Max, 07.11.): Ja, so machen wir es. DESIGN.md ist angepasst (Ziel: Ticket nach Hause, Story-Tage + endlose Tage aus Pools), P72 und P73 stehen auf `offen`, Eintrag in DECISIONS.md. Wer was baut, verteilen wir im Call.
