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
- Größe: S · Priorität: Soll · Status: in Arbeit (max, 02.11.)
- Braucht: P01 · Hängt am Art-Style-Beschluss
- Ziel: Low-Res-Rendering per URP Renderer Feature, UI in voller Auflösung darüber.
- Abnahme: Einstellbare Render-Auflösung, Dokumente bleiben scharf, WebGL-Build flüssig.

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
