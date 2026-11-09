# Mechaniken – Übersicht

Wofür: Auf einen Blick sehen, welche Spielmechaniken es gibt, wie sie ineinandergreifen und wo noch etwas nicht zusammenpasst.
Pflege: Wer ein Paket fertigstellt, das eine Mechanik neu baut oder ändert, zieht diese Datei im selben Push nach (siehe `CLAUDE.md`).
Wer ein Paket plant, prüft hier zuerst auf Konflikte. Details stehen in `PAKETE.md`, Entscheidungen in `DECISIONS.md`.

Stand: 09.11.2026 (martin)

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
nächster Morgen (Brief zeigt die Folgen) … nach den 12 Story-Tagen endlos weiter mit gezogenen Tagen (P73), bis ein Ende kommt
```

## Gebaut

| Mechanik | Paket | Was sie tut | Eingänge → Ausgänge | Daten / Einstellwerte |
|---|---|---|---|---|
| Tagesablauf | P09 (abgenommen, 07.11.); P73 | Morgen → Schicht → Abend → Nacht → nächster Tag; 12 Story-Tage, danach endlos | `CounterShift.ShiftEnded` → Abend; Bett → `NightStep` (Abrechnung) → nächster Tag; `LetterAddendum` → Brief | `Data/Days/DaySchedule.asset` |
| Schalter & Dokumente | P05, P06, P38, P40, P86, P89, P91 | Papiere ziehen, stempeln, Schublade, Nachfragen per Klick auf ein Feld, Reisende kommen durch die Hangar-Tür und gehen durch die passende Tür wieder hinaus; Verhaftung und Schleuse geschehen hinter einem fallenden Gitter, nur zu hören; lesen: bis zu 4 Papiere und Pinnwand-Zettel nebeneinander (Klick holt dazu, [R] alle), Umschauen beim Lesen; Tag 1: Korr-Tutorial mit Schritt-Tafel, [N] überspringt | Reisender → Entscheidung (`DecisionMade`) | `Data/Travelers/…`, Dokument-Prefabs |
| Briefe nach Familienzustand | P15 | Jeder Story-Tag ab Tag 2 hat zwei Fassungen; die zweite, wenn jemand mindestens krank ist | `DayManager.FamilyInTrouble` → `DayEntry.LetterFor` | `DaySchedule`, Texte in `Scripts/Editor/StoryDayContent.cs` |
| Regeln & Urteil | P07, P41 | Regeln des Tages, Pinnwand, Durchsagen; jede Entscheidung wird geprüft | Entscheidung → `Judgement` (richtig/falsch, Moral-Kosten) → bei Fehler Verwarnzettel | `DayRules`, `freeWarnings` (2) in `CounterShift` |
| Geld & Inventar | P46 | Kontostand mit Tagesbuchungen (TODAY), Inventar (I), Gegenstände mit Labels | alle Käufe/Verkäufe/Abrechnung → `Wallet` | `EconomyTuning` (Start 40 CR), `Data/Items/*` |
| Warenliste 2: Icon, Modell, Fracht-Stichwort je Ware | P82 | 24 Waren (neu: Liquor, Weapon parts, Cash bundle, Alien specimen, UV lamp; Stim injector heißt jetzt Stimulant vials). Waren wirken nicht stärker als andere, sie sind **eine andere Quelle** (z. B. beschlagnahmte Medizin statt Laden). Jede Ware mit Fracht-Stichwort (`cargoWord`) hat eine **CONFISCATED-Variante** (`Data/Items/Confiscated/`, gleiche Labels + CONFISCATED → hilft der Familie, nur der Schwarzmarkt kauft sie) | Fracht der Reisenden (P15) → Beschlagnahme (P08); Familie, Händler | `Data/Items/*`, gebaut von `Update Economy Data` |
| Beschlagnahme, Wahl am Schichtende, Disziplinarstrafen, Moral | P08 | Verhaften → Frachtbrief → Schicht-Inventar (max. 3 je Ware). Schichtende: „Nothing to report“ (80 % erwischt: Ware weg, 100 CR, 1 Disziplinarstrafe) / „Oh, seems I missed that“ (60 % ein Stück, sonst 30 CR) / „As ordered“. 3 Disziplinarstrafen = DISCHARGED. Moral: Schleuse −1, bei Unschuldigen −3 (Enden P12, Stimmung P113) | Fracht-Stichworte (P82), Schicht-Inventar (P81), Inventar (CONFISCATED-Varianten), Wallet (Strafen in TODAY), Enden (P12) | `Data/Economy/ConfiscationTuning.asset` |
| Endlose Tage | P73 | Nach dem letzten Story-Tag zieht `DayGenerator` jeden Tag aus `DayPool`: Regeln (1→3), Fahndungen, 6→8 Reisende mit Papieren passend zum Datum und einem Fehler (abgelaufen, Name/ID falsch, Siegel, verbotene Ware, Herkunft, Quarantäne, Frachtbrief fehlt, gesucht; knifflige Varianten werden häufiger), Marktereignis (35 %), Bar-Wunsch (50 %), Bestechung (25 %), Brief. Startwert neu je Durchgang, gleicher Startwert = gleiche Tage; schwerer bis ca. Tag 20 | `DaySchedule.loop` → `DayManager.Entry(day)` (Barkeeper, Bestechung, Markt wie Story-Tage); Simulator mit Startwert 7301 | `Data/Days/DayPool.asset` (GameOff/Build Day Pool), `EndlessDayTests` |
| Wiederkehrende Figuren: Auftritte, Gedächtnis, Folgen | P75; acht Figuren über 12 Story-Tage P76 (`docs/FIGUREN.md`); Fragen am Schalter, Flags und Enden P87; Verwahrung P77 | Figuren kommen geskriptet in die Schlange (fester Tag/Platz oder Zeitfenster, Bedingung über den letzten Ausgang), grüßen je nach letztem Ausgang, reden über Markt-Ereignisse. Jede Entscheidung = Ausgang je Auftritt (geholfen/verraten/neutral/beschlagnahmt/tot) → Folgen: Markt-Ereignis in n Tagen, Fahndung. Schleuse = nie wieder | Schalter (`CounterShift.PlanDay`), Markt (`Market.Schedule` → Pinnwand, Durchsage, Gerücht, Lage der Station), Fahndungsliste (RuleEvaluator) | `Data/Characters/` (Test: Tess Marlo, nur mit Dev-Tools) |
| Schicht-Inventar | P81 | Am Schalter öffnet [I] „SHIFT · CONFISCATED“ statt des eigenen Inventars (Schalter ruht solange); am Schichtende geht es an P08 (`ShiftClosing`) und wird geleert | Beschlagnahme (P08) → `PlayerEconomy.ShiftItems` | Dev: `Dev_fill_Shift` (Dev-Startwaren) |
| Labels | P64, P13, P70 | gemeinsame Kategorien für alle Systeme | Händler-Regeln, Bedarfe der Familie, später Gerüchte/Wünsche | `Data/Items/Tags/Tag_*` (13 Labels inkl. LUXURY, Liste bei P64) |
| Markt: Label-Faktoren, feste Ereignisse pro Tag (Preis, Menge, Ankauf, Sortiment), Verderb | P68 | Grundpreis × Schwankung × Labels × Ereignisse; Ankündigung als MARKET-Zettel und Durchsage | Händler, Vorrat, Bett, Brief (verdorben), später P65 | `ItemTag.priceFactor`, `Data/Market/Market_*`, `DaySchedule.marketEvents`, `EconomyTuning.perishableTag` |
| Barkeeper: Wunsch nach Label → Gerücht über morgen | P65, P93 | Der Barkeeper nennt den Wunsch im Gespräch (auch tagsüber), Tausch nur nach der Schicht, GIVE nimmt die billigste passende Ware, Gerücht = `rumour` der Ereignisse von morgen | Labels, Markt (P68), Inventar (Notiz unter TODAY) | `DayEntry.barWish`, `MarketEvent.rumour` |
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
| Ticket nach Hause & Abflug | P72, P106 | Ticketautomat im Hangar neben dem Schott (nur abends, zeigt den Fehlbetrag), Kauf zieht den Ticketpreis ab; durchs Schott in die Kabine des angedockten Shuttles (P106), am Sitz „Leave Kestrel-9 for good?“ → selbst gesteuerter Flug (W/S, Maus oder A/D, Shift) durch die Barriere; 320 m weg oder 45 s → Ende | Wallet → `PlayerEconomy.HasTicket` → `ShuttleHatch`, `ShuttleBoarding`, `ShuttleFlight` → `DayManager.FinishRun` | `HouseholdTuning.ticketPrice` (350), `Data/Departure/DepartureTuning.asset` (Flug, Endtexte) |
| Ticket-Enden | P72 (P12 hängt weitere an) | Der schlechteste Familienzustand beim Abflug entscheidet: well/weak → HOME, sick → HOME, BARELY, critical → TOO LATE? (mit Namen) | `Household` → `TicketEnding.Pick` → Endbildschirm | Texte in `DepartureTuning` |
| Familienschicksal | P69 | Nach 2 weiteren Nächten auf kritisch stirbt die Person; Warnung am Morgen davor; Tote brauchen nichts, Briefe wechseln; alle oder Lena tot → FAMILY LOST | `Household.Settle` → `PlayerEconomy.LossLetter`, `FamilyBroken` → `DayManager.GameOver` | `HouseholdTuning.criticalNightsToDeath`, Brieftexte im selben Asset |
| Moral spürbar | P113 | Nach einer Schleuse: Gedanke am Bullauge und auf der Bank, Abendgäste reden darüber, Albtraum-Zeile am nächsten Morgen. Nur Stimmung, kein Einfluss auf Geld oder Regeln | `Conduct` (heutige Schleusen), `MoraleTuning` → `PortholeView`, `HallBench`, `EveningCrowd`, Morgenbrief | `Data/Economy/MoraleTuning.asset` (`lingeringAtOrBelow`) |
| Enden | P12, P72, P87 | Ticket (HOME, HOME BARELY, TOO LATE?, HOME WITHOUT THEM) + Schlusssatz nach Moral; FAMILY LOST; DISCHARGED; STRANDED; AIRLOCKED | `TicketEnding`, `Conduct.Morale` | `DepartureTuning` (`moraleBloodyBelow`) |
| Gadgets | P16, P62 | Gekauft liegen sie links hinten auf dem Tisch; Klick schaltet. UV: zeigt Siegel, gefälschte rot. Scanner: NAME und ID, die sich widersprechen, rot | Inventar → `PaperGadgets` → `DocumentView` | Kiosk `fromDay`: UV 4, Scanner 6 |
| Spieluhr | P97 | Eine Uhrzeit für alle Wanduhren | `DayManager.Update` → `GameClock` → `ClockDisplay` | `eveningClockSeconds` |
| Reaktionen, Abendgäste | P100, P101 | Zeile nach dem Stempel; bis zu 6 Durchgelassene stehen abends in Bar und Halle | `DecisionMade` → `TravelerDialogue`, `EveningCrowd` | `DialogueTuning.speciesLines` |
| Wache, Schleuse | P104, P105 | Wache packt die Person, dann Gitter; Schleuse: Außenschott auf, Ragdoll fliegt hinaus | `TravelerPresenter` | `DialogueTuning` (`guard*`, `hatchSeconds`, `airlock*`) |
| Figuren-Folgen, Verwahrung | P85, P77 | Geschenke an die Familie, Bonus/Rüge per Durchsage, Briefzeilen; zurückgelassene Dinge bringen beim nächsten Auftritt Pfand oder Verrat; Shifter an Tag 3 verhaftet → Verhör-Durchsage am Morgen danach; Nells Sohn ist jemand, den du verhaftet hast (Jonas Varga von Tag 4, sonst die letzte Mensch-Verhaftung, sonst Jonas von anderen gefasst); am Morgen von Tag 11 fasst der Brief die Wege aller getroffenen Figuren zusammen | `CharacterDirector` | `Consequence`, `Appearance.entrust*` (`StoryCharacterBuilder`) |
| Terminal-Stil | P95 | Einheitliche Rahmen und Knöpfe | `TerminalStyle` (Editor) | Grafiken in `Art/UI/Terminal` |
| Kiosk-Gitter | P90 | Gitter unten, solange der Kiosk zu ist | `Vendor.Update` → `SecurityShutter` | – |
| Aussehen der Reisenden | P108 | Name → feste Textur, Frisur/Mütze; Passfoto gerendert | `TravelerLooks`, `TravelerPhotoBuilder` | `Data/Travelers/TravelerLooks.asset`, Fotos in `Art/Textures/Documents/Photos` |
| Schiffe im Hangar | P107 | 2–4 je Tag, eines landet morgens | `HangarShips` ← `DayStarted` | Plätze im `EconomyBuilder` |
| Münzfernrohr, Stadtlichter | P29 | 1 Credit, 30 s Blick auf den Heimatplaneten; dessen Lichter werden mit der Familie dunkler und flackern, wenn jemand kritisch ist | `CoinTelescope`, `HomeLights` ← `Household` | im Prefab `CoinTelescope`, 1 CR, 30 s |
| Asservatenschrank | P83, P32 | „As ordered“ landet im Schrank; [E] öffnet die Türen, die Waren liegen als Modelle in 6 Fächern (eigenes Modell, sonst nach Label, sonst Beweisbeutel; Türen schließen ab 3 m oder am neuen Tag); abends bei offenen Türen einmal aufbrechen: Ware (CONFISCATED) oder Disziplinarstrafe | `Confiscation.Locker`, `EvidenceLocker`, `EvidenceLockerDisplay` | `ConfiscationTuning.lockerCaughtChance`, `Data/Economy/EvidenceLockerTuning.asset` (Label → Modell) |
| Terminal-Datenbank | P34 | Klick auf den Röhrenbildschirm am Schalter → Name und ID des Ausweises sind Knöpfe (P112) → Abgleich mit der Fahndungsliste des Tages (gleiche Prüfung wie `RuleEvaluator`) und Einreiseakte (nur der Ausweis), abgelaufener Ausweis markiert; Statuszeile Lane, Zähler, Uhr | `CounterTerminal`, `TerminalLookup` ← `DayRules.Wanted`, Papiere | `Data/Counter/TerminalTuning.asset` |
| Pinnwand im Stehen | P33, P41 | [E] an der Pinnwand → Vollbild mit Aushang, Fahndungsfotos, Markt-Notiz, Memo; ältere Morgen bleiben (bis 30 Tage, A/D blättern), an der Wand lugen ältere Zettel hervor | `PinBoardCloseUp`, `PinBoardNotes` ← dieselben Texte wie `PinBoard` | im `PF_Economy` |
| Sitzplätze & Ruhemomente | P28 | Hallenbänke: sitzen, Durchsagen, abends [F] bis 23 Uhr warten; Barhocker 1–3 öffnen das Barkeeper-Gespräch; Bullauge in 103: Blick auf den Heimatplaneten mit einem Gedanken je Familienzustand; Q/Esc steht auf | `SeatView`, `HallBench`, `BarStool`, `PortholeView` ← `Household`; `DayManager.SkipEvening` | `Data/Rest/RestSpotTuning.asset` (Texte, Zeiten) |
| Familie in der Welt: Foto, Brief | P29 | Foto am Spind in 103 intakt/verblasst/zerrissen; der Morgenbrief liegt auf dem Tisch, die Schrift zittert stärker, je schlechter es der Familie geht | `FamilyPhoto`, `TableLetter` ← `Household.Changed`, `DayManager.LetterWritten` | `RestSpotTuning` |
| Sperrstunde | P109 | Um 23 Uhr Abenduhr: Gong + Durchsage, Kiosk (Gitter fällt), Automat, Drohne, Ticketautomat, GIVE beim Barkeeper schließen; Reden, Schrank-Einbruch, Shuttle, Bett bleiben; Anzeigesäule NIGHT · CURFEW | `DayManager.TradingOpen`/`IsCurfew`, `CurfewStarted` → alle Händler; Bank [F] spult bis dahin | `Data/Counter/Announcements.asset` (`curfew*`), `eveningClockSeconds` (300) |
| Zoll bei der Drohne | P110 | Sendung mit CONTRABAND/CONFISCATED: einmal pro Nacht 25 % → nur die Schmuggelware weg, 1 Disziplinarstrafe, Zeile in Abrechnung und Brief; Geschenke nie geprüft | `HomeDelivery` → `DroneCustoms` (in `SettlementWindow.Run`) → `Conduct` | `ConfiscationTuning.droneInspectChance` + Texte |
| Fernrohr: Blick aufs Zuhause | P111 | Voll gezoomt auf den Planeten → Blende zum Diorama der Heimatstraße; drei Fenster (Mom, Lena, Pip) hell/gedimmt/flackernd/dunkel je Zustand; herauszoomen → zurück | `CoinTelescope`, `TelescopeFocus`, `HomeWindows` ← `Household.Changed`, `DayStarted` | `Data/Economy/HomeViewTuning.asset`, Modell `SM_Home_Street` (bestellt, bis dahin Platzhalter) |
| Bugreport im Spiel | P84 (ersetzt P78) | Pausenmenü → REPORT BUG → Text tippen → SEND; das Backend auf Geoffrey legt ein GitHub-Issue an (Labels `bug`, `player-report`) | Version, Tag, Phase, Szene, Browser → `/api/report` | `Data/Menu/BugReportSettings.asset`, `deploy/reports` |
| Dev-Werkzeuge | P09, P56, P10 | Dev_end_Day, Hitboxen, TRAFFIC_LINE_DEV, CREDITS ±100 | – | **vor der Abgabe ausschalten** (DEV-Reiter weg) |

## Geplant (Paket offen oder Entwurf)

| Mechanik | Paket | Hängt an | Stand |
|---|---|---|---|
| Stimmen: 10 Laute je Spezies | P94 (martin) | Händler, Reisende | offen |
| Heim-Shuttle angedockt, begehbar | P106 | P72 | Lieferung liegt bereit |

## Lücken / passt noch nicht
- **Beschlagnahme vs. Simulator:** Der Economy Report rechnet Beschlagnahme und Strafen nicht mit – Mengen (`maxPerItem`) mit P15 abstimmen.
- **Tod im Simulator:** Der Economy Report rechnet ohne P69 (Grenze 0), der Tod spielt also für die Balance-Ziele keine Rolle.
- **Figuren-Geschenke und Bonus** (P85, P77) rechnet der Economy Report nicht mit; es sind wenige Credits.
- **Nells Sohn** wird beim ersten Nachfragen festgelegt (Tag 6) und zählt nur Verhaftungen bis zum Vortag. Gezogene Reisende ab Tag 13 zählen mit, spielen aber keine Rolle mehr. Das Ende (P12) wertet die Figuren noch nicht aus, nur die Zusammenfassung an Tag 11.
- **Jede Geldänderung braucht ein neues Rebalancing** (GameOff → Economy Report bis alle Ziele ✓; `EconomyBalanceTests` bricht sonst).
- **Während des Flugs gibt es kein Pausenmenü** (Steuerung gehört dem Flug, höchstens 45 s).
- **Ende AIRLOCKED nur abseits des richtigen Wegs:** Wer den Shifter an Tag 3 wie befohlen terminiert, sieht seine Frage (Tag 6) nie. Gewollt (Test der Behörde), aber die meisten Spieler werden das Ende so nicht finden.
- **Endlose Tage ohne Geschichte:** Ab Tag 13 nur gezogene Reisende ohne Figuren, Briefe aus einem kleinen Pool (10) – das Geld wächst nach dem Ticket weiter (Report: Tag 30 ≈ 1900 CR), es gibt nur das Ticket als Ziel. Reisende ab Tag 13 haben keine Antworten auf Nachfragen (P40) und keine Fracht-Geschichten.
- **Figuren an festen Plätzen (Korr an Tag 1) stehen exakt dort**, freie Figuren irgendwo davor oder dahinter (`CharacterPlanner.Queue`).
- **Welle 1 (P28, P32, P34, P33-Rest) und Welle 2 (P109–P111) sind nur mit EditMode-Tests geprüft**, nicht angespielt: Kamerapositionen, Klickflächen und Lesbarkeit beim Anspielen prüfen.
- **Terminal kennt die Tischposition als Konstante** (Tisch 1,3 m neben dem Schalter): wird der Tisch im Stations-Builder verschoben, `CounterTerminalBuilder` mitziehen.
- **Abend: 5 echte Minuten** bis zur Sperrstunde – ob das zum Handeln, Fernrohr und Barbesuch reicht, beim Anspielen prüfen.
- **Zoll im Simulator:** Der Economy Report schickt nie Schmuggel per Drohne; P110 ändert kein Geld, aber Strafpunkte aus dem Zoll fehlen im Report.
- **Pinnwand-Verlauf nur im Speicher:** ohne Spielstand beginnt er bei jedem Spielstart neu.
- **Icons nur in den Daten:** Inventar und Handelsfenster zeigen noch keine Icons (Listen nur mit Namen) – eigenes Paket.
