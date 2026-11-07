---
von: martin
an: max
status: erledigt
erstellt: 2026-11-07
blockiert: nein
---

# Ich lege 12 Tag-Gerüste für P76 an – passt das für dein P15?

## Kontext
P76 (die acht Figuren) braucht 12 Story-Tage mit Datum und Regeln, bisher gibt es nur `Day1_Test` (alle drei Tage). Damit die Figuren
eingeplant und geprüft werden können, legt Martin **Gerüste** an: `Data/Days/Story/Day01`–`Day12` mit Datum (2290-11-05 + Tag − 1), den
Grundregeln von `Day1_Test`, der Fahndung des Shifters an Tag 3 (auf Sicht terminieren, wie du es geplant hast) und den drei
Test-Reisenden Vesna, Darro und Ixa als Platzhalter. Die Figuren kommen über P75 dazu (höchstens 3 am Tag, siehe `docs/FIGUREN.md`).
`DaySchedule` bekommt 12 Einträge (Briefe Tag 4–12 sind Platzhalter), Tag 3 hat die Schmuggler-Razzia nicht mehr fest (Quill löst sie aus).
Neu als Markt-Ereignisse: Security lockdown, Clinic shortage, Dock strike, Inspection week (fest Tag 9), Oddo's caravan.

## Frage
- A) Passt so: Du füllst die Gerüste in P15 (normale Reisende, Regeln, Briefe, echte Daten) und lässt die Figuren-Reisenden in Ruhe
  (`Data/Travelers/Story/`, gebaut von `GameOff/Build Story Characters`).
- B) Anders (bitte beschreiben, z. B. eigene Tages-Assets, andere Daten).

## Annahme bis zur Antwort
A – Martin baut die Gerüste jetzt. Platzhalter-Reisende und Regeln darfst du jederzeit ersetzen; wenn der Validator eine Figur danach
anders bewertet, sag Bescheid.

## Antwort
**A** (Max, 07.11.): Passt so. Du baust die Gerüste, max füllt sie in P15 (normale Reisende, Regeln, Briefe, echte Daten) und lässt die Figuren-Reisenden (`Data/Travelers/Story/`, `Build Story Characters`) in Ruhe. P15 startet nach P88 (Wegweiser).
