---
von: martin
an: max
status: offen
erstellt: 2026-11-05 14:23
blockiert: nein
---

# Kern-Loop festlegen: Martins Fahrplan zum gemeinsamen Durchgehen (Call)

## Kontext
Martin hat einen groben Fahrplan für den Kern-Loop gemacht. Das hier ist die Vorlage, um ihn **gemeinsam im Call** durchzugehen.
Pro Thema: **Martins Vorschlag** → **offene Details** (zum Ankreuzen) → **Beschluss** (füllt ihr im Call aus).
Nach dem Call überträgt Claude die Beschlüsse in `DESIGN.md`, `DECISIONS.md` und `PAKETE.md` (Tabelle ganz unten).

Stand heute (05.11.): Schalter spielbar (P06/P07), Menü/Pause/Optionen/Audio (P02/P43/P18), **Tagesablauf P09 gebaut**
(Morgen-Brief → Schicht → „Shift over“ → Bett → nächster Tag → Platzhalter-Ende; noch nicht gepusht).
Es fehlen Geld/Moral (P08), Abrechnung (P10), Kiosk/Inventar (P11), Heimschicken (P13), Inhalte (P15), Enden (P12).
Feature-Freeze 22.11., Checkpoint „Trägt der Kern-Loop?“ am **09.11.**

Verwandte offene Frage: [Bulletin P09 vs. P41](2026-11-05_martin-an-max_bulletin-p09-p41.md) – hängt mit Thema 8 zusammen.

---

## 1. Theme „Shift“
**Martins Vorschlag:** Wie zwischen euch besprochen geht es um die **Arbeitsschicht** (ähnlich wie Papers, Please). Zusätzlich, je nach Zeit,
eine **zerfallende Station** oder ein **politischer Umschwung (Shift) auf dem Planeten**.

**Offene Details:**
- Nur „Arbeitsschicht“ wirkt für die Bewertung „Theme Interpretation“ eher wörtlich. Sollte eine Zusatzidee fest eingeplant werden?
  - [ ] A) **Politik-Shift als Muss** – fast nur Inhalt (Memos, Briefe, täglich neue Regeln, Propaganda), Zerfall als Kann
  - [ ] B) **Zerfall als Muss** – P36 (Flackern/Ausfälle) ist schon angelegt, braucht Effekte/Assets; Politik als Kann
  - [ ] C) Beides als Kann, Entscheidung nach dem Checkpoint 09.11.
- Bereits vorhanden und passend: Sonne wandert pro Tag (P24), The Shifter (Identität wechselt täglich).
- Den Abschnitt „Theme“ in `DESIGN.md` gemeinsam ausformulieren (1–2 Sätze, er ist noch leer).

**Beschluss:**

---

## 2. Länge, Tutorial-Tag und Aufgaben nach der Schicht
**Martins Vorschlag:** Das Spiel beginnt (am Wochenende) mit einem **Tutorial-Tag**: nur **3–4 Reisende**, nach der Schicht evtl.
**Aufgaben**, um sich auf der Station umzusehen. Ab dem zweiten Tag normal (ca. 5–7 Reisende).

**Offene Details:**
- Wie viele Tage insgesamt?
  - [ ] A) Tutorial + 2 Tage = 3 Tage (wie bisher geplant, aber nur 2 „echte“ Tage)
  - [ ] B) Tutorial + 3 Tage = 4 Tage (technisch billig: ein Eintrag mehr im `DaySchedule`; kostet Inhalt in P15)
- Reisende pro Tag ab Tag 2: [ ] 5 [ ] 6 [ ] 7 · Ziel-Spielzeit gesamt: ____ Minuten
- Aufgaben nach der Schicht (z. B. „Schau beim Kiosk vorbei“, „Sieh dir den Hangar an“) brauchen ein kleines **Aufgaben-System**
  (Liste oben rechts, Haken bei Erledigung) → **neues Paket** (Vorschlag P44 „Tutorial-Tag & Aufgaben“).
  - [ ] Ja, als Muss [ ] Ja, als Soll [ ] Nein, Hinweise reichen

**Beschluss:**

---

## 3. Zeit- und Gelddruck: Zeitbonus pro Reisendem
**Martins Vorschlag:** Pro Reisendem läuft ein **Zeitbonus-Timer**. Wer ihn **vorher und korrekt** abfertigt, bekommt den Bonus.

**Offene Details:**
- Wann startet die Uhr? [ ] wenn die Papiere in der Schublade liegen (Vorschlag) [ ] bei der Begrüßung (P40)
- Wie lang? [ ] 45 s [ ] 60 s [ ] pro Tag kürzer (z. B. 60 / 50 / 40 s)
- Bonus nur bei richtiger Entscheidung (Vorschlag: ja). Falsch = kein Bonus + Verwarnzettel wie bisher.
- Anzeige: [ ] Uhr/Zeiger auf dem Schaltertisch [ ] Balken im HUD
- Tutorial-Tag ohne Timer? [ ] ja [ ] nein
- → **neues Paket** (Vorschlag P45 „Zeitbonus am Schalter“), hängt an `CounterShift.TravelerArrived`/`DecisionMade`.

**Beschluss:**

---

## 4. Feierabend: Umfang
**Martins Vorschlag:** Erst einmal die **zwei Muss-Sachen**: **Kiosk mit Gadget** (Dokumentenscanner) und **Heimschicken** im Hangar.
Der Rest (Schwarzmarkt/Verkaufen an der Bar, Diner, Barkeeper-Gespräche, Getränkeautomat …) bleibt in der Liste, kommt aber später.

**Offene Details:**
- **P13 Heimschicken** ist bisher **Soll** → wird **Muss**. [ ] einverstanden
- Beides braucht ein **Inventar** (steckt in P11). P11 schrumpft auf „Kiosk + Inventar“, Schwarzmarkt wird eigenes Soll/Kann-Paket.
  [ ] einverstanden
- Das Geschenk des Shifters (Tag 3, „steckt dir etwas für die Familie zu“) landet dann im Inventar und kann heimgeschickt werden.

**Beschluss:**

---

## 5. Geld und Familie
**Martins Vorschlag:** Wer **korrekt arbeitet, verdient genug**, um die Bedarfe zu zahlen, und es bleibt etwas übrig, um sich den
**Scanner** kaufen zu können. Kommt später der Schwarzmarkt dazu (zusätzliches Geld), wird das nochmal überarbeitet.

**Offene Details:**
- Wenn fehlerfreies Arbeiten alles deckt, entsteht Spannung nur durch Fehler, Zeitdruck und **Ereignisse**. Vorschlag: ein Ereignis
  pro Tag über die Briefe, z. B. Tag 2 „Pip ist krank“ → Medizin kostet extra → Scanner jetzt oder Medizin? [ ] ja [ ] nein
- **Beispielrechnung** (nur zum Diskutieren, Werte kommen später in ein ScriptableObject):

  | Posten | Wert |
  |---|---|
  | Startgeld | 40 |
  | Lohn pro richtig abgefertigtem Reisenden | 10 |
  | Zeitbonus | +5 |
  | Abzug ab der 3. Citation am Tag | −5 |
  | Miete Zimmer 103 pro Nacht | 30 |
  | Essen / Wärme pro Nacht | 15 / 10 |
  | Medizin (nur wenn jemand krank ist) | 20 |
  | Dokumentenscanner (Kiosk) | 60 |

  Beispiel Tag 2, 6 Reisende fehlerfrei: 60 Lohn + bis zu 30 Bonus = 60–90 → nach Miete/Essen/Wärme (55) bleiben 5–35.
  Mit Zeitbonus ist der Scanner an Tag 3 drin, ohne Bonus eher nicht. [ ] passt ungefähr [ ] knapper [ ] großzügiger

**Beschluss:**

---

## 6. Niederlagen (Game Over)
**Martins Vorschlag:** Man verliert, wenn man sich die **Miete im Apartment nicht mehr leisten** kann, wegen **zu vieler Citations gefeuert**
wird oder jemanden **zu Unrecht aus der Luftschleuse** wirft. Die **Familie** kann man unterstützen oder vernachlässigen, **ohne Game Over**
(sie färbt Briefe und Ende).

**Offene Details:**
- Miete: [ ] sofort Game Over, wenn sie nicht bezahlt werden kann [ ] erst Mahnung, beim zweiten Mal raus
- Gefeuert: [ ] ab ___ Citations an einem Tag [ ] ab ___ Citations insgesamt (heute: die ersten 2 pro Tag sind Verwarnungen, danach Abzug)
- „Zu Unrecht geschleust“, Vorschlag: Schleuse bei jemandem, der **durchgelassen oder nur abgewiesen** werden müsste.
  Bei „Verhaften“-Fällen zählt die Schleuse weiterhin als richtig, kostet aber Moral (Regel vom 04.11.). [ ] einverstanden
- Game-Over-Bildschirm: kurzer Text je nach Grund + „Try again“ (Tag neu starten?) / „Main menu“. [ ] Tag neu starten [ ] nur Menü

**Beschluss:**

---

## 7. Enden
**Martins Vorschlag:** Mehrere Enden – **planen Max und Martin gemeinsam**.

**Offene Details (Anregung):** Abhängig von Moral (Luftschleuse), Familie (versorgt/vernachlässigt) und der Shifter-Entscheidung an Tag 3?
Wie viele? [ ] 2 [ ] 3 [ ] mehr

**Beschluss:**

---

## 8. Einführung: Memos und Vorgesetzter
**Martins Vorschlag:** Regeln werden **wie in Papers, Please als offizielle Regierungsdokumente (Memos)** vorgestellt. Evtl. kommt als
**erster Reisender der Vorgesetzte mit gefälschten Dokumenten**, um dich zu testen und ein kleines Intro zu geben.

**Offene Details:**
- **Widerspruch zu einem Beschluss:** `DECISIONS.md` (02.11., max): „**Kein sichtbarer Vorgesetzter** (nur Durchsagen/Memos)“.
  - [ ] A) Revidieren: Vorgesetzter erscheint einmal am Tutorial-Tag als Test-Reisender (braucht ein NPC-Modell, P40)
  - [ ] B) Beibehalten: Der Test kommt per Durchsage („Your first traveler is a test. Check everything.“), ein normaler Test-Reisender
- Memos im **eigenen Look** (Kestrel-9-Behörde, Stil Alien: Isolation), nicht zu nah an Papers, Please. [ ] einverstanden
- Memos/Bulletin überschneiden sich mit **P41** (Pinnwand) → siehe verwandte Frage oben. Wer baut die Memo-Dokumente?
  [ ] Max (P41) [ ] Martin (Tutorial-Paket)

**Beschluss:**

---

## 9. Wer baut was, und in welcher Reihenfolge?
Vorschlag als Startpunkt (Max hat P41/P40 reserviert):

| Paket | Inhalt | Vorschlag wer |
|---|---|---|
| P09 | Tagesablauf (gebaut) | martin ✔ |
| P41 / P40 | Pinnwand / Reisende in der Kammer | max |
| P08 | Geld & Moral aus den Entscheidungen | ? |
| P10 | Abrechnung (Miete, Essen, Wärme, Medizin) + Game Over Miete | ? |
| P46 (neu) | Geld, Inventar & Handelsfenster (Grundgerüst, Voraussetzung für P08/P10/P11/P13) | martin (in Arbeit) |
| P11 (neu zugeschnitten) | Kiosk + Scanner kaufen (nutzt P46) | ? |
| P13 (→ Muss) | Heimschicken per Drohne | ? |
| P44 (neu) | Tutorial-Tag & Aufgaben nach der Schicht | martin? |
| P45 (neu) | Zeitbonus am Schalter | ? |
| P15 | Inhalte für alle Tage inkl. Shifter, Ereignisse, Briefe | beide? |
| P12 | Enden + Game-Over-Bildschirme | beide |

Bis zum **Checkpoint 09.11.**: Vorschlag P08 + P10 + P45, damit man einen Tag mit Geld durchspielen kann.

**Nachtrag 05.11. (martin):** Martin baut jetzt **P46 Geld, Inventar & Handelsfenster (Grundgerüst)** – Kontostand, Gegenstände,
Inventar und ein allgemeines Handelsfenster (auch als „Send home“ für die Drohne). **P08, P10, P11 und P13 bauen darauf auf und
werden erst begonnen, wenn P46 fertig ist** (steht so in `PAKETE.md`). Wer P08/P10/P11/P13 übernimmt, bleibt im Call offen.

**Nachtrag 05.11. (martin) – zwei Punkte aus P46 für den Call:**
- **Beschlagnahme braucht echte Gegenstände:** Reisende führen ihre Fracht bisher nur als Text (`TravelerData`, Frachtliste,
  Regel „verbotene Waren“ vergleicht Wörter). Für P08 („Beschlagnahme ins Inventar“) müsste ein Reisender `ItemData` mitführen
  (z. B. Liste `carriedItems`, die zur Frachtliste passt). Das betrifft deinen Code.
  [ ] Max ergänzt `TravelerData` [ ] Martin ergänzt es mit Claim [ ] anders: ____
- **Idee zum Theme „Shift“:** ein Preisfaktor pro Tag in `EconomyTuning` (Preise steigen von Tag zu Tag, passend zum politischen
  Umschwung). Ein Wert pro Tag, große Wirkung. [ ] ja [ ] nein [ ] später

**Beschluss:**

---

## Folgen für die Pakete (füllt Claude nach dem Call aus)
| Paket | Änderung |
|---|---|
| P11 | ? |
| P13 | ? |
| P10 | ? |
| P12 | ? |
| P15 | ? |
| P44 (neu) | ? |
| P45 (neu) | ? |

## Annahme bis zur Antwort
Nichts wird am Kern geändert, bis ihr das gemeinsam durchgegangen seid. Martin macht P09 fertig (Tagesablauf, unabhängig von diesen
Entscheidungen; die Anzahl der Tage ist nur ein Eintrag im `DaySchedule`).

## Antwort
