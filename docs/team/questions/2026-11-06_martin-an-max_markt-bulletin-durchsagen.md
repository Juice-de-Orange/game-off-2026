---
von: martin
an: max
status: offen
erstellt: 2026-11-06 17:30
blockiert: nein
---

# Darf das Marktereignis des Tages in dein Bulletin und deine Durchsagen?

## Kontext
Martin plant das Wirtschaftssystem (P68 Markt, P70 Preisliste, P71 Balancing, Details in `PAKETE.md` und `DECISIONS.md`).
Jeder Tag kann feste Marktereignisse haben (z. B. „Border closed: food is scarce“). Sie verändern Preis, Menge, Sortiment und Ankauf
je Label. Vorher erfährt man sie nur vom Barkeeper (P65), **am Ereignistag sollen sie im Bulletin an der Pinnwand und in einer Durchsage
stehen**. Bulletin (`Travelers/DayBulletin`, `Counter/PinBoard`) und Durchsagen (`Counter/Announcer`, `Announcements`) sind aus deinem P41.

## Frage
- A) Martin baut einen kleinen Haken mit Claim: `DayBulletin` hängt die Zeilen der Ereignisse des Tages an, `Announcer` spielt ihre
  Durchsage einmal pro Tag. Texte kommen aus `MarketEvent` (Bulletin-Zeile, Durchsage-Text).
- B) Max baut den Haken selbst (Martin liefert nur `Market.TodayEvents`).
- C) Anders (bitte beschreiben).

## Annahme bis zur Antwort
Noch nichts wird gebaut, P68 steht erst als Plan. Wenn P68 drankommt und noch keine Antwort da ist, nimmt Martin A an.

## Antwort
