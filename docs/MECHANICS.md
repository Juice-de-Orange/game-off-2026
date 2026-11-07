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
| Labels | P64, P13, P70 | gemeinsame Kategorien für alle Systeme | Händler-Regeln, Bedarfe der Familie, später Gerüchte/Wünsche | `Data/Items/Tags/Tag_*` (13 Labels inkl. LUXURY, Liste bei P64) |
| Markt: Label-Faktoren, feste Ereignisse pro Tag (Preis, Menge, Ankauf, Sortiment), Verderb | P68 | Grundpreis × Schwankung × Labels × Ereignisse; Ankündigung als MARKET-Zettel und Durchsage | Händler, Vorrat, Bett, Brief (verdorben), später P65 | `ItemTag.priceFactor`, `Data/Market/Market_*`, `DaySchedule.marketEvents`, `EconomyTuning.perishableTag` |
| Barkeeper: Wunsch nach Label → Gerücht über morgen | P65 | Schild an der Bar, Dialog nach der Schicht, GIVE nimmt die billigste passende Ware, Gerücht = `rumour` der Ereignisse von morgen | Labels, Markt (P68), Inventar (Notiz unter TODAY) | `DayEntry.barWish`, `MarketEvent.rumour` |
| Balancing & Sparziel | P71 | Simulator spielt 12 Tage mit den echten Regeln; Ziele (Ticket nach ~7–8 Tagen guter Arbeit) als Test | alles Geld; Ticket (P72) | `HouseholdTuning.ticketPrice` (350), `docs/ECONOMY-REPORT.md` |
| Preisliste & Marktpreis | P70 | 18 Waren mit Grundpreis, Labels, Tagesmenge und fester Schwankung je Tag; Sortiment Holly/Dealer | Marktpreis (`Market.PriceOf`) → Händler (× Faktor), Bett (× 1,35) | `Data/Items/Item_*` (`dailyChange`), `Stock_*` |
| Händler & Vorrat | P46, P11 (teilweise) | Kaufen/Verkaufen zu Faktoren; Vorrat pro Tag, „Sold out“, morgens aufgefüllt; Verkauftes liegt beim Händler | Inventar ↔ Geld; `PlayerEconomy.Supply` | `Stock_*_Test.asset` (Einträge mit Tagesmenge) |
| Schwarzmarkt-Regel | P64 | nur der Schwarzmarkt kauft CONTRABAND/CONFISCATED, und nichts anderes | Label → `VendorStock.Buys` | `refusedTags`, `onlyTags` |
| Handel nur nach der Schicht | Grundregel (06.11.) | Händler, Drohne und jede Handelsoption nur abends | Phase Evening | Dev-Knöpfe `Dev_force_Trade` / `Dev_force_Send` |
| Frachtdrohne | P13 (Abnahme bei martin) | Gegenstand nach Hause schicken; FOOD/POWER/MEDICINE deckt den Bedarf heute Nacht | Inventar → `PlayerEconomy.Delivery` → Abrechnung | Bedarf → Label in `Household.asset` |
| Abrechnung | P10, P13, P70 | Lohn pro **richtig** entschiedenem Reisendem, Abzug für Verwarnungen über den freien, Miete, Bedarfe selbst anklicken (nichts vorausgewählt), RETURN zurück; **Miete nicht bezahlbar → Game Over „STRANDED“** (Platzhalter) | Reisende + Verwarnungen + Lieferungen → Wallet + Familie | `Data/Economy/Household.asset`: Lohn 15, Abzug 10, Miete 30, Geld am Bett = Händlerwert × 1,35 |
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
| Balancing über 3 Tage (Economy Report) | P71 (geplant) | alles Geld | nach P68 |
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
- **Jede Geldänderung braucht ein neues Rebalancing** (GameOff → Economy Report bis alle Ziele ✓; `EconomyBalanceTests` bricht sonst).
- **Vernachlässigen lohnt sich:** Ohne Folgen von „critical“ (P69) und ohne Familienprüfung beim Ticket (P72) ist „nichts versorgen“ der schnellste Weg nach Hause.
- **Endlos noch nicht gebaut:** Der Simulator wiederholt die Story-Tage ab Tag 4, bis P73 Pools liefert; das Spiel selbst endet noch nach Tag 3.
- **Waren ohne Zweck:** Pistol, Data chip, Antibiotics (beim Dealer teurer als Medizin am Bett) und Wertsachen (keine Quelle) – Ziel für P65-Wünsche und P08. Scanner erst mit P16.
