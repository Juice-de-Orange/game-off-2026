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

## Reihenfolge (Stand 04.11. abends, max)
Der Kern-Loop wird in dieser Reihenfolge fertig; wer frei ist, nimmt das nächste offene Muss-Paket.
1. **P41** Regeln an der Pinnwand (Bulletin, Fahndungsliste, Durchsagen, im Sitzen umschauen) — ohne das kennt der Spieler die Regeln nicht
2. **P40** Reisende in der Prüfkammer & Gespräch (NPC am Fenster, Sprechblasen mit Alien-Übersetzung, Nachfragen per Feld-Klick)
3. **P08** Entscheidungen & Moral (Geld/Abzug aus `CounterShift.DecisionMade`, Moral, Beschlagnahme, Schleusen-Effekt)
4. **P09** Tagesablauf (3 Tage, `CounterShift.BeginDay`/`ShiftEnded`) → **P10** Geld, Familie & Abrechnung
5. **P15** Inhalte für 3 Tage inkl. Story von **The Shifter** (siehe `DESIGN.md`) → **P12** Enden
6. Parallel: **P02** Startbildschirm (martin, fertig) → **P43** Optionen in Reitern → **P18** Audio-Grundgerüst (martin, fertig) →
   **P46** Geld, Inventar & Handelsfenster (martin) – **P08, P10, P11, P13 erst danach**, weil sie darauf aufbauen; **P19** WebGL-Build & itch-Upload, **P42** Lieferungen einbauen (max, sobald die Pipeline liefert)
7. Danach Soll-Pakete (P13, P14, P16, P25–P36) nach Zeit.

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
- Größe: M · Priorität: Muss · Status: offen
- Braucht: P07, P46 · **erst bauen, wenn P46 fertig ist** (Grundstruktur für Geld und Inventar)
- Ziel: Durchlassen/Abweisen/Verhaften/Luftschleuse mit Folgen.
- Enthalten: Lohn/Abzug je Entscheidung, Beschlagnahme bei Verhaftung (Gegenstände ins Inventar),
  versteckter Moral-Wert (sinkt bei Luftschleuse, stark bei Unschuldigen), einfache Effekte (Wache, Schleuse)
- Nicht enthalten: Enden (P12)
- Abnahme: Alle vier Aktionen verändern Geld/Moral/Inventar korrekt.
- Festgelegt im Asset-Durchgang (max, 02.11.): Luftschleuse: Außenschott **klappt nach außen ins All** (Rework mit Angel bestellt), Sog, Warnlampe dreht rot, Reisender wird hinausgerissen. Verhaftung: Warnlampe rot. Beschlagnahmtes landet physisch im Asservatenschrank (P32).

### P09 — Tagesablauf
- Größe: M · Priorität: Muss · Status: in Arbeit (martin, 05.11.)
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

### P10 — Geld, Familie & Abrechnung
- Größe: M · Priorität: Muss · Status: offen
- Braucht: P09, P46 · **erst bauen, wenn P46 fertig ist** (Grundstruktur für Geld und Inventar)
- Ziel: Abrechnungsbildschirm am Tagesende wie ein Haushaltsbuch.
- Enthalten: Lohn, Miete, Familienbedarfe (Essen, Wärme, Medizin) zum Ankreuzen, Zustand der Familienmitglieder,
  Folgen bei Nichtbezahlen, Werte in einem ScriptableObject
- Nicht enthalten: Pakete schicken (P13)
- Abnahme: Geld fließt über 3 Tage korrekt, Familie reagiert auf Nichtbezahlen.

### P11 — Läden: Gadget-Shop & Bar-Schwarzmarkt
- Größe: M · Priorität: Muss · Status: offen
- Braucht: P03, P08, P46 · **erst bauen, wenn P46 fertig ist** (Grundstruktur für Geld und Inventar)
- Ziel: Kaufen und Verkaufen über ein gemeinsames Shop-System.
- Enthalten: Shop-UI, Bar (illegale Gegenstände kaufen, Beschlagnahmtes verkaufen), Dokumentenscanner kaufbar
- Nicht enthalten: Gadget-Funktion (P16)
- Abnahme: Scanner kaufen, beschlagnahmtes Item an der Bar verkaufen, Geld stimmt.
- **Hinweis vor Beginn (martin, 05.11.):** Das Shop-UI ist mit **P46** schon fertig (Handelsfenster, Kaufen/Verkaufen, Faktoren, abgelehnte
  Labels). **Noch offen und zuerst zu lösen:** Die Ware beim Händler **wird beim Kauf nicht weniger** – das Sortiment ist in P46 bewusst
  unbegrenzt (Bug-Liste **B04**). Für P11 vorgesehen (Zuschnitt im Kern-Loop-Call, siehe Frage an max):
  - **Händler-Inventar:** begrenzter Vorrat pro Gegenstand, sinkt beim Kauf, füllt sich morgens auf; Verkauftes landet beim Händler
  - **Preisliste:** echte Gegenstände und Preise für Holly, Barkeeper und Dealer statt der Test-Inhalte (evtl. Preisfaktor pro Tag)
  - **Angebote:** Tagesangebote/Rabatte, Ware nur an bestimmten Tagen, Sortiment pro Tag (z. B. Dealer an Tag 3 anders)
  - evtl. Teilung: **P11 Kiosk** (Muss) + **Schwarzmarkt** als eigenes Soll-Paket

### P12 — Enden
- Größe: S · Priorität: Muss · Status: offen
- Braucht: P10, P08
- Ziel: Nach Tag 3 oder bei Niederlage ein Endbildschirm.
- Enthalten: Niederlage (Familie zerbrochen / gefeuert), mindestens 2 Enden je nach Moral & Familie
- Abnahme: Jedes Ende lässt sich gezielt erspielen.

### P13 — Hangar: Pakete an die Familie
- Größe: S · Priorität: Soll · Status: offen
- Braucht: P10, P11, P46 · **erst bauen, wenn P46 fertig ist** (Grundstruktur für Geld und Inventar)
- Ziel: Gegenstände aus dem Inventar per Frachtdrohne nach Hause schicken.
- Enthalten: Versand-UI im Hangar, Wirkung auf Familienbedarf, Risiko bei Schmuggelware
- Abnahme: Ein Medikament schicken senkt den Medizinbedarf am Abend.

### P14 — Reisende laufen
- Größe: M · Priorität: Soll · Status: offen
- Braucht: P04, P07
- Ziel: Reisende kommen aus dem Hangar zum Schalter und gehen danach weg (NavMesh).
- Abnahme: Eine Schicht läuft mit sichtbar ankommenden Reisenden.

### P15 — Inhalte: Fälle für 3 Tage
- Größe: M · Priorität: Muss · Status: offen
- Braucht: P07
- Ziel: Alle Reisenden, Regeln, Briefe und Texte für 3 Tage (~15 Fälle, Englisch).
- Abnahme: Kompletter Durchlauf 10–15 Minuten ohne Platzhaltertexte.

### P16 — Gadget: Dokumentenscanner
- Größe: S · Priorität: Muss · Status: offen
- Braucht: P06, P11
- Ziel: Gekaufter Scanner markiert gefälschte Felder auf Dokumenten.
- Abnahme: Mit Scanner wird ein gefälschtes Feld sichtbar markiert.

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
- Größe: S · Priorität: Muss · Status: offen
- Braucht: P01
- Ziel: Erster Build auf itch.io (Entwurf) bis 05.11., danach wiederholbarer Ablauf.
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
- Ziel: Lebendiges All und sichtbarer Schichtbeginn.
- Enthalten: Großschiffe (Frachter, Tanker, Patrouille, kleine Pendler) auf festen Bahnen mit Zufallsabständen; Shuttle fliegt durch die
  Barriere ein/aus (`ShuttleTraffic.Arrive()/Depart()`, Barriere pulst), Schnittstelle für P09 (Tagesablauf ruft Arrive/Depart)
- Abnahme: Alle paar Minuten fliegt etwas sichtbar vorbei; Arrive/Depart per Testknopf im Editor sichtbar.
- Hinweis (martin, 05.11.): Für die vorbeifliegenden Schiffe gibt es mit **P47** schon den Baustein `ShipTraffic` (Bahnen, Schiffsliste,
  Zufallsabstände, Obergrenze, Wiederverwenden, keine Lichter/Schatten) samt Tuning-Asset – für Main nur ein eigenes Tuning (seltener)
  und eine Instanz anlegen. Alle Schiffsmodelle (`Art/Models/Ships/`) sind geliefert.
- Festgelegt im Asset-Durchgang (max, 02.11.): Shuttle mit **Rampe** (Rework bestellt): landen → Rampe klappt zischend herunter → Reisende steigen nacheinander aus und stellen sich an; abends Rampe hoch und Abflug.

### P26 — Details & Ausstattung
- Größe: M · Priorität: Soll · Status: offen
- Braucht: P22 und Lieferungen
- Ziel: Station wirkt bewohnt und heruntergekommen.
- Enthalten: Rohre, Kabel, Lüftungen, Verteilerkästen, Feuerlöscher, Müll, Pflanzen, Schilder/Poster/Schmutz-Decals, Putzroboter (fährt per
  Skript), Leuchtschilder, Ventilatoren, Uhr, Lautsprecher, Münzfernrohr und Bänke auf der Plattform, Gerüst und Kran im Hangar
- Abnahme: Jeder Raum hat sichtbare Details; Draw Calls/Build-Größe im WebGL-Rahmen (Build < 40 MB).

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

### P30 — Getränkeautomat
- Größe: S · Priorität: Soll · Status: offen
- Braucht: P10 (Geld), P11 (Inventar) · Assets: `SM_Hall_VendingMachine` (da), `SM_Prop_SodaCan` (bestellt)
- Ziel: Kleine Versuchung, Geld auszugeben.
- Enthalten: [E] "NOVA COLA – 2 Credits": Geld weg, Dose fällt rumpelnd heraus, ins Inventar; nach Hause schicken (P13) oder am Schalter
  trinken (kleiner Bonus, z. B. Schichtzeit kurz langsamer)
- Abnahme: Kaufen, Inventar, Wirkung und Versand funktionieren.

### P31 — Anzeigesäule mit Live-Ankunftsplan
- Größe: S · Priorität: Soll · Status: offen
- Braucht: P09 · Assets: `SM_Hall_DepartureBoard` (da)
- Ziel: Auf einen Blick sehen, wie viel Schicht noch kommt.
- Enthalten: TextMeshPro auf den CRT-Flächen: nächstes Shuttle, Reisende in der Schlange, Tag/Schicht-Uhr, Warnungen ("SECURITY LEVEL RAISED")
- Abnahme: Anzeige folgt dem Spielstand.

### P32 — Asservatenschrank mit echten Gegenständen
- Größe: M · Priorität: Soll · Status: offen
- Braucht: P08 · Assets: `SM_Counter_Locker` (da), `SM_Item_*` (6, bestellt)
- Ziel: Beschlagnahmtes ist greifbar.
- Enthalten: [E] Türen schwingen auf, Gegenstände liegen sichtbar in den Fächern, Rausnehmen = Inventar (Bar verkaufen / nach Hause schicken)
- Abnahme: Verhaftung legt Gegenstände in den Schrank, man kann sie entnehmen.

### P33 — Pinnwand, Lautsprecher & Warnlampe
- Größe: M · Priorität: Soll · Status: offen
- Braucht: P07, P09 · Assets: `SM_Counter_Board`, `SM_Counter_Speaker`, `SM_Counter_WarningLamp` (da)
- Ziel: Der unsichtbare Vorgesetzte und die Regeln sind im Raum präsent.
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
- Größe: S · Priorität: Soll · Status: offen
- Braucht: P09, P20
- Ziel: Der Zustand der Station verschiebt sich über die Tage (Theme "Shift").
- Enthalten: Neonschild "THE LAST ORBIT" flackert (ein Buchstabe fällt aus); an Tag 3 fallen Schilder und Lichter häufiger aus;
  `LightFlicker` wiederverwenden, Stärke pro Tag
- Abnahme: Tag 1 ruhig, Tag 3 sichtbar instabil.

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
- Größe: M · Priorität: Muss · Status: offen (für **max** reserviert)
- Braucht: P07 · Folgt auf: —
- Ziel: Man hat ein Gegenüber: der Reisende steht in der Prüfkammer am Fenster und redet.
- Enthalten: NPC-Modell (SK_NPC_*) erscheint am Fenster und geht nach der Entscheidung durch die richtige Tür ab (einfaches
  Ein-/Ausblenden, Laufen ist P14); Sprechblase über dem Reisenden mit Begrüßung; Alien-Spezies sprechen erst in Glyphen, die per
  „Entschlüsseln“-Welle (links nach rechts, ~0,4 s/Zeile, leichter Cyan-Schimmer) übersetzt werden; Nachfragen per Klick auf ein Feld
  des hochgehaltenen Dokuments (Antwort aus `TravelerData`)
- Abnahme: Jeder Test-Reisende steht in der Kammer, begrüßt, antwortet auf Feld-Klicks; Aliens werden lesbar „übersetzt“.

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
- Größe: S · Priorität: Soll · Status: offen (für **max** reserviert, sobald geliefert)
- Braucht: Lieferungen zu M015 · Folgt auf: P39, P38, P06, P07
- Ziel: Die Platzhalter von heute werden durch die bestellten Modelle und Grafiken ersetzt.
- Enthalten: PRs mergen (je Lieferung ein neuer Branch + PR, siehe `ASSETS.md`), `GameOff/Integrate Asset Deliveries`, dann die
  Builder neu laufen lassen: `Build Station Graybox` (Außenhülle, Dachplatten, Hangar-Rippen, Greebles), `Build Sky & Exterior`
  (Knoten, Andockflansche, Solarmasten, Antennensockel), `Build Documents` (Dokument-Hintergründe, Verwarnzettel, Drucker-Modell
  ersetzt die graue Box); Stempel-Abdrücke `T_Stamp_*`; Licht neu backen; Achsen/Pivots prüfen (Builder rechnen mit den Einträgen)
- Nicht enthalten: neue Funktionen
- Abnahme: Rundflug außen und Blick auf den Schaltertisch ohne graue Platzhalter; Konsole sauber; Tests grün.


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
- Stand (max, 05.11.): **Live unter https://kestrel9.maxlworks.com** (Ablauf, Rollback, Secrets: `docs/DEPLOY.md`).
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
- Größe: S · Priorität: Kann · Status: offen, **vorgemerkt, noch nicht bauen und nichts bestellen** (Idee max, 05.11.)
- Braucht: — · Folgt auf: M014 (Katzenbild)
- Ziel: Das Zimmer des Spielers fühlt sich persönlich an.
- Enthalten: Bullauge größer (heute Ø 0,6 m, `SM_Station_Wall_Porthole_2x3`, Zimmer 103); Katzenbild etwas höher (`SM_Motel_CatPainting`, heute y 1,3 m, `StationGrayboxBuilder.BuildProps`); neues Gemälde vom Hund **Elo** (Bestellung erst, wenn das Paket dran ist; dann fragt Claude Max nach einem Foto als Vorlage, wie beim Katzenbild)
- Nicht enthalten: weitere Möbel, andere Zimmer
- Abnahme: Alle drei Punkte im Zimmer zu sehen; Licht neu gebacken; das Bullauge passt zum Außenmodell.
- Hinweis: Größeres Bullauge heißt neues Wandmodul über die Pipeline und neuer Lichtbake. Wird das Wohnmodul (P53) gebaut, zuerst dort klären, wo Zimmer 103 am Ende liegt.

### P52 — Mehr Figuren: Händler an Kiosk und Bar
- Größe: M · Priorität: Soll · Status: offen (Idee max, 05.11.)
- Braucht: P46 · Folgt auf: P46
- Ziel: Statt grauer Boxen stehen echte Figuren an Kiosk und Bar.
- Enthalten: `SM_NPC_Holly.fbx` (ältere Frau), `SM_NPC_Barkeeper.fbx` (vierarmiges Alien mit Atemmaske, Schlauch zum Rucksack), `SM_NPC_Dealer.fbx` (sitzt shady auf einem Barhocker, dunkle, abgewetzte Kleidung mit Kapuze), Beschreibungen von Martin (Handover 05.11.); Einträge in `ASSET-REQUESTS.md`; nach der Lieferung `GameOff/Build Economy into Main`
- Nicht enthalten: Dialoge, Animationen über eine Idle-Haltung hinaus, weitere Bewohner (kommen mit P53)
- Abnahme: Die drei Figuren stehen an ihren Plätzen statt der Boxen, Handel funktioniert unverändert.
- Stand 05.11.: **Bei der Pipeline bestellt** (drei Einträge `SM_NPC_*` in `ASSET-REQUESTS.md`, `needed_by` 16.11.). Das Paket wartet auf die Lieferung, Einbau macht max.

### P53 — Wohnmodul begehbar: Zimmer für alle Figuren
- Größe: L · Priorität: Soll · Status: offen, **erst Entwurf, dann Entscheidung** (Idee max, 05.11.)
- Braucht: Entwurf + Absprache max/martin (Umfang gegen Feature-Freeze 22.11.) · Folgt auf: P04, P27
- Ziel: Das Motel hat genug Platz, und jede Figur kann ein eigenes Zimmer haben, was die Immersion trägt.
- Enthalten: großer Blender-Auftrag an die Pipeline: Innenraum des Wohnmoduls (Gänge, Zimmer) und eine begehbare Verbindung von der Station dorthin; Einbau über die Builder inkl. NavMesh und Lichtbake
- Nicht enthalten: Tagesabläufe der Bewohner, Inhalte der Zimmer über die Grundausstattung hinaus
- Abnahme: Der Spieler läuft ohne Ladezeit von der Station ins Wohnmodul, die Zimmer sind erreichbar, von außen stimmt das Modell mit innen überein, und der WebGL-Build bleibt flüssig.
- Ausgangslage (05.11.): Die Motelzimmer 101–103 liegen im Hauptblock (x −20..−8, z 0..6, Höhe 3 m). Das Wohnmodul existiert nur als Außenmodell `SM_Ext_HabModule` (liegender Zylinder 12 × 12 × 30 m, zwei Stück: `Hab_West`, `Hab_East` in `SkyExteriorBuilder`) ohne Innenraum.
- Im Entwurf zu klären: Welches Modul, wie viele Zimmer; ob das Innere den Außenzylinder ersetzt oder hineinpasst; wo die Verbindung ansetzt (Motelgang-Westende?) und wie sie von außen aussieht; bleibt Zimmer 103, wo es ist, oder zieht es um (Laufwege, Bett, `PF_DayFlow`, P51); was Größe und Lichtbake für den WebGL-Build bedeuten (heute 19 MB).
