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
| Tagesablauf | P09 (abgenommen, 07.11.) | Morgen → Schicht → Abend → Nacht → nächster Tag, 3 Tage | `CounterShift.ShiftEnded` → Abend; Bett → `NightStep` (Abrechnung) → nächster Tag; `LetterAddendum` → Brief | `Data/Days/DaySchedule.asset` |
| Schalter & Dokumente | P05, P06, P38, P40 | Papiere ziehen, lesen, stempeln, Schublade, Nachfragen per Klick auf ein Feld, Reisende in der Prüfkammer | Reisender → Entscheidung (`DecisionMade`) | `Data/Travelers/…`, Dokument-Prefabs |
| Regeln & Urteil | P07, P41 | Regeln des Tages, Pinnwand, Durchsagen; jede Entscheidung wird geprüft | Entscheidung → `Judgement` (richtig/falsch, Moral-Kosten) → bei Fehler Verwarnzettel | `DayRules`, `freeWarnings` (2) in `CounterShift` |
| Geld & Inventar | P46 | Kontostand mit Tagesbuchungen (TODAY), Inventar (I), Gegenstände mit Labels | alle Käufe/Verkäufe/Abrechnung → `Wallet` | `EconomyTuning` (Start 40 CR), `Data/Items/*` |
| Warenliste 2: Icon, Modell, Fracht-Stichwort je Ware | P82 | 24 Waren (neu: Liquor, Weapon parts, Cash bundle, Alien specimen, UV lamp; Stim injector heißt jetzt Stimulant vials). Waren wirken nicht stärker als andere, sie sind **eine andere Quelle** (z. B. beschlagnahmte Medizin statt Laden). Jede Ware mit Fracht-Stichwort (`cargoWord`) hat eine **CONFISCATED-Variante** (`Data/Items/Confiscated/`, gleiche Labels + CONFISCATED → hilft der Familie, nur der Schwarzmarkt kauft sie) | Fracht der Reisenden (P15) → Beschlagnahme (P08); Familie, Händler | `Data/Items/*`, gebaut von `Update Economy Data` |
| Beschlagnahme, Wahl am Schichtende, Disziplinarstrafen, Moral | P08 | Verhaften → Frachtbrief → Schicht-Inventar (max. 3 je Ware). Schichtende: „Nothing to report“ (80 % erwischt: Ware weg, 100 CR, 1 Disziplinarstrafe) / „Oh, seems I missed that“ (60 % ein Stück, sonst 30 CR) / „As ordered“. 3 Disziplinarstrafen = DISCHARGED. Moral: Schleuse −1, bei Unschuldigen −3 (nur für P12) | Fracht-Stichworte (P82), Schicht-Inventar (P81), Inventar (CONFISCATED-Varianten), Wallet (Strafen in TODAY), Enden (P12) | `Data/Economy/ConfiscationTuning.asset` |
| Wiederkehrende Figuren: Auftritte, Gedächtnis, Folgen | P75; Inhalte P76; Verwahrung P77 | Figuren kommen geskriptet in die Schlange (fester Tag/Platz oder Zeitfenster, Bedingung über den letzten Ausgang), grüßen je nach letztem Ausgang, reden über Markt-Ereignisse. Jede Entscheidung = Ausgang je Auftritt (geholfen/verraten/neutral/beschlagnahmt/tot) → Folgen: Markt-Ereignis in n Tagen, Fahndung. Schleuse = nie wieder | Schalter (`CounterShift.PlanDay`), Markt (`Market.Schedule` → Pinnwand, Durchsage, Gerücht, Lage der Station), Fahndungsliste (RuleEvaluator) | `Data/Characters/` (Test: Tess Marlo, nur mit Dev-Tools) |
| Schicht-Inventar | P81 | Am Schalter öffnet [I] „SHIFT · CONFISCATED“ statt des eigenen Inventars (Schalter ruht solange); am Schichtende geht es an P08 (`ShiftClosing`) und wird geleert | Beschlagnahme (P08) → `PlayerEconomy.ShiftItems` | Dev: `Dev_fill_Shift` (Dev-Startwaren) |
| Labels | P64, P13, P70 | gemeinsame Kategorien für alle Systeme | Händler-Regeln, Bedarfe der Familie, später Gerüchte/Wünsche | `Data/Items/Tags/Tag_*` (13 Labels inkl. LUXURY, Liste bei P64) |
| Markt: Label-Faktoren, feste Ereignisse pro Tag (Preis, Menge, Ankauf, Sortiment), Verderb | P68 | Grundpreis × Schwankung × Labels × Ereignisse; Ankündigung als MARKET-Zettel und Durchsage | Händler, Vorrat, Bett, Brief (verdorben), später P65 | `ItemTag.priceFactor`, `Data/Market/Market_*`, `DaySchedule.marketEvents`, `EconomyTuning.perishableTag` |
| Barkeeper: Wunsch nach Label → Gerücht über morgen | P65 | Schild an der Bar, Dialog nach der Schicht, GIVE nimmt die billigste passende Ware, Gerücht = `rumour` der Ereignisse von morgen | Labels, Markt (P68), Inventar (Notiz unter TODAY) | `DayEntry.barWish`, `MarketEvent.rumour` |
| Balancing & Sparziel | P71 | Simulator spielt 12 Tage mit den echten Regeln; Ziele (Ticket nach ~7–8 Tagen guter Arbeit) als Test | alles Geld; Ticket (P72) | `HouseholdTuning.ticketPrice` (350), `docs/ECONOMY-REPORT.md` |
| Bestechungszettel | P66 | Zettel auf dem Tisch an Tagen mit Bestechung; Durchlassen → Umschlag in der Abrechnung, Ablehnen → kein Gerücht heute und morgen | Schalter-Entscheidung, Abrechnung, Barkeeper (P65) | `DayEntry.bribe` |
| Preisliste & Marktpreis | P70 | 18 Waren mit Grundpreis, Labels, Tagesmenge und fester Schwankung je Tag; Sortiment Holly/Dealer | Marktpreis (`Market.PriceOf`) → Händler (× Faktor), Bett (× 1,35) | `Data/Items/Item_*` (`dailyChange`), `Stock_*` |
| Händler & Vorrat | P46, P11 (teilweise) | Kaufen/Verkaufen zu Faktoren; Vorrat pro Tag, „Sold out“, morgens aufgefüllt; Verkauftes liegt beim Händler | Inventar ↔ Geld; `PlayerEconomy.Supply` | `Stock_*_Test.asset` (Einträge mit Tagesmenge) |
| Schwarzmarkt-Regel | P64 | nur der Schwarzmarkt kauft CONTRABAND/CONFISCATED, und nichts anderes | Label → `VendorStock.Buys` | `refusedTags`, `onlyTags` |
| Handel nur nach der Schicht | Grundregel (06.11.) | Händler, Drohne und jede Handelsoption nur abends | Phase Evening | Dev-Knöpfe `Dev_force_Trade` / `Dev_force_Send` |
| Getränkeautomat | P30 | [E] kauft sofort eine Nova Cola zum Marktpreis, 6 pro Tag, nur abends (tagsüber „Out of order“); noch keine Trink-Wirkung | Geld → Inventar (LUXURY) | `Data/Economy/Stock_Vending.asset` |
| Anzeigesäule in der Halle | P31 | linker CRT: Tag, Phase, nächstes Shuttle (fiktive Uhrzeit, rückt mit jeder Entscheidung) und Dock; rechter CRT: Marktereignisse des Tages | DayManager, CounterShift, `Market.TodayEvents` | `HallBoard` (erstes Shuttle 08:00, alle 25 min) |
| Lage der Station: Flackern, Neon-Aussetzer, Stromausfälle | P36 | Krisen-Ereignisse heben die Lage, sie halbiert sich an ruhigen Tagen; je schlimmer, desto öfter flackert und fällt es aus | Markt-Ereignisse (`crisis`) → `StationMood` → `LightFlicker`, Neon-Emission | `MarketEvent.crisis`, `Data/Days/StationMoodTuning.asset` |
| Frachtdrohne | P13 (Abnahme bei martin) | Gegenstand nach Hause schicken; FOOD/POWER/MEDICINE deckt den Bedarf heute Nacht | Inventar → `PlayerEconomy.Delivery` → Abrechnung | Bedarf → Label in `Household.asset` |
| Abrechnung | P10, P13, P70 | Lohn pro **richtig** entschiedenem Reisendem, Abzug für Verwarnungen über den freien, Miete, Bedarfe selbst anklicken (nichts vorausgewählt), RETURN zurück; **Miete nicht bezahlbar → Game Over „STRANDED“** (Platzhalter) | Reisende + Verwarnungen + Lieferungen → Wallet + Familie | `Data/Economy/Household.asset`: Lohn 15, Abzug 10, Miete 30, Geld am Bett = Händlerwert × 1,35 |
| Familie | P10 | Mom, Lena, Pip; Stufen well → weak → sick → critical; ohne Essen alle schlechter, ohne Wärme Pip, Medizin heilt | Abrechnung → `Household` → Zeilen im Morgenbrief | `Household.asset` (Namen, Texte je Stufe) |
| Verkehr & Atmo | P47, P55, P67 | Schiffe vor den Fenstern, Stations-Loop | – (Stimmung) | `ShipTraffic_*.asset`, `SFX_Amb_Station_Loop` |
| Ticket nach Hause & Abflug | P72 | Ticketautomat im Hangar neben der Rampe (nur abends, zeigt den Fehlbetrag), Kauf zieht den Ticketpreis ab; an der Rampe „Leave Kestrel-9 for good?“ → selbst gesteuerter Flug (W/S, Maus oder A/D, Shift) durch die Barriere; 320 m weg oder 45 s → Ende | Wallet → `PlayerEconomy.HasTicket` → `ShuttleFlight` → `DayManager.FinishRun` | `HouseholdTuning.ticketPrice` (350), `Data/Departure/DepartureTuning.asset` (Flug, Endtexte) |
| Ticket-Enden | P72 (P12 hängt weitere an) | Der schlechteste Familienzustand beim Abflug entscheidet: well/weak → HOME, sick → HOME, BARELY, critical → TOO LATE? (mit Namen) | `Household` → `TicketEnding.Pick` → Endbildschirm | Texte in `DepartureTuning` |
| Bugreport im Spiel | P84 (ersetzt P78) | Pausenmenü → REPORT BUG → Text tippen → SEND; das Backend auf Geoffrey legt ein GitHub-Issue an (Labels `bug`, `player-report`) | Version, Tag, Phase, Szene, Browser → `/api/report` | `Data/Menu/BugReportSettings.asset`, `deploy/reports` |
| Dev-Werkzeuge | P09, P56, P10 | Dev_end_Day, Hitboxen, TRAFFIC_LINE_DEV, CREDITS ±100 | – | **vor der Abgabe ausschalten** (DEV-Reiter weg) |

## Geplant (Paket offen oder Entwurf)

| Mechanik | Paket | Hängt an | Stand |
|---|---|---|---|
| Asservatenschrank aufbrechen | P83 (Kann) | P08, P32 | offen |
| Familienschicksal: Folgen von „kritisch“ | P69 (Entwurf) | Familie → P12 | mit max abstimmen |
| Enden (Familie, Moral, gefeuert) | P12 | Familie, P08, P69 | offen |
| Briefe reagieren auf den Zustand | P15 | Familie | offen |
| Balancing über 3 Tage (Economy Report) | P71 (geplant) | alles Geld | nach P68 |
| Gadgets: Scanner, UV-Lampe | P16, P62 | Schalter, Kiosk | Scanner-Funktion unklar (siehe Lücken) |
| Familie in der Welt (Stadtlichter, Foto) | P29 | Familie | Assets bestellt |

## Lücken / passt noch nicht
- **Brief von Tag 3** sagt fest „Pip is sick now“, auch wenn Pip gesund ist → P15.
- **Moral ohne Wirkung:** P08 zählt sie, aber erst P12 (Enden) liest sie.
- **Beschlagnahme vs. Simulator:** Der Economy Report rechnet Beschlagnahme und Strafen nicht mit – Mengen (`maxPerItem`) mit P15 abstimmen.
- **P11 nur halb:** Schwarzmarkt-Einkauf und Beschlagnahmtes verkaufen warten auf P08.
- **Scanner (P16):** In den Daten gibt es nur „gefälschtes Siegel“, und das deckt laut P58 die UV-Lampe auf. Was der Scanner zeigt, ist offen.
- **Schmuggel nach Hause:** geht ohne Wirkung und ohne Risiko → später mit P08.
- **Niederlage:** Bisher nur „Miete nicht bezahlbar“ (Platzhalter). Familie kann dauerhaft „critical“ sein, ohne Folge → P69, P12.
- **Jede Geldänderung braucht ein neues Rebalancing** (GameOff → Economy Report bis alle Ziele ✓; `EconomyBalanceTests` bricht sonst).
- **Vernachlässigen lohnt sich noch halb:** Das Ticket-Ende wertet die Familie jetzt (TOO LATE?), aber ohne Folgen von „critical“ (P69) bleibt „nichts versorgen“ der schnellste Weg zum Ticket.
- **Ticket erst ab Tag ~7 erreichbar, das Spiel endet aber nach Tag 3** → braucht P15/P73. Bis dahin nur mit Dev (`Dev_free_Ticket`, CREDITS ±100).
- **Während des Flugs gibt es kein Pausenmenü** (Steuerung gehört dem Flug, höchstens 45 s).
- **Endlos noch nicht gebaut:** Der Simulator wiederholt die Story-Tage ab Tag 4, bis P73 Pools liefert; das Spiel selbst endet noch nach Tag 3.
- **Waren ohne Quelle bis P08:** Pistol, Waffenteile, Wertsachen, Alien-Zeug, Liquor kommen nur über die Fracht (Beschlagnahme, P08) – bis dahin gibt es sie im Spiel nicht.
- **Icons nur in den Daten:** Inventar und Handelsfenster zeigen noch keine Icons (Listen nur mit Namen) – eigenes Paket.
- **Gadgets ohne Laden:** Scanner (P16) und UV-Lampe (P62) haben noch keinen Verkäufer. Scanner erst mit P16.
