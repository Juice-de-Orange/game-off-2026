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
- Größe: S · Priorität: Muss · Status: in Arbeit (max, 02.11.)
- Braucht: — · Folgt auf: —
- Ziel: Unity-Projekt steht, beide können klonen und öffnen.
- Enthalten: Unity 6000.3.23f1, Vorlage Universal 3D (URP), Ordnerstruktur nach `CLAUDE.md`/`docs/ASSETS.md`,
  Pakete (Input System, Cinemachine 3, ProBuilder, AI Navigation, TextMeshPro, DOTween), WebGL-Einstellungen
  (WebGL 2, Brotli + Decompression Fallback, WebAssembly 2023, IL2CPP, Strip Engine Code), Szenen Boot/Menu/Main,
  Sandbox-Szenen `Max_Sandbox`/`Martin_Sandbox`, `M_Placeholder.mat`
- Nicht enthalten: Gameplay
- Abnahme: Leerer WebGL-Build läuft lokal im Browser, beide haben das Projekt geöffnet.
- Stand 02.11. (max): Projekt steht und ist gepusht. WebGL-Build (14,9 MB) startet lokal im Browser ✔ (von Max bestätigt). Offen: Martin öffnet das Projekt einmal → dann `fertig`.

### P02 — Startbildschirm & Menü
- Größe: M · Priorität: Muss · Status: offen (für **martin** reserviert)
- Braucht: P01
- Ziel: Alles vom Spielstart bis zum Menü.
- Enthalten: Boot-/Splash-Screen, Hauptmenü (Start, Optionen: Lautstärke/Maus-Empfindlichkeit, Credits, Beenden
  ausgeblendet im Web), Übergang ins Spiel (`Main`), Pausemenü (Esc)
- Nicht enthalten: Spielinhalte, finale Grafik (Platzhalter)
- Abnahme: Start → Menü → Spiel → Pause → zurück ins Menü funktioniert im WebGL-Build.

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
- Größe: L · Priorität: Muss · Status: offen
- Braucht: P05
- Ziel: Dokumente als scharfe 2D-Karten auf dem Tisch, mit der Maus ziehbar, Daten kommen aus ScriptableObjects.
- Enthalten: 2–3 Dokumenttypen (z. B. Ausweis, Einreiseerlaubnis, Frachtliste), Drag & Drop, Rückgabe an den Reisenden
- Nicht enthalten: Prüflogik (P07), Gadgets
- Abnahme: Ein Test-Reisender legt Dokumente hin, man kann sie lesen, ziehen und zurückgeben.
- Festgelegt im Asset-Durchgang (max, 02.11.): Dokumente kommen über die **animierte Durchreiche** (Schublade fährt mit Klacken herein, Papiere gleiten auf den Tisch; Rückgabe = in die Schublade ziehen, sie fährt hinaus). Modelle: `SM_Counter_DocSlot`, `SM_Station_Window_Small_Slot`.

### P07 — Reisende & Regeln
- Größe: L · Priorität: Muss · Status: offen
- Braucht: P06
- Ziel: Datenmodell und Prüflogik: Ist der Reisende korrekt?
- Enthalten: `TravelerData` (Aussehen, Dokumente, eingebaute Fehler, Gepäck), `DayRules` (Regeln pro Tag),
  Auswertung richtig/falsch je Entscheidung, Regel-Bulletin als Dokument
- Nicht enthalten: Inhalte für alle Tage (P15), Bewegung der Reisenden (P14)
- Abnahme: 3 Test-Reisende mit unterschiedlichen Fehlern werden korrekt bewertet.

### P08 — Entscheidungen & Moral
- Größe: M · Priorität: Muss · Status: offen
- Braucht: P07
- Ziel: Durchlassen/Abweisen/Verhaften/Luftschleuse mit Folgen.
- Enthalten: Lohn/Abzug je Entscheidung, Beschlagnahme bei Verhaftung (Gegenstände ins Inventar),
  versteckter Moral-Wert (sinkt bei Luftschleuse, stark bei Unschuldigen), einfache Effekte (Wache, Schleuse)
- Nicht enthalten: Enden (P12)
- Abnahme: Alle vier Aktionen verändern Geld/Moral/Inventar korrekt.
- Festgelegt im Asset-Durchgang (max, 02.11.): Luftschleuse: Außenschott **klappt nach außen ins All** (Rework mit Angel bestellt), Sog, Warnlampe dreht rot, Reisender wird hinausgerissen. Verhaftung: Warnlampe rot. Beschlagnahmtes landet physisch im Asservatenschrank (P32).

### P09 — Tagesablauf
- Größe: M · Priorität: Muss · Status: offen
- Braucht: P03
- Ziel: Morgen → Schicht → Feierabend → Schlafen → nächster Tag, 3 Tage.
- Enthalten: `DayManager`, Morgen-Brief + Bulletin, Schichtende nach Anzahl Reisender, Bett in der Unterkunft beendet den Tag
- Nicht enthalten: Abrechnung (P10)
- Abnahme: Drei Tage hintereinander durchspielbar (mit Test-Reisenden).

### P10 — Geld, Familie & Abrechnung
- Größe: M · Priorität: Muss · Status: offen
- Braucht: P09
- Ziel: Abrechnungsbildschirm am Tagesende wie ein Haushaltsbuch.
- Enthalten: Lohn, Miete, Familienbedarfe (Essen, Wärme, Medizin) zum Ankreuzen, Zustand der Familienmitglieder,
  Folgen bei Nichtbezahlen, Werte in einem ScriptableObject
- Nicht enthalten: Pakete schicken (P13)
- Abnahme: Geld fließt über 3 Tage korrekt, Familie reagiert auf Nichtbezahlen.

### P11 — Läden: Gadget-Shop & Bar-Schwarzmarkt
- Größe: M · Priorität: Muss · Status: offen
- Braucht: P03, P08
- Ziel: Kaufen und Verkaufen über ein gemeinsames Shop-System.
- Enthalten: Shop-UI, Bar (illegale Gegenstände kaufen, Beschlagnahmtes verkaufen), Dokumentenscanner kaufbar
- Nicht enthalten: Gadget-Funktion (P16)
- Abnahme: Scanner kaufen, beschlagnahmtes Item an der Bar verkaufen, Geld stimmt.

### P12 — Enden
- Größe: S · Priorität: Muss · Status: offen
- Braucht: P10, P08
- Ziel: Nach Tag 3 oder bei Niederlage ein Endbildschirm.
- Enthalten: Niederlage (Familie zerbrochen / gefeuert), mindestens 2 Enden je nach Moral & Familie
- Abnahme: Jedes Ende lässt sich gezielt erspielen.

### P13 — Hangar: Pakete an die Familie
- Größe: S · Priorität: Soll · Status: offen
- Braucht: P10, P11
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
- Größe: S · Priorität: Soll · Status: offen
- Braucht: P01
- Ziel: AudioMixer (Musik/SFX/Voice), einfacher Weg, Sounds abzuspielen, Lautstärke aus dem Menü (P02).
- Abnahme: Lautstärkeregler wirken getrennt.

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
- Größe: L · Priorität: Soll · Status: in Arbeit (max, 04.11.)
- Braucht: Lieferung der Hüll-Assets (M015) · Folgt auf: P24
- Ziel: Die Station ist von außen ein zusammenhängendes Bauwerk.
- Enthalten: Fachwerk verbindet Station, Habmodule, Solarflügel (auf Masten), Antenne (Sockel auf dem Hallendach), Radiatoren am
  Hangar, Andockarm zum Frachter; Außenhülle aus Platten um alle Räume (Platzhalter, bis die Pipeline liefert), Kanten, Rohre,
  Greebles; Fenster und Aussichtskuppel bleiben frei
- Abnahme: Rundflug außen: nichts schwebt, keine Innenwand-Rückseiten sichtbar.
