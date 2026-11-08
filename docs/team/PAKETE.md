# Entwicklungspakete

**Zentrales Entwicklungskonzept:** Die ganze Arbeit ist in klar abgegrenzte Pakete aufgeteilt. Wer Zeit hat,
nimmt sich ein offenes Paket und baut es fertig. Es gibt keine festen Zuständigkeiten pro System
(Ausnahmen: finale Assets → Max, Startbildschirm → Martin).

## Regeln
1. **Paket nehmen:** `git pull` → bei einem offenen Paket `Status` auf `in Arbeit (<name>, <Datum>)` setzen →
   **sofort committen + pushen** (`Take: P07`). Wer zuerst pusht, hat es.
2. **Höchstens ein Paket pro Person gleichzeitig** (zwei, wenn eines blockiert ist).
3. **Nur im Umfang bauen.** Was unter "Nicht enthalten" steht oder neu auffällt → neues Paket anlegen, nicht nebenbei bauen.
4. **Szenen/Prefabs/geteilte Assets** brauchen trotzdem einen `/claim` (siehe `CLAUDE.md`).
5. **Fertig heißt:** Abnahmekriterien erfüllt, keine Konsolenfehler, gepusht, Status `fertig (<name>, <Datum>)`,
   kurze Notiz unter "Ergebnis" (wo liegt es, wie benutzt man es).
6. **Folgepakete:** Muss etwas Fertiges überarbeitet werden, entsteht ein neues Paket mit `Folgt auf: Pxx`.
   Fertige Pakete werden nicht wieder geöffnet.
7. **Neue Pakete** darf jeder anlegen (nächste freie Nummer). Pakete, die den Kern des Spiels ändern
   (`DESIGN.md` → Muss), vorher kurz absprechen.
8. **Braucht ein Paket Assets:** Anforderung in `docs/ASSET-REQUESTS.md` eintragen, mit Platzhalter weiterbauen.

**Größen:** `S` ≈ ein halber Tag · `M` ≈ 1–2 Tage · `L` ≈ 3–4 Tage. Ein Paket größer als `L` wird aufgeteilt.
**Priorität:** `Muss` / `Soll` / `Kann` wie in `DESIGN.md`. Muss-Pakete zuerst.

## Reihenfolge (Stand 05.11. abends, max)
Der Kern-Loop wird in dieser Reihenfolge fertig; wer frei ist, nimmt das nächste offene Muss-Paket.
1. ~~Kern-Loop-Call~~ hat stattgefunden (martin, 06.11.); P58 ist entschieden
2. **P09** Tagesablauf abnehmen (max, mit Kontroll-Gameplay; martin hat im Dev-Modus getestet)
3. **P08** Entscheidungen & Moral → **P10** Geld, Familie & Abrechnung
4. **P15** Inhalte für 3 Tage inkl. **The Shifter** (auch echte Antworten der Reisenden) → **P12** Enden
5. **P11** Kiosk, **P13** Heimschicken, **P64** Schwarzmarkt kauft nur Schmuggelware; **P19** itch-Upload ist auf später verschoben (martin, 06.11.)
6. Danach Soll-Pakete nach Zeit: P50 Türschilder, P14 Reisende laufen, P16, P25–P36; P53 Wohnmodul erst nach Entwurf und Absprache

## Vorlage
```
### Pxx — <Name>
- Größe: S/M/L · Priorität: Muss/Soll/Kann · Status: offen
- Braucht: <Pakete, die vorher fertig sein müssen, oder —>
- Folgt auf: <Pxx oder —>
- Ziel: <ein Satz>
- Enthalten: <Liste>
- Nicht enthalten: <Liste>
- Abnahme: <woran man sieht, dass es fertig ist>
- Ergebnis: <nach Fertigstellung>
```

## Pakete

> **Entwurf (31.10.)** — erste Liste von Claude aus `DESIGN.md` abgeleitet. Am 02.11. nach dem Theme gemeinsam
> prüfen, anpassen und freigeben. Erst danach werden Pakete genommen (außer P01 und P02).

### P01 — Projekt-Setup
- Größe: S · Priorität: Muss · Status: fertig (max, 02.11.; abgeschlossen von martin, 05.11.)
- Braucht: — · Folgt auf: —
- Ziel: Unity-Projekt steht, beide können klonen und öffnen.
- Enthalten: Unity 6000.3.23f1, Vorlage Universal 3D (URP), Ordnerstruktur nach `CLAUDE.md`/`docs/ASSETS.md`,
  Pakete (Input System, Cinemachine 3, ProBuilder, AI Navigation, TextMeshPro, DOTween), WebGL-Einstellungen
  (WebGL 2, Brotli + Decompression Fallback, WebAssembly 2023, IL2CPP, Strip Engine Code), Szenen Boot/Menu/Main,
  Sandbox-Szenen `Max_Sandbox`/`Martin_Sandbox`, `M_Placeholder.mat`
- Nicht enthalten: Gameplay
- Abnahme: Leerer WebGL-Build läuft lokal im Browser, beide haben das Projekt geöffnet.
- Stand 02.11. (max): Projekt steht und ist gepusht. WebGL-Build (14,9 MB) startet lokal im Browser ✔ (von Max bestätigt). Offen: Martin öffnet das Projekt einmal → dann `fertig`.
- Stand 05.11. (martin): Martin hat das Projekt mit 6000.3.23f1 geöffnet (erster Import ohne Fehler) und einen WebGL-Build gebaut und im Browser getestet ✔ → `fertig`.

### P02 — Startbildschirm & Menü
- Größe: M · Priorität: Muss · Status: fertig (martin, 05.11.)
- Braucht: P01
- Ziel: Alles vom Spielstart bis zum Menü.
- Enthalten: Boot-/Splash-Screen, Hauptmenü (Start, Optionen: Lautstärke/Maus-Empfindlichkeit, Credits, Beenden
  ausgeblendet im Web), Übergang ins Spiel (`Main`), Pausemenü (Esc)
- Nicht enthalten: Spielinhalte, finale Grafik (Platzhalter)
- Abnahme: Start → Menü → Spiel → Pause → zurück ins Menü funktioniert im WebGL-Build.
- Ergebnis (martin, 05.11.):
  - **Generiert, nicht von Hand ändern:** `Boot.unity`, `Menu.unity`, `Prefabs/UI/PF_PauseMenu.prefab` und dessen Instanz in `Main.unity`
    baut **GameOff → Build Menu Scenes** (`Scripts/Editor/MenuSceneBuilder.cs`). Änderungen im Builder machen, dann neu bauen.
  - **Boot:** Splash (Team alphabetisch, Titel), blendet ein/aus, Taste/Klick überspringt → Menu (`Scripts/UI/BootSplash.cs`).
  - **Menu:** Start Shift / Options / Credits / Quit (Quit im Web ausgeblendet), Esc führt aus Unterseiten zurück (`MainMenu.cs`).
    Credits kommen beim Bauen aus `CREDITS.md` (Assets alphabetisch) → nach neuen Credits Build Menu Scenes neu laufen lassen.
  - **Menü-Hintergrund (Variante A):** Instanzen von `PF_SkyRig`, `PF_Exterior`, `PF_Station_Graybox` + Kamera, die langsam um die
    Station schwenkt (`MenuCameraDrift.cs`). Echtzeit-Lampen der Station werfen nur im Menü keine Schatten (Instanz-Override, Leistung im Web).
    **Nach jedem Neubau der Station/Außenwelt einmal Build Menu Scenes**, damit das Menü aktuell ist. Gebackenes Licht gibt es im Menü nicht.
  - **Optionen:** Lautstärke + Maus-Empfindlichkeit (Faktor auf `PlayerTuning.mouseSensitivity`), gespeichert in PlayerPrefs
    (`Scripts/Core/GameSettings.cs`, im Browser auch nach Neuladen). `FirstPersonController` liest `GameSettings.MouseSensitivity`.
  - **Pause in Main** (`PauseMenu.cs`): Esc oder P beim freien Laufen → Zeit steht, Ton pausiert, Maus frei; Resume / Options / Main Menu.
    Am Schalter bleibt Esc beim Schalter (Schritt zurück/aufstehen), erst im Stehen pausiert Esc. Im Browser schluckt Chrome das erste Esc
    (gibt nur die Maus frei) → `Plugins/WebGL/PointerLock.jslib` fragt den Browser direkt, verlorene Maus-Sperre pausiert. Im Vollbild
    verlässt Chrome dabei auch das Vollbild (nicht abstellbar). Nach Resume im Browser einmal ins Bild klicken.
  - Szenennamen: `Scripts/Core/SceneNames.cs`. Getestet im Editor und im WebGL-Build (Chrome, auch Vollbild), 0 Fehler/Warnungen.
  - Offen, in Frage an max (`questions/2026-11-05_martin-an-max_schrift-und-unity-splash.md`): Orbitron für Titel, Unity-Splash abschalten,
    Variante B (Menü direkt in Main mit Kameraflug) als Folgepaket.
  - Optionen überarbeitet (Reiter Audio/Controls/Display) → Folgepaket **P43**.

### P03 — Spieler: Laufen & Interagieren
- Größe: S · Priorität: Muss · Status: fertig (max, 02.11.)
- Braucht: P01
- Ziel: Ego-Perspektive, WASD + Maus, E = interagieren.
- Enthalten: CharacterController-basierter Controller, Maus-Look (Pointer Lock im Browser), Interaktions-Raycast
  mit Hinweistext ("[E] Schlafen"), Interface/Komponente `Interactable`
- Nicht enthalten: Springen, Rennen, Kopfwippen
- Abnahme: In der Sandbox herumlaufen und ein Test-Objekt per E auslösen.
- Ergebnis: Prefab `Prefabs/Player/PF_Player.prefab` (CharacterController r=0,3 m, Kamera 1,65 m, HUD mit Fadenkreuz + Prompt),
  steht in `Main.unity` am `PlayerSpawn` (Zimmer 103). Skripte in `Scripts/Player/`: `FirstPersonController` (WASD, Maus-Look, Maus wird
  per Klick gefangen; `SetControlEnabled(false)` für Schalteransicht/Menüs), `PlayerInteractor` (Raycast + E), `Interactable`
  (Prompt-Text + UnityEvent `onInteract`, oder `Interact()` überschreiben). Werte in `Data/Player/PlayerTuning.asset`
  (4 m/s, Empfindlichkeit 0,12°/px, Reichweite 2,2 m). Input: `Data/Input/PlayerControls.inputactions`. Im Play-Modus getestet: Laufen, Wandkollision, Blick, E.

### P04 — Station als Graybox
- Größe: M · Priorität: Muss · Status: fertig (max, 02.11.)
- Braucht: P01
- Ziel: Begehbare Station aus ProBuilder-Blöcken in Endgröße.
- Enthalten: Zollschalter/Wachstation, Hangar, Bar, Motel mit eigenem Zimmer, Diner (leerer Raum reicht), Gänge
- Nicht enthalten: finale Modelle, Licht-Feinschliff
- Abnahme: Alle Räume in `Main` erreichbar, Laufwege unter ~15 Sekunden.
- Ergebnis: Grundriss `docs/STATION.md` (+ `docs/station/bauplan.html`). Prefab `Prefabs/Station/PF_Station_Graybox.prefab` in `Main.unity`,
  erzeugt per Menü **GameOff → Build Station Graybox** (`Scripts/Editor/StationGrayboxBuilder.cs`). NavMesh gebacken (Agent-Radius 0,3 m),
  alle Räume erreichbar, längster Weg Bar → Zimmer 103 = 8 s. Props sind benannte Platzhalter-Blöcke; Marker für Spawn, Schalter-Kamera, Reisenden-Standpunkt.

### P05 — Schalter-Ansicht
- Größe: M · Priorität: Muss · Status: fertig (max, 02.11.)
- Braucht: P03
- Ziel: Am Schalter E drücken → Kamera fährt auf den Tisch, Maus wird frei; Verlassen per Esc/Taste.
- Enthalten: Cinemachine-Übergang, Tischfläche, Knöpfe Verhaften/Luftschleuse, Stempel Durchlassen/Abweisen
- Nicht enthalten: Dokumentenlogik
- Abnahme: Wechsel hin und zurück ohne Ruckeln, Knöpfe lösen Events aus.
- Ergebnis: Prefab `Prefabs/Counter/PF_CounterStation.prefab` in `Main.unity` am Schalterstuhl (-2, -9). E am Stuhl → Cinemachine-Blende
  (0,6 s) auf `DeskVCam`, Maus frei, Hinweis "[Q] Leave counter"; Q → zurück. Klickbare Platzhalter auf dem Tisch: Stempel
  APPROVE/DENY, Knopf ARREST, Knopf AIRLOCK unter Schutzkappe (1. Klick Kappe auf, 2. Klick auslösen). Für P06/P07/P08:
  `CounterStation.ActionTriggered` (Event mit `CounterAction` Approve/Deny/Arrest/Airlock), `SeatedChanged`, `IsSeated`;
  jedes `CounterClickable` hat zusätzlich ein UnityEvent `onPressed`. Spieler-Prefab hat jetzt `PlayerCamera` mit CinemachineBrain
  + `PlayerVCam` am Kopf. Alle vier Aktionen + Hin/Zurück im Play-Modus getestet.

### P06 — Dokumente
- Größe: L · Priorität: Muss · Status: fertig (max, 04.11.)
- Braucht: P05
- Ziel: Dokumente als scharfe 2D-Karten auf dem Tisch, mit der Maus ziehbar, Daten kommen aus ScriptableObjects.
- Enthalten: 2–3 Dokumenttypen (z. B. Ausweis, Einreiseerlaubnis, Frachtliste), Drag & Drop, Rückgabe an den Reisenden
- Nicht enthalten: Prüflogik (P07), Gadgets
- Abnahme: Ein Test-Reisender legt Dokumente hin, man kann sie lesen, ziehen und zurückgeben.
- Ergebnis: Dokumente sind 3D-Papiere (Entscheidung 04.11.): Ausweis 12×8 cm, Einreiseerlaubnis 15×21 cm, Frachtliste 21×30 cm
  (`Prefabs/Documents/PF_Doc_*`, gebaut von `GameOff/Build Documents`), Text in Share Tech Mono, Daten aus `DocumentData`
  (`Data/Documents/`). Schublade fährt zum Reisenden und bringt die Papiere, sie gleiten auf den Tisch; ziehen, Klick = vor die Kamera
  zum Lesen (Klick/Esc/Rechtsklick legt zurück), stempeln direkt auf dem Papier, Papiere in die Schublade ziehen, Klick auf die
  Schublade gibt sie zurück. Bei Retro-Look an zeichnet eine Overlay-Kamera die Papiere im Sitzen in voller Auflösung.
  **Für P07:** `DocumentDesk.Deliver(IReadOnlyList<DocumentData>)`, Events `DocumentsDelivered` / `DocumentsReturned`, Entscheidung weiter
  über `CounterStation.ActionTriggered`; Feld-Keys: name, species, origin, id, expires, purpose, duration, issued, ship, destination, cargo
  (`DocumentData.Get(key)`); ob ein Papier gestempelt ist: `StampTarget.IsStamped/LastStamp`. Testlieferung beim Hinsetzen
  (`deliverTestOnSit`) abschalten, sobald echte Reisende kommen.
- Festgelegt im Asset-Durchgang (max, 02.11.): Dokumente kommen über die **animierte Durchreiche** (Schublade fährt mit Klacken herein, Papiere gleiten auf den Tisch; Rückgabe = in die Schublade ziehen, sie fährt hinaus). Modelle: `SM_Counter_DocSlot`, `SM_Station_Window_Small_Slot`.

### P07 — Reisende & Regeln
- Größe: L · Priorität: Muss · Status: fertig (max, 04.11.)
- Braucht: P06
- Ziel: Datenmodell und Prüflogik: Ist der Reisende korrekt?
- Enthalten: `TravelerData` (Aussehen, Dokumente, eingebaute Fehler, Gepäck), `DayRules` (Regeln pro Tag),
  Auswertung richtig/falsch je Entscheidung, Regel-Bulletin als Dokument
- Nicht enthalten: Inhalte für alle Tage (P15), Bewegung der Reisenden (P14)
- Abnahme: 3 Test-Reisende mit unterschiedlichen Fehlern werden korrekt bewertet.
- Ergebnis: Regeln wie in DESIGN.md "Regeln am Schalter". `TravelerData`, `DayRules` (Regeln per [SerializeReference]: gesperrte
  Herkunft, Pflichtpapier, verbotene Ware, Quarantäne; Fahndungsliste mit Verhaften/Terminieren), `RuleEvaluator` (Grundregeln +
  Tagesregeln, Priorität Pflicht-Schleuse > Verhaften > Abweisen > Durchlassen), `DecisionJudge`. Vergleiche exakt bzw. ganze Wörter
  (kein "ARMS" in "FARMSTUFF"). **Fehlerfreiheit:** `GameOff/Validate Travelers` + 41 EditMode-Tests (`Tests/Editor/`), u. a. alle Tage
  im Projekt: berechnete Entscheidung muss der beabsichtigten entsprechen, Beinahe-Treffer werden gemeldet, jedes Pflichtfeld muss im
  Prefab stehen. `CounterShift`: Stempel auf den Papieren + alles in die Schublade → Entscheidung (ohne Stempel / unvollständig
  blockiert die Schublade mit Hinweis, Papiere lassen sich wieder herausziehen); Verhaften/Schleuse-Knopf zieht die Papiere ein.
  Fehler → Verwarnzettel aus dem Drucker (2 Verwarnungen, dann PENALTY). Test-Tag `Data/Days/Day1_Test` mit 6 Reisenden.
  **Für P08:** `CounterShift.DecisionMade(traveler, chosen, Judgement)` — `Judgement.Correct/MoraleCost/Disobeyed`, `CounterShift.Citations`.
  **Für P09:** `CounterShift.BeginDay(DayRules)`, Event `ShiftEnded`. **Für P15:** Inhalte im Format von `TravelerContentBuilder`
  (oder direkt als Assets), danach `GameOff/Validate Travelers` bzw. Tests laufen lassen; `trickIntended` für absichtliche Beinahe-Treffer
  (The Shifter), `storyTag`.

### P08 — Entscheidungen & Moral
- Größe: M · Priorität: Muss · Status: fertig (martin, 07.11.)
- Braucht: P07, P46 · **erst bauen, wenn P46 fertig ist** (Grundstruktur für Geld und Inventar)
- Ziel: Durchlassen/Abweisen/Verhaften/Luftschleuse mit Folgen.
- Enthalten: Lohn/Abzug je Entscheidung, Beschlagnahme bei Verhaftung (Gegenstände ins Inventar),
  versteckter Moral-Wert (sinkt bei Luftschleuse, stark bei Unschuldigen), einfache Effekte (Wache, Schleuse)
- Nicht enthalten: Enden (P12)
- Abnahme: Alle vier Aktionen verändern Geld/Moral/Inventar korrekt.
- **Zum Besprechen (Max + Martin, vor P08):** Labels hängen an der Sorte Gegenstand (`ItemData`), nicht am einzelnen Stück. Eine normale und eine beschlagnahmte Taschenuhr sind darum zwei Gegenstände („Pocket watch“ und „Confiscated pocket watch“ mit `CONFISCATED`); ein Stück im Inventar bekommt kein Label nachträglich. Beschlagnahme legt direkt die beschlagnahmte Variante ins Inventar. Folge: Jeder beschlagnahmbare Gegenstand braucht zwei Assets. Alternative wäre ein Merkmal pro Inventar-Stapel („beschlagnahmt“), das kostet Umbau an `Inventory`. Erst gemeinsam entscheiden, sonst kollidiert es mit P08. (aus P64, martin 06.11.)
- Vorschlag Martin (06.11.): Schicht-Inventar nur am Tisch, Wahl am Schichtende (Nothing to report / Oh, seems I missed that / As ordered), Verwarnungen (P07) und Disziplinarstrafen getrennt, 3 Disziplinarstrafen = Game Over, alle Zahlen vorläufig → `questions/2026-11-06_martin-an-max_schichtinventar-beschlagnahme.md`, gemeinsam festlegen.
- **Festgelegt im Call (max + martin, 07.11.):** Vorschlag Martin gilt (Frage `schichtinventar-beschlagnahme`, A). Damit gehört zu P08:
  - Beschlagnahme legt Gegenstände ins **Schicht-Inventar (P81)**, nicht direkt ins Inventar.
  - **Wahl am Schichtende:** „Nothing to report“ (alles behalten, 80 % erwischt → 100 CR + 1 Disziplinarstrafe) · „Oh, seems I missed that“
    (60 %: ein Gegenstand nach Wahl als CONFISCATED-Variante ins Inventar, erwischt → nur Geldstrafe) · „As ordered“ (alles an den
    Asservatenschrank, vorerst gelöscht; Aufbrechen = P83).
  - **Disziplinarstrafen** als eigener Zähler über alle Tage, getrennt von den Verwarnungen (P07 bleibt); 3 = Game Over (Ende in P12).
  - Beschlagnahmte Ware ist eine **eigene Item-Variante** mit CONFISCATED (Varianten legt P82 an).
  - Alle Zahlen vorläufig in einem **Tuning-Asset** in `Data/`; jede Geldänderung → Rebalancing.
  - Braucht damit zusätzlich: **P81, P82**.
- Stand (martin, 07.11.):
  - **Verhaften** liest den Frachtbrief (`CargoReader`, Fracht-Stichworte aus P82) und legt die Waren ins Schicht-Inventar (P81), höchstens
    `maxPerItem` (3) je Ware – mit den Frachtmengen aus P15 abzustimmen (Martin).
  - **Schichtende:** Fenster „END OF SHIFT · CONFISCATIONS“ (nur wenn etwas beschlagnahmt wurde, Schalter ruht solange): Nothing to report
    (80 % erwischt → **Ware weg** (Martin), 100 CR, 1 Disziplinarstrafe; sonst alles als CONFISCATED-Variante ins Inventar) · Oh, seems I
    missed that (Ware anklicken; 60 % ein Stück als Variante, sonst 30 CR) · As ordered (alles an den Schrank, vorerst gelöscht).
  - **Disziplinarstrafen** (`Conduct`, über den ganzen Lauf): bei 3 nach CONTINUE Ende „DISCHARGED“ (`DayManager.FinishRun`, Text bis P12).
  - **Moral** (versteckt, `Conduct.Morale`): Schleuse −1, Schleuse bei Unschuldigen −3; liest P12.
  - Lohn/Abzug je Entscheidung gab es schon (P07/P10/P70: Lohn nur für richtige, Strafe ab der 3. Verwarnung) – unverändert.
  - Effekte (Warnlampe, Schott) bleiben bei P33 / dem bestellten Schott-Rework (Martin).
  - Zahlen in `Data/Economy/ConfiscationTuning.asset`. Tests `ConfiscationTests` (7), 130/130 grün; in einer Kopie von Main durchgespielt
    (Quill verhaftet → 3 Stimulant vials, Fenster, alle drei Antworten, Entlassung), Konsole 0 Fehler.
  - In Main eingebaut (07.11.) und dort geprüft: Quill verhaftet → Schicht-Inventar, Fenster am Schichtende, Schalter gesperrt; 0 Fehler.
- Festgelegt im Asset-Durchgang (max, 02.11.): Luftschleuse: Außenschott **klappt nach außen ins All** (Rework mit Angel bestellt), Sog, Warnlampe dreht rot, Reisender wird hinausgerissen. Verhaftung: Warnlampe rot. Beschlagnahmtes landet physisch im Asservatenschrank (P32).

### P09 — Tagesablauf
- Größe: M · Priorität: Muss · Status: fertig (martin, 06.11.; abgenommen von max, 07.11.)
- Braucht: P03
- Ziel: Morgen → Schicht → Feierabend → Schlafen → nächster Tag, 3 Tage.
- Enthalten: `DayManager`, Morgen-Brief + Bulletin, Schichtende nach Anzahl Reisender, Bett in der Unterkunft beendet den Tag
- Nicht enthalten: Abrechnung (P10)
- Abnahme: Drei Tage hintereinander durchspielbar (mit Test-Reisenden).
- Stand 05.11. (martin): gebaut und im Editor getestet, **noch nicht abgenommen**: Das echte Schichtende (alle Reisenden abgefertigt)
  ist noch nicht gespielt, weil am Schalter gerade P40/P41 entstehen; im Editor nur simuliert bzw. per Dev-Knopf.
  - `Scripts/Day/`: `DayManager` (Morgen → Schicht → Feierabend → Nacht → nächster Tag, nach dem letzten Tag Platzhalter-Ende),
    `DayFlowUI` (Brief, Hinweise, Abblende, Bett-Abfrage, Ende), `DaySchedule` + `DayEntry` (`Data/Days/DaySchedule.asset`: 3 × `Day1_Test`,
    Platzhalter-Briefe, „New today“-Zeile; Annahme Bulletin siehe Frage an max).
  - `Prefabs/Day/PF_DayFlow.prefab` in Main, gebaut von **GameOff → Build Day Flow into Main** (`DayFlowBuilder`). Bett-Hitbox wird an
    das Bett-Modell `SM_Motel_Bed` angepasst. Bett fragt immer nach („End day N?“ / „Travelers are still waiting“).
  - Nutzt `CounterShift.BeginDay`/`ShiftEnded` und `SkyRig.SetDay` unverändert. Ereignisse: `DayStarted`, `ShiftOver`, `DayEnded`, `RunFinished`.
  - **Dev-Werkzeuge** (`DayManager.showDevTools`, an): roter **Dev_end_Day**-Knopf im Bett-Fenster + sichtbare Bett-Hitbox.
    **Vor der Abgabe ausschalten.** Bekannt: Dev_end_Day mitten im Reisenden lässt dessen Papiere bis zum nächsten Tag auf dem Tisch.
- Übergabe 06.11. (martin → max): Martin hat die drei Tage im Dev-Modus durchgespielt. **Offen ist nur noch die Abnahme mit dem
  Kontroll-Gameplay (max):** drei Tage hintereinander, jede Schicht endet, weil alle Reisenden abgefertigt sind (nicht über Dev_end_Day),
  dann Feierabend → Bett → nächster Tag, nach Tag 3 das Platzhalter-Ende. Dabei prüfen, dass der Tisch morgens leer ist (B05).
  Klappt das, setzt max P09 auf `fertig`.

### P10 — Geld, Familie & Abrechnung
- Größe: M · Priorität: Muss · Status: fertig (martin, 06.11.)
- Braucht: P09, P46 · **erst bauen, wenn P46 fertig ist** (Grundstruktur für Geld und Inventar)
- Ziel: Abrechnungsbildschirm am Tagesende wie ein Haushaltsbuch.
- Enthalten: Lohn, Miete, Familienbedarfe (Essen, Wärme, Medizin) zum Ankreuzen, Zustand der Familienmitglieder,
  Folgen bei Nichtbezahlen, Werte in einem ScriptableObject
- Nicht enthalten: Pakete schicken (P13)
- Abnahme: Geld fließt über 3 Tage korrekt, Familie reagiert auf Nichtbezahlen.
- Ergebnis (martin, 06.11.): von Martin im Spiel abgenommen ("läuft perfekt").
  - **Abrechnung** nach „SLEEP“, vor der Abblende (`SettlementWindow`, in `PF_Economy`, gebaut von `EconomyBuilder`): Lohn pro
    abgefertigtem Reisenden, Abzug je Verwarnzettel über den freien (`CounterShift.FreeWarnings`), Miete (immer), Essen / Heizung /
    Medizin zum Anklicken (nur, was bezahlbar ist; Medizin nur, wenn jemand krank ist), Familie vorher → nachher, Kontostand vorher → nachher.
  - **Familie** (`Household`, reine Regeln): Mom, Lena, Pip (wie in den Platzhalter-Briefen). Stufen well → weak → sick → critical.
    Ohne Essen alle +1, ohne Heizung die Kälteempfindlichen (Pip) +1, Medizin heilt Kranke um 1, Essen + Heizung lassen Geschwächte erholen.
    Folgen stehen im **Morgenbrief** ab Tag 2 (eine Zeile pro Person).
  - **Werte** (vorläufig) in `Data/Economy/Household.asset`: Lohn 15/Reisender, Abzug 10, Miete 30. Bedarfe seit P13: Geld am Bett = Händlerwert × 1,35 → Essen 11, Heizung 15, Medizin 20.
  - **Testgeld:** Optionen → DEV → CREDITS −100 / +100 (nur im Spiel, im Kontobuch als „Dev“, Kontostand daneben). Fällt mit dem DEV-Reiter vor der Abgabe weg.
  - Für andere Pakete: `DayManager.NightStep` und `DayManager.LetterAddendum` (Haken für die Nacht und den Brief),
    `PlayerEconomy.Household`, `PlayerEconomy.FamilyReport()`, `Payroll`.
  - **Für P15 (Inhalte):** Die Platzhalter-Briefe erzählen Pips Krankheit fest („Pip is sick now“), die Familie hat aber jetzt einen
    echten Zustand. Briefe sollten auf den Zustand reagieren statt ihn vorzugeben.
  - Geprüft: 82/82 EditMode-Tests (neu: `SettlementTests`), Play-Modus Main: Tag 1 mit 6 Reisenden → +90, −30 Miete; ohne Essen →
    alle „weak“ im Brief von Tag 2, Kontostand 85; Tag 2 → 3 läuft. Kein WebGL-Build.

### P11 — Läden: Gadget-Shop & Bar-Schwarzmarkt
- Größe: M · Priorität: Muss · Status: teilweise (martin, 06.11.): Händlervorrat (B04) fertig, Rest wartet auf das P08-Gespräch
- Braucht: P03, P08, P46 · **erst bauen, wenn P46 fertig ist** (Grundstruktur für Geld und Inventar)
- Ziel: Kaufen und Verkaufen über ein gemeinsames Shop-System.
- Enthalten: Shop-UI, Bar (illegale Gegenstände kaufen, Beschlagnahmtes verkaufen), Dokumentenscanner kaufbar
- Nicht enthalten: Gadget-Funktion (P16)
- Abnahme: Scanner kaufen, beschlagnahmtes Item an der Bar verkaufen, Geld stimmt.
- Stand 06.11. (martin): **Händlervorrat (B04) gebaut**, der Rest wartet auf das P08-Gespräch (Beschlagnahme, Schwarzmarkt).
  - `VendorStock` hat jetzt Einträge mit Tagesmenge (`StockEntry`), die alten Listen hat `EconomyBuilder` umgestellt.
  - `VendorSupply` (pro Händler in `PlayerEconomy.Supply`): Kauf −1, bei 0 „Sold out. More tomorrow morning.“, jeden Morgen auf die
    Tagesmenge aufgefüllt; was man dem Händler verkauft, liegt danach bei ihm im Regal. Anzahl steht im Handelsfenster („×3“).
  - Tagesmengen (vorläufig): Scanner 1, Ration 3, Schmerzmittel 2, Cola 5, Spice 3.
  - Geprüft: Cola ausverkauft nach 5 Käufen, am nächsten Morgen wieder 5.
- **Hinweis vor Beginn (martin, 05.11.):** Das Shop-UI ist mit **P46** schon fertig (Handelsfenster, Kaufen/Verkaufen, Faktoren, abgelehnte
  Labels). **Noch offen und zuerst zu lösen:** Die Ware beim Händler **wird beim Kauf nicht weniger** – das Sortiment ist in P46 bewusst
  unbegrenzt (Bug-Liste **B04**). Für P11 vorgesehen (Zuschnitt im Kern-Loop-Call, siehe Frage an max):
  - **Händler-Inventar:** begrenzter Vorrat pro Gegenstand, sinkt beim Kauf, füllt sich morgens auf; Verkauftes landet beim Händler
  - **Preisliste:** echte Gegenstände und Preise für Holly, Barkeeper und Dealer statt der Test-Inhalte (evtl. Preisfaktor pro Tag)
    → jetzt **P70** (Preisliste & Sortiment) und **P68** (Markt: Preise und Ereignisse pro Tag), Durchrechnen in **P71**
  - **Angebote:** Tagesangebote/Rabatte, Ware nur an bestimmten Tagen, Sortiment pro Tag (z. B. Dealer an Tag 3 anders)
  - evtl. Teilung: **P11 Kiosk** (Muss) + **Schwarzmarkt** als eigenes Soll-Paket
  - **Nachtrag (martin, 06.11.):** Schmuggelware kauft nur der Schwarzmarkt (P64). Der Barkeeper soll nichts kaufen, sondern Wünsche gegen Gerüchte tauschen (P65, Entwurf) und manchmal Bestechungszettel schicken (P66, Entwurf).

### P12 — Enden
- Größe: S · Priorität: Muss · Status: offen
- Hinweis (Call 07.11.): neue Enden: **Game Over nach 3 Disziplinarstrafen** (P08) und **„Airlocked“** – wer dem Shifter bei seiner Schlüsselentscheidung hilft, fliegt an Tag 7 durch die Schleuse (`docs/FIGUREN.md`). Hauptende ist das Ticket (P72).
- Braucht: P10, P08
- Ziel: Nach Tag 3 oder bei Niederlage ein Endbildschirm.
- Enthalten: Niederlage (Familie zerbrochen / gefeuert), mindestens 2 Enden je nach Moral & Familie
- Abnahme: Jedes Ende lässt sich gezielt erspielen.
- Ergänzung (martin, 06.11.): Die Enden lesen den echten Familienzustand aus P10 (`PlayerEconomy.Household`, Stufen well/weak/sick/critical
  für Mom, Lena, Pip), z. B. alle gesund / jemand krank / jemand kritisch am Ende von Tag 3. „Familie zerbrochen“ als Niederlage
  kommt aus den Regeln von **P69**. Dazu Disziplinarstrafen (3 = Game Over, Vorschlag an Max) und Moral (P08).
  Schon da (Platzhalter, P13): **Miete nicht bezahlbar → „STRANDED“**; P12 ersetzt den Text durch ein echtes Ende. Haken: `DayManager.GameOver(title, body)`.
- Angepasst (martin, 07.11., nach dem Call): Enthalten sind jetzt auch **„Discharged“** (3 Disziplinarstrafen aus P08, Zähler über alle Tage) und
  **„Airlocked“** (Shifter-Schlüsselentscheidung, P75/P76). Abnahme: beide Enden lassen sich mit Dev-Knöpfen gezielt auslösen.

### P13 — Hangar: Pakete an die Familie
- Größe: S · Priorität: Soll · Status: fertig (martin, 06.11.)
- Braucht: P10, P11, P46 · **erst bauen, wenn P46 fertig ist** (Grundstruktur für Geld und Inventar)
- Ziel: Gegenstände aus dem Inventar per Frachtdrohne nach Hause schicken.
- Offen (Max, 06.11., P58): Die Drohne hat keine Öffnungszeit, die Händler öffnen nur nach der Schicht. Beim Bau entscheiden.
- **Festgelegt (martin, 06.11., DECISIONS.md):** Die Familie fordert Gegenstände (Essen, Wärme, Medizin).
  - Was vor dem Schlafengehen an der Drohne abgeliefert wurde, ist in der Abrechnung (P10) schon **abgehakt** („covered by drone“, kostet nichts).
  - Die Waren gibt es **billiger beim Händler** (oder beschlagnahmt am Schalter, P08).
  - Sonst schickt man beim Schlafengehen **Geld**: Preis ≈ **Händlerpreis × 1,35** (35 % teurer).
  - **Platzhalterpreise** folgen dieser Regel, z. B. Ration 8 CR beim Händler → Essen 11 CR am Bett; Schmerzmittel 15 → Medizin 20.
    Für Wärme braucht es eine Ware (z. B. Heizpatrone mit Label `HEAT` oder `TECH`, ca. 11 CR → 15 CR am Bett).
  - Die Kosten am Bett sollen sich aus dem Händlerpreis ableiten (Faktor 1,35 im `HouseholdTuning`), nicht von Hand gepflegt werden.
  - Marktverfügbarkeit (was es wann zu welchem Preis gibt) plant **P68**.
- Enthalten: Versand-UI im Hangar, Wirkung auf Familienbedarf, Risiko bei Schmuggelware
- Abnahme: Ein Medikament schicken senkt den Medizinbedarf am Abend.
- Stand 06.11. (martin):
  - **Drohne** (`DroneTerminal`): nur nach der Schicht offen, sonst „The next drone leaves after your shift.“ (Dev-Knopf `Dev_force_Send`).
    Was man schickt, landet in `PlayerEconomy.Delivery` (`HomeDelivery`).
  - **Abrechnung**: Ein geschickter Gegenstand mit dem Label des Bedarfs deckt ihn ab (grün „by drone (Ration pack)“, kostet nichts).
    Sonst Geld am Bett = Wert des Referenz-Gegenstands × 1,35 (`HouseholdTuning.moneyFactor`): Essen 11 (Ration 8), Wärme 15
    (Fusion Cell 11), Medizin 20 (Schmerzmittel 15). Lieferungen werden nach CONFIRM geleert. Gesperrte Zeilen sind jetzt dunkel.
    **Nichts ist vorausgewählt** (Martin, 06.11.): Der Spieler klickt selbst an, was die Familie bekommt; nur Drohnen-Lieferungen sind schon abgehakt.
    **RETURN** (oder Esc) links neben CONFIRM verlässt die Abrechnung ohne Buchung, der Abend geht weiter (`DayManager.CancelNight`).
  - **Miete nicht bezahlbar = Game Over (Platzhalter):** Reicht Kontostand + Lohn − Abzug nicht für die Miete, zeigt die Abrechnung das rot
    („You can't pay the rent“). RETURN → noch etwas verkaufen; CONFIRM → Abblende, „STRANDED – You lost everything and are stranded in
    space.“, einziger Knopf RETURN TO MENU (`DayManager.GameOver`, `DayFlowUI.ShowEnd(title, body, button)`). Echte Enden: P12.
  - **Neu:** Label `POWER`, Item `Fusion Cell` (11 CR, im Kiosk, 2 pro Tag). Bedarf → Label: Essen FOOD, Wärme POWER, Medizin MEDICINE.
  - Schmuggelware und Wertsachen gehen ohne Wirkung nach Hause (Risiko kommt mit P08).
  - Geprüft: 84/84 EditMode-Tests; Play-Modus: Drohne in der Schicht zu, abends Ration abgeliefert → FOOD in der Abrechnung abgehakt.

### P14 — Reisende laufen
- Größe: M · Priorität: Soll · Status: teilweise (max, 08.11.): in der Prüfkammer durch P91 erledigt, Wege durch den Hangar offen
- Braucht: P04, P07
- Ziel: Reisende kommen aus dem Hangar zum Schalter und gehen danach weg (NavMesh).
- Abnahme: Eine Schicht läuft mit sichtbar ankommenden Reisenden.

### P15 — Inhalte: Fälle für 3 Tage
- Größe: M · Priorität: Muss · Status: fertig (max, 08.11.; Anspielen durch Max steht aus) · Zuständig: max (Call 07.11.)
- Hinweis (Call 07.11.): Umfang jetzt **12 Story-Tage** (von hinten kürzbar), normale Reisende neben den Figuren aus P76 (max. 3 Begegnungen/Tag, mit Martin pro Tag abgleichen). Ein fester Reisender wird Nells Sohn, falls der Spieler niemanden verhaftet hat.
- Braucht: P07
- Ziel: Alle Reisenden, Regeln, Briefe und Texte für 3 Tage (~15 Fälle, Englisch).
- Regel (Max, 06.11., P58): Reisende mit gefälschtem Siegel erst an Tagen, an denen die UV-Lampe verfügbar ist; der Validator soll das prüfen.
- Abnahme: Kompletter Durchlauf 10–15 Minuten ohne Platzhaltertexte.
- Stand Etappe 1 (max, 08.11.): Tage 1–4 gefüllt (`Scripts/Editor/StoryDayContent.cs`, Reisende in `Data/Travelers/Normal/`; `Build Story Characters` baut sie). Regeln wachsen von Tag zu Tag, Tag 1 nur ORVAN. Briefe mit zweiter Fassung bei kranker Familie. Geprüft: Validate Travelers ok, 150/150 EditMode-Tests, Economy Report unverändert (alle ✓), WebGL Tag 1 gestartet. **Offen:** Max spielt 1–4 an; Tage 5–12 (Etappe 2, dort auch Kael Dross von der Liste nehmen); Figuren an Tag 1–4 haben noch keine eigenen Feld-Antworten (Martin baut die Figuren).
- Stand Etappe 2 (max, 08.11.): Tage 5–12 gefüllt, 37 weitere Reisende, alle Briefe in zwei Fassungen, Regeln je Tag; Tag 9 Inspection week mit allen Regeln. Fahndungen sind jetzt je Tag eigene (Kael Dross nur Tag 3). Geprüft: Validate Travelers, 155/155 Tests, Economy Report unverändert (alle ✓), je Tag 6 Begegnungen auf dem vorgesehenen Pfad. Nicht enthalten: gefälschte Siegel (P16/P62 fehlen), eigene Figuren-Antworten (P98). Die Platzhalter `Filler_*` in `StoryCharacterBuilder` sind jetzt unbenutzt.
- Ergänzung (martin, 06.11.): **Briefe reagieren auf den Familienzustand**, statt ihn vorzugeben (der Platzhalter von Tag 3 sagt fest
  „Pip is sick now“, auch wenn Pip gesund ist). Je Tag Brieftext pro Zustand oder Bausteine je Person; Haken ist
  `DayManager.LetterAddendum` bzw. `PlayerEconomy.FamilyReport()` (heute eine Zeile pro Person unter dem Brief).

### P16 — Gadget: Dokumentenscanner
- Größe: S · Priorität: Muss · Status: offen
- Braucht: P06, P11
- Ziel: Gekaufter Scanner markiert gefälschte Felder auf Dokumenten.
- Assets: `SM_Item_DocScanner` (bestellt am 06.11.)
- Abnahme: Mit Scanner wird ein gefälschtes Feld sichtbar markiert.
- Hinweis (martin, 07.11., P68): Der Scanner ist bis dahin **nicht im Kiosk-Sortiment** (sonst kauft man 60 CR ohne Nutzen). Mit P16 in `EconomyBuilder.BuildPriceList` wieder per `AddEntry(kiosk, scanner, 1)` aufnehmen.

### P17 — Retro-Look
- Größe: S · Priorität: Soll · Status: fertig (max, 02.11.)
- Braucht: P01 · Hängt am Art-Style-Beschluss
- Ziel: Low-Res-Rendering per URP Renderer Feature, UI in voller Auflösung darüber.
- Abnahme: Einstellbare Render-Auflösung, Dokumente bleiben scharf, WebGL-Build flüssig.
- Ergebnis: Komponente `Scripts/Core/RetroLook.cs` an `PF_Player/PlayerCamera`: Die Spielerkamera rendert in eine Point-gefilterte
  RenderTexture (Höhe aus `Data/Rendering/RetroLookSettings.asset`, Standard 360 px, Breite passend zum Seitenverhältnis), die
  bildschirmfüllend hinter aller UI liegt → Welt pixelig, Screen-Space-UI scharf. Ein-/Ausschalten zur Laufzeit: `RetroLook.RetroEnabled`
  (für das Optionsmenü P02). Fasst keine Projekt-Assets an. Wichtig: Maus → Strahl immer über Viewport-Koordinaten rechnen
  (siehe `CounterStation.TryClick`), nicht `ScreenPointToRay`. Build läuft im Browser (max, 02.11.). **Auf Wunsch von Max per Standard AUS** (`enabledByDefault = false`) — wie der
  Retro-Look final aussehen soll, wird entschieden, wenn die Assets drin sind (dann ggf. Folgepaket).

### P18 — Audio-Grundgerüst
- Größe: S · Priorität: Soll · Status: fertig (martin, 05.11.)
- Braucht: P01, P43 (die Regler Music/Effects/Voice kommen in den Reiter AUDIO aus P43)
- Ziel: AudioMixer (Musik/SFX/Voice), einfacher Weg, Sounds abzuspielen, Lautstärke aus dem Menü (P02).
- Abnahme: Lautstärkeregler wirken getrennt.
- Ergebnis (martin, 05.11.):
  - **Mixer** `Audio/GameMixer.mixer`: Master → Music / SFX / Voice, freigegebene Lautstärken `MasterVolume`, `MusicVolume`,
    `SfxVolume`, `VoiceVolume`. Im Web-Build kann Unity am Mixer **nur die Gruppenlautstärke** ändern, keine Effekte (Unity-Doku 6000.3).
  - **`Scripts/Audio/AudioManager.cs`** + `Prefabs/Audio/PF_AudioManager.prefab` (in Boot, Menu und Main, es läuft immer genau einer;
    Main lässt sich direkt starten). Abspielen ohne Referenz: `AudioManager.PlayMusic(clip)` (Schleife, Überblendung),
    `PlaySfx(clip)` (2D), `PlaySfxAt(clip, position)` (3D), `PlayVoice(clip)`. Gruppen für eigene AudioSources: `AudioManager.SfxGroup` usw.
  - **AudioSources ohne Mixer-Gruppe** (z. B. die Türen aus dem Stations-Builder) hängt der AudioManager bei jedem Szenenwechsel an SFX.
    Neue Quellen besser direkt einer Gruppe zuweisen.
  - **Optionen** (Reiter AUDIO aus P43): Master, Music, Effects, Voice in `GameSettings` (0..1, AudioManager rechnet in dB um).
    Beim Loslassen eines Reglers spielt ein im Code erzeugter Vorschau-Ton in dieser Gruppe (`SliderRelease`), keine Assets nötig.
  - Pause: Spielgeräusche pausieren (`AudioListener.pause`), Musik, UI-Klicks (`Sfx2D`) und Vorschau laufen weiter.
  - Im Browser startet Ton erst nach dem ersten Klick/Tastendruck (Browser-Regel) – der Splash fängt das ab.
  - Getestet im Editor (Mixer-Werte, Routing aller 10 Türen) und im WebGL-Build mit Ton in Chrome. Echte Sounds fehlen noch (Max/Pipeline).

### P19 — WebGL-Build & itch-Upload
- Größe: S · Priorität: Muss · Status: offen, auf später verschoben (martin, 06.11.)
- Braucht: P01
- Ziel: Erster Build auf itch.io (Entwurf) bis 05.11., danach wiederholbarer Ablauf.
- Hinweis (max, 05.11.): `GameOff.EditorTools.WebBuild.Build` ist der Build-Einstieg. Die Vorlage für itch muss abgelehnte Maussperren abfangen und 16:9 halten wie `Assets/WebGLTemplates/Kestrel9/index.html` (Ausnahme: Impressum-Fußzeile nur für die Live-Version), siehe P49.
- Abnahme: Build läuft auf itch.io in Chrome und Firefox, Ablauf ist in `docs/` beschrieben.

### P20 — Licht & Stimmung der Station
- Größe: M · Priorität: Soll · Status: fertig (max, 02.11.)
- Braucht: P04 · Folgt auf: P04 (Licht-Feinschliff war dort ausgeklammert)
- Ziel: Die Graybox fühlt sich nach der Station aus `STYLE.md` an: kaltes Neon in Gängen, warmes Licht in Bar/Diner, eine Leuchtfarbe pro Raum.
- Enthalten: Lichter pro Raum (mehrere, Raumfarbe), Light Probes, Leuchtstreifen-Platzhalter mit Emission, gebackenes Licht (niedrige Lightmap-Auflösung,
  passt zum Retro-Look), Nebel, dunkles Ambient, Weltraum-Skybox mit Platzhalter `T_Sky_Space.png`, flackernde Neonröhre (Skript),
  rotes Warnlicht in der Prüfkammer, Licht-Setup reproduzierbar per Editor-Menü
- Nicht enthalten: finale Modelle/Texturen (Pipeline), Post-Processing
- Abnahme: Jeder Raum hat seine Leuchtfarbe, Stimmung entspricht `STYLE.md`, WebGL-Build läuft, Licht lässt sich nach Graybox-Änderung per Menü neu backen.
- Ergebnis: siehe `docs/STATION.md` → "Licht & Stimmung". Light Probes doch enthalten (Schalter-Knöpfe waren sonst schwarz). Von Max im Browser abgenommen (02.11.).

### P21 — Station mit dem Bausatz bauen (Teil 1)
- Größe: M · Priorität: Muss · Status: fertig (max, 02.11.)
- Braucht: P04, P20 · Folgt auf: P04
- Ziel: Die Station zeigt die gelieferten Modelle statt Grau-Blöcken, wo es sie schon gibt.
- Enthalten: Builder setzt Boden-, Decken-, Wand- und Türwand-Module im 2-m-Raster; Schalter-Möbel (Tisch, Stuhl, Spind, Pinnwand,
  Durchreiche), Bar (Tresen, Hocker). Kollision/NavMesh bleiben die unsichtbaren Graybox-Blöcke. Wo Module fehlen (Fenster,
  schmale Türen, Wandvarianten, Restflächen), bleibt sichtbare Graybox. Lightmap-UVs für Modelle, neu backen.
- Nicht enthalten: weitere Assets (kommen automatisch dazu, wenn der Builder erneut läuft), Teil 2 nach den nächsten Lieferungen.
- Abnahme: Station sieht in allen Räumen nach Bausatz aus, wo Module existieren; Wege/NavMesh unverändert; Licht gebacken; keine Konsolenfehler.
- Ergebnis: Builder hat jetzt zwei Ebenen: unsichtbare Graybox-Kollision (Wege/NavMesh identisch, nachgemessen) und `Visual/<Raum>` mit
  Modulen (566 Modul-Renderer) bzw. sichtbarer Graybox-Füllung, wo noch Module fehlen (148 Stücke: Fenster, schmale Türen, 2-m-Durchgang,
  Kiosk-Durchreiche, oberster Meter der Bar, Hangartor). Schalter mit echtem Tisch/Stuhl/Spind/Pinnwand/Durchreiche, Bar mit Tresen + Hockern.
  Neue Lieferungen: nur *Build Station Graybox* + *Setup Main Lighting + Bake* ausführen — vorhandene Modelle werden automatisch genutzt.
  Fehlt noch für den vollen Look: Fenster, schmale Tür, Wandvarianten (Polster/Holz), Schiebetüren, Möbel der übrigen Räume.

### P22 — Grundriss 2.0: großer Hangar, Aussichtsplattform, Barfenster
- Größe: M · Priorität: Soll · Status: fertig (max, 02.11.)
- Braucht: P21 · Folgt auf: P04, P21
- Ziel: Station nach dem erweiterten Grundriss (`docs/STATION.md`, Stand 2.0).
- Enthalten: Hangar 36 × 28 × 16 m; halbrunde Aussichtsplattform (r = 5 m, 2 Stufen tiefer, 8 Glassegmente) nördlich der Halle;
  Barfenster Nordwand; Builder-Unterstützung für Halbkreis, Stufen, Hangar-Module 4 × 4 m und Fenster-Wandmodule; Licht/NavMesh neu
- Nicht enthalten: Energiebarriere (P23), Außenwelt/Himmel (P24), Schiffe (P25), Detail-Props (P26)
- Abnahme: Alle Räume inkl. Plattform begehbar, Stufen ohne Hängenbleiben, Wege < 15 s, Licht gebacken.
- Ergebnis: Builder kann Halbkreis-Plattform (eigenes ProBuilder-Mesh für Boden/Decke, Stufe, 8 Glassegmente mit Brüstung),
  Hangar-Modulsatz 4 × 4 m (sobald geliefert) und Fenster-Wandmodule. Hangar 36 × 28 × 16 m, Barfenster x 10..18. Alle Wege
  komplett (NavMesh), weitester Weg Schalter → Hangar-Süd 11 s; Spieler läuft im Play-Modus ohne Hängenbleiben auf die Plattform.
  Bauplan `docs/station/bauplan.html` und `docs/STATION.md` auf Stand 2.0. Barriere ist noch Platzhalter (P23).

### P23 — Energiebarriere am Hangartor
- Größe: S · Priorität: Soll · Status: fertig (max, 02.11.)
- Braucht: P22
- Ziel: Sichtbare, lebendige Energiebarriere statt unsichtbarem Kraftfeld.
- Enthalten: Shader (halbtransparent blau, wandernde Scanlinien, Randglühen, Rauschen), Emitter-Rahmen (`SM_Hangar_BarrierEmitter`),
  Flacker-Effekt per Methode `Pulse()` (z. B. wenn ein Schiff durchfliegt), Platz für Brumm-Sound (Audio kommt mit P18)
- Abnahme: Barriere im Spiel und im WebGL-Build sichtbar animiert, Spieler kommt nicht hindurch.
- Ergebnis: Shader `Art/Shaders/EnergyBarrier.shader` (URP, unlit, additiv, doppelseitig: Schimmer, aufsteigende Scanlinien, Randglühen,
  Puls), Material `Art/Materials/Hangar/M_Hangar_EnergyBarrier.mat` (Farben/Tempo im Inspector einstellbar), Komponente
  `Scripts/Core/EnergyBarrier.cs` mit `Pulse(stärke)` + leichtem Leerlauf-Flackern. Builder setzt Feld 28 × 12 m + unsichtbaren Blocker +
  Emitter-Rahmen (Graybox mit Leuchtlinse, später `SM_Hangar_BarrierEmitter`). Getestet: Spieler bleibt stehen, Puls klingt in ~1 s ab.
  WebGL-Build mit Barriere noch nicht im Browser angesehen.

### P24 — Himmel & Außenwelt
- Größe: M · Priorität: Soll · Status: fertig (max, 02.11.)
- Braucht: P22
- Ziel: Beeindruckender Blick nach draußen.
- Enthalten: Sterne/Nebel-Skybox, Sonne (Billboard + Lichtrichtung), Gasriese mit Ring, Heimatplanet mit Nachtlichtern, Mond — als ferne
  Objekte unter einem `SkyRig`; `SkyRig.SetDay(1..3)` dreht Sonne/Planeten pro Tag (Theme "Shift"); Außenmodule der Station
  (Rumpf, Träger, Solarflügel, Antennen, Andockarm, Hangar-Einfassung, Plattform-Kuppel) passend zu den Fenstern platziert
- Abnahme: Aus Plattform, Bar, Halle und Hangar sieht man Station, Sonne und Planeten; `SetDay` verändert sichtbar die Szene.
- Ergebnis: siehe `docs/STATION.md` → "Himmel & Außenwelt". Menü *GameOff → Build Sky & Exterior*; `SkyRig.SetDay(1..3)` dreht den Himmel
  (0/18/36°) und senkt die Sonne (Echtzeit-Licht mit Schatten durch die Fenster). Alles mit Platzhaltern (prozedurale Planeten-Texturen
  am finalen Pfad, graue Außenmodule) — Lieferungen der Pipeline werden beim nächsten Builder-Lauf übernommen. Im Editor geprüft
  (Plattform Tag 1 + 3, Hangar); im WebGL-Build noch nicht angesehen.

### P25 — Verkehr: vorbeifliegende Schiffe & Shuttle-Ankunft
- Größe: M · Priorität: Soll · Status: offen
- Braucht: P23, P24
- Ziel: Sichtbarer Schichtbeginn: das Shuttle mit den Reisenden.
- Enthalten: ~~Großschiffe auf festen Bahnen~~ → übernimmt **P55** (martin, 05.11.); Shuttle fliegt durch die
  Barriere ein/aus (`ShuttleTraffic.Arrive()/Depart()`, Barriere pulst), Schnittstelle für P09 (Tagesablauf ruft Arrive/Depart)
- Abnahme: Arrive/Depart per Testknopf im Editor sichtbar; Tagesablauf ruft Arrive/Depart.
- Hinweis (martin, 05.11.): Für die vorbeifliegenden Schiffe gibt es mit **P47** schon den Baustein `ShipTraffic` (Bahnen, Schiffsliste,
  Zufallsabstände, Obergrenze, Wiederverwenden, keine Lichter/Schatten) samt Tuning-Asset – für Main nur ein eigenes Tuning (seltener)
  und eine Instanz anlegen. Alle Schiffsmodelle (`Art/Models/Ships/`) sind geliefert.
- Festgelegt im Asset-Durchgang (max, 02.11.): Shuttle mit **Rampe** (Rework bestellt): landen → Rampe klappt zischend herunter → Reisende steigen nacheinander aus und stellen sich an; abends Rampe hoch und Abflug.

### P26 — Details & Ausstattung
- Größe: M · Priorität: Soll · Status: offen, Deko eingebaut (max, 06.11.), Bewegung und Schilder fehlen
- Braucht: P22 und Lieferungen
- Ziel: Station wirkt bewohnt und heruntergekommen.
- Enthalten: Rohre, Kabel, Lüftungen, Verteilerkästen, Feuerlöscher, Müll, Pflanzen, Schilder/Poster/Schmutz-Decals, Putzroboter (fährt per
  Skript), Leuchtschilder, Ventilatoren, Uhr, Lautsprecher, Münzfernrohr und Bänke auf der Plattform, Gerüst und Kran im Hangar
- Abnahme: Jeder Raum hat sichtbare Details; Draw Calls/Build-Größe im WebGL-Rahmen (Build < 40 MB).
- Stand (max, 06.11.): Alle gelieferten `SM_Prop_*`, `SM_Motel_TV` und `SM_Motel_DoorSign` stehen im Spiel, 42 Stücke, im Builder unter `BuildDecor`. Halle, Schalter, Gänge, Bar, Diner, Kiosk, Hangar, Plattform und Motel; im Editor je Raum angesehen.
  - Offen: Putzroboter und Ventilatoren bewegen sich nicht und sind noch statisch gebaut (Flags, Lichtbake und Occlusion müssen dafür mit angefasst werden), Münzfernrohr ohne Funktion (P29), Poster und Schmutz-Decals gibt es noch nicht.
  - Nachtrag aus der Code-Prüfung: Die Türschilder zeigten alle 101, jetzt je eigene Atlaszeile über `M_Motel_DoorSign_102/_103`. Feuerlöscher und Verteilerkasten im Hangar hingen vor dem Fenster, jetzt auf der Wand. Hinter Wand-Deko stehen keine Detail-Wandmodule mehr. Eimer, Kabel und Pflanze ragen nicht mehr in die Wand.
  - Download 17,7 statt 17,3 MB, Speicher 82 statt 80 MB (zusammen mit P51). Bildrate im Wechsel, CPU ×4, Median aus drei Paaren: Schalter 28,1 gegen 28,6 fps, Zimmer 48,3 gegen 44,3, Menü 23,5 gegen 20,2, Pinnwand 36,9 gegen 43,7, Stehen 32,0 gegen 38,9. Die letzten beiden liegen niedriger, die Einzelläufe überlappen aber (30–45 fps), ein echter Verlust ist nicht belegt und nicht ausgeschlossen.
  - `OcclusionCheck` findet 132 statt 133 begehbare Punkte: Ein Bodenobjekt steht auf einem Rasterpunkt, alle anderen Punkte sind weiter erreichbar. Der Weg zu Fuß ist damit im Editor geprüft, nicht mit echten Tasten.

> **P27–P36: Mechaniken aus dem Asset-Durchgang** (max, 02.11.). Max hat jedes gelieferte Asset durchgesehen und festgelegt,
> was man damit tun kann. Alle Soll; Reihenfolge und Verteilung im Call.

### P27 — Automatische Türen mit Zugangsstufen
- Größe: S · Priorität: Soll · Status: fertig (max, 02.11.)
- Braucht: P22, P03 · Assets: `SM_Station_Door_Slide`, `_Narrow` (da), `SM_Station_Wall_Door_2x3_Narrow` (bestellt)
- Ziel: Schiebetüren öffnen sich zischend beim Näherkommen.
- Enthalten: Türkomponente (Trigger, Gleiten in die Wandnut, Sounds-Platzhalter), Zugangsstufen: Personaltüren nur für den Spieler,
  Zimmer 101/102 verriegelt (rotes Lämpchen, "LOCKED"-Ton), Reisende (NavMesh) öffnen Publikumstüren; Builder setzt Türen in alle Türöffnungen
- Abnahme: Alle Türen öffnen/schließen, verriegelte bleiben zu, NavMesh-Wege funktionieren weiter.
- Ergebnis: `Scripts/Station/SlidingDoor.cs` + `DoorAccess` (Public / PlayerOnly / Locked / Scripted). Builder setzt 10 Türen (Tabelle `Doors`
  im Builder): Diner, Bar öffentlich; Zimmer 103 + 3 Personaltüren nur Spieler; 101/102 verriegelt (rotes Lämpchen); Prüfkammer-Türen
  per Skript (`Open()`/`Close()` für P08). Türblatt gleitet in die Wand, Sensor-Trigger beidseitig, NavMesh-Hindernisse halten Reisende
  aus Personal-/verriegelten Türen und aus geschlossenen Prüfkammertüren. Sounds per Feld (Aufträge `SFX_Door_*`).
  Getestet: Zimmer 103 raus, 101 bleibt zu, Bar rein, Prüfkammertür bleibt zu bis `Open()`. Schmale Türen noch in Graybox-Wand.

### P28 — Sitzplätze & Ruhemomente
- Größe: M · Priorität: Soll · Status: offen
- Braucht: P05 (Sitz-Logik), P09 (Zeit vorspulen)
- Ziel: Man kann sich hinsetzen und die Station auf sich wirken lassen.
- Enthalten: Wartebänke in der Halle: [E] hinsetzen (Kamera tiefer, Blick zur Plattform), Durchsagen hören, im Feierabend Zeit bis zum
  Schlafen vorspulen; Barhocker: hinsetzen öffnet das Barkeeper-Gespräch (Drink kaufen, Gerüchte, Schwarzmarkt-Einstieg → P11);
  Bullauge Zimmer 103: [E] vor dem Schlafen Kamerafahrt ans Fenster, Blick auf den Heimatplaneten, kurzer Gedanke je nach Familienzustand
- Abnahme: Alle drei Sitz-/Blickpunkte funktionieren, Q/ESC beendet sie sauber.

### P29 — Münzfernrohr & sichtbares Familienschicksal
- Größe: M · Priorität: Soll · Status: offen
- Braucht: P24, P10 (Familienzustand) · Assets: `SM_Prop_CoinTelescope`, `UI_Telescope_Overlay`, `UI_Family_Photo` (bestellt)
- Ziel: Die Familie ist in der Welt sichtbar, nicht nur in Briefen.
- Enthalten: Fernrohr: 1 Credit, Okular-Overlay, Maus schwenkt, Mausrad zoomt, 20 s Zeit; Heimatstadt auf dem Planeten markiert, ihre
  Lichter flackern/erlöschen je nach Familienzustand (auch von Plattform und Bullauge sichtbar); Familienfoto im Motelspind verblasst/reißt;
  Morgenbrief liegt physisch auf dem Tisch in 103, Handschrift wird zittriger
- Abnahme: Familienzustand ändert sichtbar Stadtlichter, Foto und Brief.
- Hinweis (martin, 06.11.): Der Familienzustand existiert seit P10: `PlayerEconomy.Household` (Stufe je Person), Event `Household.Changed`.

### P30 — Getränkeautomat
- Größe: S · Priorität: Soll · Status: fertig (martin, 07.11.)
- Braucht: P10 (Geld), P11 (Inventar) · Assets: `SM_Hall_VendingMachine` (da), `SM_Prop_SodaCan` (bestellt)
- Ziel: Kleine Versuchung, Geld auszugeben.
- Enthalten: [E] "NOVA COLA – 2 Credits": Geld weg, Dose fällt rumpelnd heraus, ins Inventar; nach Hause schicken (P13) oder am Schalter
  trinken (kleiner Bonus, z. B. Schichtzeit kurz langsamer)
- Abnahme: Kaufen, Inventar, Wirkung und Versand funktionieren.
- Hinweis (martin, 06.11.): Grundregel – jeder Handel (Händler, Drohne, Automat) ist nur nach der Schicht möglich.
- Ergebnis (martin, 07.11.): `Shops/VendingMachine.cs`, gebaut von EconomyBuilder (Trigger am Modell, Sortiment `Stock_Vending`: Nova Cola, 6/Tag).
  [E] kauft sofort eine Dose zum Marktpreis (Ton + Hinweis), leer → „Sold out“, tagsüber Dialog „Out of order“ (Dev-Knopf `Dev_force_Buy`).
  **Trink-Bonus bewusst weggelassen** (Martin) → später eigenes Paket. Versand per Drohne geht wie bei jeder Ware. Economy Report ✓.

### P31 — Anzeigesäule mit Live-Ankunftsplan
- Größe: S · Priorität: Soll · Status: fertig (martin, 07.11.)
- Braucht: P09 · Assets: `SM_Hall_DepartureBoard` (da)
- Ziel: Auf einen Blick sehen, wie viel Schicht noch kommt.
- Enthalten: TextMeshPro auf den CRT-Flächen: nächstes Shuttle, Reisende in der Schlange, Tag/Schicht-Uhr, Warnungen ("SECURITY LEVEL RAISED")
- Abnahme: Anzeige folgt dem Spielstand.
- Ergebnis (martin, 07.11.): `UI/HallBoard.cs`, gebaut von DayFlowBuilder (PF_DayFlow). Je Seite der Säule zwei CRTs mit dunklem Hintergrund:
  links Tag + Phase + nächstes Shuttle (fiktiv: 08:00 + 25 min je Entscheidung) + Dock, rechts die Marktereignisse des Tages (`bulletinLine`) oder „NONE TODAY“.
  Kopfzeile des Modells bleibt frei. Warteschlangen-Länge und Sicherheitswarnungen nicht enthalten (gibt es noch nicht als Daten).

### P32 — Asservatenschrank mit echten Gegenständen
- Größe: M · Priorität: Soll · Status: offen
- Braucht: P08 · Assets: `SM_Counter_Locker` (da), `SM_Item_*` (6, bestellt)
- Ziel: Beschlagnahmtes ist greifbar.
- Enthalten: [E] Türen schwingen auf, Gegenstände liegen sichtbar in den Fächern, Rausnehmen = Inventar (Bar verkaufen / nach Hause schicken)
- Abnahme: Verhaftung legt Gegenstände in den Schrank, man kann sie entnehmen.
- Hinweis (martin, 07.11., nach dem Call): Seit dem Call landet Beschlagnahmtes erst im Schicht-Inventar (P81); in den Schrank kommt es nur mit
  „As ordered“ (P08). Frei entnehmen widerspricht dem – P32 zeigt die Gegenstände, **herausholen nur durch Aufbrechen (P83)**.

### P33 — Pinnwand, Lautsprecher & Warnlampe
- Größe: M · Priorität: Soll · Status: offen
- Braucht: P07, P09 · Assets: `SM_Counter_Board`, `SM_Counter_Speaker`, `SM_Counter_WarningLamp` (da)
- Ziel: Der unsichtbare Vorgesetzte und die Regeln sind im Raum präsent.
- Hinweis (max, 05.11.): Zettel im Sitzen und Durchsagen macht schon P41; hier bleiben Nahansicht im Stehen, Fotos/Memos und die Lampen.
- Enthalten: Pinnwand [E] Nahansicht: Tagesregeln, Fahndungsfotos, Memos — jeden Morgen kommen Zettel dazu, alte bleiben; Lautsprecher-Lämpchen
  leuchtet bei Durchsagen (Lob, Rüge, neue Anweisung mitten in der Schicht, Text bis Voicelines kommen); Warnlampe dreht rot bei
  Verhaftung/Luftschleuse, gelb wenn ein Reisender eintritt
- Abnahme: Alle drei reagieren auf das Spielgeschehen.

### P34 — Terminal-Datenbank am Schalter
- Größe: M · Priorität: Soll · Status: offen
- Braucht: P07 · Assets: CRT im `SM_Counter_Desk`, `SM_Counter_CRTTerminal` (da)
- Ziel: Zweites Prüfwerkzeug neben den Dokumenten.
- Enthalten: Klick aufs Terminal → Name/ID anklicken → Abgleich mit Fahndungsliste und Einreiseakte (grüne Schrift); Statusanzeige zeigt
  Lane-Status und Uhr; später durch Gadgets erweiterbar
- Abnahme: Ein gesuchter Reisender lässt sich über das Terminal entlarven.

### P35 — Durchsuchbare Fracht im Hangar
- Größe: M · Priorität: Soll · Status: offen
- Braucht: P07, P08 · Assets: `SM_Hangar_Crate`, `_Small` (da), `SM_Item_*`
- Ziel: Gameplay im Hangar über die Drohne hinaus.
- Enthalten: markierte Kisten gehören zu Reisenden; [E] durchsuchen findet Beweise (Schmuggelware zu einem Fall) — oder man steckt etwas
  ein (Risiko, Moral-Wert)
- Abnahme: Ein Fall lässt sich über einen Kistenfund lösen; Einstecken verändert Moral.

### P36 — Lebendige Station: Flackern & Stromausfälle
- Größe: S · Priorität: Soll · Status: fertig (martin, 07.11.)
- Braucht: P09, P20
- Ziel: Der Zustand der Station verschiebt sich über die Tage (Theme "Shift").
- Enthalten: Neonschild "THE LAST ORBIT" flackert (ein Buchstabe fällt aus); an Tag 3 fallen Schilder und Lichter häufiger aus;
  `LightFlicker` wiederverwenden, Stärke pro Tag
- Abnahme: Tag 1 ruhig, Tag 3 sichtbar instabil.
- Ergebnis (martin, 07.11.): Stärke kommt aus der **Lage der Station** (Martin): `MarketEvent.crisis` (Grenze 0,4, Schmuggler 0,3, Kälte 0,3);
  `Day/StationMood` (in PF_DayFlow) rechnet jeden Morgen Krisen von heute + Hälfte von gestern, max. 1 → Tag 1: 0, Tag 2: 0,4, Tag 3: 0,8.
  Flacker-Lampen: Pause 15–30 s (ruhig) bis 1–3 s (Lage 1); Neonschilder (Bar-Schild, Neon_Bar/Diner/Kiosk inkl. Buchstabe E) setzen ab 0,2
  aus (Emission aus); ab 0,5 Stromausfälle alle 45–90 s für 2–4 s. Werte in `Data/Days/StationMoodTuning.asset`, Tests `StationMoodTests`.
  Grenze: Die Station hat nur 2 Echtzeit-Flackerlampen, der Rest ist gebacken – ein Stromausfall macht nicht die ganze Halle dunkel.

### P37 — Station-Fixes innen
- Größe: M · Priorität: Muss · Status: fertig (max, 04.11.)
- Braucht: — · Folgt auf: P21, P22, P27
- Ziel: Die Station sieht von innen stimmig aus, keine Clipping-, Flimmer- oder Türfehler mehr.
- Enthalten: Wandmodule nach echter Tiefe ausrichten (Lüftungen/Screens/Bögen clippen nicht), Wand-Props (Schilder, Boards,
  Bilder) auf der echten Wandfläche, keine Varianten hinter Wand-Props; Türen auf Zellmitten mit Türwand-Modulen (auch 1,2 m),
  Hangar-Türen mit Laibung, Türrahmen ohne Z-Fighting, Motel-Türen/Trigger repariert; Kiosk-Front (breitere Durchreiche, Sturz);
  gelieferte Modelle statt Platzhalter (Hangar, Motel, Kiosk-Regal, Kranschiene)
- Nicht enthalten: Außenhülle (P39), Schalter-Interaktion (P38)
- Abnahme: Durch jede Tür laufen, von beiden Seiten sauber; keine Lüftung/kein Schild in der Wand; keine grauen Platzhalter im Hangar.
- Ergebnis: Türen außerhalb des 2-m-Rasters bekommen eine eigene Zelle mit Türwand-Modul (auch 1,2 m schmal), Reststreifen als
  gestauchte Grundwand; Hangar-Unterreihe mit Türen/Fenster ist eine Stations-Fassade. Service↔Schalter-Tür nach Z −9 verlegt,
  Schalter→Hangar-Fenster auf X −6..−2 (zwei Fenster-Module), Bullauge 103 auf Z 5 (Bullaugen-Wand). Varianten nicht in Eckzellen und
  nicht hinter Wand-Props; Wand-Props sitzen mit der Rückseite auf der Wandfläche; Rundecken entfallen nur an Öffnungen des eigenen
  Raums (Motel-Bogen war dadurch zugestellt). Kiosk-Front mit Sturz, Brüstung und Pfosten. Alle Platzhalter ersetzt (Hangar, Motel,
  Kiosk-Regal, Kranschiene). Play-Test: Türen 103/Service öffnen, 101 bleibt zu.

### P38 — Schalter: Sessel, Aufstehen, Stempel
- Größe: M · Priorität: Muss · Status: fertig (max, 04.11.)
- Braucht: — · Folgt auf: P05
- Ziel: Am Schalter fühlt sich die Bedienung richtig an.
- Enthalten: Sessel leicht anwählbar, Spieler sitzt auf dem Sessel; Aufstehen mit Q/Esc/Rechtsklick; Stempel hochheben, mit der
  Maus führen und auf ein (Platzhalter-)Dokument stempeln, Abdruck bleibt sichtbar; Schnittstelle `StampTarget` für P06
- Nicht enthalten: echte Dokumente (P06)
- Abnahme: Hinsetzen auf Anhieb, Esc steht auf, Stempelabdruck an der geklickten Stelle.
- Ergebnis: Sitz-Trigger 0,1–1,8 m hoch (auf Augenhöhe anwählbar); Aufstehen mit Q/Esc/Rechtsklick, Spieler steht danach hinter
  dem Sessel (`StandPoint`), Steuerung kommt erst nach dem Kamera-Blend zurück. Stempel: Klick hebt an (`StampTool`), er schwebt über
  der Fläche unter der Maus, Klick aufs Blatt (`StampTarget`) drückt und setzt einen Abdruck (lesbar zur Kamera), Q/Esc/Rechtsklick
  oder Klick daneben legt zurück. Entscheidung weiter über `CounterStation.ActionTriggered`. Tuning in `Data/Counter/StampTuning.asset`,
  Abdruck-Texturen sind Platzhalter (Requests `T_Stamp_*`). **Für P06:** `StampTarget` auf die echten Dokumente (unskaliertes Objekt mit
  Collider, Optik als Kind).

### P39 — Außenstation 2.0
- Größe: L · Priorität: Soll · Status: fertig (max, 04.11.)
- Braucht: Lieferung der Hüll-Assets (M015) · Folgt auf: P24
- Ziel: Die Station ist von außen ein zusammenhängendes Bauwerk.
- Enthalten: Fachwerk verbindet Station, Habmodule, Solarflügel (auf Masten), Antenne (Sockel auf dem Hallendach), Radiatoren am
  Hangar, Andockarm zum Frachter; Außenhülle aus Platten um alle Räume (Platzhalter, bis die Pipeline liefert), Kanten, Rohre,
  Greebles; Fenster und Aussichtskuppel bleiben frei
- Abnahme: Rundflug außen: nichts schwebt, keine Innenwand-Rückseiten sichtbar.
- Ergebnis: Neues Layout im `SkyExteriorBuilder`: Knoten auf dem Hallendach → Strebe nach Norden → Zentralknoten → Streben West/Ost →
  Andockflansche → Habmodule (liegen N-S, Schriftzug zur Station gedreht); Solarflügel auf Masten auf den Knoten, Antenne mit Sockel auf
  dem Hallendach, Radiatoren am Hangar, Andockarm Ost-Hab → Frachter. Hülle (`StationGrayboxBuilder.BuildHull`) aus 4-m-Platten über
  allen Außenflächen inkl. Dächern und Unterseite, Öffnungen (Fenster, Hangarmaul, Deck, Außenluke) bleiben frei; Hangar mit Außenrippen,
  Dächer mit Technik-Kästen/Tanks/Rohren. Alles noch Graybox-Platzhalter in Hüllgrau: die Pipeline-Lieferungen aus M015 tauschen sich
  beim nächsten Builder-Lauf automatisch ein (Achsen/Pivots nach den Einträgen).

### P40 — Reisende in der Prüfkammer & Gespräch
- Größe: M · Priorität: Muss · Status: fertig (max, 05.11.)
- Braucht: P07 · Folgt auf: —
- Ziel: Man hat ein Gegenüber: der Reisende steht in der Prüfkammer am Fenster und redet.
- Enthalten: NPC-Modell (SK_NPC_*) erscheint am Fenster und geht nach der Entscheidung durch die richtige Tür ab (einfaches
  Ein-/Ausblenden, Laufen ist P14); Sprechblase über dem Reisenden mit Begrüßung; Alien-Spezies sprechen erst in Glyphen, die per
  „Entschlüsseln“-Welle (links nach rechts, ~0,4 s/Zeile, leichter Cyan-Schimmer) übersetzt werden; Nachfragen per Klick auf ein Feld
  des hochgehaltenen Dokuments (Antwort aus `TravelerData`)
- Abnahme: Jeder Test-Reisende steht in der Kammer, begrüßt, antwortet auf Feld-Klicks; Aliens werden lesbar „übersetzt“.
- Ergebnis (max, 05.11.):
  - **Figuren:** `GameOff/Build Travelers` baut `PF_NPC_<Spezies>` + `AC_NPC_<Spezies>` (Idle, Talk, Walk). `Travelers/TravelerPresenter` stellt sie auf die Standmarkierung; nach der Entscheidung gehen sie etwa 1 m zur Tür und verschwinden (durchlassen/verhaften Nordtür, abweisen Hangartür, Schleuse Außenschott).
  - **Sprechblase:** `Counter/SpeechBubble`, im `SeatedHUD` neben dem Kopf; ihre Zeit läuft nur, solange man sie sieht. Glyphen + Welle: `Counter/AlienText` (griechische und kyrillische Zeichen aus der Ausweichschrift). Werte: `Data/Counter/DialogueTuning.asset`.
  - **Nachfragen:** Beim Lesen eines Reisendenpapiers färbt sich das Feld unter der Maus; Klick → `DocumentDesk.FieldClicked` → `Travelers/TravelerDialogue`. Allgemeine Antworten wiederholen nur Gedrucktes.
  - **Für P15:** `TravelerData.answers` ist bei allen Test-Reisenden leer, und `AnswerFor` kennt nur den Feldnamen, nicht das Papier. Für Fälle, in denen Ausweis und Genehmigung sich widersprechen, muss P15 das erweitern.
  - Schreibtischansicht jetzt 20° geneigt, die Durchsage steht unten, damit Köpfe frei bleiben. Nach neuen Figuren-Lieferungen zuerst `Build Travelers`, dann `Build Test Day`.
  - Geprüft: 71/71 EditMode-Tests, WebGL-Build lokal in Chrome: alle fünf Spezies, Begrüßung, Feld-Klick, Übersetzungswelle, Abgang per ARREST. **Nicht geprüft:** Abgang per Stempel und Schleuse im Bild (gleicher Code, andere Tür); Position der Durchsage nach dem Umzug prüfe ich in P42.

### P41 — Regeln an der Pinnwand
- Größe: M · Priorität: Muss · Status: fertig (max, 05.11.)
- Braucht: P07 · Folgt auf: P33
- Ziel: Man kennt die Regeln des Tages während der Schicht.
- Enthalten: Tagesbulletin + Fahndungsliste (mit Befehl Verhaften/Terminieren) als Zettel an der Pinnwand im Schalterraum, scharf
  gezeichnet; Lautsprecher-Durchsagen mit Untertitel (Schichtbeginn, neue Regel, Verwarnung); im Sitzen umschauen (A/D bzw. Pfeiltasten,
  ±70°), Klick auf einen Zettel holt ihn zum Lesen heran
- Abnahme: Alle Regeln von Tag 1 sind im Sitzen lesbar; Durchsage zu Schichtbeginn erscheint.
- Ergebnis (max, 05.11.):
  - **Zettel:** `Counter/PinBoard` hängt Bulletin und Fahndungsliste an `SM_Counter_Board`, neu bei jedem `CounterShift.DayBegun`. Text: `Travelers/DayBulletin` (Grundregeln: `RuleEvaluator.StandingRules`, von Hand neben dem Prüfcode; Tagesregeln: `BulletinText`). Prefab `PF_Doc_Notice`, Typ `DocumentType.Notice`, nicht stempelbar. `DocumentDesk.Pin/Unpin`.
  - **Umschauen:** `Counter/SeatedLook`, Werte in `Data/Counter/SeatedLookTuning.asset` (±70°, Neigung geht dabei auf waagerecht). Nutzt `Player/Move`, das Eingabe-Asset ist unverändert.
  - **Durchsagen:** `Counter/Announcer`, Texte und spätere Clips in `Data/Counter/Announcements.asset`. Schichtbeginn über `CounterShift.ShiftStarted`, neue Regel über `BeginDay(rules, newToday)` (eine Zeile in `DayManager`), Verwarnung über `DecisionMade`. Platzhalter-Ton, Sprachaufnahmen stehen im Backlog.
  - Alles gebaut von **GameOff → Build Documents**. Die Zettelposition in `WireBoard` ist eine Kopie der Pinnwand aus `StationGrayboxBuilder.BuildProps`: wird die Pinnwand verschoben, dort mit anpassen.
  - Geprüft: 64/64 EditMode-Tests, WebGL-Build lokal in Chrome (Durchsagen, Umschauen, beide Zettel lesen, Sperren beim Lesen und mit Stempel, neuer Tag). **Nicht echt geprüft:** Hinsetzen wurde per Skript ausgelöst und A/D simuliert, echte Tasten testet max beim Spielen.
  - **Für P09/P08 (martin):** Verwarnzettel bleiben am nächsten Tag auf dem Tisch liegen, nach einem Dev-Sprung auch die Papiere des letzten Reisenden. Bitte bei der Abnahme von P09 entscheiden, ob `BeginDay` den Tisch leeren soll. **P09 kann jetzt abgenommen werden.**

### P42 — Pipeline-Lieferungen vom 04.11. einbauen
- Größe: S · Priorität: Soll · Status: fertig (max, 05.11.)
- Braucht: Lieferungen zu M015 · Folgt auf: P39, P38, P06, P07
- Ziel: Die Platzhalter von heute werden durch die bestellten Modelle und Grafiken ersetzt.
- Enthalten: PRs mergen (je Lieferung ein neuer Branch + PR, siehe `ASSETS.md`), `GameOff/Integrate Asset Deliveries`, dann die
  Builder neu laufen lassen: `Build Station Graybox` (Außenhülle, Dachplatten, Hangar-Rippen, Greebles), `Build Sky & Exterior`
  (Knoten, Andockflansche, Solarmasten, Antennensockel), `Build Documents` (Dokument-Hintergründe, Verwarnzettel, Drucker-Modell
  ersetzt die graue Box); Stempel-Abdrücke `T_Stamp_*`; Licht neu backen; Achsen/Pivots prüfen (Builder rechnen mit den Einträgen)
- Nicht enthalten: neue Funktionen
- Abnahme: Rundflug außen und Blick auf den Schaltertisch ohne graue Platzhalter; Konsole sauber; Tests grün.
- Ergebnis (max, 05.11.): Lieferungen P39, P06, P07, P38 von max freigegeben und gemergt; alle Einträge `final`. Station hat die Außenhülle, Tisch hat Drucker, echte Papiere, Stempel-Abdrücke. Licht neu gebacken (39 s). **Neu: GameOff → Rebuild All After Deliveries** führt alle Builder in der richtigen Reihenfolge aus, danach `Setup Main Lighting + Bake`. Geprüft im WebGL-Build: Menü mit Hülle von außen, Tisch, Frachtliste, Verwarnzettel, Stempel; Rundflug außen nur aus dem Menü-Blickwinkel. Wellenmuster von Zettel und Frachtliste stört beim Lesen nicht, kein Rework nötig. 71/71 EditMode-Tests.


### P43 — Optionsmenü mit Kategorien
- Größe: S · Priorität: Soll · Status: fertig (martin, 05.11.)
- Braucht: P02 · Folgt auf: P02
- Ziel: Optionen übersichtlich in Reitern statt einer langen Liste von Reglern, gleiches Panel im Haupt- und Pausemenü.
- Enthalten: Reiter **AUDIO** (Master; Music/Effects/Voice kommen mit P18), **CONTROLS** (Maus-Empfindlichkeit, Y-Achse umkehren,
  Tastenübersicht zum Lesen); pro Reiter „Reset to defaults“; alles wirkt sofort und wird gespeichert (`GameSettings`); Esc führt zurück;
  **Vollbild-Knopf** oben rechts im Haupt- und Pausemenü (Symbol mit vier Ecken, „FULLSCREEN“ beim Drüberfahren, folgt dem echten
  Zustand), kein Tastenkürzel. Statt eines Reiters DISPLAY (Änderung 05.11. auf Martins Wunsch: Vollbild ist die einzige
  Anzeige-Einstellung im Browser und gehört sichtbar ins Menü, nicht versteckt in die Optionen)
- Nicht enthalten: Retro-Look-Schalter (Entscheidung max, P17), Untertitel (kommen mit den Voicelines), Tastenbelegung ändern,
  Grafikqualität/Auflösung, Sprache, Helligkeit (bewusst weggelassen, siehe `DECISIONS.md` 05.11.)
- Abnahme: Im Haupt- und Pausemenü lassen sich alle Reiter öffnen, jede Einstellung wirkt sofort, bleibt nach Neuladen erhalten
  und lässt sich zurücksetzen; im WebGL-Build getestet.
- Ergebnis (martin, 05.11.):
  - Gebaut von **GameOff → Build Menu Scenes** (`MenuSceneBuilder.BuildOptionsPanel`, `FullscreenCorner`). Neue Einstellung =
    eine Zeile im passenden Reiter dort (Zeilen 50 px Abstand ab `RowTop`), Wert in `GameSettings`, Anzeige in `OptionsPanel.Refresh`.
  - `Scripts/UI/OptionsPanel.cs`: Reiter AUDIO (Master) und CONTROLS (Empfindlichkeit, Invert Y, Tastenübersicht), Reset pro Reiter.
  - `Scripts/Core/GameSettings.cs`: neu `InvertY`, Standardwerte als Konstanten, `ResetAudio`/`ResetControls`.
    `FirstPersonController` kehrt die Y-Achse um, wenn `GameSettings.InvertY` an ist.
  - `Scripts/UI/FullscreenButton.cs` + `Plugins/WebGL/Fullscreen.jslib`: Vollbild-Knopf oben rechts (Haupt- und Pausemenü).
    Im Browser schaltet das Plugin beim Loslassen der Maus um (Unity allein bräuchte zwei Klicks); das Symbol folgt dem echten Zustand.
  - Unterseiten (Options/Credits) dunkeln die Station mit 98,5 % ab: Das Projekt mischt UI im linearen Farbraum, darunter scheint sie zu stark durch.
  - Getestet im Editor und im WebGL-Build in Chrome (Vollbild mit einem Klick, Esc, Invert, Reset, Werte nach Neuladen), 0 Fehler/Warnungen.

### P46 — Geld, Inventar & Handelsfenster (Grundgerüst)
- Größe: M · Priorität: Muss · Status: fertig (martin, 05.11.)
- Braucht: P03 · Folgt auf: —
- Ziel: **Eine gemeinsame Grundstruktur für Geld- und Inventarverwaltung**, die alle späteren Pakete nur benutzen statt eigene zu bauen.
  **P08, P10, P11 und P13 werden erst gebaut, wenn P46 fertig ist.**
- Enthalten: **Kontostand** (Startgeld, Einnahmen/Ausgaben mit Grund, Ereignis bei Änderung, Anzeige im HUD); **Gegenstände** als
  ScriptableObjects (`ItemData`: Name, Preis, Symbol, Schmuggelware) und ein **Inventar** (hinzufügen, entfernen, Liste); ein
  **allgemeines Handelsfenster** (Angebot ↔ Inventar, Kaufen/Verkaufen), wiederverwendbar für jeden Händler und als „Send home“
  für die Drohne; alle Zahlen (Startgeld, Preise) in einem Tuning-Asset unter `Data/`; Test-Gegenstände und Test-Händler in der Sandbox
- Nicht enthalten: Lohn/Abzug aus Entscheidungen (P08), Abrechnung am Abend (P10), Kiosk-Inhalt und Scanner (P11), Drohne im Hangar (P13),
  echte Preise und Balancing (nach dem Kern-Loop-Call)
- Abnahme: Mit dem Test-Händler lässt sich kaufen und verkaufen, Kontostand und Inventar stimmen und bleiben über den Tageswechsel
  erhalten; eine zweite Fenster-Variante „Send home“ entfernt Gegenstände; Schnittstellen in der Ergebnis-Notiz beschrieben.
- Ergebnis (martin, 05.11.):
  - **Zugriff für alle Pakete:** `PlayerEconomy.Instance` (`Scripts/Economy/`, liegt in Main in `PF_Economy`, gilt für einen Durchlauf):
    - `Wallet.Add(10, "Wage · Vesna")` Einnahme · `Wallet.TrySpend(60, "Kiosk · Scanner")` Kauf (false ohne genug Geld) ·
      `Wallet.Charge(30, "Rent")` Pflichtzahlung, darf ins Minus (P10 entscheidet über Game Over) · `Wallet.Balance`, `Wallet.Changed`
    - `Wallet.Today`: Kontobewegungen des Tages mit Grund; wird jeden Morgen geleert (`DayManager.DayStarted`)
    - `Inventory.Add(item)` / `Remove(item)` / `Count(item)` / `Items`, `Inventory.Changed`; gleiche Gegenstände stapeln sich (keine Herkunft)
  - **Daten** (Inspector, nicht Code): `Data/Items/Item_*.asset` (`ItemData`: Name, Beschreibung, Grundwert, Labels),
    `Data/Items/Tags/Tag_*.asset` (`ItemTag`: Name + Farbe), `Data/Economy/Stock_*.asset` (`VendorStock`: Titel, Sortiment,
    Verkaufs-/Ankaufsfaktor, abgelehnte Labels, nur nach der Schicht offen), `Data/Economy/EconomyTuning.asset` (Startgeld 40, Währung CR).
    Test-Inhalte: 6 Gegenstände, 4 Labels, 3 Sortimente (Kiosk, Bar leer, Schwarzmarkt) – werden nur angelegt, wenn sie fehlen.
  - **Fenster:** `TradeWindow` (Entwurf A: Angebot | deine Gegenstände | Details + BUY/SELL, Kontostand oben rechts) mit
    `OpenTrade(stock)` und `OpenSendHome(titel, info, onSend)`; `InventoryWindow` (Entwurf 1, Taste **I**, nur beim freien Laufen):
    Gegenstände, Details mit Labels/Wert, **TODAY**. Der Kontostand ist nur in diesen Fenstern sichtbar.
  - **Welt** (`Scripts/Shops/`): `Vendor` (E: Platzhalter-Stimmton auf dem Voice-Kanal, dann Handelsfenster; „nur nach der Schicht“
    zeigt sonst „<Name> is closed“) für **Holly** (Kiosk), **Barkeeper** und **Dealer** (Barhocker 4); `DroneTerminal` an
    `SM_Hangar_CargoDrone` (Hitbox umschließt Drohne + Pad, Send home, Ereignis `Sent` für P13). Figuren = graue Boxen, bis `Art/Models/Characters/SM_NPC_Holly.fbx`,
    `SM_NPC_Barkeeper.fbx`, `SM_NPC_Dealer.fbx` geliefert sind (bestellt am 05.11., P52) → dann neu bauen.
  - Gebaut von **GameOff → Build Economy into Main** (`EconomyBuilder`). Dev-Werkzeuge (`DayManager.showDevTools`): rote Hitboxen,
    roter **Dev_force_Trade** bei geschlossenen Händlern, Startgegenstände Contraband spice + Pocket watch zum Testen.
  - Getestet im Editor: kaufen (zu teuer → „Not enough credits“), verkaufen, Kiosk lehnt CONTRABAND ab, Schwarzmarkt zahlt mehr,
    Drohne entfernt Gegenstände, Geld/Inventar bleiben über den Tageswechsel, TODAY wird morgens geleert. 0 Fehler/Warnungen.
  - **Einmalige Gegenstände:** `ItemData.unique` (z. B. Scanner, Story-Gegenstände) – höchstens einer im Inventar, Händler verkaufen
    ihn dann nicht noch einmal („You already have one.“). `Inventory.CanAdd(item)` prüft das.
  - **Neuen Gegenstand anlegen:** Rechtsklick in `Data/Items/` → Create → GameOff → Economy → Item, Datei `Item_<PascalName>.asset`
    (Name, Beschreibung, Grundwert, Labels, ggf. unique). Labels: `Data/Items/Tags/Tag_<NAME>.asset`. Sortimente:
    `Data/Economy/Stock_<Händler>.asset`. Danach EditMode-Tests laufen lassen.
  - **Maus nach Fenstern:** `FirstPersonController.SetControlEnabled(true)` fängt die Maus sofort wieder (im Browser über
    `PointerLockBridge` → `PointerLock.jslib`, solange der Klick/Tastendruck noch als Nutzeraktion zählt). Gilt für Brief, Handel,
    Inventar, Bett, Pause, Aufstehen. Nach Esc braucht es weiter einen Klick (Browser-Regel).
  - **Tests:** `Tests/Editor/EconomyTests.cs` (13 Tests: Kontostand, TODAY, Inventar, unique, Preise, abgelehnte Labels, alle Assets gültig).
  - **Bewusst offen (gehört zu anderen Paketen):**
    - Beschlagnahme (P08): Reisende führen ihre Fracht bisher nur als Text (`TravelerData`, Max). Für „Beschlagnahme ins Inventar“
      brauchen sie echte `ItemData` → Frage an max in der Kern-Loop-Abfrage.
    - Wirkung bei der Familie (P13/P10), z. B. Medizin schicken deckt den Medizinbedarf: dafür ein eigenes Feld am Gegenstand,
      **nicht** die Labels (Labels = Anzeige + Händler-Filter).
    - Benutzen von Gegenständen (P30, z. B. Nova Cola trinken): Knopf im Inventar kommt dort dazu.
    - Händler-Vorrat pro Tag: Sortiment ist unbegrenzt, bis das Balancing etwas anderes verlangt.

### P47 — Menü-Hintergrund: Schiffsverkehr
- Größe: S · Priorität: Soll · Status: fertig (martin, 05.11.)
- Braucht: P02 · Folgt auf: P02
- Ziel: Im Hauptmenü fliegen ab und zu Schiffe an der Station vorbei – das All wirkt lebendig.
- Enthalten: wiederverwendbarer Baustein **`ShipTraffic`** (feste Bahnen mit zufälligen Abständen und Richtungen, Schiffsliste pro Bahn,
  Tempo, Obergrenze gleichzeitiger Schiffe, Schiffe werden wiederverwendet statt neu erzeugt, keine Lichter, keine Schatten, keine
  Kollision); drei Bahnen im Menü (hinten Frachter/Tanker, Mitte Patrouille, vorn Pendler); Werte in einem Tuning-Asset unter `Data/`;
  alle Modelle aus `Art/Models/Ships/` (geliefert)
- Nicht enthalten: Verkehr im Spiel, Shuttle-Landung, Rampe, Kopplung an den Tagesablauf (alles P25 – nutzt diesen Baustein)
- Abnahme: Im Menü sieht man innerhalb von ~10 s ein Schiff; nichts fliegt durch die Station oder die Kamera; im WebGL-Build flüssig.
- Ergebnis (martin, 05.11.):
  - **Baustein** `Scripts/Core/ShipTraffic.cs` + `TrafficLane.cs` + `ShipTrafficTuning.cs`: Bahnen (von/bis, Streuung, Schiffe, Größe, Tempo,
    Abstand, erste Verzögerung, Richtungswechsel), Obergrenze `maxActive`, `modelYaw` = 180 (Schiffs-FBX zeigen mit der Nase nach −Z).
    Schiffe werden einmal erzeugt und wiederverwendet, ohne Lichter, Schatten und Kollision; läuft mit unskalierter Zeit.
  - **Menü:** `Data/Menu/ShipTraffic_Menu.asset` (hinten Frachter/Tanker alle 20–40 s, Mitte Patrouille 30–60 s, vorn Pendler 10–20 s;
    Abstand zur Station nachgerechnet ≥ 21 m). Wird nur angelegt, wenn es fehlt – Werte im Inspector ändern, dann **GameOff → Build Menu
    Scene Only** (neu: baut nur `Menu.unity`, Boot und Main bleiben unberührt).
  - **Für P25:** eigenes `ShipTrafficTuning` für Main (seltener) anlegen und ein `ShipTraffic`-Objekt damit in Main setzen.
  - Getestet im Editor und im WebGL-Build (Chrome), 0 Fehler/Warnungen.

### P48 — Menü direkt in Main mit Kameraflug
- Größe: M · Priorität: Kann · Status: offen
- Braucht: P09 · Folgt auf: P02
- Ziel: Das Menü sieht aus wie das Spiel, und „Start Shift“ geht nahtlos ins Spiel über.
- Enthalten: Spielstart direkt in `Main.unity` mit Außenkamera (echtes gebackenes Licht), Menü darüber, bei „Start Shift“ Kameraflug in
  Zimmer 103 zum Spieler
- Nicht enthalten: neue Menüpunkte, Änderungen am Tagesablauf
- Abnahme: Menü zeigt die Station mit gebackenem Licht; „Start Shift“ landet ohne Szenenwechsel beim Spieler in Zimmer 103; läuft im WebGL-Build. Nur in der Polish-Woche, nur wenn alle Muss-Pakete fertig sind.

### P49 — Live-Version auf kestrel9.maxlworks.com + Dev-Schalter im Menü
- Größe: M · Priorität: Soll · Status: wartet auf Unity-Lizenz + Secrets (max)
- Braucht: P02, P43 · Folgt auf: P43
- Ziel: Der aktuelle Stand ist jederzeit öffentlich spielbar und aktualisiert sich nachts von selbst; Dev-Werkzeuge lassen sich im Menü schalten.
- Enthalten: Reiter DEV im Optionsmenü (`GameSettings.DevTools`, Standard: Editor an, Build aus); Build-Skript `WebBuild`, eigene Web-Vorlage; Docker-Container auf Geoffrey hinter nginx; GitHub-Actions-Workflow (nachts + per Knopf, baut nur bei geänderten Spieldateien), Rollback; `docs/DEPLOY.md`
- Nicht enthalten: Repo öffentlich machen, itch-Upload (P19), Schrift und Splash (Martin)
- Abnahme: `https://kestrel9.maxlworks.com` lädt und ist spielbar; Schalter wirkt sofort in Haupt- und Pausemenü; ein Workflow-Lauf spielt eine neue Version ein, ein Lauf ohne Änderung baut nicht; die anderen Sites auf Geoffrey laufen unverändert.
- Stand (max, 05.11. abends): **Live unter https://kestrel9.maxlworks.com** (Ablauf, Handeinspielen, Rollback, Secrets: `docs/DEPLOY.md`), aktuell mit P40, P41, P42, P54, P55, P56.
  - Geprüft: Spiel lädt öffentlich und startet bis Zimmer 103, nur eigene Domain wird geladen; Deploy über den beschränkten Schlüssel; kaputte Version rollt von selbst zurück; falscher Image-Name wird abgewiesen; andere Sites unverändert; 61/61 EditMode-Tests.
  - **Offen für die Abnahme:** Unity-Lizenz + Secrets (max), dann erster Workflow-Lauf; rote Hitboxen und Dev-Knöpfe im Spiel mit Schalter an/aus noch nicht gesehen (nur Menü und Speicherung geprüft).
  - **Für P19 (itch):** Die dortige Web-Vorlage muss abgelehnte Maussperren genauso abfangen wie `Assets/WebGLTemplates/Kestrel9/index.html` (`errorHandler`), sonst stoppt Unitys `alert` das Spiel. Ein 16:9-Bild ist dort ebenfalls nötig: Das Menü ist für 960×540 gebaut und wird in schmalen Fenstern links abgeschnitten.

### P50 — Türen zeigen, wohin sie führen
- Größe: M · Priorität: Soll · Status: offen (Idee max, 05.11.)
- Braucht: — · Folgt auf: P27
- Ziel: Man findet sich zurecht, weil jede Tür zeigt, wohin sie führt, und das Station-typisch abwechslungsreich statt immer gleich.
- Enthalten: Hinweis an jeder Tür, zum Raum passend und unterschiedlich (Schilder, Wandschablonen, Leuchtschrift, Bodenmarkierung, Plakat …); Durchgang über die Builder (`StationGrayboxBuilder`), vorhandene Modelle zuerst nutzen (`SM_Hall_Sign`, `SM_Motel_DoorSign`, `T_Decal_Signs`, `SM_Bar_NeonSign`), fehlende Varianten über `ASSET-REQUESTS.md`
- Nicht enthalten: Türlogik und Zugangsstufen (P27), Wegweiser-Karte oder HUD
- Abnahme: Von jeder Tür aus erkennt man im Spiel ohne Karte das Ziel; nicht alle Hinweise sehen gleich aus; lesbar im WebGL-Build.

### P51 — Zimmer 103 wohnlicher
- Größe: S · Priorität: Kann · Status: offen, Bilder hängen (max, 06.11.), größeres Bullauge fehlt
- Braucht: — · Folgt auf: M014 (Katzenbild)
- Ziel: Das Zimmer des Spielers fühlt sich persönlich an.
- Enthalten: Bullauge größer (heute Ø 0,6 m, `SM_Station_Wall_Porthole_2x3`, Zimmer 103); Katzenbild etwas höher (`SM_Motel_CatPainting`, heute y 1,3 m, `StationGrayboxBuilder.BuildProps`); neues Gemälde vom Hund **Elo** (Bestellung erst, wenn das Paket dran ist; dann fragt Claude Max nach einem Foto als Vorlage, wie beim Katzenbild)
- Nicht enthalten: weitere Möbel, andere Zimmer
- Abnahme: Alle drei Punkte im Zimmer zu sehen; Licht neu gebacken; das Bullauge passt zum Außenmodell.
- Hinweis: Größeres Bullauge heißt neues Wandmodul über die Pipeline und neuer Lichtbake. Wird das Wohnmodul (P53) gebaut, zuerst dort klären, wo Zimmer 103 am Ende liegt.

### P52 — Mehr Figuren: Händler an Kiosk und Bar
- Größe: M · Priorität: Soll · Status: fertig (max, 06.11.)
- Braucht: P46 · Folgt auf: P46
- Ziel: Statt grauer Boxen stehen echte Figuren an Kiosk und Bar.
- Enthalten: `SM_NPC_Holly.fbx` (ältere Frau), `SM_NPC_Barkeeper.fbx` (vierarmiges Alien mit Atemmaske, Schlauch zum Rucksack), `SM_NPC_Dealer.fbx` (sitzt shady auf einem Barhocker, dunkle, abgewetzte Kleidung mit Kapuze), Beschreibungen von Martin (Handover 05.11.); Einträge in `ASSET-REQUESTS.md`; nach der Lieferung `GameOff/Build Economy into Main`
- Nicht enthalten: Dialoge, Animationen über eine Idle-Haltung hinaus, weitere Bewohner (kommen mit P53)
- Abnahme: Die drei Figuren stehen an ihren Plätzen statt der Boxen, Handel funktioniert unverändert.
- Ergebnis: Lieferung von Max freigegeben und eingebaut, alle drei Figuren stehen an ihren Plätzen statt der Boxen (Einzelbilder aus dem Editor geprüft). Im WebGL-Build mit Figuren geprüft: Kauf bei Holly, Fenster von Dealer und Barkeeper öffnen sich. Die Händler wurden dabei per Befehl geöffnet, der Weg zu ihnen und [E] sind nicht geprüft. Download jetzt 15,0 MB (vorher 14,7).
- Stand 05.11.: **Bei der Pipeline bestellt** (drei Einträge `SM_NPC_*` in `ASSET-REQUESTS.md`, `needed_by` 16.11.). Das Paket wartet auf die Lieferung, Einbau macht max.

### P53 — Wohnmodul begehbar: Zimmer für alle Figuren
- Größe: L · Priorität: Soll · Status: fertig (max, 06.11.)
- Braucht: Entwurf + Absprache max/martin (Umfang gegen Feature-Freeze 22.11.) · Folgt auf: P04, P27
- Ziel: Das Motel hat genug Platz, und jede Figur kann ein eigenes Zimmer haben, was die Immersion trägt.
- Enthalten: großer Blender-Auftrag an die Pipeline: Innenraum des Wohnmoduls (Gänge, Zimmer) und eine begehbare Verbindung von der Station dorthin; Einbau über die Builder inkl. NavMesh und Lichtbake
- Nicht enthalten: Tagesabläufe der Bewohner, Inhalte der Zimmer über die Grundausstattung hinaus
- Abnahme: Der Spieler läuft ohne Ladezeit von der Station ins Wohnmodul, die Zimmer sind erreichbar, von außen stimmt das Modell mit innen überein, und der WebGL-Build bleibt flüssig.
- Ausgangslage (05.11.): Die Motelzimmer 101–103 liegen im Hauptblock (x −20..−8, z 0..6, Höhe 3 m). Das Wohnmodul existiert nur als Außenmodell `SM_Ext_HabModule` (liegender Zylinder 12 × 12 × 30 m, zwei Stück: `Hab_West`, `Hab_East` in `SkyExteriorBuilder`) ohne Innenraum.
- Im Entwurf zu klären: Welches Modul, wie viele Zimmer; ob das Innere den Außenzylinder ersetzt oder hineinpasst; wo die Verbindung ansetzt (Motelgang-Westende?) und wie sie von außen aussieht; bleibt Zimmer 103, wo es ist, oder zieht es um (Laufwege, Bett, `PF_DayFlow`, P51); was Größe und Lichtbake für den WebGL-Build bedeuten (heute 19 MB).
- Ergebnis (max, 06.11.): Das Wohnmodul ist ohne Ladezeit begehbar: Tür am Westende des Motelgangs, Röhre mit Treppe, Schott, Flur, 12 Zimmer. Eigener Builder `StationHabBuilder`, eigenes Prefab `PF_Station_Hab`, nur in Main. 201–208 und 212 verriegelt, 209–211 offen. Außen `SM_Ext_HabModule_Entry` und die Röhre.
  - Geprüft: Weg mit echten Tasten im Build von Zimmer 103 bis Flurende und in ein offenes Zimmer; 158 statt 132 begehbare Punkte im Flood-Fill, 0 Unterschiede im Occlusion-Bildvergleich (3.792 Ansichten); 90/90 Tests; keine neuen Konsolenfehler; Tageswechsel über `DevEndDay` und drei Runden Menü und zurück. Nicht geprüft: Mausführung und Gefühl auf der Treppe, andere Geräte, Abfertigung im Modul-Stand mit echten Tasten.
  - NavMesh: eigene Surface plus `NavMeshLink` durch die Röhrentür. Offene Zimmer: Pfad vollständig, verriegelte: Pfad endet an der Tür. Beim Einbau fiel auf, dass `NavMeshRebake` nie etwas gespeichert hat (asynchron, zu spät für SaveScene und `-batchmode`), Main hatte also gar kein NavMesh. Jetzt synchron, 38 KB Station und 22 KB Modul in `Scenes/Main/`.
  - Sonne: Die Innenwände hatten helle Flecken (Spalt am Bogen, Lücken kleiner als der Shadow-Bias). Dagegen helfen fünf große Quader `SunBlockers`, die nur Schatten werfen. Außen an der Röhre sieht man dadurch dunkle Bänder; von Main aus kaum zu sehen, im Menü gibt es die Blöcke nicht.
  - Zahlen: Download 18,0 statt 17,7 MB, Speicher 81–84 statt 82 MB. Bildrate gegen den Stand davor mit CPU ×4, Median aus je drei Läufen im Wechsel: Menü 19,1 gegen 20,1 fps, Zimmer 41,7 gegen 43,8, Schalter 24,4 gegen 24,6, Pinnwand 29,3 gegen 31,0, Stehen 23,4 gegen 28,6. Die Streuung ist groß (Stehen je nach Satz 19–41 fps), zwei weitere Sätze zeigen keinen Verlust; ein kleiner Verlust beim Stehen ist nicht ausgeschlossen.
  - Offen: am Flurende Nord bleiben links und rechts vom Bildschirm 10 cm Spalt zum Schott (2 m Wand im 2,2 m Flur, so im Bauplan). Die Hab-Teile `SM_Station_Hab_*` sind fertig; nicht verwendete Kit-Teile stehen im Handover.

### P54 — Orbitron am Schalter
- Größe: S · Priorität: Soll · Status: fertig (max, 05.11.)
- Braucht: P41 · Folgt auf: P41
- Ziel: Der Schalter nutzt dieselbe Schrift wie der Rest des Spiels (Beschluss von martin, 05.11.).
- Enthalten: `DocumentPrefabBuilder` auf `Orbitron SDF`: Prüfdokumente, Verwarnzettel, Pinnwand-Zettel, Texte in `PF_CounterStation` (Hinweiszeile, Meldung, Untertitel); Felder und Größen anpassen, weil Orbitron breiter ist
- Nicht enthalten: neue Layouts, Martins Bereiche
- Abnahme: Im WebGL-Build sind alle Texte am Schalter in Orbitron, kein Feld läuft über, und die Regeln sind gut lesbar.
- Ergebnis (max, 05.11.): `DocumentPrefabBuilder` nutzt `Orbitron SDF` für Papiere und alle Texte in `PF_CounterStation`. Einzeilige Felder schrumpfen bis auf 60 %, statt überzulaufen; `Field_body`, `Field_cargo`, `Field_reason` brechen um. Hinweiszeile gekürzt. Ausweis, Genehmigung, Frachtliste, Verwarnzettel und Bulletin im WebGL-Build geprüft; Fahndungsliste nutzt dasselbe Layout, in Orbitron aber nicht einzeln angesehen.

### P55 — Verkehr vor Plattform- und Barfenster
- Größe: S · Priorität: Soll · Status: fertig (martin, 05.11.)
- Braucht: P24, P47 · Folgt auf: P47 (übernimmt die vorbeifliegenden Großschiffe aus P25)
- Ziel: Auch im Spiel fliegen Schiffe vor der Glasfront der Aussichtsplattform und dem Barfenster vorbei – so oft wie im Hauptmenü.
- Enthalten: `ShipTraffic` (P47) mit eigenem Tuning `Data/Traffic/ShipTraffic_Main.asset`; Bahnen nördlich der Station quer vor beiden
  Fenstern (hinten Frachter/Tanker, Mitte Patrouille, vorn Pendler), beide Richtungen; `PF_Traffic` in Main, gebaut von
  **GameOff → Build Traffic into Main**; Abstandsprüfung zu Station und Außenwelt im Builder; Schiffe vor dem Heimatplaneten
- Nicht enthalten: Shuttle-Landung, Rampe, Tagesablauf (P25), Sound der Schiffe, Verkehr am Hangar-Tor
- Abnahme: Von der Plattform und am Barfenster sieht man innerhalb von ~10 s ein Schiff; nichts fliegt durch Station, Außenwelt oder
  hinter dem Planeten; im WebGL-Build flüssig.
- Ergebnis (martin, 05.11.): `PF_Traffic` in Main (Wurzelobjekt), gebaut von **GameOff → Build Traffic into Main**. Drei Bahnen
  nördlich der Station, hoch genug über dem Außen-Fachwerk (Abstand ≥ 46 m): hinten Frachter/Tanker (z 300), Mitte Patrouille (z 160),
  vorn Pendler (z ≈ 105), max. 4 Schiffe gleichzeitig. Tuning: `Data/Traffic/ShipTraffic_Main.asset` (im Inspector ändern; der
  Builder legt es nur an, wenn es fehlt). **Nebel:** Mains Nebel hat die Schiffe verschluckt → eigener Shader `GameOff/ShipNoFog`
  (`Art/Shaders/ShipNoFog.shader`, Umgebungslicht + Sonne `_GameOffSunDir` + Emission, kein Nebel). Der Builder erzeugt daraus
  `Art/Materials/Ships/NoFog/M_Ship_*_NoFog.mat` (Texturen/Farben aus Max' `M_Ship_*` – nach neuer Schiffslieferung einfach neu
  bauen) und `Prefabs/Environment/Ships/PF_Ship_*_NoFog.prefab`. Das Menü (P47) nutzt weiter die normalen Schiffe. Feinschliff
  (Bahnen, Größen, Licht) später als Optimierung.

### P56 — Dev-Werkzeug: Verkehrslinien sichtbar und einstellbar
- Größe: M · Priorität: Kann · Status: fertig (martin, 05.11.)
- Braucht: P55, P49 · Folgt auf: P55
- Ziel: Mit dem Schalter **TRAFFIC_LINE_DEV** (Optionen → DEV) sieht man die Verkehrsbahnen vor den Fenstern als farbige Linien und
  kann sie live in einem kleinen Dev-Panel einstellen – in Unity landen die Werte direkt im Tuning-Asset.
- Enthalten: Linie je Bahn, so dick wie ihr Schiff, mit Farbe + Schild (**CARGO** orange, **CIVILIAN** cyan, **PATROL** blau);
  durchsichtige Streuungs-Box; Warnung, wenn eine Bahn der Station näher als 15 m kommt; Panel: Bahn wählen, Entfernung, Höhe,
  Seitlich, Neigung, Drehung, Länge, Schiffsgröße, Tempo, Abstand, Streuung; Knöpfe „Schiff jetzt“, Zeitraffer ×4, „Werte kopieren“,
  „Zurücksetzen“, „Bahn speichern“ (SAVE LANE, Wunsch Martin); Maus frei, solange der Modus an ist; Tab = Umschauen. Eigener Schalter, unabhängig von DEV TOOLS.
- Nicht enthalten: Bahnen anlegen/löschen, Schiffstypen wechseln, Menü-Verkehr (P47), Ziehgriffe in der Szenenansicht
- Abnahme: Schalter an → Linien + Panel erscheinen, Regler verschieben die Linie sofort, neue Schiffe fliegen auf der neuen Bahn;
  nach dem Play-Modus stehen die Werte im Asset; im WebGL-Build kopiert „Werte kopieren“ die Werte; Schalter aus → alles weg.
- Ergebnis (martin, 05.11.): eigener Schalter **TRAFFIC_LINE_DEV** im DEV-Tab (unabhängig von DEV TOOLS, Wunsch Martin). `TrafficLaneDevTool`
  (`Scripts/UI/`) sitzt auf `PF_Traffic` und baut Linien + Panel selbst (Shader `GameOff/DevLine`: kein Nebel, immer obenauf).
  Bahnen heißen jetzt **Cargo / Patrol / Civilian**, Farbe im Feld `devColor` der Bahn. Bedienung: Sobald der Modus an ist, ist die **Maus frei** fürs Panel (Spieler steht);
  **Tab** = Umschauen/Laufen ⇄ zurück zur Maus; Bahn oben wählen; Regler wirken sofort (neue Flüge). **SAVE LANE**: in Unity sofort ins Asset
  `ShipTraffic_Main` gespeichert; im WebGL-Build als JSON in die Zwischenablage → in Unity **GameOff → Paste Traffic Lanes**.
  **COPY ALL** = alle Bahnen so kopieren, **RESET** = zurück zum letzten Speicherstand, **SHIP NOW** + **TIME x4** zum schnellen
  Anschauen. Rote, blinkende Linie = zu nah an der Station (< 15 m inkl. Streuung und Schiffsbreite).
  Achtung: Alle drei Bahnen liegen vom Fenster aus fast auf derselben Höhe am Himmel (~31° über dem Fachwerk) → Linien überdecken sich.

### P57 — Nachtlauf: Bugs beheben, Spiel optimieren
- Größe: L · Priorität: Muss · Status: fertig (max, 06.11.)
- Braucht: — · Folgt auf: P40, P41, P42, P55
- Ziel: Bekannte und neu gefundene Bugs sind behoben, Ruckler sind belegt und reduziert, Spiel lädt und läuft spürbar besser.
- Enthalten: B01, B05, Softlock Papier unter der Schublade, einfache Bugs; Messung im WebGL-Build; Grundlast (Occlusion, Schatten, HDR), Download und RAM, Skripte
- Nicht enthalten: neue Funktionen, Spielregeln und Spielwerte (P58)
- Abnahme: Vorher-Nachher-Zahlen im Ergebnis; Tests grün; vollständiger Durchlauf im WebGL-Build ohne neue Konsolenfehler.
- Ergebnis: Die Schiffe verursachen die Ruckler nicht, die Grundlast war es. Gemessen in `tools/measure-webgl.mjs` (Chrome ohne Bildratenbremse, 1600 × 900, RTX 2070 Super, Ryzen 2700X), „CPU ×4“ steht für ein schwächeres Gerät:

  | Wert | Vorher | Nachher |
  |---|---|---|
  | Download | 19,4 MB | 14,7 MB |
  | Speicher im Spiel, belegt | 130 MB | 80 MB |
  | Speicher gesamt | 138 MB | 128 MB |
  | fps Menü, CPU ×4 | 18,5 | 25,0 |
  | fps Zimmer, CPU ×4 | 16,0 | 59,6 |
  | fps Schalter, CPU ×4 | 25,2 | 38,4 |
  | fps Pinnwand, CPU ×4 | 24,9 | 43,8 |
  | fps Stehen am Schalter, CPU ×4 | 21,4 | 38,2 |
  | Schalter, CPU ×4: mittlerer Frame / 99 % | 39,9 ms / 79,0 ms | 25,2 ms / 60,1 ms |
  | fps ohne Bremse: Menü / Zimmer / Schalter | 98 / 103 / 152 | 150 / 353 / 212 |

  - **Geholfen:** Occlusion Culling (Zimmer ×3), Static Batching (15–25 %, kostet 2,5 MB), PC-Stufe im Web-Build ausgeschlossen, Himmel komprimiert, HDR und ungenutzte Licht-Funktionen aus, Code-Stripping Medium, kleinere Code-Generierung. Skripte: Tür, Sprechblase, Einstellungen im Spieler, Dev-Werkzeug nur noch bei Bedarf.
  - **Geprüft und verworfen:** Schattendistanz 25 m (Sonne auf fernen Hangarwänden), weniger Schattenwerfer für die Sonne (das Innere wird in 99 % der Ansichten heller). Der Echtzeit-Schatten kostet weiter etwa ein Drittel des Frames, siehe P58 Punkt 6.
  - **Bugs:** B05 behoben und im Build bestätigt. B01 abgesichert, aber nie nachgestellt, also nicht bestätigt. Dazu: Papier unter der Schublade, schiefe und klebende Papiere, alte Durchsage am Morgen, Tab im Dev-Werkzeug, mögliche Endlosschleife im Verkehr, Null-Absicherung beim Händler.
  - **Speicher:** kein Leck. Zehn Wechsel Menü und Spiel: nach dem Aufbau stabil bei 79 und 83 MB, gesamt bleibt 128 MB.
  - **Geprüft:** 72 EditMode-Tests grün; Durchlauf im WebGL-Build (`tools/drive-webgl.mjs`): Lesen, Ziehen, Stempeln, Schublade, Verhaften, Schleuse, Verwarnzettel, Tageswechsel, leerer Tisch am Morgen, Pause, Kauf, Drohne, Schwarzmarkt, Menü und zurück; keine neuen Konsolenfehler. Bildvergleich `OcclusionCheck`: 3.192 Ansichten von 133 begehbaren Punkten, ohne Sonne kein Unterschied.
  - **Nicht geprüft:** echte Tasten und der Weg zum Stuhl, Geräte außer diesem PC, andere Browser als Chrome. Der Papierschutz an der Schublade lief nur als Code, der Fall wurde im Build nicht ausgelöst. Vereinzelte einzelne lange Frames (148–180 ms in 3 von etwa 10 Läufen ohne Bremse) bleiben ungeklärt: nie mit CPU ×4, an wechselnden Stellen.

### P58 — Entscheidungen aus dem Nachtlauf
- Größe: S · Priorität: Soll · Status: fertig (max, 06.11.; Punkt 2 martin, 06.11.)
- Braucht: — · Folgt auf: P57
- Ziel: Die im Nachtlauf gefundenen Fragen zu Spielregeln und Spielwerten sind entschieden. Was daraus zu bauen ist, wird ein eigenes Paket oder geht in das genannte.
- Nicht enthalten: Änderungen an Spielregeln ohne Entscheidung. P57 hat hier bewusst nichts geändert.

- Ergebnis (Max, 06.11.): 1 Vorrat, morgens aufgefüllt → P11. 2 Martin: Schmuggelware (`CONTRABAND`, `CONFISCATED`) kauft nur der Schwarzmarkt → P64. 3 Klappe schließt wieder → P59. 4 gefälschte Siegel erst ab der Lampe → P15. 5 mit P13 entscheiden. 6 so lassen, später auf echten schwachen Geräten prüfen. 7 akzeptiert. 8 gestrichen: kein Bug.

**1. Händlervorrat (B04)**
- Befund: Ware beim Händler sinkt beim Kauf nicht.
- Beleg: In P46 bewusst unbegrenzt, Lösung ist für P11 vorgesehen.
- Optionen: (a) in P11 Vorrat pro Händler, füllt sich jeden Morgen auf; Kosten: S, gehört schon zu P11. (b) unbegrenzt lassen; kostet nichts, verstärkt aber Punkt 2.
- Empfehlung: (a).

**2. Geld aus dem Nichts durch Handel**
- Befund: Kiosk verkauft zum Grundwert, der Schwarzmarkt kauft zum 1,2-fachen. Wer pendelt, verdient ohne Risiko unbegrenzt.
- Beleg: `Data/Economy/Stock_Kiosk_Test.asset` (`sellFactor: 1`), `Stock_BlackMarket_Test.asset` (`buyFactor: 1.2`). Die Werte stammen aus Test-Daten.
- Optionen: (a) Schwarzmarkt kauft nur beschlagnahmte oder verbotene Ware; Kosten: klein, Regel über Tags. (b) `buyFactor` unter 1; Kosten: ein Wert, aber der Schwarzmarkt verliert seinen Reiz. (c) so lassen, bis der Vorrat aus Punkt 1 die Menge begrenzt.
- Empfehlung: (a), passt zur Geschichte.

**3. Schleusenklappe bleibt offen**
- Befund: Die Schutzklappe über dem roten Knopf geht beim ersten Klick auf und danach nie wieder zu. Ab dem zweiten Einsatz löst ein einziger Klick die Schleuse aus, der doppelte Schutz vor dem Fehlklick fehlt.
- Beleg: `Scripts/Counter/CounterClickable.cs:41-47`, `_coverOpen` wird nirgends zurückgesetzt.
- Optionen: (a) Klappe schließt nach dem Auslösen und jeden Morgen; Kosten: wenige Zeilen. (b) so lassen.
- Empfehlung: (a). Das ist eine Spielgefühl-Frage und deshalb nicht von mir entschieden.

**4. Gefälschtes Siegel ohne Gadget nicht erkennbar**
- Befund: Ein gefälschtes Siegel verlangt „Verhaften“, ist laut Design aber nur mit Gadget sichtbar. Wer das Gadget nicht hat, kann nur raten und bekommt trotzdem einen Fehler.
- Beleg: `Scripts/Travelers/RuleEvaluator.cs:62-63`, `docs/DESIGN.md` Abschnitt Fälschung und Gadgets (UV-Lampe).
- Optionen: (a) Reisende mit gefälschtem Siegel erst an Tagen, an denen die Lampe verfügbar ist; Kosten: Inhaltsprüfung im Validator. (b) ohne Gadget kein Fehler; Kosten: Sonderfall in der Regel. (c) gewollt lassen.
- Empfehlung: (a).

**5. Drohne hat keine Öffnungszeit**
- Befund: Die Fracht-Drohne ist jederzeit nutzbar, die Händler nur nach der Schicht.
- Beleg: `Scripts/Shops/DroneTerminal.cs:43`, im Gegensatz zu `Vendor`.
- Optionen: (a) wie Händler nur nach der Schicht; Kosten: wenige Zeilen. (b) so lassen, weil das Paket nach Hause erst mit P13 wirkt.
- Empfehlung: mit P13 entscheiden.

**6. Echtzeit-Sonne im Inneren**
- Befund: Die Sonne ist das einzige Echtzeit-Schattenlicht und kostet etwa ein Drittel des Frames, obwohl das Innere gebacken ist. Jenseits der Schattendistanz von 50 m scheint sie durch Wände und hellt ferne Flächen auf. Das war schon vorher so.
- Beleg: Ganz ohne Schatten CPU ×4: Schalter 31 → 42 fps, Zimmer 52 → 65, aber das Innere wird ausgewaschen (Vergleichsbilder im Handover beschrieben). `OcclusionCheck.CheckShadowDistance`: 50 gegen 100 m unterscheidet sich in 2.399 von 3.192 Ansichten.
- Optionen: (a) Sonne wirkt nur außen, das Innere bekommt seine Sonnenflecken gebacken; Kosten: M, Licht-Layer, neuer Bake, Sonnenflecken wandern nicht mehr mit den Tagen. (b) so lassen; kostet nichts und bleibt beim jetzigen Aussehen.
- Empfehlung: (a), falls das Spiel auf schwachen Geräten noch zu langsam ist. Art-Entscheidung, deshalb nicht von mir gemacht.

**7. Heller Streifen im Hangar durch Occlusion**
- Befund: Blendet Occlusion einen Schattenwerfer aus, kann auf der Hangarwand kurz ein schmaler Sonnenstreifen erscheinen.
- Beleg: `OcclusionCheck.Run`: 122 von 3.192 Ansichten, höchstens 0,8 % der Bildfläche; ohne Sonne keine Ansicht.
- Optionen: (a) akzeptieren. (b) verschwindet mit Punkt 6 (a). (c) Hangar-Teile vom Ausblenden ausnehmen; Kosten: klein, aber etwas mehr Grundlast.
- Empfehlung: (a), Max schaut es sich im Hangar an.

**8. Dev-Sprung im Sitzen: gestrichen**
- Der Befund war falsch: `DevEndDay` ist im Spiel nur über den Bett-Dialog erreichbar, dort sitzt man nie. Ausgelöst hatte es nur das Testskript per `SendMessage`.

### P59 — Schleusenklappe schließt nach dem Auslösen
- Größe: XS · Priorität: Soll · Status: fertig (max, 06.11.)
- Braucht: — · Folgt auf: P58
- Ziel: Die Schutzklappe schützt bei jedem Reisenden vor dem Fehlklick, nicht nur beim ersten.
- Enthalten: Klappe geht nach dem Auslösen wieder zu. Beim Aufstehen schließt sie schon heute.
- Abnahme: Im WebGL-Build braucht auch der zweite Reisende zwei Klicks.
- Ergebnis: Im WebGL-Build bestätigt, die Klappe schließt nach dem Auslösen, auch der zweite Reisende braucht zwei Klicks. 72 Tests grün.

### P60 — Letzte Platzhalter ablösen
- Größe: M · Priorität: Soll · Status: fertig (max, 06.11.)
- Braucht: — · Folgt auf: P42, P52
- Ziel: Im Spiel ist nichts mehr grau oder einfarbig gebaut, und die Dokumente zeigen echte Passfotos.
- Bestand (`GameOff/List Placeholders`, 06.11.): 254 Renderer in Main und Menu, dazu das Silhouetten-Foto auf den Dokumenten und zwei Testtöne.
- Bestellt (14 Einträge `package: P60` in `ASSET-REQUESTS.md`, bis 16.11.):
  - Schalterraum und Kammer (P1): `SM_Counter_WindowWall_6x3`, `SM_Lock_WindowWall_6x3`
  - Passfotos (P1): `T_Doc_Photo_Human`, `_Vellani`, `_Quobb`, `_Xirrik`, `_Sorvak`
  - Halle und Decke (P2): `SM_Hall_WallEnd_1x6`, `SM_Station_LightStrip`
  - Rumpf (P2): `T_Ext_HullFiller_A_Albedo`, `_B_Albedo`, kachelnd, für Unterseite und Reststücke
  - Ton: `SFX_PA_Chime` (P2), `SFX_Vendor_Greet` (P3)
  - Kleinkram (P3): `SM_Station_LockLamp`
  - Früher bestellt, geliefert und eingebaut (06.11., von Max angehört): `SFX_Door_Open`, `SFX_Door_Close`, `SFX_Door_Locked` (P27)
- Einbau nach der Lieferung, jeweils Builder ändern: `StationGrayboxBuilder` (Wandmodule, Leuchten, Lampe; die Blöcke bleiben unsichtbar als Kollision; Rumpf-Material mit Kachel statt Farbe), `TravelerContentBuilder` (Foto je Art statt Silhouette), `Announcer` und `Vendor` (Ton statt Testton). Danach `Rebuild All`, Bake, `Check Occlusion Bake`, `GameOff/List Placeholders` muss leer sein.
- Nicht enthalten, weil nicht bei der Pipeline bestellbar oder keine Grafik: gesprochene Durchsagen (Voicelines, ElevenLabs offen), Platzhalter-Briefe und -Texte (P15), Platzhalter-Ende (P12), Test-Sortimente der Händler (P11).
- Abnahme: `GameOff/List Placeholders` meldet nichts mehr; Foto passt zur Figur in der Kammer; Tests grün.
- Ergebnis: Schalter- und Kammerwand, Hallenenden, 49 Deckenleuchten, Türlampen, Rumpfhaut und Passfotos je Art sind eingebaut. `List Placeholders` meldet in Main, Menu und Boot nichts mehr. 85 Tests grün, Bildvergleich ohne Sonne ohne Abweichung. Im Build durchgespielt: Dokumente mit Fotos, Stempeln, Schublade, Schleuse, Abend, Kiosk, Abrechnung, nächster Tag, Menü und zurück.
  - `SFX_PA_Chime` und `SFX_Vendor_Greet` kommen mit P61, sie brauchen die Tonanbindung.
  - Download 15,9 MB statt 15,1 MB, belegter Speicher 81 statt 79 MB.
  - Bildrate: kein Einbruch belegt, aber nicht sauber bewiesen. Im direkten Wechsel war derselbe Aufbau ohne Wandtexturen schneller als der Stand davor (CPU ×4, Schalter 31 und 34 gegen 29 und 25 fps). Die Schlussmessung lief während der PC benutzt wurde und streut zu stark (Schalter 17 und 24 gegen 30 und 24 fps). Einzelläufe auf diesem PC schwanken um mehr als 20 %, Vergleiche nur im Wechsel messen.
  - Nicht geprüft: echte Tasten und Laufwege, andere Geräte und Browser. Halle, Hangar, Gang und Rumpf nur als Editor-Ansichten, nicht im Build abgelaufen.

### P61 — Ton am Schalter und in der Station
- Größe: M · Priorität: Soll · Status: fertig (max, 06.11.; nach Gehör abgenommen, 07.11.)
- Braucht: P18 · Folgt auf: P18
- Ziel: Was der Spieler tut, ist zu hören, und die Station klingt nach einem Ort. Bisher liegt keine einzige Sounddatei im Projekt.
- Bestellt (18 Einträge `package: P61` in `ASSET-REQUESTS.md`, bis 16.11.):
  - Schalter (P1): `SFX_Stamp_Approve`, `SFX_Stamp_Deny`, `SFX_Paper_Slide`, `SFX_Drawer_Open`, `SFX_Drawer_Close`, `SFX_Printer_Citation`, `SFX_Button_Arrest`, `SFX_Cover_Flip`, `SFX_Airlock_Fire`
  - Allgemein (P2): `SFX_Paper_Pickup`, `SFX_Footstep_Metal_01` bis `_03`, `SFX_UI_Click`, `SFX_Amb_Station_Loop`
  - Räume und Handel (P3): `SFX_Amb_Bar_Loop`, `SFX_Amb_Hangar_Loop`, `SFX_Trade_Buy`
- Einbau nach der Lieferung: Felder und Aufrufe in `DocumentDesk`, `CounterClickable`, `CounterStation`, `FirstPersonController`, UI und Handelsfenster; Raumklänge je Bereich. Alles über `AudioManager` und die Mixer-Gruppen aus P18.
- **Hinweis für max (martin, 06.11.):** `SFX_Amb_Station_Loop.ogg` hat Martin selbst geliefert (Freesound 77838, Jovica, CC BY 4.0;
  51 s stereo, 486 KB; Feedback-Zeile in `ASSET-REQUESTS.md`, Status bitte setzen). Vorab läuft er schon in Main über **P67**
  (`AmbienceBuilder`, `PF_Ambience`, eine 2D-Quelle für die ganze Station, Lautstärke 0,35). Für die Raumklänge pro Bereich bitte darauf
  aufbauen oder ersetzen; `AmbienceBuilder` gehört dann dir. Die Credits im Hauptmenü brauchen noch **Build Menu Scenes**.
- Nicht enthalten: Musik (Max und Martin organisieren sie selbst, Einbau erst danach), Stimmen.
- Abnahme: Jede Aktion am Schalter ist zu hören, Lautstärkeregler wirken, keine Konsolenfehler, Download nach dem Einbau gemessen.
- Ergebnis: Stempel, Papier, Schublade, Drucker, Verhaften, Schleuse, Klappe, Durchsagen-Gong, Händlergruß, Kauf, Schritte, Klick in Menüs und Raumklang für Station, Bar und Hangar sind eingebaut. Alles liegt in `Data/Audio/SoundSet.asset`, auch die Lautstärken: **dort nach Gehör abgleichen, ohne Code und ohne Builder** (`GameOff/Build Sound Set` lässt sie stehen).
  - Geprüft im WebGL-Build: Jeder Sound wird wirklich gestartet (im Browser mitgezählt, Dauer passt zum Clip). Die Schleuse ohne Reisenden spielt nur die Klappe, in der Pause startet nichts. 90 Tests grün, keine neuen Konsolenfehler.
  - Nicht geprüft: **der Klang selbst, die Lautstärken und die Regler**, der Testbrowser ist stumm. Die Überblendung zwischen den Räumen und die Raumklänge von Bar und Hangar sind nur als Logik getestet, nicht abgelaufen. Das nimmt Max auf der Live-Seite ab.
  - Download 17,3 statt 15,9 MB (Sounds 2,2 MB, davon je rund 0,65 MB die drei Raumklänge). Falls zu viel: Vorbis-Qualität der Raumklänge senken.
  - Bildrate: kein messbarer Unterschied, Median aus drei Paaren im Wechsel, CPU ×4: Schalter 25,2 gegen 26,6 fps, Pinnwand 34,6 gegen 32,8, Menü 21,8 gegen 20,2. Speicher 80 MB, stabil über Tage und Szenenwechsel.
  - Nebenbei: `DocumentDesk`, `CounterShift` und `CounterClickable` haben nur Sound-Aufrufe bekommen. Die Clownsschuh-Sounds liegen bereit und sind nicht verdrahtet (P63).

### P62 — Gadget: UV-Lampe
- Größe: M · Priorität: Soll · Status: offen, Modell bestellt (max, 06.11.)
- Braucht: P06, P11 · Folgt auf: P58
- Assets: `SM_Item_UVLamp` (bestellt am 06.11.)
- Ziel: Mit der Lampe sieht der Spieler gefälschte Siegel, ohne sie bleiben sie unsichtbar.
- Enthalten: Gegenstand, Lampe auf dem Schaltertisch, UV-Ansicht auf den Dokumenten für `ForgedSeal`.
- Nicht enthalten: weitere Gadgets. Reisende mit gefälschtem Siegel kommen über P15, erst ab Verfügbarkeit der Lampe (P58 Punkt 4).
- Offen: freigeschaltet oder gekauft, und an welchem Tag (`DESIGN.md`, Gadgets). Klären Max und Martin im Call.
- Abnahme: Ohne Lampe kein Hinweis aufs Siegel, mit Lampe klar sichtbar; Validator prüft die Regel aus P58.

### P63 — Clownsschuhe
- Größe: S · Priorität: Kann · Status: offen, Assets bestellt (max, 06.11.)
- Braucht: P08, P46, P61 · Folgt auf: —
- Assets: `SM_Item_ClownShoes`, `SFX_ClownShoe_Squeak_01` bis `_03` (bestellt am 06.11.)
- Ziel: Beschlagnahmte Clownsschuhe lassen sich anziehen, dann quietscht jeder Schritt, bis man sie auszieht.
- Enthalten: Gegenstand, Anziehen und Ausziehen im Inventar, Schrittgeräusch wechselt.
- Abnahme: Mit Schuhen quietscht jeder Schritt, ohne Schuhe normal.

### P64 — Schwarzmarkt kauft nur Schmuggelware
- Größe: XS · Priorität: Soll · Status: fertig (martin, 06.11.)
- Braucht: P46 · Folgt auf: P58 (Punkt 2)
- Ziel: Kein risikoloses Geld mehr durch Pendeln zwischen Kiosk und Schwarzmarkt.
- Enthalten: neues Label `Tag_CONFISCATED`; `VendorStock` bekommt eine Liste „kauft nur mit diesen Labels“ (`onlyTags`); Kiosk und Bar
  lehnen `CONTRABAND` und `CONFISCATED` ab, der Schwarzmarkt kauft nur Ware mit einem der beiden; `EconomyBuilder` legt das so an
  (auch auf bestehenden Assets), Menü **GameOff → Update Economy Data** (nur Daten, keine Szene); EditMode-Test; gemeinsame Label-Liste.
- **Label-Liste (martin, 06.11.)** – `Data/Items/Tags/Tag_<NAME>.asset`, Farben aus `docs/STYLE.md`:
  | Label | Bedeutung, wer nutzt es |
  |---|---|
  | CONTRABAND | verboten; nur der Schwarzmarkt kauft; Risiko bei der Drohne (P13) |
  | CONFISCATED | am Schalter beschlagnahmt (P08); nur der Schwarzmarkt kauft |
  | FOOD | Essen und Trinken; Familienbedarf (P10/P13) |
  | MEDICINE | Medizin; Familienbedarf |
  | GADGET | Werkzeug für den Schalter (Scanner, UV-Lampe P62) |
  | DRUG | Drogen, z. B. Spice; Ware für Gerüchte-Preise (P65) |
  | TECH | Elektronik, Ersatzteile |
  | WEAPON | Waffen, Munition; immer auch CONTRABAND |
  | VALUABLE | Schmuck, Uhren, Wertsachen; Tauschware für die Wunschliste des Barkeepers (P65) |
  | ALIEN | fremde, nicht-menschliche Waren |
  | PERISHABLE | verdirbt nach einigen Tagen (Regel erst mit P10/P11) |
  | POWER | Energie für die Heizung zu Hause, z. B. Fusion Cell; deckt den Bedarf Wärme (P13, martin 06.11.) |
  | LUXURY | Genussmittel, z. B. Nova Cola; deckt keinen Bedarf (P70, martin 06.11.) |

  Kategorie-Labels (DRUG, TECH, WEAPON, VALUABLE, ALIEN) sind das, worauf sich Gerüchte und Wunschlisten beziehen.
  Labels hängen am Gegenstandstyp, nicht am einzelnen Stück: Beschlagnahmte Ware sind eigene Gegenstände mit `CONFISCATED`
  (z. B. „Confiscated pocket watch“), die legt P08 an.
- **Zum Besprechen (Max + Martin, vor P08):** Labels hängen an der Sorte Gegenstand (`ItemData`), nicht am einzelnen Stück. Eine normale und eine beschlagnahmte Taschenuhr sind darum zwei Gegenstände („Pocket watch“ und „Confiscated pocket watch“ mit `CONFISCATED`); ein Stück im Inventar bekommt kein Label nachträglich. Beschlagnahme legt direkt die beschlagnahmte Variante ins Inventar. Folge: Jeder beschlagnahmbare Gegenstand braucht zwei Assets. Alternative wäre ein Merkmal pro Inventar-Stapel („beschlagnahmt“), das kostet Umbau an `Inventory`. Erst gemeinsam entscheiden, sonst kollidiert es mit P08.
- Nicht enthalten: Preise und Balancing, Händlervorrat (P11), woher beschlagnahmte Ware ins Inventar kommt (P08).
- Abnahme: Uhr oder Ration lässt sich am Schwarzmarkt nicht verkaufen, Schmuggelware am Kiosk nicht; Tests grün.
- Ergebnis (martin, 06.11.): `VendorStock.onlyTags` + `Buys()`; `EconomyBuilder.ApplyTradeRules` trägt die Regeln auch in bestehende
  Sortimente ein (fügt nur hinzu); 7 neue Labels angelegt, Spice hat zusätzlich `DRUG`, die Taschenuhr `VALUABLE`.
  Neue Labels oder Regeln: **GameOff → Update Economy Data** (nur Daten, keine Szene). Geprüft: 73/73 EditMode-Tests (neu:
  `VendorStock_OnlyTags_BuysOnlyTaggedItems`), Play-Modus Main: Schwarzmarkt kauft Spice (14 CR), nicht die Uhr; Kiosk kauft die Uhr
  (10 CR), nicht Spice; Bar kauft nichts. Konsole 0 Fehler. Kein WebGL-Build.

### P65 — Barkeeper: Wunschliste & Gerüchte
- Größe: M · Priorität: Soll · Status: fertig (martin, 07.11.)
- Braucht: P68 (Markt, Ereignisse pro Tag), P64 · Folgt auf: —
- Ziel: Der Barkeeper kauft nichts für Geld, sondern tauscht Gegenstände gegen Wissen für den nächsten Tag.
- Enthalten: Täglich hängt vor der Bar ein Schild mit Gegenständen oder Labels, die der Barkeeper haben will. Wer liefert, bekommt ein
  Gerücht oder eine Geheiminfo für morgen, z. B. „Ein Schmuggler wurde festgenommen, morgen ist alles mit DRUG teurer, z. B. Spice“. Der Spieler
  kann abends beim Schwarzmarkt vorsorgen und am nächsten Tag teurer verkaufen. Daten je Tag (Wunsch, Gerücht, Preiswirkung pro Label)
  als ScriptableObject in `Data/Days/`.
- Nicht enthalten: Bestechungszettel (P66), Gespräch am Barhocker (P28).
- Vorschlag an Max: `questions/2026-11-06_martin-an-max_barkeeper-p65-p66.md` (bei Zustimmung legt sein Claude es fest). Entschieden (Max, 06.11.): Ersetzt den Bar-Ankauf. Ein Gerücht pro Tag, immer wahr.
- Abnahme: An Tag 1 Wunsch erfüllen → Gerücht; an Tag 2 ist der Preis für das genannte Label beim Schwarzmarkt wie angekündigt.
- Ergebnis (martin, 07.11.):
  - **Wunsch nach Label** (`DayEntry.barWish`): Tag 1 „2× LUXURY“ (Nova Cola), Tag 2 „1× TECH“ (Data chip beim Dealer), Tag 3 keiner.
  - **Barkeeper** (`Shops/Barkeeper`, ersetzt den Händler an der Bar): nur nach der Schicht; Dialog „Today I'd like … You have …“ → **GIVE**
    (nur mit genug passender Ware, nimmt die billigste zuerst) → Dialog „A RUMOUR“; danach steht das Gerücht bis zum Morgen im Inventar
    unter TODAY („RUMOUR · …“, anklicken = ganzer Text). Schon bedient: zeigt das Gerücht erneut. Kein Wunsch: „Nothing I need today.“
  - **Gerücht = `rumour` der Marktereignisse von morgen** (`MarketEvent.rumour`, `BarWishRules.Rumour`) → immer wahr; nichts los → „Quiet times ahead.“
  - **Schild** neben der Bar-Tür in der Halle (`BarWishSign`, Platzhalter-Tafel mit Text, morgens neu); Modell `SM_Bar_WishBoard` bestellt.
  - Geprüft: 98/98 EditMode-Tests (neu `BarkeeperTests`); Play-Modus: Tag 1 Schild, Bar während der Schicht zu, mit 1 Cola kein GIVE,
    mit 2 Cola Gerücht über die Grenze (Notiz im Inventar), Tag 2 Notiz weg, Data chip → Gerücht über Schmuggler und Kälte; Bild vom Schild.

### P66 — Barkeeper: Bestechungszettel
- Größe: S–M · Priorität: Soll · Status: fertig (martin, 07.11.)
- Braucht: P08, P10 · Folgt auf: —
- Ziel: Moralische Versuchung am Schalter: Geld dafür, eine bestimmte Person durchzulassen.
- Enthalten: Zu Schichtbeginn liegt manchmal ein Zettel vom Barkeeper auf dem Tisch („Lass Person XY durch, nach der Schicht
  bekommst du 200 CR“). Wer XY durchlässt, bekommt das Geld in der Abrechnung (P10). Die Regelprüfung wertet ein falsches Durchlassen
  trotzdem als Fehler. Zettel je Tag in den Tages- bzw. Reisenden-Daten.
- Vorschlag an Max: siehe Frage `barkeeper-p65-p66`. Entschieden (Max, 06.11.): Weigern kostet nichts. Abweisen oder Verhaften bringt nur kein Geld, XY wird normal bewertet. Das Durchlassen bleibt ein normaler Fehler aus P07.
- Abnahme: Zettel erscheint am vorgesehenen Tag; XY durchlassen → Geld nach der Schicht; abweisen → kein Geld.
- Ergebnis (martin, 07.11.):
  - **Zettel** (`Shops/BribeNote`): an Tagen mit Bestechung (`DayEntry.bribe`) liegt beim ersten Hinsetzen ein Zettel „A NOTE“ links vorn auf dem
    Schaltertisch (angepinnt wie die Pinnwand-Zettel, daher ignorieren Schublade und Beschlagnahme ihn; anklickbar zum Lesen), weg bei Schichtende.
  - **Platzhalter:** Tag 3 (nicht Tag 2, siehe unten), **Quill Barragh**, **60 CR**, Text vom Barkeeper („… Burn this. – L.“).
  - **Durchlassen** → Abrechnung „Envelope from the bar +60 CR“; die Entscheidung wird ganz normal bewertet (Verwarnzettel, kein Lohn für ihn).
  - **Abweisen/Verhaften → Barkeeper ist sauer (Martin, 07.11., ändert Max' „Weigern kostet nichts“):** am selben und am nächsten Abend kein
    Gerücht („Not today, officer. You know why.“). Kein Geldverlust.
  - **Warum Tag 3:** Auf Tag 2 hätte ein ehrlicher Spieler genau das lohnende Schmuggler-Gerücht verloren (Report: 0 CR statt +7 je Stück).
  - **Balancing (P71-Regel):** Simulator kennt Bestechung (Spielweise „Corrupt“); neues Ziel „korrupt Ticket nicht vor Tag 6“ – erfüllt (Tag 6,
    Ehrlich mit Gerüchten ebenfalls Tag 6, Clever Tag 8). Report neu.
  - Geprüft: 102/102 EditMode-Tests (neu `BribeTests`); Play-Modus Tag 3: Zettel liegt da (Bild), Quill durchgelassen → +60 in der Abrechnung.

### P67 — Stations-Atmo vorab hörbar
- Größe: XS · Priorität: Soll · Status: fertig (martin, 06.11.)
- Braucht: P18 · Folgt auf: — · Teil von **P61** vorgezogen (max baut dort weiter)
- Ziel: Der gelieferte Loop `SFX_Amb_Station_Loop.ogg` ist im Spiel zu hören, bevor P61 die Raumklänge pro Bereich einbaut.
- Enthalten: Builder **GameOff → Build Ambience into Main** setzt `Prefabs/Audio/PF_Ambience.prefab` (AudioSource 2D, Loop, SFX-Gruppe) in Main;
  Aufnahme in **Rebuild All**.
- Nicht enthalten: Raumklänge pro Bereich (Bar, Hangar), Überblenden zwischen Räumen – das bleibt in P61.
- Abnahme: In Main läuft die Atmo nahtlos, SFX-Regler wirkt, im Pausenmenü pausiert sie, Konsole 0 Fehler.
- Ergebnis (martin, 06.11.): `Scripts/Editor/AmbienceBuilder.cs`, Menü **GameOff → Build Ambience into Main**, auch in `Rebuild All`.
  `PF_Ambience` = AudioSource 2D, Loop, Gruppe SFX, Lautstärke 0,35 (Konstante im Builder, P61 stimmt nach Gehör ab).
  Geprüft im Play-Modus: läuft ab Start, springt nach 51 s nahtlos auf den Anfang, pausiert mit `AudioListener.pause` (Pausenmenü),
  Konsole 0 Fehler. Der Loop selbst: Freesound 77838 (Jovica, CC BY 4.0), Sekunde 6–60 mit 3 s Überblendung, Quelle in `ArtSource/Audio/`.
  **Weiter in P61 (max).**

### P68 — Markt: Preise und Ereignisse pro Tag
- Größe: M · Priorität: Muss (Vorschlag, Preise und Bett hängen daran) · Status: fertig (martin, 07.11.)
- Braucht: P11 (Händlervorrat), P10, P13, P70 (Waren) · Folgt auf: — · Hängt zusammen mit P65 (Gerüchte), P15 (Inhalte), P71 (Balancing)
- Ziel: Ein Markt bestimmt, was es wann zu welchem Preis gibt – mit festen Ereignissen pro Tag (politische Lage, Tagesereignisse).
- **Preisformel (martin, 06.11., DECISIONS.md):**
  - Jede Ware hat einen **Basismarktpreis** (`ItemData.baseValue`).
  - **Marktpreis** = Basis × Grundfaktor **jedes** Labels der Ware (alle multipliziert) × Faktoren der **Ereignisse** des Tages je Label.
    Grundfaktor als neues Feld am `ItemTag` (Standard 1; z. B. CONTRABAND ×1,5, PERISHABLE ×0,8).
  - **Händlerpreis** = Marktpreis × Verkaufsfaktor des Händlers; **Ankauf** = Marktpreis × Ankaufsfaktor (`sellFactor`/`buyFactor`);
    Ereignisse können den Ankauf je Label zusätzlich ändern.
  - **Bett** = Marktpreis (ohne Händler) × 1,35 – Ereignisse wirken also auch am Bett.
  - Alles multiplikativ, gerundet erst am Ende.
- **Ereignisse:** fest pro Tag (keine Zufälle), `MarketEvent` (ScriptableObject, `Data/Market/`): Name, Bulletin-Zeile, Durchsage-Text,
  Wirkungen je Label (Preisfaktor, Mengenfaktor – 0 = nicht erhältlich –, Ankaufsfaktor) und Sortiment-Änderungen je Händler (Ware dazu/weg).
  `DayEntry` bekommt eine Liste `marketEvents`.
- **Ankündigung:** vorher **nur über den Barkeeper** (P65 liest die Ereignisse von morgen); am Ereignistag über **Bulletin an der Pinnwand**
  und **Durchsagen** (P41, max – Haken nötig, Frage `markt-bulletin-durchsagen`). Das Handelsfenster nennt keinen Grund.
- Enthalten: `Market` (reine Klasse in `PlayerEconomy`, bei `DayStarted` mit den Ereignissen gesetzt); `VendorStock.PriceFor/OfferFor`,
  `VendorSupply.Refill` (Menge) und `HouseholdTuning.MoneyCost` (Bett) fragen ihn; EditMode-Tests (Formel, Menge 0, Ankauf, Bett,
  Tag ohne Ereignis = Grundwerte); Validator: jedes Ereignis hat Bulletin- und Durchsage-Text.
- Nicht enthalten: Gerüchte (P65), Texte der Ereignisse (P15), Warenliste (P70), Durchrechnen (P71).
- Abnahme: An einem Tag mit Ereignis ist die betroffene Ware sichtbar teurer, knapper oder fehlt; das Bett zieht mit; Bulletin und
  Durchsage nennen das Ereignis.
- Ergebnis (martin, 07.11.):
  - **Formel umgesetzt** (`Market`): Marktpreis = Basis × (1 + Tagesschwankung) × `ItemTag.priceFactor` jedes Labels × Ereignis-Preisfaktoren;
    Ankauf zusätzlich × Ereignis-Ankaufsfaktor; Menge = Tagesmenge × Ereignis-Mengenfaktor, **aufgerundet** (mind. 1, nur Faktor 0 = weg);
    **jeden Morgen genau die Tagesmenge** im Regal (Reste vom Vortag zählen nicht). Bett folgt dem Marktpreis.
  - **Label-Grundfaktoren:** CONTRABAND ×1,3, WEAPON ×1,2, VALUABLE ×1,2, LUXURY ×1,2, PERISHABLE ×0,8, Rest ×1,0.
  - **Ereignisse** (`MarketEvent`, `Data/Market/`, je Tag in `DaySchedule` → `marketEvents`):
    Tag 2 „Border closed“ – FOOD ×1,3, Menge ×0,5, Fresh fruit fehlt. Tag 3 „Smugglers busted“ – DRUG ×1,8, Ankauf CONTRABAND ×1,3,
    Dealer +2 Spice; „Cold snap“ – POWER ×1,25. Spice-Schwankung Tag 3 jetzt 0 (Gerücht lohnt: Tag 2 kaufen ≈ 28, Tag 3 verkaufen 44).
  - **Ankündigung:** dritter Zettel **MARKET** an der Pinnwand (nur an Tagen mit Ereignis; die drei Zettel sitzen jetzt bei −0,33 / 0 / +0,33,
    `DocumentPrefabBuilder.WireBoard`), Durchsage nach Schichtbeginn (`Announcer`, Text ohne Aufnahme).
  - **Verderb:** PERISHABLE-Ware im Inventar ist am Morgen weg (`EconomyTuning.perishableTag`), der Brief nennt es („Spoiled overnight: …“).
    Vorher per Drohne Geschicktes zählt.
  - **Scanner** vorerst nicht im Kiosk (kommt mit P16 zurück).
  - Haken: `DayManager.BeginDay` → `Market.Begin(day, events)` vor dem Zettel-Aufhängen; `DayManager.Entry(day)` für P65 (Ereignisse von morgen).
  - Geprüft: 95/95 EditMode-Tests (neu `MarketTests`); Play-Modus 3 Tage: Tag 1 Spice beim Dealer 23; Tag 2 Essen teurer und halb so viel,
    Fresh fruit fehlt, Zettel und Durchsage, verdorbene Frucht im Brief; Tag 3 Spice 42 ×5, Ankauf 44, Wärme am Bett 22. Konsole 0 Fehler.

### P69 — Familienschicksal: was „kritisch“ bedeutet
- Größe: S · Priorität: Muss (Vorschlag) · Status: offen, Entwurf (martin, 06.11.) – mit Max abstimmen
- Braucht: P10 · Folgt auf: P10 · Gehört zu den Bedingungen von P12, Texte in P15, sichtbar in P29
- Ziel: Der Zustand der Familie hat eine Grenze: Wer zu lange kritisch ist, bleibt nicht folgenlos.
- Enthalten (Vorschlag): Regel, was nach einer oder mehreren Nächten auf „critical“ passiert (z. B. Person geht ins Krankenhaus und
  fällt als Bedarf weg, verlässt die Familie oder stirbt); „Familie zerbrochen“ als Niederlage für P12; Ankündigung im Morgenbrief
  vorher („Pip is getting worse every night“); Werte (Anzahl Nächte) im `HouseholdTuning`; EditMode-Tests für die Regel.
- Offen für Max: Wie hart darf es sein (Tod ja/nein)? Ab wann ist die Familie „zerbrochen“?
- Nicht enthalten: Endbildschirme (P12), Brieftexte (P15), Stadtlichter/Foto (P29).
- Abnahme: Drei Nächte ohne Versorgung führen sichtbar zur angekündigten Folge; mit Versorgung erholt sich die Familie.

### P70 — Preisliste & Sortiment
- Größe: S–M · Priorität: Muss · Status: fertig (martin, 06.11.)
- Braucht: P46, P64, P13 · Folgt auf: P11 (Rest „Preisliste“) · Vor P68 und P71
- Ziel: Echte Waren statt Test-Inhalten – die Dinge, auf die sich Bedarfe, Ereignisse und Gerüchte beziehen.
- Enthalten: Waren mit Basismarktpreis, Labels und Tagesmenge (Texte Englisch): je Bedarf 2–3 Waren (FOOD, POWER, MEDICINE),
  einige Wertsachen (VALUABLE), Schmuggelware (CONTRABAND/DRUG/WEAPON), Gadgets (Scanner, UV-Lampe P62); Grundfaktoren der Labels;
  Sortiment für **Holly (Kiosk)** und den **Dealer (Schwarzmarkt)**; die **Bar** verkauft und kauft nichts (P65). Test-Inhalte ersetzen.
- Nicht enthalten: Modelle/Icons (Liste im Fenster reicht), Ereignisse (P68), Durchrechnen (P71).
- Abnahme: Kiosk und Dealer zeigen das neue Sortiment; jeder Bedarf lässt sich mit mindestens zwei Waren decken; Tests grün.
- Ergebnis (martin, 06.11., von Martin in Unity getestet): vollständige Preisliste (alles vorläufig, in Assets einstellbar):
  - **Lohn** 15 CR pro **richtig** entschiedenem Reisenden, falsche bringen 0 CR (Abrechnung: „5 of 6 correct × 15“); Abzug 10 CR je
    Verwarnzettel über die 2 freien. **Miete** 30 CR (Pflicht). **Bett** = Marktpreis der Referenzware × 1,35 (Ration, Fusion cell,
    Painkillers) → Tag 1: Essen 11, Wärme 15, Medizin 20; Tag 3: 14 / 18 / 26.
  - **Marktpreis** = Grundpreis × (1 + feste Schwankung des Tages) – `ItemData.dailyChange` (Tag 1/2/3), `Market.PriceOf`; Händler × Verkaufs-
    bzw. Ankaufsfaktor, Bett × 1,35, gerundet am Ende. P68 setzt Label-Faktoren und Ereignisse obendrauf.
  - **Holly (Kiosk, ×1,0 / Ankauf ×0,5):** Protein bar 5 (FOOD, 4/Tag), Ration pack 8 (FOOD, 3), Canned stew 12 (FOOD, 2),
    Fresh fruit 14 (FOOD+PERISHABLE, 1), Battery pack 6 (POWER, 3), Fusion cell 11 (POWER, 2), Cough syrup 9 (MEDICINE, 2),
    Painkillers 15 (MEDICINE, 2), Nova Cola 2 (**LUXURY**, 5), Document scanner 60 (GADGET, 1).
  - **Dealer (Schwarzmarkt, ×1,5 / Ankauf ×1,2):** Contraband spice 12 (CONTRABAND+DRUG, 3), Stim injector 18 (CONTRABAND+DRUG+MEDICINE, 2),
    Antibiotics 30 (CONTRABAND+MEDICINE, 1), Data chip 25 (CONTRABAND+TECH, 2), Pistol 60 (CONTRABAND+WEAPON, 1).
  - **Ohne Händler:** Pocket watch 20, Silver ring 35 (VALUABLE), Alien trinket 30 (ALIEN+VALUABLE). Bar handelt nicht (P65).
  - Schwankungen Tag 1/2/3 stehen je Ware im Asset (Tendenz: bis Tag 3 knapper und teurer); P71 rechnet nach, P68/P15 passen an die Story an.
  - Neues Label **LUXURY** (Genussmittel; Nova Cola, FOOD entfernt, sonst deckte eine 2-CR-Cola das Familienessen).
  - Geprüft: 85/85 EditMode-Tests (neu: Marktpreis/Bett je Tag, ≥ 2 Waren je Bedarf im Kiosk, Nova Cola ohne FOOD, Schwankung 0 oder 3 Tage);
    Play-Modus: Listen, Preise Tag 1 und Tag 3, Lohn nur für richtige.

### P71 — Balancing über 3 Tage
- Größe: S · Priorität: Muss · Status: fertig (martin, 07.11.)
- Braucht: P68, P70, P10, P13 · Folgt auf: —
- Ziel: Lohn, Abzüge, Miete, Bedarfe und Marktereignisse ergeben über 3 Tage ein knappes, aber schaffbares Spiel.
- Enthalten: Menü **GameOff → Economy Report**: rechnet typische Spielweisen durch (alles beim Händler + Drohne / alles Geld am Bett /
  sparen / viele Fehler am Schalter) mit den Ereignissen der Tage → Kontostand und Familienzustand je Abend. Ziel festlegen
  (z. B. „knapp, aber schaffbar; wer nicht versorgt, verliert“), Werte in den Assets anpassen. EditMode-Test: die Standard-Spielweise
  übersteht 3 Tage, „nichts versorgen“ führt zu P69-Folgen.
- Nicht enthalten: neue Mechaniken.
- Abnahme: Report zeigt für jede Spielweise 3 Abende; Standard-Spielweise endet mit Geld ≥ 0 und ohne kritische Familie.
- **Neu ausgerichtet (martin, 07.11.):** Das Spiel wird **endlos** (P73), Ziel ist ein **teures Ticket nach Hause** (P72), mit guter Arbeit
  und versorgter Familie in **ca. 7 Tagen**. P71 rechnet pro Tag über ~12 Tage statt 3 Tage. Ziele: Clever 6/6 → Ticket Tag 6–8; am Bett 6/6
  → Tag 9–11; 4/6 → nicht vor Tag 12 oder Bedarfe auslassen; nichts versorgen → Familie kritisch bis Tag 3; Gerücht +5–8 CR je Spice
  (Bar-Wünsche 5–15 CR). Ereignisse nehmen die billigste Ware weg; Rundung „,5 rundet auf“.
- **Regel:** Jede Geldanpassung (Lohn, Miete, Preise, Faktoren, Ereignisse, Wünsche, Mengen, Ticketpreis) braucht danach ein neues
  Rebalancing: `GameOff → Economy Report` bis alle Ziele ✓ (die Balance-Tests erzwingen es).
- Ergebnis (martin, 07.11.): alle Ziele ✓ – Tabelle in **`docs/ECONOMY-REPORT.md`** (vom Menü erzeugt, nicht von Hand ändern).
  - **Werkzeuge:** `EconomySimulator` (Editor, spielt N Tage mit den echten Regeln: Markt, Vorrat, Preise, Bett, Lohn, Familie, Barkeeper)
    und Menü **GameOff → Economy Report**; Test `EconomyBalanceTests` prüft dieselben Ziele.
  - **Ergebnis (12 Tage):** Clever fehlerfrei → Ticket **Tag 8**, Familie gesund; mit Gerüchten → **Tag 7** (+6,6 CR je spekuliertem Stück,
    Wünsche zusammen 40 CR); alles am Bett → **Tag 11**; 2 Fehler/Tag → kein Ticket bis Tag 12 (Clever 192 CR, Bett pendelt um 30 CR);
    nichts versorgen → Familie kritisch an Tag 3.
  - **Ticketpreis 350 CR** (`HouseholdTuning.ticketPrice`); Lohn 15, Abzug 10, Miete 30, Startgeld 40 und Bettfaktor 1,35 bleiben.
  - **Geändert:** „Border closed“ nimmt zusätzlich die Protein bar weg, „Cold snap“ die Battery packs; „Smugglers busted“ DRUG ×1,5 statt ×1,8,
    kein Zusatz-Ankauf für CONTRABAND mehr; Spice ohne Tag-2-Aufschlag; neue Kiosk-Ware **Spare fuse** (TECH, 6 CR, 2/Tag) für den Tag-2-Wunsch;
    Preise runden jetzt „,5 auf“ (z. B. Painkillers Tag 2: 17 statt 16).
  - **Wichtig für P69/P72:** Wer die Familie gar nicht versorgt, hat das meiste Geld (Tag 5 schon 340 CR). Solange „critical“ keine Folge hat
    und das Ticket die Familie nicht prüft, ist Vernachlässigen der schnellste Weg nach Hause → P69 (Folgen) und P72 (Ende abhängig von der Familie)
    müssen das abfangen. Der Simulator zählt das Ticket nur bei gesunder Familie.

### P72 — Ticket nach Hause
- Größe: M · Priorität: Muss (Vorschlag) · Status: fertig (max, 07.11.) · Zuständig: max (Call 07.11.)
- Braucht: P71 (Ticketpreis), P12 · Folgt auf: —
- Ziel: Das Spielziel: genug sparen, ein teures Ticket kaufen und mit einem Schiff vom Hangar aus heimfliegen – „das Ende der Game Jam“.
- Enthalten: Ticket-Terminal im Hangar (nur abends, wie jeder Handel), Kauf zum Ticketpreis (`HouseholdTuning.ticketPrice`, von P71 festgelegt),
  ins Schiff steigen, Abflug (man kann auch absichtlich aus der Map fliegen), Endbildschirm abhängig vom Familienzustand (P12).
- Nicht enthalten: Flugsteuerung über das Nötigste hinaus, weitere Enden (P12).
- Abnahme: Mit genug Geld abends Ticket kaufen → Schiff → Abflug → Ende; ohne genug Geld zeigt das Terminal, wie viel fehlt.
- Hinweis aus P71 (martin, 07.11.): Ohne Familienprüfung ist „Familie vernachlässigen“ der schnellste Weg zum Ticket (Tag 5). Das Ende muss den Familienzustand werten (P12/P69).
- Ergebnis (max, 07.11.): `Scripts/Departure/` – `TicketTerminal` (Hangar, X -4,5 Z -21, graue Box bis `SM_Hangar_TicketTerminal` geliefert ist; nur abends; Fehlbetrag; Dev `Dev_free_Ticket`), `ShuttleBoarding` (Rampe, „Leave Kestrel-9 for good?“), `ShuttleFlight` (Kopie des Shuttle-Modells, Verfolgerkamera, W/S Schub, Maus oder A/D, Shift Boost, keine Kollision; Cinemachine-Brain wird dafür abgeschaltet; Ende bei 320 m oder 45 s), `TicketEnding` (HOME / HOME, BARELY / TOO LATE? nach dem schlechtesten Familienzustand, Namen im Text), `DayManager.FinishRun` als gemeinsame Stelle für Enden (P12 hängt sich dort an). Builder **GameOff/Build Departure into Main** (in Rebuild All). Werte und Texte in `Data/Departure/DepartureTuning.asset`. Bestellt: `SM_Hangar_TicketTerminal`, `SFX_Shuttle_Engine_Loop`. Geprüft: Tests `DepartureTests`; WebGL-Build im Headless-Chrome: Terminal zeigt „You have 40 CR. You need 310 CR more“, Dev-Ticket, LEAVE, Flug durch die Barriere ins All (Bilder), nach 17 s Ende HOME. Kein Rebalancing nötig (Ticketpreis unverändert). Das Ticket ist im normalen Spiel erst mit mehr Tagen erreichbar (P15/P73).

### P73 — Endlose Tage
- Größe: M–L · Priorität: Muss (Vorschlag) · Status: fertig (martin, 07.11.) · Zuständig: martin (Call 07.11.)
- Zuschnitt (Martin, 07.11.): Reisende ab Tag 13 werden **erzeugt** (Pools für Namen, Spezies, Herkunft, Fracht-Stichworte, Grüße, Fehlertypen; Papiere passend zum Datum), Startwert **neu je Durchgang** (gleicher Startwert = gleiche Tage), **langsam schwerer** (Regeln 1→3, Reisende 6→8, gedeckelt um Tag 20).
- Hinweis (Call 07.11.): Ab Tag 13 **keine Figuren** mehr, nur normale Reisende aus Pools.
- Braucht: P09, P15, P68 · Folgt auf: P09
- Ziel: Das Spiel endet nicht mehr nach Tag 3: erst die geschriebenen Story-Tage (P15), danach eine Schleife aus Pools.
- Enthalten: Pools für Reisende, Regeln, Marktereignisse und Bar-Wünsche; jeder Tag nach der Story wird mit festem Startwert gezogen
  (testbar, gleiche Folge bei gleichem Start); `DayManager` endet nicht mehr nach dem letzten Tag; Briefe für die Schleife; Validator
  prüft gezogene Tage wie die geschriebenen.
- Nicht enthalten: Ticket/Abflug (P72), Enden (P12), neue Story-Inhalte (P15).
- Abnahme: 12 Tage am Stück spielbar ohne Fehler; Tag 4+ unterscheiden sich; Balance-Tests (P71) laufen über die gezogenen Tage.
- Ergebnis (martin, 07.11.):
  - `DaySchedule` hat ein Feld `loop` (DayPool): nach dem letzten Story-Tag zieht `DayGenerator` jeden Tag für den Startwert des Durchgangs
    (`DayManager.RunSeed`, neu je Durchgang, steht im Log). Der Tagesablauf endet nicht mehr, nur noch über ein Ende (Ticket, Game over).
  - Gezogen werden: Regeln (Herkunft gesperrt, Ware verboten, Quarantäne, Frachtbrief Pflicht; 1 → 3), Fahndungen (KAEL DROSS immer, dazu
    0–2 neue, manche „terminieren“), 6 → 8 Reisende mit Papieren passend zum Datum und einem Fehler, knifflige Varianten (gestern abgelaufen,
    ein Buchstabe/eine Ziffer falsch) 10 % → 50 %; Steigerung über 8 Tage. Jeder Reisende wird beim Ziehen mit den Regeln geprüft.
  - Dazu Marktereignis (35 %), Bar-Wunsch (50 %), Bestechung (25 %, 30–60 CR) und ein Brief aus 10 Briefen; „New today“ = neue Regel.
  - Inhalte in `GameOff/Build Day Pool` (`Data/Days/DayPool.asset`, läuft auch in Build Day Flow) – Werte dort ändern.
  - Rebalancing: Simulator zieht die Tage mit Startwert 7301; neue Ziele „30 Tage ohne Fehler: kein Aus, Familie nie krank“ und
    „1 Fehler am Tag: 30 Tage“ – alle ✓, bestehende Ziele unverändert.
  - Geprüft: 142/142 Tests (neu `EndlessDayTests`: 5 Startwerte × 30 Tage durch den Validator, gleicher Startwert = gleicher Tag, Datum,
    Steigerung); Main im Play-Modus Tag 12 → 13/14/25: Datum, Brief, Regeln, 6/6/8 Reisende mit Modell und Foto, 0 Fehler.
    Main und PF_DayFlow mussten nicht neu gebaut werden.
  - Offen: Gezogene Reisende haben keine Antworten auf Nachfragen (P40); Endlos-Briefe ohne Bezug zur Familie (Zustand kommt weiter darunter).

### P74 — TRAFFIC_LINE_DEV: beim Start aus, RETURN im Panel
- Größe: XS · Priorität: Kann · Status: fertig (martin, 07.11.)
- Braucht: P56 · Folgt auf: P56
- Ziel: Das Dev-Werkzeug für die Schiffslinien öffnet sich nicht mehr von selbst und lässt sich direkt verlassen.
- Enthalten: `GameSettings.TrafficLineDev` wird nicht mehr gespeichert (jeder Spielstart beginnt mit „aus“, ein alter gespeicherter Wert wird
  gelöscht); Knopf **RETURN** unten im Panel schaltet das Werkzeug aus und gibt die Steuerung zurück.
- Ergebnis (martin, 07.11.): Ursache war der gespeicherte Schalter (PlayerPrefs) – wer ihn einmal eingeschaltet hatte, bekam das Werkzeug bei
  jedem Start. Geprüft im Play-Modus: alter Wert „an“ gespeichert → Start mit „aus“; RETURN schließt das Panel, Steuerung zurück (Bild).

### P75 — Wiederkehrende Figuren: Gedächtnis, Auftritte, Folgen
- Größe: M–L · Priorität: Muss (Vorschlag) · Status: fertig (martin, 07.11.) · Zuständig: martin
- Braucht: P07, P09, P41 (Pinnwand), P68 (Markt-Ereignisse), P36 (Lage der Station) · Inhalt dazu: P76 · Beschlagnahme als Ausgang erst mit P08
- Ziel: Acht Figuren (Ablauf: `docs/FIGUREN.md`) kommen geskriptet wieder, erinnern sich, wie man sie behandelt hat, und ihre Geschichte verändert die Station.
- Enthalten:
  - **Figur** (`RecurringCharacter`, ScriptableObject in `Data/Characters/`): Id, Name, Spezies, Liste von **Auftritten**.
  - **Auftritt:** eigener `TravelerData` (eigene Papiere je Begegnung); **wann** = fester Tag oder Zeitfenster (von–bis, Spielraum);
    **Platz in der Schlange** = fest (z. B. Tag 2 als dritte Person) oder frei; **Bedingung** = was vorher passiert sein muss
    (lebt / geholfen / nicht verraten / beschlagnahmt / beliebig). Ist die Bedingung nicht erfüllt, kommt die Figur nicht (oder ein
    anderer Auftritt greift, z. B. „kommt zurück, um sich zu rächen“).
  - **Ausgang pro Auftritt, im Skript festgelegt** (Martin, 07.11.): Für jede Entscheidung (Durchlassen, Abweisen, Verhaften,
    Schleuse) steht im Auftritt, was sie für die Figur bedeutet: **geholfen / verraten / neutral / beschlagnahmt / tot**. Schleuse = tot,
    die Figur kommt nie wieder. Beschlagnahmt greift erst mit P08 (bis dahin zählt es wie verraten).
  - **Gedächtnis** (`CharacterMemory`, für den laufenden Durchgang, kein Speichern über den Neustart, wie alles andere): pro Figur alle
    Ausgänge mit Tag. Abfragen: lebt, letzter Ausgang, wie oft geholfen/verraten.
  - **Reaktion bei der nächsten Begegnung:** Begrüßung und Antworten je nach letztem Ausgang (Dank, Vorwurf, Misstrauen).
  - **Figuren reden über Ereignisse:** zusätzliche Begrüßungszeile je Markt-Ereignis des Tages (z. B. Grenze zu → „They closed the border
    behind me.“).
  - **Figuren lösen Ereignisse aus:** Ein Ausgang kann für morgen ein Markt-Ereignis setzen (Preis, Menge, `crisis`); es läuft
    wie die festen Ereignisse über Pinnwand, Durchsage, Gerücht des Barkeepers (P65) und die Lage der Station (P36).
  - **Fahndung zu einer Figur:** Ein Ausgang kann für einen späteren Tag einen Eintrag auf der Fahndungsliste setzen (verhaften /
    auf Sicht terminieren). Er hängt als **Steckbrief mit Passfoto** (`T_Doc_Photo_<Spezies>`) an der Pinnwand am Kontrolltisch und
    wird vom `RuleEvaluator` wie jede Fahndung geprüft – Steckbrief und richtige Entscheidung passen also immer zusammen.
  - **Einbau:** `CounterShift.BeginDay` setzt die fälligen Auftritte an ihren Platz in die Schlange; `DayManager` hängt ausgelöste
    Ereignisse an die Ereignisse des Tages (vor `Market.Begin`) und ausgelöste Fahndungen an die Fahndungsliste.
  - **Prüfung:** Validator spielt für jede Figur alle Zweige durch (jede Entscheidung bei jedem Auftritt) und prüft, dass die
    berechnete Entscheidung zur beabsichtigten passt; EditMode-Tests für Bedingung, Platz, Gedächtnis, ausgelöste Ereignisse/Fahndungen.
  - Dev-Werkzeug: Gedächtnis im Dev-Reiter anzeigen und setzen (zum Testen späterer Tage).
- Nicht enthalten: die Figuren und ihre Texte (P76), Beschlagnahme selbst (P08), Endbildschirme (P12).
- Abnahme: Eine Testfigur mit drei Auftritten: Tag 1 frei, Tag 2 als dritte Person nur wenn geholfen, sonst Tag 3 auf der Fahndungsliste;
  jeder Zweig spielbar, Begrüßung reagiert, ein ausgelöstes Ereignis ändert Preise und Lage der Station am nächsten Tag.
- Stand (martin, 07.11.):
  - `Scripts/Characters/`: `RecurringCharacter` (Figur mit Auftritten), `Appearance` (Reisender, Tag von–bis, Platz 0 = beliebig,
    Bedingung, Ausgang je Entscheidung, Begrüßung nach letztem Ausgang, Zeilen zu Markt-Ereignissen, Folgen), `CharacterMemory`,
    `CharacterPlanner` (wer kommt heute, wo in der Schlange; Zeitfenster: jeder Tag 1/(Tage übrig), am letzten Tag sicher; max. 3 am Tag,
    feste Plätze zuerst), `CharacterDirector` (in `PF_DayFlow`).
  - Haken: `CounterShift.PlanDay` (Tag vor dem Start ändern → `DayRules.RuntimeCopy` mit Schlange + Fahndungen, das Asset bleibt),
    `TravelerPresenter.GreetingFor`, `Market.Schedule` (ausgelöste Ereignisse laufen wie feste: Pinnwand, Durchsage, Lage der Station;
    der Barkeeper kennt sie fürs Gerücht). DayManager blieb unverändert (Claim max).
  - Folgen je Ausgang: Markt-Ereignis in n Tagen, Fahndung (Name der Figur, Anordnung, ab Tag, wie lange). Schleuse = tot, nie wieder.
  - Testfigur „Tess Marlo“ (`GameOff/Build Test Character`, läuft auch mit Build Day Flow), nur mit Dev-Tools aktiv. In einer Main-Kopie
    geprüft: Tag 1 abgewiesen → Tag 2 Kälteeinbruch (Fusionszelle 11,0 → 15,8 CR), Tess nicht da → Tag 3 als Erste, auf der Fahndungsliste,
    Regeln sagen Verhaften, Begrüßung „You sent me away…“. Der Zweig „geholfen“ (Tag 2 an Platz 3, Dank) ist per Test geprüft.
  - Tests `CharacterTests` (8, darunter der Validator: jeder Auftritt jeder Figur wird mit den möglichen Fahndungen richtig bewertet), 138/138 grün.
  - In Main eingebaut (07.11.): Tess steht an Tag 1 in der Schlange (mit Dev-Tools). **Achtung:** `Build Day Flow into Main` allein baut
    PF_DayFlow neu – danach zeigen `PF_Departure` (day, dialogs) und `PF_PauseMenu` (dayManager) ins Leere; `Rebuild All` macht es richtig
    (Departure und Menüs kommen danach), sonst Departure/Menu-Builder nachlaufen lassen. Hier von Hand neu verkabelt, 0 fehlende Verweise.
  - **Folgepakete nötig:** Steckbrief **mit Passfoto** an der Pinnwand
    (heute steht die Fahndung als Text auf dem WANTED-Zettel); Dev-Reiter fürs Gedächtnis (PF_PauseMenu bei max); Folgen per Brief/Paket
    ohne Begegnung (FIGUREN.md Punkt 6) und Bedingungen über andere Figuren/Familie (Punkt 5) – kommen mit P76, wenn die Inhalte es brauchen.
- Zusammenhang: P73 (endlos) zieht nach den Story-Tagen weitere Auftritte aus Zeitfenstern; P15 (Fälle) plant die Figuren-Auftritte
  zwischen den übrigen Reisenden ein; die bisherigen festen Ereignisse (Tag 2 Grenze, Tag 3 Schmuggler + Kälte) können ganz oder teilweise
  von Figuren ausgelöst werden (Entscheidung in P76).

### P76 — Die wiederkehrenden Figuren (8)
- Größe: M · Priorität: Muss (Vorschlag) · Status: fertig (martin, 07.11.) · Zuständig: martin
- Zuschnitt (Martin, 07.11.): P76 = alle 8 Figuren mit Auftritten, Papieren, Begrüßungen, Ereignissen und Fahndungen (was P75 kann),
  die neuen Markt-Ereignisse aus `docs/FIGUREN.md` mit Rebalancing und **12 Tag-Gerüste** (Datum, Grundregeln, Platzhalter-Reisende),
  die Max in P15 füllt. Was P75 noch nicht kann, kommt in **P85–P87**.
- Braucht: P75 · Folgt auf: — · Teil der Inhalte von P15 · Beschlagnahme-Zweige erst mit P08
- Ziel: Fünf Figuren mit kleinen Geschichten über die Tage, die zum Theme „Shift“ passen und Geld, Familie, Moral und Station verbinden.
- Enthalten: Figuren-Daten, Papiere je Auftritt, Begrüßungen/Antworten je Ausgang und Ereignis, ausgelöste Ereignisse und Fahndungen,
  Passfotos aus den vorhandenen Spezies-Modellen.
- Vorschlag (martin, 07.11.):
  1. **The Shifter** (Mensch, schon als `T_Test_06_Shifter`, DESIGN.md): jeden Tag neue Identität, erkennbar an einem Detail.
     Tag 1–2 harmlos; Tag 3 **immer** Steckbrief „auf Sicht terminieren“ (wie Max geplant hat). Schleuse = Befehl befolgt (Moral), verhaftet = Befehl
     verweigert, durchgelassen = er steckt dir etwas für die Familie zu. Der Faden der Hauptgeschichte.
  2. **Quill Barragh** (Mensch, Kurier des Barkeepers): Tag 1 sauber und freundlich; Tag 3 mit Spice im Gepäck und dem
     Bestechungszettel (P66). Durchgelassen = geholfen (Umschlag, später größere Aufträge); verhaftet/beschlagnahmt = verraten →
     **löst „Smugglers busted“ für den nächsten Tag aus** (statt fest an Tag 3), der Barkeeper schmollt.
  3. **Mira Talsk** (Quobb, Mutter auf dem Weg zu ihren Kindern – Spiegel des Spielers): Tag 2 während „Border closed“, Erlaubnis seit
     einem Tag abgelaufen. Durchgelassen = geholfen (Regelbruch, Verwarnung) → sie kommt später zurück und bringt Essen für deine Familie
     (FOOD gedeckt); abgewiesen = verraten → sie versucht es Tage später mit gefälschten Papieren (Verhaften richtig, aber bitter).
  4. **Dr. Iro Senn** (Xirrik, Ärztin): bringt Antibiotika ohne Einfuhrschein. Geholfen → wenn deine Familie krank ist, kommt sie mit
     Medizin wieder (MEDICINE gedeckt); beschlagnahmt (P08) = Antibiotika im Inventar (Schwarzmarkt), aber **löst „Clinic shortage“ aus**
     (Medizin teurer, Krise ↑) und sie kommt nie wieder.
  5. **Inspector Vaal Korr** (Sorvak, Prüfer der Behörde): **Tag 1 als erste Person – Tutorial** (Martin, 07.11.): Er erklärt den
     Schalter Schritt für Schritt (Papiere ziehen, Feld anklicken, Regeln an der Pinnwand, stempeln) und lässt sich selbst prüfen.
     Später kommt er **verdeckt** wieder (z. B. Tag 2 als dritte Person) mit einem kleinen, verlockenden Fehler: „Helfen“ heißt hier
     nach Vorschrift entscheiden → Lob per Durchsage, Lohnbonus; durchgelassen = Rüge (zusätzliche Verwarnung), er kommt unangekündigt
     wieder. Zeigt: Nicht jedes Wegsehen ist gut. Das Tutorial geht erst, wenn P75 steht (fester Platz 1 an Tag 1, eigene Texte je Schritt).
- Figuren und Ablauf werden mit Max genauer gemacht (Frage `figuren-und-ablauf`).
- Ergänzt (martin, 07.11.): drei weitere Figuren, die zwischen Tag 5 und 8 Stück für Stück dazukommen (**Tobin Rask**, **Nell Varga**,
  **Pim Oddo**), und der **Ablaufplan über 12 Story-Tage** (kürzbar) in `docs/FIGUREN.md` – dort stehen Auftritte, Ausgänge und Ereignisse.
- Offen: siehe `docs/FIGUREN.md` → „Offen“. Schmuggler-Razzia von Quill auslösen lassen statt fest an Tag 3 → nach Plan und neuen Figuren
  (mit Rebalancing).
- Abnahme: Jede Figur hat mindestens zwei Auftritte mit Reaktion auf den vorherigen Ausgang; jeder Zweig validiert; ein Durchgang
  über die Story-Tage zeigt alle acht.
- Ergebnis (martin, 07.11.):
  - `GameOff/Build Story Characters` (läuft auch in `Build Day Flow`): 8 Figuren in `Data/Characters/Story/`, 27 Auftritte mit eigenen
    Papieren (`Data/Travelers/Story/`, `Data/Documents/Story/`), Begrüßungen je letztem Ausgang, Zeilen zu Ereignissen, Folgen. Ablauf wie
    `docs/FIGUREN.md`, mit zwei Änderungen: **Razzia an Tag 6** (3 Tage nach Quills Verhaftung – an Tag 3/4 schmollt der Barkeeper, P66),
    und Korr kommt an Tag 4 **verdeckt als ODAN VREL** (Erlaubnis einen Tag abgelaufen; nach Vorschrift abweisen = geholfen).
  - **12 Tag-Gerüste** `Data/Days/Story/Day01–Day12` (Datum 2290-11-05 + Tag − 1, Grundregeln, Tag 3 zusätzlich ANTIBIOTICS verboten und
    der Shifter „PELL MARROW“ auf Sicht terminieren). Platzhalter-Reisende füllen jeden Tag auf **6 Begegnungen** (auf dem richtigen Weg
    durch die Figuren): Vesna, Darro, Ixa + ARIN DOLE, BEKA NUR, CORIN HASK. `DaySchedule` hat 12 Tage; Briefe 4–12 sind Platzhalter (P15).
  - Neue Markt-Ereignisse: Security lockdown, Clinic shortage, Dock strike, Inspection week (fest Tag 9), Oddo's caravan.
  - **Rebalancing:** Simulator spielt jetzt den richtigen Weg durch die Figuren (Begegnungen + ausgelöste Ereignisse); Razzia DRUG ×1,8 → ×2,0;
    Bar-Wunsch nur noch an Tag 5 dazu (2× LUXURY, vor der Razzia). Economy Report: alle Ziele ✓ (Clever Tag 7, Bett Tag 10, Gerücht 7,8 CR).
  - Geprüft: 137/137 Tests (Validator: jeder Auftritt jeder Figur mit möglichen Fahndungen richtig bewertet), Validate Travelers ok;
    Durchlauf in Main über 12 Tage mit den beabsichtigten Entscheidungen zeigt alle acht, Regeln = Absicht, Ereignisse am richtigen Tag, 0 Fehler.
  - Für P15 (max): Gerüste füllen, Figuren-Reisende in Ruhe lassen; Frachtzeilen mit den Fracht-Stichworten aus P82.
  - Offen in P85–P87: Geschenke/Lohn/Briefe/Bedingungen, Korr-Tutorial, Shifter-Bitte und Ende „Airlocked“; Verwahrung P77.

### P77 — Verwahrung: Figuren geben dir Dinge zur Aufbewahrung
- Größe: S–M · Priorität: Soll (Vorschlag) · Status: offen (im Call 07.11. bestätigt) · Zuständig: martin
- Braucht: P75 (Gedächtnis, Auftritte), P46 (Inventar), P11 (Händler) · Folgt auf: P75 · Inhalte in P76
- Ziel: Vertrauen wird greifbar: Eine Figur lässt dir etwas da, und du entscheidest, ob du es hütest oder zu Geld machst.
- Enthalten (Vorschlag):
  - Ein Auftritt kann einen Gegenstand **zur Verwahrung** übergeben (eigene Ware mit Label, z. B. `ENTRUSTED`, erkennbar im Inventar
    mit Name der Figur und Tag der Rückgabe).
  - **Verkaufen:** Gegenstand wie jede Ware beim Händler loswerden → sofort Geld; bei der nächsten Begegnung zählt das als
    **verraten** (die Figur reagiert, kommt ggf. nicht wieder oder setzt eine Folge aus P75).
  - **Verwahren und gegen Pfand herausgeben:** Gegenstand bis zum Auftritt behalten, dann der Figur zurückgeben; sie zahlt den
    **Pfandbetrag** (im Auftritt festgelegt) → zählt als **geholfen**.
  - Ist der Gegenstand bei der Rückgabe weg (verkauft, verloren, beschlagnahmt), reagiert die Figur entsprechend.
  - Werte (Pfandbeträge, Verkaufswerte) in den Daten; **jede Geldänderung braucht ein neues Rebalancing** (Economy Report).
- Offen: Wo wird zurückgegeben – am Schalter beim nächsten Auftritt oder abends (Handel nur nach der Schicht)? Darf man verwahrte Dinge
  per Drohne nach Hause schicken? Kostet das Verwahren etwas (Platz, Miete)?
- Nicht enthalten: Beschlagnahme (P08), neue Händler.
- Abnahme: Eine Figur gibt einen Gegenstand ab; Verkaufen bringt Geld und die Figur reagiert verraten; Verwahren bringt beim nächsten
  Auftritt den Pfand und die Figur reagiert geholfen.

### P78 — Bugreport-Knopf im Pausenmenü
- Größe: S · Priorität: Soll · Status: fertig (max, 07.11.; live seit 3177208) · Zuständig: max (Call 07.11.)
- Braucht: P49 · Folgt auf: —
- Ziel: Testspieler melden Bugs mit einem Klick als vorausgefülltes GitHub-Issue (Frage `bugreport-server`, Antwort B).
- Enthalten: Knopf „REPORT BUG“ im Pausenmenü (über `Build Menu Scenes`), öffnet `github.com/Juice-de-Orange/game-off-2026/issues/new` mit Titel und Text: Version (Commit, im Build eingebaut über `WebBuild`), Tag, Phase, Szene, Browser; Hinweis im Text, keine persönlichen Daten einzutragen.
- Nicht enthalten: eigener Server, Sammeln mehrerer Bugs im Spiel.
- Abnahme: In der Live-Version öffnet der Knopf ein neues Tab mit vorausgefülltem Issue samt richtigem Commit; das Spiel läuft weiter.
- Ergebnis (max, 07.11.): Knopf „REPORT BUG“ im Pausenmenü (zwischen OPTIONS und MAIN MENU, `MenuSceneBuilder`). `BugReport` baut den Link (Label `bug`, Titel „[Bug] Day n, Phase: “, Text mit Version + Commit-Link, Tag, Phase, Szene, Browser, Hinweis auf persönliche Daten). `BugReportButton` + `Plugins/WebGL/Browser.jslib` öffnen das Tab beim `pointerup` des Browsers (wie der Vollbild-Knopf), sonst blockt der Popup-Blocker. `WebBuild` setzt `bundleVersion` nur für den Build auf `0.1.0+<commit>` (GITHUB_SHA, sonst git). Geprüft: lokaler WebGL-Build in Headless-Chrome, echter Klick → neues Tab mit richtigem Issue (Commit 5e4839c, Day 1, Morning, Main), Spiel läuft nach RESUME weiter; Test `BugReportTests`, 109/109 grün. **Solange das Repo privat ist, sehen Spieler ohne Zugriff eine 404.** `tools/measure-webgl.mjs`: MAIN MENU im Pausenmenü liegt jetzt bei v = 0,769.

### P79 — DeliveryIntegrator: Icons und Dokumente nicht verstellen
- Größe: XS · Priorität: Soll · Status: fertig (max, 07.11.) · Zuständig: max (Call 07.11.)
- Braucht: — · Folgt auf: —
- Ziel: `GameOff/Integrate Asset Deliveries` (auch in Rebuild All) ändert nur, was er ändern soll.
- Enthalten: Waren-Icons (`Art/UI/Icons`, Pixel-Art, Point, Sprite Multiple) und Dokument-Texturen (`Art/Textures/Documents`) bleiben, wie sie sind; heute stellt der Integrator Icons auf Bilinear/Single und Dokumente auf Point (am 07.11. von Hand zurückgesetzt).
- Nicht enthalten: andere Importregeln.
- Abnahme: Integrator zweimal laufen lassen → `git status` zeigt keine geänderten `.meta` unter Icons und Documents.
- Ergebnis (max, 07.11.): `DeliveryIntegrator` überspringt `Art/UI/Icons` (UI-Regel) und `Art/Textures/Documents` (Welt-Regel; die setzte Point, `DocumentPrefabBuilder` wieder Bilinear – die beiden haben sich gegenseitig umgestellt). Zwei Batch-Läufe: „nothing to do“, keine geänderten `.meta`. Regel in `ASSETS.md`.

### P80 — Umschlag-Modell beim Bestechungszettel
- Größe: XS · Priorität: Kann · Status: offen · Zuständig: martin (Call 07.11.)
- Braucht: P66 · Folgt auf: P66
- Ziel: Der Bestechungszettel liegt sichtbar in `SM_Prop_BribeEnvelope` (geliefert und eingebaut am 07.11., Material `M_Prop_BribeEnvelope`).
- Enthalten: Modell am `NoteSpot` des `BribeNote` (EconomyBuilder), nur sichtbar, solange der Zettel liegt.
- Nicht enthalten: neue Mechanik.
- Abnahme: Tag 3 → Umschlag liegt links neben den Papieren, verschwindet mit dem Zettel.

### P81 — Schicht-Inventar
- Größe: S–M · Priorität: Muss · Status: fertig (martin, 07.11.) · Zuständig: martin (Call 07.11.)
- Braucht: P46 · Folgt auf: P46
- Ziel: Was während der Schicht beschlagnahmt wird, liegt in einem **eigenen Inventar am Tisch**, nicht im normalen Inventar
  (Frage `schichtinventar-beschlagnahme`, im Call bestätigt).
- Enthalten: zweites `Inventory` „Schicht“ (in `PlayerEconomy`), nur während der Schicht am Schalter offen (eigene Taste oder Fach am Tisch,
  Anzeige wie das Inventar aus P46); im Sitzen lässt sich das normale Inventar nicht öffnen; am Schichtende wird es geleert
  (was damit passiert, entscheidet P08). Dev-Knopf zum Befüllen mit Test-Ware.
- Nicht enthalten: die Wahl am Schichtende und Strafen (P08), Asservatenschrank (P32/P83).
- Abnahme: Test-Ware ins Schicht-Inventar → am Tisch sichtbar, normales Inventar dort gesperrt; nach der Schicht ist das Schicht-Inventar leer,
  das normale Inventar unverändert.
- Ergebnis (martin, 07.11.): `PlayerEconomy.ShiftItems` (zweites `Inventory`). Sitzend öffnet [I] dasselbe Inventar-Fenster als
  „SHIFT · CONFISCATED“; der Schalter (Klicks, Q/Esc, Umschauen) ruht solange (`CounterStation.SetInputBlocked`), Esc schließt nur das Fenster.
  Eigenes Inventar am Tisch weiterhin gesperrt. Schichtende (`CounterShift.ShiftEnded`) → `PlayerEconomy.ShiftClosing(inventory)` für P08,
  danach geleert. Roter Knopf `Dev_fill_Shift` (nur mit Dev-Tools) legt die Dev-Startwaren hinein. Gebaut von `Build Economy`. Tests:
  `ShiftInventoryTests` (3), 112/112 grün; im Play-Modus geprüft, Konsole 0 Fehler. Offen: Der Sitz-Hinweis unten nennt [I] noch nicht
  (Text kommt aus der Schalter-Verkabelung) – mit P08, wenn das Schicht-Inventar wirklich gefüllt wird.

### P82 — Warenliste 2: jede Ware mit Modell, Zweck und Icon
- Größe: M · Priorität: Muss · Status: fertig (martin, 07.11.) · Zuständig: martin (Call 07.11.)
- Braucht: P70, P71 · Folgt auf: P70
- Ziel: Alle Waren haben einen Grund, sie zu kaufen, zu beschlagnahmen oder zu verkaufen, und sehen im Inventar fertig aus
  (Frage `warenliste-2`, im Call: A).
- Enthalten:
  - **Icons an die Waren:** die 12 gelieferten `UI_Icon_Item_*` (P46) in `ItemData.icon`; Icons für die übrigen Waren und die neuen Waren
    bestellen (Stil wie Nova Cola, Entscheidung 07.11.).
  - **Gelieferte Modelle werden Waren:** Liquor → LUXURY · Weapon parts → CONTRABAND + WEAPON · Cash bundle → VALUABLE ·
    Alien specimen → ALIEN + CONTRABAND · Pill bottle → MEDICINE · UV lamp → GADGET (einmalig, mit P62); Clown shoes bleiben bei P63.
    `ItemData` bekommt ein Feld für sein Modell (z. B. Nova Cola → `SM_Prop_SodaCan`).
  - **Jede Ware bekommt einen Zweck:** teure Waren wirken stärker (z. B. Canned stew macht Geschwächte gesund), Antibiotics heilen „critical“
    (zusammen mit P69), Waffen und Wertsachen sind Ziele für Bar-Wünsche (P65) und kommen aus Beschlagnahme (P08) oder Bestechung (P66).
  - **Beschlagnahmte Varianten** (Call 07.11.): eigene Items „Confiscated …“ mit Label CONFISCATED für alles, was beschlagnahmt werden kann
    (Feld in `ItemData`, das auf die Variante zeigt); P08 benutzt sie.
  - **Fracht-Stichworte = Warennamen**, z. B. „Stimulant vials“ statt „Stim injector“ (Regeln und Reisende mit anpassen, Validate Travelers).
  - **Rebalancing:** Economy Report, bis alle Ziele ✓.
- Nicht enthalten: Beschlagnahme selbst (P08), Asservatenschrank (P32), Gadget-Funktionen (P16, P62).
- Abnahme: Jede Ware hat Icon (oder Platzhalter mit Bestellung), Modell-Verweis, mindestens einen Zweck und eine Quelle; Test prüft das;
  Validate Travelers und EditMode-Tests grün; Economy Report alle Ziele ✓.
- Ergebnis (martin, 07.11.):
  - **Zweck (Martin):** Waren wirken *nicht* stärker – sie sind eine **andere Quelle** als der Laden (z. B. beschlagnahmte Medizin nach Hause,
    wenn das Geld fehlt). Familienregeln unverändert. Jede Ware kann irgendwo verkauft werden und kommt von einem Händler oder aus der Fracht.
  - `ItemData`: neue Felder `model`, `cargoWord` (Fracht-Stichwort, ganzes Wort, groß), `confiscatedVariant`.
  - Neue Waren: Liquor 14 (LUXURY), Weapon parts 45 (CONTRABAND+WEAPON), Cash bundle 40 (VALUABLE), Alien specimen 50 (ALIEN+CONTRABAND),
    UV lamp 45 (GADGET, einmalig, Verkauf mit P62). Ohne Händler: Quelle ist die Fracht (P08). **Stim injector → Stimulant vials** (GUID gleich).
  - Modelle: Pill bottle = Painkillers, Data chip, Scanner, Liquor, Weapon parts, Cash bundle, Alien specimen, UV lamp, Nova Cola = SodaCan.
  - Icons: 12 gelieferte zugeordnet, 12 weitere bestellt (M022) mit Platzhaltern – alle 24 Waren haben ein Icon.
  - **22 CONFISCATED-Varianten** in `Data/Items/Confiscated/` (gleiche Labels + CONFISCATED, gleicher Preis/Icon), bei jedem Lauf von
    `Update Economy Data` neu abgeglichen. Fracht-Stichworte: RATION, PROTEIN, STEW, FRUIT, COLA, BATTERY, FUSION, FUSE, SYRUP, PAINKILLERS,
    ANTIBIOTICS, STIMULANT, SPICE, CHIP, PISTOL, WEAPON, WATCH, RING, TRINKET, CASH, LIQUOR, SPECIMEN.
  - Tests `GoodsListTests` (4), 123/123 grün; Economy Report unverändert, alle Ziele ✓; Validate Travelers ok.
  - Nicht enthalten / offen: Icons werden in Inventar und Handelsfenster noch nicht angezeigt (eigenes Paket); Bar-Wünsche nach Waffen/
    Wertsachen kommen mit den Tagesplänen (P15/P73).

### P83 — Asservatenschrank aufbrechen
- Größe: S · Priorität: Kann · Status: offen · Zuständig: martin (Call 07.11.)
- Braucht: P08, P32 · Folgt auf: —
- Ziel: Was mit „As ordered“ an den Schrank ging, ist nicht ganz verloren – mit Risiko kommt man wieder dran.
- Enthalten: Der Schrank bekommt ein eigenes Inventar (statt Löschen in P08); abends [E] am Schrank → aufbrechen (Chance, erwischt zu werden →
  Disziplinarstrafe, Werte im Tuning-Asset aus P08); Entnommenes bekommt die CONFISCATED-Variante (P82).
- Nicht enthalten: Minispiel zum Schlossknacken.
- Abnahme: „As ordered“ → Ware liegt im Schrank; Aufbrechen gibt sie (als CONFISCATED) ins Inventar oder bringt eine Disziplinarstrafe.

### P84 — Bugreport direkt im Spiel (Backend auf Geoffrey)
- Größe: M · Priorität: Soll · Status: fertig (max, 07.11.; live geprüft: Test-Issue #14 mit Labels angelegt und geschlossen) · Zuständig: max
- Braucht: P49, P78 · Folgt auf: P78 (ersetzt den GitHub-Link)
- Ziel: Tester melden Bugs ohne GitHub-Konto direkt im Spiel; daraus wird automatisch ein GitHub-Issue.
- Enthalten: Formular im Pausenmenü (Text + automatisch Version, Tag, Phase, Szene, Browser), kleines Backend auf Geoffrey
  (`deploy/reports/`, Python ohne Pakete, Container neben dem Spiel, `/api/report` über den Spiel-nginx), Issue per Bot-Token mit
  Labels `bug` + `player-report`, Begrenzung pro IP (nur im Speicher), CORS für den itch.io-Build; GitHub-Link aus P78 entfällt.
- Nicht enthalten: Screenshots, Antworten an den Tester, eigene Admin-Seite.
- Abnahme: In der Live-Version Text tippen → SEND → „Thanks“ mit Issue-Nummer, Issue liegt im Repo; zu viele Berichte → freundliche
  Ablehnung; ohne Backend lädt das Spiel trotzdem.
- Ergebnis (max, 07.11.): Backend `deploy/reports` (Python ohne Pakete, 12 Tests) läuft auf Geoffrey als `kestrel9-reports`; Spiel-nginx leitet `/api/report` weiter, Host-nginx setzt `X-Forwarded-For`. Begrenzung 5/10 min pro Client (IPv6 /64), 60/h gesamt, nur erfolgreiche zählen; CORS nur Kestrel + itch; `@` wird entschärft. Formular `BugReportForm` im Pausenmenü (P tippt, Esc/BACK zurück und bricht Senden ab). GitHub-Link aus P78 entfernt. Token: Max mit `python deploy/set-report-token.py`. Datenschutz auf maxlworks.com (EN/DE) ergänzt. Geprüft: Formular im WebGL-Build (Tippen inkl. „p“, Fehlermeldung ohne Backend), `curl` live.

### P85 — Figuren-Folgen ohne Begegnung und neue Bedingungen
- Größe: M · Priorität: Soll (Vorschlag) · Status: offen, Entwurf (martin, 07.11.) · Zuständig: martin
- Braucht: P75, P76 · Folgt auf: P75
- Ziel: Was `docs/FIGUREN.md` (Punkte 5 und 6) verlangt und P75 noch nicht kann.
- Enthalten: Geschenke an die Familie (Mira bringt Essen = FOOD heute Nacht gedeckt, Senn Medizin), Lohnbonus/Rüge per Durchsage (Korr),
  Zeile im Morgenbrief oder Paket (Shifter schickt etwas, Verhör-Durchsage, Zusammenfassung Tag 11), Bedingungen über die Familie
  („jemand krank“) und über andere (Nells Sohn = wen du verhaftet hast, sonst ein fester Reisender aus P15).
- Nicht enthalten: Tutorial (P86), Shifter-Ende (P87), Verwahrung (P77).
- Abnahme: Jede dieser Folgen ist mit der Testfigur oder einer echten Figur einmal spielbar; Tests für die neuen Bedingungen.

### P86 — Korr-Tutorial an Tag 1
- Größe: M · Priorität: Soll (Vorschlag) · Status: fertig (max, 08.11.) · Zuständig: max (umgetragen 08.11.)
- Braucht: P75, P76 · Folgt auf: —
- Ziel: Inspector Vaal Korr kommt an Tag 1 als Erster und erklärt den Schalter Schritt für Schritt (Papiere ziehen, Feld anklicken, Regeln an
  der Pinnwand, stempeln, Schublade); er lässt sich selbst prüfen.
- Enthalten: Schritt-Texte in der Sprechblase, die erst weitergehen, wenn der Schritt getan ist; überspringbar; nur an Tag 1.
- Abnahme: Neuer Spieler kommt ohne Vorwissen durch Korrs Prüfung.
- Ergebnis (max, 08.11.): Tafel oben im Sitz-HUD statt in der Sprechblase (die Blase verschwindet, wird von Antworten ersetzt und ist beim Blick zur Pinnwand unsichtbar). `Counter/CounterTutorial` + `TutorialProgress` (immer der erste offene Schritt, nichts wird blockiert), [N] überspringt. `CharacterPlanner.Queue`: feste Plätze stehen jetzt exakt, Korr ist an Tag 1 immer Erster. Geprüft: 155/155 EditMode-Tests, Validate Travelers ok, WebGL bis Schritt Pinnwand und Überspringen; Stempeln und Lade nicht automatisiert geprüft. **Abnahme als neuer Spieler: Max.**

### P87 — Shifter: Schlüsselentscheidung und Ende „Airlocked“
- Größe: S–M · Priorität: Muss (Vorschlag) · Status: fertig (martin, 08.11.) · Zuständig: martin
- Zuschnitt (Martin, 08.11.): Bitte an **Tag 6**: Der Shifter fragt am Schalter, ob du **dem Widerstand beitrittst** (Frage mit JOIN/REFUSE).
  Beigetreten → an Tag 7 kommt er als Erster, enthüllt sich als Spion, Sicherheit kommt, Ende **AIRLOCKED** am Schalter. Abgelehnt → Tag 7 Wendung ohne Ende.
- Braucht: P75, P76, P12 · Folgt auf: —
- Ziel: Call 07.11.: Der Shifter gibt sich als Widerstandskämpfer aus; wer ihm bei **einer** großen Bitte hilft, fliegt an Tag 7 durch die
  Schleuse (Ende „Airlocked“).
- Enthalten: Tag und Inhalt der Bitte (offen in `docs/FIGUREN.md`), die Wendung an Tag 7, das Ende.
- Abnahme: Mit Hilfe bei der Bitte endet Tag 7 mit „Airlocked“, sonst geht das Spiel weiter.
- Ergebnis (martin, 08.11.):
  - Figuren-System kann jetzt **Fragen am Schalter** (Auftritt: `questionTitle/Body`, JOIN/REFUSE-Knöpfe im Dialog, Antwortzeile in der
    Sprechblase; Zustimmen setzt ein **Flag**) und **Enden am Schalter** (`endingTitle/Body/Warning` → Hinweis, dann `DayManager.FinishRun`).
    Neue Bedingungen `FlagSet` / `FlagNotSet`.
  - Shifter Tag 6 („CASS ODEN“, saubere Papiere): „JOIN THE RESISTANCE?“. JOIN → Tag 7 kommt er als Erster („TOMAS REY“), enthüllt sich,
    „SECURITY TO CUSTOMS…“, Ende **AIRLOCKED**. REFUSE → Tag 7 Wendung ohne Ende („Es gibt keinen Widerstand … du hast bestanden.“).
  - Nur wer ihn an Tag 3 **nicht** terminiert, sieht Tag 6/7 (der Steckbrief bleibt der Test der Behörde) – der richtige Weg lässt ihn
    sterben, darum ändert sich am Economy Report nichts (alle ✓, unverändert).
  - Geprüft: 147/147 Tests (neu `ShifterTests`), Validate Travelers ok; Play-Modus Tag 6 → Frage → JOIN → Tag 7 → Hinweis → AIRLOCKED, 0 Fehler.
    Main/PF_DayFlow nicht neu gebaut (nur Daten + Skripte).

### P88 — Wegweiser zeigen zu den richtigen Räumen
- Größe: S · Priorität: Soll · Status: fertig (max, 08.11.) · Zuständig: max
- Braucht: — · Folgt auf: —
- Ziel: Wer durch die Station läuft, findet Schalter, Diner, Bar, Kiosk, Motel und Hangar über die Schilder.
- Befund (max, 07.11.): Alle drei Schilder (`SM_Hall_Sign_Staff/_Motel/_Bar`) zeigen „MOTEL“ – der Builder verschiebt den 6-Zeilen-Atlas
  `T_Hall_Sign_*` nicht. Außerdem ist das Modell ein beidseitiges Hängeschild (Pfeil zeigt von beiden Seiten in dieselbe Weltrichtung,
  nach lokal −X), hängt aber flach an der Wand.
- Enthalten: Material je Atlas-Zeile (wie `M_Motel_DoorSign_10x`); Hängeschilder quer zum Weg mit Pfeil zur richtigen Tür: in der Halle am
  Motel-Eingang (CUSTOMS, DINER, KIOSK, BAR) und vor der Dienst-Tür (MOTEL, DINER, BAR, KIOSK), im Dienstgang (CUSTOMS, HANGAR – STAFF ONLY);
  die drei Wandschilder fallen weg. Alles im `StationGrayboxBuilder`.
- Nicht enthalten: neue Schild-Modelle, Bodenmarkierungen.
- Abnahme: Jedes Schild zeigt den richtigen Raum, und sein Pfeil zeigt zur richtigen Tür (Bilder aus Main); Occlusion-Check grün.
- Stand (max, 07.11.): Code im `StationGrayboxBuilder` (`BuildWayfinding`, `HangSigns`, `HallSignMaterial`) und fünf Materialien `M_Hall_Sign_<Zeile>` sind gepusht. Geprüft in einer Sandbox-Kopie von Main (Bilder): Motel-Eingang CUSTOMS →, DINER →, KIOSK →, ← BAR (Rückseite gleiche Richtung); vor der Dienst-Tür ← MOTEL, ← DINER, BAR →, KIOSK →; Dienstgang CUSTOMS zur Schalter-Tür, HANGAR – STAFF ONLY zur Hangar-Tür. **Offen:** Build Station + Build Day Flow + Setup Main Lighting + Bake + Check Occlusion Bake, sobald Martin Main freigibt (P76). `PF_Station_Graybox` und Main sind bis dahin unverändert.
- Ergebnis (max, 08.11.): Über Rebuild All After Deliveries eingebaut, Licht und Occlusion neu gebacken. Bilder aus Main: alle acht Schilder zeigen den richtigen Raum, alle Pfeile zur richtigen Tür. Validate Travelers ok, 147/147 EditMode-Tests. **Occlusion-Check nicht ganz grün:** 1 von 3768 Ansichten weicht ab (103 von 36864 px, (-9 | 1,6 | 1), Blick nach oben), dieselbe Ansicht wie am 07.11.; on/off-Bild sehen gleich aus.

### P89 — Schaltertisch: Papiere nebeneinander, Lade, Deckel, Beschriftung
- Größe: M · Priorität: Muss · Status: fertig (max, 08.11.) · Zuständig: max
- Braucht: P06, P38 · Folgt auf: P06
- Ziel: Papiere lassen sich vergleichen, ohne eins wegzulegen; der Tisch ist auf einen Blick verständlich.
- Enthalten: mehrere Papiere und Pinnwand-Zettel gleichzeitig lesen (bis zu 4 in einer Reihe, [R] alle, Umschauen beim Lesen); Lade dreht die Front „DOCUMENTS“ zum Beamten; Deckel des Schleusenknopfs klappt zur richtigen Seite; Beschriftung ARREST und AIRLOCK.
- Nicht enthalten: neue Modelle, Tutorial (P86).
- Abnahme: Ausweis und Erlaubnis lassen sich nebeneinander lesen und per Feld-Klick befragen; Lade und Deckel sehen richtig aus; beide Knöpfe sind vom Stuhl aus lesbar beschriftet.
- Ergebnis (max, 08.11.): `DocumentDesk` hält bis zu 4 Papiere/Zettel in einer Reihe (Klick holt dazu, Klick aufs Papier legt es zurück, [R] alle, Esc alle zurück; mindestens 3 Plätze, damit ein einzelnes Papier den Tisch frei lässt). Umschauen beim Lesen geht, die Papiere folgen. Lade: Front zum Beamten, fährt entlang lokal +X. Beide Knöpfe um 180° gedreht, Deckel klappt Richtung Fenster auf, Beschriftung ARREST/AIRLOCK auf dem Tisch (`GameOff/Build Documents`). Geprüft: Validate Travelers ok, 150/150 EditMode-Tests, WebGL-Build mit Bildern von Lade, Deckel, Reihe, Umschauen.

### P90 — Fehler aus den GitHub-Issues, Runde 1
- Größe: M · Priorität: Muss · Status: teilweise (max, 08.11.): nur #21 wartet auf die Lieferung · Zuständig: max
- Braucht: — · Folgt auf: P89
- Ziel: Die klaren Fehler aus den Issues sind weg.
- Enthalten: #27 Airlock-Deckel klappt nicht mehr in den Tisch (Winkel +110, gemessen; schon mit P86 gebaut). #15 Sperr-Ton nur auf [E] an der gesperrten Tür, nicht beim Vorbeigehen. #17 Flackern an den Hangar-Wänden finden und beheben. #18 grüne Kisten im Hangar stecken nicht mehr ineinander. #21 Ladengitter füllt die ganze Front und ist entweder offen oder zu. #23 Figur sitzt auf dem Barhocker statt darin.
- Abnahme: Jeder Punkt ist im Spiel nachgeprüft; danach die Issues schließen (Max).
- Stand (max, 08.11.): #27 Deckel bleibt über dem Tisch (Winkel +110, gemessen). #15 gesperrte Türen: Ton nur auf [E] (`SlidingDoor`). #18 Kiste verschoben. #23 Dealer 13 cm höher, sitzt auf dem Hocker. #17 Mipmaps für alle Hangar-Texturen: 26–43 % weniger Flimmern bei 3 cm Bewegung (gemessen), **nicht ganz weg, Max prüft, ob es das gemeldete Flackern war**. #21 braucht ein neues Modell: `SM_Kiosk_Counter` auf rework, `SM_Kiosk_Shutter` bestellt; offen, bis geliefert. Geprüft: 155/155 EditMode-Tests, Validate Travelers ok, Occlusion wie bisher 1 Ansicht (105 px). #15 im Spiel nicht automatisiert geprüft.

### P91 — Reisende gehen wirklich, Verhaftung hinter dem Gitter
- Größe: L · Priorität: Soll · Status: fertig (max, 08.11.) · Zuständig: max
- Braucht: — · Folgt auf: P14, P40
- Ziel: Reisende kommen und gehen sichtbar durch die Türen; eine Verhaftung ist spürbar, ohne dass eine Wache zu sehen ist (#28).
- Enthalten: Reisende betreten die Kammer zu Fuß von außerhalb der Sicht und gehen nach der Entscheidung durch die passende Tür. Nur bei Verhaftung fällt ein Sichtgitter vor die Scheibe, dazu Schritte, Handschellen, Gerangel. Kein Gitter beim normalen Wechsel (Max, 08.11.). Sounds bestellen.
- Abnahme: Niemand erscheint oder verschwindet sichtbar aus dem Nichts; Verhaftung ist nur über Gitter und Ton erzählt.
- Ergebnis (max, 08.11.): Reisende kommen durch die Hangar-Tür, gehen nach dem Durchlassen durch die Hallentür, nach dem Abweisen zurück. Verhaftung und Schleuse hinter dem `SecurityShutter` (auch die Schleuse, sonst verschwände die Figur am Außenschott). Papiere kommen erst, wenn die Figur steht. Unsichtbare Sperre in den Kammertüren: Der Spieler kommt nicht hinein. Noch Platzhalter: graue Lamellen und vorhandene Töne, bestellt sind `SM_Counter_SecurityShutter`, `SFX_Arrest_Scuffle`, `SFX_Shutter_Drop`, `SFX_Shutter_Rise`. Geprüft: 155/155 Tests, Validate Travelers, Occlusion, WebGL (Hereinlaufen, ARREST + Gitter). Hinausgehen nach dem Stempeln nicht automatisiert geprüft.

### P92 — Diner außer Betrieb
- Größe: S · Priorität: Soll · Status: fertig (max, 08.11.) · Zuständig: max
- Braucht: — · Folgt auf: P26
- Ziel: Das Diner wirkt glaubwürdig geschlossen (#26) und lässt sich später wieder nutzen.
- Enthalten: Preise weg, Schild „OUT OF SERVICE“, Spinnweben, Licht aus. Die Tür bleibt offen, damit man die Stimmung sehen kann (Änderung beim Bau, 08.11.; Sperren ist `DoorAccess.Locked` in `Doors`).
- Abnahme: Das Diner verspricht nichts mehr, was es nicht hält.
- Ergebnis (max, 08.11.): Schild „OUT OF SERVICE“ über der Tür, Preistafel und Neon weg, Deckenröhren aus, Tresen ohne Leuchtstreifen, eine schwache Lampe, Spinnweben in den Ecken (`T_Diner_Cobweb_Albedo`, per Skript gezeichnet). `BuildDinerOutOfService` im `StationGrayboxBuilder`. Geprüft: Validate Travelers, EditMode-Tests, Bilder aus Main.

### P93 — Barkeeper sagt seinen Wunsch
- Größe: S · Priorität: Soll · Status: fertig (max, 08.11.) · Zuständig: max
- Braucht: — · Folgt auf: P65
- Ziel: Der Wunsch des Barkeepers kommt im Gespräch statt vom Schild (#22).
- Enthalten: Schild `BarWishSign` entfernt, Barkeeper nennt den Wunsch beim Ansprechen. Martin im Handover informieren (sein P65).
- Abnahme: Kein Schild mehr vor der Bar; der Wunsch ist beim Barkeeper zu erfahren.
- Ergebnis (max, 08.11.): `BarWishSign` entfernt, der Barkeeper nennt den Wunsch im Gespräch, auch tagsüber vor dem Öffnen. `SM_Bar_WishBoard` ist damit unbenutzt (Vermerk in `ASSET-REQUESTS.md`). Geprüft: Validate Travelers, EditMode-Tests, Bilder aus Main.

### P94 — Stimmen: 10 Laute je Spezies
- Größe: M · Priorität: Soll · Status: offen · Zuständig: martin (08.11., im Urlaub per Claude; Abnahme durch Max)
- Braucht: — · Folgt auf: P61
- Ziel: Figuren reden abwechslungsreich statt mit einem einzigen Ton (#16).
- Enthalten: Bei der Pipeline bestellen: je Spezies 10 kurze Murmel-Laute; zufällig abspielen, Tonhöhe leicht variieren.
- Abnahme: Keine zwei Äußerungen klingen gleich; Download-Zuwachs gemessen.

### P95 — Menüs im Terminal-Stil
- Größe: L · Priorität: Soll · Status: offen · Zuständig: max
- Braucht: — · Folgt auf: P02, P43, P46
- Ziel: Alle Menüs und Fenster sehen nach dem Spiel aus statt nach Standard (#20).
- Enthalten: Gemeinsamer Stil im Builder: dunkle Metallrahmen, CRT-Grün/Bernstein, Warnstreifen. Hauptmenü, Optionen, Pause, Inventar, Handel, Abrechnung. Berührt Martins Fenster, also vorher absprechen und claimen.
- Abnahme: Alle Fenster teilen denselben Stil und bleiben bedienbar.

### P96 — Lebendige Händler
- Größe: S · Priorität: Soll · Status: fertig (max, 08.11.) · Zuständig: max
- Braucht: — · Folgt auf: P52
- Ziel: Barkeeper, Holly und Dealer stehen nicht mehr starr da (#24).
- Enthalten: Kleines Skript: leichtes Atmen, Schwanken, ab und zu Kopfdrehung für die statischen Modelle.
- Abnahme: Figuren wirken in Ruhe lebendig, ohne neue Assets.
- Ergebnis (max, 08.11.): `Shops/IdleBreathing` + `Data/Economy/IdleBreathingTuning.asset`: Atmen, leichtes Schwanken und ab und zu eine kleine Drehung der ganzen Figur (die Modelle haben kein Skelett, einzelne Köpfe drehen sich also nicht). Geprüft: Validate Travelers, EditMode-Tests, Bilder aus Main.

### P97 — Eine gemeinsame Spieluhr
- Größe: S · Priorität: Soll · Status: offen · Zuständig: martin (08.11., im Urlaub per Claude; Abnahme durch Max)
- Braucht: — · Folgt auf: P09, P31
- Ziel: Alle Uhren zeigen dieselbe Spielzeit und laufen weiter (#19).
- Enthalten: Spielzeit aus der Phase: Morgen 06:00, Schicht 08:00–18:00 nach Fortschritt der Reisenden, Abend bis 23:00. Kein Einfluss auf Geld oder Regeln.
- Abnahme: Alle Uhren der Station stimmen überein und bewegen sich.

### P98 — Eigene Feld-Antworten für die Figuren an Tag 1–4
- Größe: S · Priorität: Soll · Status: offen · Zuständig: martin (08.11., im Urlaub per Claude)
- Braucht: P76 · Folgt auf: P15
- Ziel: Korr, Quill, Shifter, Mira und Senn antworten an Tag 1–4 auf Feld-Klicks persönlich statt mit der allgemeinen Zeile („It says ...“); die normalen Reisenden daneben tun das schon.
- Enthalten: 2–3 Antworten je Auftritt im `StoryCharacterBuilder` (`FieldAnswer`, wie `StoryDayContent`). Die Antworten dürfen nicht zum falschen Papier passen: Ein Feldschlüssel gilt für alle Papiere des Reisenden.
- Nicht enthalten: Tage 5–12, neue Mechanik.
- Abnahme: Validate Travelers und EditMode-Tests grün; keine Figur an Tag 1–4 antwortet mit der allgemeinen Zeile.

### P99 — Dealer sitzt richtig auf dem Hocker
- Größe: S · Priorität: Muss · Status: offen · Zuständig: offen
- Braucht: — · Folgt auf: P90
- Ziel: Der Dealer in der Bar sitzt sichtbar auf dem Hocker (#23, wieder geöffnet).
- Enthalten: Nach dem Anspielen 08.11.: Mit y 0,58 steckt der Sitz noch in seinem Schoß, er muss noch etwas höher (`VendorSpot` Dealer im `EconomyBuilder`). Danach im Bild von der Seite prüfen.
- Abnahme: Kein Hocker ragt durch die Figur, die Füße schweben nicht sichtbar.

### P100 — Reisende reagieren auf die Entscheidung
- Größe: M · Priorität: Soll · Status: offen · Zuständig: offen
- Braucht: — · Folgt auf: P40, P91
- Ziel: Wer abgewiesen wird, regt sich auf; wer durchkommt, ist erleichtert. Bisher sagt niemand etwas (Anspielen 08.11.).
- Enthalten: Eine Zeile nach dem Stempel in der Sprechblase, bevor die Figur geht: je Reisendem optional eigene Zeilen (`StoryDayContent`, Figuren je Auftritt), sonst ein Pool je Spezies für abgewiesen und durchgelassen. Darf das Tempo nicht bremsen.
- Abnahme: Abgewiesene beschweren sich hörbar lesbar, mit Abwechslung; die Schicht wird nicht spürbar länger.

### P101 — Durchgelassene sind wirklich in der Station
- Größe: L · Priorität: Soll · Status: offen · Zuständig: offen
- Braucht: — · Folgt auf: P91, P96
- Ziel: Wen man durchlässt, trifft man abends in der Station wieder (Anspielen 08.11., wichtig für die Immersion). Bisher nicht geplant.
- Enthalten: Entwurf, Umfang klären: Durchgelassene des Tages stehen oder sitzen abends in Halle und Bar auf festen Plätzen, mit Idle und einer Zeile beim Ansprechen. Offen: wie viele gleichzeitig (Leistung WebGL), ob sie laufen, ob Abgewiesene oder Verhaftete sichtbar fehlen.
- Abnahme: Mindestens ein am selben Tag Durchgelassener steht abends sichtbar in der Station.

### P102 — Dokument-Geräusche: leiser und abwechslungsreicher
- Größe: S · Priorität: Soll · Status: offen · Zuständig: offen
- Braucht: — · Folgt auf: P61, P89
- Ziel: Die Papier-Geräusche nerven nicht mehr (Anspielen 08.11.).
- Enthalten: Lautstärke von `paperPickup` und `paperSlide` senken, Varianten bestellen und zufällig mit leichter Tonhöhe abspielen. Ton nach Gehör: Max.
- Abnahme: Viele Papiere nacheinander klingen unaufdringlich.
