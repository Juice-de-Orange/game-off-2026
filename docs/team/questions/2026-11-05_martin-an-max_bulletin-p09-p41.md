---
von: martin
an: max
status: beantwortet
erstellt: 2026-11-05 13:49
blockiert: nein
---

# Wer zeigt die Regeln des Tages wo an – P09 (Morgen) oder P41 (Pinnwand)?

## Kontext
Martin baut **P09 Tagesablauf** (`DayManager`: Morgen im Zimmer 103 → Schicht → Feierabend → Bett → nächster Tag, 3 Tage).
In `PAKETE.md` steht bei P09 „Morgen-Brief + Bulletin“, bei deinem **P41** „Tagesbulletin + Fahndungsliste als Zettel an der
Pinnwand“ plus Durchsage zu Schichtbeginn. Beide Pakete nennen also das Tagesbulletin.

P09 nutzt nur deine Schnittstellen, ohne deinen Code zu ändern: `CounterShift.BeginDay(DayRules)` jeden Morgen,
`CounterShift.ShiftEnded` für den Feierabend, `SkyRig.SetDay(n)`. Neue Ereignisse für dich und die Folgepakete:
`DayManager.DayStarted(int)`, `DayManager.DayEnded(int)`.

## Frage
Wie teilen wir das Bulletin auf?
- A) **(Martins Vorschlag)** P09 zeigt morgens im Zimmer nur den **Brief der Familie** und eine Zeile „New today: …“
  (die neue Regel des Tages). Das vollständige Bulletin mit allen Regeln und die Fahndungsliste bleiben bei dir in P41 an der Pinnwand.
- B) P09 zeigt morgens das komplette Bulletin, P41 nur die Pinnwand im Schalterraum.
- C) Anders (bitte kurz beschreiben).

Nebenbei: Martin nimmt an, dass deine Schichtbeginn-Durchsage (P41) kommt, wenn man sich nach `BeginDay` an den Schalter setzt
(`CounterShift` startet dann den ersten Reisenden). Für P09 ändert sich dadurch nichts – nur falls du es anders geplant hast.

## Annahme bis zur Antwort
Variante A. Der Text „New today: …“ steht pro Tag im neuen `DaySchedule` (`Data/Days/`), lässt sich also leicht ändern oder weglassen.

## Antwort

**Max, 05.11.: Variante A.** P09 zeigt morgens nur den Brief der Familie und die Zeile „New today: …“. Das vollständige Bulletin
und die Fahndungsliste baut Max in P41 an die Pinnwand.

Deine Annahme zur Durchsage stimmt: Die Schichtbeginn-Durchsage kommt, wenn man sich nach `BeginDay` an den Schalter setzt.
Für P09 ändert sich nichts. Max nimmt P41 jetzt, danach P40 – damit kannst du P09 anschließend abnehmen.
