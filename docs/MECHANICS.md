# Mechaniken – Übersicht

Wofür: Auf einen Blick sehen, welche Spielmechaniken es gibt, wie sie ineinandergreifen und wo noch etwas nicht zusammenpasst.
Pflege: Wer ein Paket fertigstellt, das eine Mechanik neu baut oder ändert, zieht diese Datei im selben Push nach (siehe `CLAUDE.md`).
Wer ein Paket plant, prüft hier zuerst auf Konflikte. Details stehen in `PAKETE.md`, Entscheidungen in `DECISIONS.md`.

Stand: 06.11.2026 (martin)

## Der Kreislauf eines Tages
```
Morgen: Brief der Familie (+ Zustand je Person) · Bulletin/Regeln an der Pinnwand
  ↓
Schicht am Schalter: Reisende prüfen → Stempel/Schublade, Verhaften, Schleuse
  ↓            ↘ Fehler → Verwarnzettel (2 pro Tag frei, danach Abzug)
Feierabend (nur jetzt Handel): Kiosk · Schwarzmarkt · Drohne nach Hause
  ↓
Bett → Abrechnung: Lohn − Abzug − Miete − Bedarfe (oder per Drohne gedeckt) → Familie ändert sich
  ↓
nächster Morgen (Brief zeigt die Folgen) … nach Tag 3: Ende (Platzhalter)
```

## Gebaut

| Mechanik | Paket | Was sie tut | Eingänge → Ausgänge | Daten / Einstellwerte |
|---|---|---|---|---|
| Tagesablauf | P09 (Abnahme bei max) | Morgen → Schicht → Abend → Nacht → nächster Tag, 3 Tage | `CounterShift.ShiftEnded` → Abend; Bett → `NightStep` (Abrechnung) → nächster Tag; `LetterAddendum` → Brief | `Data/Days/DaySchedule.asset` |
| Schalter & Dokumente | P05, P06, P38, P40 | Papiere ziehen, lesen, stempeln, Schublade, Nachfragen per Klick auf ein Feld, Reisende in der Prüfkammer | Reisender → Entscheidung (`DecisionMade`) | `Data/Travelers/…`, Dokument-Prefabs |
| Regeln & Urteil | P07, P41 | Regeln des Tages, Pinnwand, Durchsagen; jede Entscheidung wird geprüft | Entscheidung → `Judgement` (richtig/falsch, Moral-Kosten) → bei Fehler Verwarnzettel | `DayRules`, `freeWarnings` (2) in `CounterShift` |
| Geld & Inventar | P46 | Kontostand mit Tagesbuchungen (TODAY), Inventar (I), Gegenstände mit Labels | alle Käufe/Verkäufe/Abrechnung → `Wallet` | `EconomyTuning` (Start 40 CR), `Data/Items/*` |
| Labels | P64, P13 | gemeinsame Kategorien für alle Systeme | Händler-Regeln, Bedarfe der Familie, später Gerüchte/Wünsche | `Data/Items/Tags/Tag_*` (12 Labels, Liste bei P64) |
| Händler & Vorrat | P46, P11 (teilweise) | Kaufen/Verkaufen zu Faktoren; Vorrat pro Tag, „Sold out“, morgens aufgefüllt; Verkauftes liegt beim Händler | Inventar ↔ Geld; `PlayerEconomy.Supply` | `Stock_*_Test.asset` (Einträge mit Tagesmenge) |
| Schwarzmarkt-Regel | P64 | nur der Schwarzmarkt kauft CONTRABAND/CONFISCATED, und nichts anderes | Label → `VendorStock.Buys` | `refusedTags`, `onlyTags` |
| Handel nur nach der Schicht | Grundregel (06.11.) | Händler, Drohne und jede Handelsoption nur abends | Phase Evening | Dev-Knöpfe `Dev_force_Trade` / `Dev_force_Send` |
| Frachtdrohne | P13 (Abnahme bei martin) | Gegenstand nach Hause schicken; FOOD/POWER/MEDICINE deckt den Bedarf heute Nacht | Inventar → `PlayerEconomy.Delivery` → Abrechnung | Bedarf → Label in `Household.asset` |
| Abrechnung | P10, P13 | Lohn pro Reisendem, Abzug für Verwarnungen über den freien, Miete, Bedarfe selbst anklicken (nichts vorausgewählt), RETURN zurück; **Miete nicht bezahlbar → Game Over „STRANDED“** (Platzhalter) | Reisende + Verwarnungen + Lieferungen → Wallet + Familie | `Data/Economy/Household.asset`: Lohn 15, Abzug 10, Miete 30, Geld am Bett = Händlerwert × 1,35 |
| Familie | P10 | Mom, Lena, Pip; Stufen well → weak → sick → critical; ohne Essen alle schlechter, ohne Wärme Pip, Medizin heilt | Abrechnung → `Household` → Zeilen im Morgenbrief | `Household.asset` (Namen, Texte je Stufe) |
| Verkehr & Atmo | P47, P55, P67 | Schiffe vor den Fenstern, Stations-Loop | – (Stimmung) | `ShipTraffic_*.asset`, `SFX_Amb_Station_Loop` |
| Dev-Werkzeuge | P09, P56, P10 | Dev_end_Day, Hitboxen, TRAFFIC_LINE_DEV, CREDITS ±100 | – | **vor der Abgabe ausschalten** (DEV-Reiter weg) |

## Geplant (Paket offen oder Entwurf)

| Mechanik | Paket | Hängt an | Stand |
|---|---|---|---|
| Moral, Lohn je Entscheidung, Beschlagnahme | P08 | Schalter, Abrechnung | wartet auf Gespräch: Schicht-Inventar + Wahl am Schichtende (Frage an max) |
| Schicht-Inventar, „Nothing to report / missed that / as ordered“ | Vorschlag (Frage `schichtinventar-beschlagnahme`) | P08, Labels CONFISCATED | mit max besprechen |
| Disziplinarstrafen (3 = Game Over), getrennt von Verwarnungen | Vorschlag (gleiche Frage) | P08, P12 | mit max besprechen |
| Beschlagnahmte Ware: eigene Items oder Merkmal pro Stapel | offen (P64/P08) | Inventar, Schwarzmarkt | mit max besprechen |
| Familienschicksal: Folgen von „kritisch“ | P69 (Entwurf) | Familie → P12 | mit max abstimmen |
| Enden (Familie, Moral, gefeuert) | P12 | Familie, P08, P69 | offen |
| Briefe reagieren auf den Zustand | P15 | Familie | offen |
| Marktverfügbarkeit (Politik, Tagesereignisse → Menge/Preis) | P68 (Entwurf) | Händler, Abrechnung (× 1,35) | erst planen |
| Barkeeper: Wunschliste → Gerüchte | P65 (Entwurf) | Labels, P68 | Frage an max |
| Barkeeper: Bestechungszettel | P66 (Entwurf) | P08, Abrechnung | Frage an max |
| Gadgets: Scanner, UV-Lampe | P16, P62 | Schalter, Kiosk | Scanner-Funktion unklar (siehe Lücken) |
| Getränkeautomat | P30 | Geld, Inventar | offen, nur nach der Schicht |
| Familie in der Welt (Stadtlichter, Foto) | P29 | Familie | Assets bestellt |
| Bugreport im Spiel | Frage `bugreport-server` | Live-Server | Frage an max |

## Lücken / passt noch nicht
- **Brief von Tag 3** sagt fest „Pip is sick now“, auch wenn Pip gesund ist → P15.
- **Lohn ohne Moral:** Lohn zählt nur Reisende; Moral, Bestechung, Beschlagnahme fehlen → P08, P66.
- **P11 nur halb:** Schwarzmarkt-Einkauf und Beschlagnahmtes verkaufen warten auf P08.
- **Scanner (P16):** In den Daten gibt es nur „gefälschtes Siegel“, und das deckt laut P58 die UV-Lampe auf. Was der Scanner zeigt, ist offen.
- **Schmuggel nach Hause:** geht ohne Wirkung und ohne Risiko → später mit P08.
- **Niederlage:** Bisher nur „Miete nicht bezahlbar“ (Platzhalter). Familie kann dauerhaft „critical“ sein, ohne Folge → P69, P12.
- **Preise sind Platzhalter** (35-%-Regel) bis P68.
