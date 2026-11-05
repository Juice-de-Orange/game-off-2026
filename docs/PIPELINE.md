# Pipeline-Kanal

Kommunikation zwischen dem Team (max, martin + ihre Claudes) und Max' Asset-Pipeline (Blender-Worker).
Für alles, was **nicht** in einen einzelnen Asset-Eintrag gehört: kaputte Einträge, unklare Angaben, Rückfragen,
Hinweise zu Regeln, Blocker. Nachbesserungen an einem fertigen Asset laufen weiter über `feedback` + `status: rework`
in `docs/ASSET-REQUESTS.md`.

## Wege
- **Team → Pipeline:** Eintrag unten anhängen und auf `main` pushen. Die Pipeline liest diese Datei wie
  `ASSET-REQUESTS.md` direkt von `main`.
- **Pipeline → Team:** Die Pipeline pusht nie auf `main`. Sie hängt ihre Meldung in dieser Datei an und schickt sie
  **im nächsten Asset-PR mit** (oder in einem eigenen PR `assets/meldungen`, wenn es dringend ist oder blockiert).
  Max kann eine Meldung auch selbst hier eintragen, wenn die Pipeline sie ihm direkt geschickt hat.
- Wer eine Meldung erledigt, setzt `status: erledigt` und schreibt eine Zeile `antwort`.
- Zu Sessionbeginn (`/sync`) prüft jeder Claude diese Datei auf `status: offen` mit `an: team` und kümmert sich zuerst darum.

## Regeln für Einträge in `ASSET-REQUESTS.md` (Lehre aus M001)
- Jeder YAML-Block muss mit einem YAML-Parser lesbar sein. Vor dem Pushen prüfen: `python tools/check_asset_requests.py`
- Werte mit `: ` (Doppelpunkt + Leerzeichen), `#`, `[`, `{` oder führendem `*`/`&`/`!` in Anführungszeichen setzen
  oder als Block (`>`) schreiben.

## Format
```
### M<Nr> — <Kurztitel>
- datum: YYYY-MM-DD
- von: pipeline | max | martin
- an: team | pipeline
- betrifft: <Asset-Dateiname oder Thema>
- status: offen | erledigt
- meldung: <was ist los>
- antwort: <wer, was getan wurde>
```

## Meldungen

### M001 — Kaputter YAML-Eintrag SM_Hangar_DronePad
- datum: 2026-11-02
- von: pipeline (über max)
- an: team
- betrifft: SM_Hangar_DronePad.fbx
- status: erledigt
- meldung: Der Eintrag ließ sich nicht lesen, das YAML war fehlerhaft.
- antwort: max/Claude, 02.11.: `interaction: Terminal: Paket ...` enthielt einen zweiten Doppelpunkt. Wert in Anführungszeichen
  gesetzt. Alle 50 YAML-Blöcke danach mit PyYAML geprüft, keine weiteren Fehler. Prüfskript `tools/check_asset_requests.py`.

### M002 — Pipeline kann keine Pull Requests öffnen
- datum: 2026-11-02
- von: max
- an: pipeline
- betrifft: Lieferablauf (`assets/*`-Branches)
- status: erledigt
- meldung: Die Branches `assets/P00-sample`, `assets/P04-station-kit-a`, `assets/P05-counter-a` kamen an, aber ohne PR — mit dem
  Deploy-Key kann man pushen, aber keine PRs über die GitHub-API anlegen. Max (bzw. sein Claude) hat die PRs #1–#3 selbst
  angelegt und gemergt; alle drei sind integriert (Materialien, Importeinstellungen) und auf `final`.
  Bis das gelöst ist: Branch pushen reicht, Max legt den PR an. Dauerhafte Lösung: ein fein abgestimmter Token nur für dieses
  Repo mit "Pull requests: write", oder die Pipeline meldet fertige Branches per Push-Nachricht.
- antwort: pipeline (Lead), 02.11.: So bleibt es: Die Pipeline pusht `assets/*`-Branches. Der Lead schickt Max zu jedem
  fertigen Paket den PR-Link (`.../pull/new/assets/<slug>`), Titel, Text, Vergleichsbild und Vorschauen in den Chat (Max'
  Wahl vom 02.11.). Ein Token mit PR-Rechten ist nicht nötig; will Max ihn später doch, legt er ihn an.

### M005 — Stations-Baukasten: fehlende Teile und offene Maße
*(von der Pipeline als M003 angelegt; umbenannt, weil M003 schon vergeben war)*
- datum: 2026-11-02
- von: pipeline
- an: team
- betrifft: SM_Station_* (Baukasten A/B, PRs P04-station-kit-a/-b)
- status: erledigt
- meldung: Beim Bau von Baukasten B fielen Lücken auf, die keinen Eintrag haben. Bitte je Punkt sagen: Eintrag anlegen
  (dann bauen wir es) oder nicht nötig.
  1. Sturzstreifen über den Fenstern: Window_Small endet bei 2,2 m in 3-m-Räumen (es fehlen 2 x 0,8 m), Window_Panorama
     bei 4,5 m in der 6-m-Halle (2 x 1,5 m).
  2. Eine Wand mit dem Achteck-Loch für SM_Station_Porthole (0,66 m über die Flächen, Mitte auf 1,5 m).
  3. Eine 1,2-m-Türwand für die schmale Tür: Für die 5 Personaltüren aus STATION.md haben wir die Variante
     `SM_Station_Door_Slide_Narrow` (1,2 m) gebaut, dazu `SM_Station_Window_Small_Slot`. Beide haben keinen eigenen Eintrag;
     sie liegen im Branch, bitte Einträge nachziehen oder sagen, dass ihr sie nicht wollt.
  4. Brüstung: STATION.md hat das Fenster Schalter -> Hangar auf 1,0 m, der Eintrag Window_Small auf 0,9 m (gebaut: 0,9 m).
  5. Luftschleusen-Tür: 0,4 m tief laut Eintrag, steht vorne 0,06 m und hinten 0,1 m aus der Wand. Gleitet sie im Spiel in
     die Wand wie die Schiebetür, steht sie vor; bitte die Öffnungsbewegung nennen.
  6. Schiebetür 0,136 m tief statt 0,15 (innerhalb der 2 cm des Eintrags), damit sie offen in den Nuten der Wand verschwindet.
- antwort: max, 02.11.:
  1.+2. **Beides bauen:** neue Einträge `SM_Station_Lintel_4x08` (Sturzstreifen) und `SM_Station_Wall_Porthole_2x3`.
  3. **Einträge nachgezogen** für `SM_Station_Door_Slide_Narrow` und `SM_Station_Window_Small_Slot` (beide final), neuer Eintrag
     `SM_Station_Wall_Door_2x3_Narrow` (1,2-m-Türwand).
  4. Brüstung **0,9 m** übernommen (STATION.md angepasst).
  5. Schleusentor **klappt nach außen weg** (seitliche Angel) → `SM_Station_Hatch_Airlock` auf rework: Pivot `hinge_left`
     an der Nordkante, geschlossen bündig in der Wand.
  6. Schiebetür 0,136 m passt.

### M003 — Neue Aufträge "Station 2.0" (50 Einträge) + Reihenfolge
- datum: 2026-11-02
- von: max
- an: pipeline
- betrifft: `ASSET-REQUESTS.md`, Pakete P22–P26
- status: offen
- meldung: 50 neue Einträge (41 Modelle, 9 Texturen). Bitte in dieser Reihenfolge:
  1. P22 Bausatz-Ergänzungen bis 09.11.: `SM_Station_Window_Curved`, `SM_Station_Floor_DeckRound`, `SM_Station_Ceiling_DeckRound`,
     `SM_Station_Steps_2m`, `SM_Station_Wall_Window_2x3`, `SM_Hangar_Wall_4x4`, `SM_Hangar_Floor_4x4`, `SM_Hangar_Ceiling_4x4`,
     `SM_Hangar_BarrierEmitter` — plus die Varianten `_Narrow` (Tür) und `_Padded`/`_Panel` (Wand) aus den alten Einträgen.
  2. Die übrigen offenen P1-Einträge aus der ersten Liste (Halle, Hangar, Motel, Bar, Kiosk, Fenster, Schotts).
  3. P24/P25 Außen, Schiffe, Himmel bis 13.11. — **Außen- und Schiffsmodelle mit niedriger Texeldichte (4–8 px/m)**, sie werden
     nur aus der Ferne gesehen; Browser-Downloadgröße im Blick behalten.
  4. P26 Details bis 16.11.
  Geändert: `T_Sky_Space` nur noch Sterne + Nebel (Planeten sind eigene Objekte); `SM_Station_Window_Panorama` entfällt.
- antwort:

### M004 — Hangartor: Eintrag 16 x 7 m, Grundriss 2.0 hat 28 x 12 m
- datum: 2026-11-02
- von: pipeline
- an: team
- betrifft: SM_Hangar_Door_Big.fbx
- status: erledigt
- meldung: Der Eintrag verlangt das Tor mit 16 x 7 m, so ist es im Branch `assets/P04-hangar` gebaut. Der neue Grundriss
  (Station 2.0 auf main) hat aber eine 28 x 12 m große Hangar-Öffnung mit Emitter-Segmenten (P23). Bitte den Eintrag
  auf die richtige Größe setzen (dann `status: rework`) oder schreiben, dass das Tor nur einen Teil der Öffnung füllt.
- antwort: max, 02.11.: **Tor entfällt.** Die Öffnung bilden Energiebarriere + `SM_Hangar_BarrierEmitter`. `SM_Hangar_Door_Big` wird nicht verbaut (Eintrag auf P3, Hinweis im Feedback).

### M006 — Nachbesserungen + neue Aufträge aus dem Asset-Durchgang
- datum: 2026-11-02
- von: max
- an: pipeline
- betrifft: `ASSET-REQUESTS.md`
- status: offen
- meldung: Max hat alle gelieferten Assets durchgesehen und Funktionen festgelegt. Daraus:
  - **rework** `SM_Hangar_Shuttle`: Heckrampe als eigenes Unterobjekt `Ramp` mit Pivot an der Angel (klappt per Skript hoch/runter).
  - **rework** `SM_Station_Hatch_Airlock`: siehe M005 Punkt 5.
  - **neu**: `SM_Station_Lintel_4x08`, `SM_Station_Wall_Porthole_2x3`, `SM_Station_Wall_Door_2x3_Narrow`,
    Beschlagnahme-Gegenstände `SM_Item_*` (6 Stück), `UI_Telescope_Overlay`, `UI_Family_Photo` (3 Zustände).
  Reihenfolge: zuerst die drei Bausatz-Teile und die beiden Reworks, dann Items/UI.
- antwort:

### M007 — Mehr Abwechslung und Rundungen (21 neue Einträge)
- datum: 2026-11-02
- von: max
- an: pipeline
- betrifft: `ASSET-REQUESTS.md`, Bausatz
- status: offen
- meldung: Max findet die Wände zu gleich und die Station zu eckig. Neu (alle P1 bis 09.11., außer Geländer):
  - Wandvarianten als **eigene Dateien** (bisher nur als "Varianten" im Grundwand-Eintrag, wurden nicht geliefert):
    `_Padded`, `_Panel`, `_Pipes`, `_Vent`, `_Damaged`, `_Screen`, `_Lockers`, dazu `SM_Station_Wall_2x1`, `SM_Station_Floor_2x2_Grate`,
    `SM_Station_Ceiling_2x2_Light`. Bitte künftig jede Variante als eigenen Eintrag behandeln.
  - Rundungen: `SM_Station_Ceiling_Arch_2x2` (Tonnengewölbe in Gängen), `SM_Station_Wall_CornerRound`, `SM_Station_Pillar_Round`,
    `SM_Station_DoorTrim_Round` (+ `_Narrow`), `SM_Station_Wall_Arch_2x3`, `SM_Station_DeckArch`, `SM_Hall_CeilingDome`,
    `SM_Deck_Railing`, `SM_Hangar_RibArch`.
  Der Builder setzt alle automatisch, sobald die Dateien da sind. Reihenfolge: Wandvarianten zuerst, dann Gewölbe/Ecken/Bögen.
- antwort:

### M008 — Offene Maße und Details aus den Lieferungen vom 02.11.
- datum: 2026-11-02
- von: pipeline
- an: team
- betrifft: SM_Hall_DepartureBoard, SM_Diner_Counter, SM_Hall_Planter, SM_Motel_TV, SM_Kiosk_Counter, Motel-Farben
- status: erledigt
- meldung: Alles ist so gebaut, wie die Einträge es verlangen. Folgende Punkte bitte entscheiden (Eintrag ändern +
  `status: rework`, oder "passt so"):
  1. Abflugtafel: Eintrag 3,5 m hoch, die Halle ist 6 m hoch; die Seile enden bei 3,5 m in einem Flansch statt an der Decke.
  2. Diner-Tresen: hat eine abgesenkte Ausgabestelle; der Eintrag sagt durchgehend 1,1 m.
  3. Pflanzkübel: das violette Pflanzenlicht bräuchte eine Emission-Textur, die der Eintrag nicht vorsieht (jetzt nur Albedo).
  4. Motel-TV: der Pivot liegt laut Eintrag 0,2 m vor der Wand.
  5. Kiosk-Tresen: 1,0 m wie im Eintrag; Rollgitter und violettes Schild sitzen deshalb in/an der Tresenkante. Sollen sie
     darüber hängen, braucht der Eintrag mehr Höhe (size_m).
  6. Motel: das Möbel-Türkis ist Palette C2 (heller), das Stations-Türkis (Tank, Pflanzkübel) C1. Absicht oder angleichen?
- antwort: max, 03.11.: 1. Abflugtafel: Seile bis zur Decke (6 m) -> rework. 2. Diner-Tresen: passt so. 3. Pflanzkuebel: Emission-Textur nachliefern -> rework. 4. Motel-TV: passt so. 5. Kiosk-Tresen: Gitter und Schild ueber dem Tresen, Hoehe 2,4 m -> rework. 6. Motel-Tuerkis C2 ist Absicht, bleibt.


### M009 — Außenwelt A (P24): Achsen von Träger und Solarflügel, Schriftzug, Antennenlicht
- datum: 2026-11-02
- von: pipeline
- an: team
- betrifft: SM_Ext_TrussSegment, SM_Ext_SolarArray, SM_Ext_HabModule, SM_Ext_Antenna (`SkyExteriorBuilder.cs`)
- status: erledigt
- meldung: Alle fünf sind genau nach `size_m` gebaut ([x, y, z] mit z = oben, wie alle Einträge). Für `SkyExteriorBuilder.cs` heißt das:
  1. `SM_Ext_TrussSegment` [3, 3, 20] steht bei Rotation 0 20 m hoch; der Platzhalter liegt entlang Unity Z. Liegend: zusätzlich 90° um X,
     also `Euler(90, -55, 0)` (Truss_West) und `Euler(90, 50, 0)` (Truss_East). Oder den Eintrag auf [3, 20, 3] ändern (dann rework).
  2. `SM_Ext_SolarArray` [30, 1, 10] steht aufrecht, die Zellen schauen nach Unity -Z; der Platzhalter liegt flach. Flach mit Zellen nach oben:
     `Euler(110, 8, 0)` (Solar_North) und `Euler(65, -15, 0)` (Solar_South). Oder den Eintrag auf [30, 10, 1] ändern.
  3. `SM_Ext_HabModule`: der Schriftzug KESTREL-9 / HAB 2 läuft entlang der Achse auf der Vorderseite und steht bei der Rolle von Hab_East
     `Euler(0, 0, 90)` aufrecht (bei `Euler(0, 0, -90)` stünde er kopf). Bei Hab_West `Euler(90, 20, 0)` zeigt er in den Himmel. Die Fensterreihen
     liegen auf ±X wie beim Platzhalter.
  4. `SM_Ext_Antenna` hat ein eigenes rotes Blinklicht (Emission) auf 17,5 m, bei y = 7 also auf 24,5. `Nav_Antenna` (y 25,5) schwebt 0,5 m
     über der Spitze: auf 24,5 setzen oder weglassen.

### M011 — Landeplatte: Terminal-Pfosten braucht 1,42 m Höhe (SM_Hangar_DronePad)
- datum: 2026-11-02
- von: pipeline
- an: team
- betrifft: SM_Hangar_DronePad.fbx (P13, Branch `assets/P26-hangar-extras`)
- status: erledigt
- meldung: Der Eintrag will "daneben ein Terminal-Pfosten mit CRT" (Interaktion "Paket an Familie schicken"), hat aber
  `size_m: [3.0, 3.0, 0.2]`. Ein bedienbarer Pfosten mit CRT ist ~1,4 m hoch. Geliefert: der Pfosten steht auf der
  vorderen rechten Ecke der Platte (von vorne gesehen; Unity ca. (+1,2, 0, -1,2) vom Pivot), innerhalb der 3 x 3 m,
  das Modell ist **3,0 x 3,0 x 1,42 m**. Bitte `size_m` auf `[3.0, 3.0, 1.42]` setzen (Collider: Platte 0,2 m hoch
  plus Pfosten). Die Oberseite der Platte liegt auf 0,2 m: die Frachtdrohne steht mit Rotation 0 auf dem Pivot der
  Platte + 0,2 m nach oben, Nase nach vorne (-Z) wie der Pfeil auf der Platte. Die Nummerierung M008/M009 ist auf
  mehreren Branches doppelt vergeben; diese Meldung nimmt deshalb M011.

### M012 — NPCs A: Rig-Pipeline + 2 Spezies (SK_NPC_Human, SK_NPC_Vellani)
*(von der Pipeline als M009 angelegt; umbenannt, weil M009 schon vergeben war)*
- datum: 2026-11-02
- von: pipeline
- an: team
- betrifft: `SK_NPC_Human.fbx`, `SK_NPC_Vellani.fbx` (Branch `assets/npc-a`)
- status: erledigt
- meldung: Max wollte NPCs mit Skelett (5 Spezies, hier die ersten 2). Für diese Figuren gibt es **keine Einträge in
  `ASSET-REQUESTS.md`**. Wenn ihr sie dort verfolgen wollt, legt bitte Einträge an (Pfad, Größe, Clips); die Pipeline
  liefert dann gegen diese Einträge.
  - Dateien:
    - `Assets/_Project/Art/Models/Characters/SK_NPC_Human.fbx` (1687 Tris, 1 Slot `Body`, 1,80 m)
    - `SK_NPC_Vellani.fbx` (1982 Tris, 1 Slot `Body`, 2,20 m mit Kamm)
    - Texturen: `Assets/_Project/Art/Textures/Characters/T_NPC_<Spezies>_Albedo.png` (128 x 128, Point)
    - `.blend` unter `ArtSource/Models/Characters/`
    - Vorschauen: `docs/asset-previews/SK_NPC_*.png` (3/4 + Gesicht auf 2 m), `_Turn` (T-Pose, vorne, seitlich),
      `_Clips` (Idle, Walk, Talk) und `lineup_npc-a.png` (am SM_Counter_Desk).
  - **Rig:**
    - Unity-Humanoid-Namen (Hips, Spine, Chest, UpperChest, Neck, Head, Left/Right Shoulder/UpperArm/LowerArm/Hand/
      UpperLeg/LowerLeg/Foot/Toes); Fäustlinge ohne Fingerknochen.
    - Ruhepose = T-Pose. In der FBX ist sie auch die Standardpose, Humanoid-Avatar per Auto-Mapping.
    - Zusatzteile beim Vellani: `Ext_Crest`, `Ext_Frill_L/_R`. Humanoid ignoriert sie, die Clips bewegen sie.
    - Max. 4 Gewichte pro Vertex.
  - **Clips:** als Takes in derselben FBX: `Idle`, `Walk`, `Talk`.
    - Alle loopen sauber (letzter Frame = erster), bitte "Loop Time" an.
    - 30 fps; Längen Mensch 2,0 / 1,0 / 2,5 s, Vellani 2,4 / 1,2 / 2,0 s.
    - Walk läuft auf der Stelle (Laufband). Passende Agent-Geschwindigkeit: Mensch 0,83 m/s, Vellani 0,86 m/s.
    - Talk: Gesten zum Schalter hin, Füße bleiben stehen.
  - **Ausrichtung:** Anders als die Props schauen die Figuren nach **Unity +Z** (transform.forward), Pivot unten
    mittig zwischen den Füßen. Wie jedes Blender-Rig tragen der Knoten `Armature` und das Mesh die Achsdrehung X -90.
    Die Importoption "Bake Axis Conversion" entfernt sie, wenn sie stört.
  - Import: Rig "Humanoid", Avatar "Create From This Model"; Material `M_NPC_<Spezies>` mit der Albedo (Point, keine
    Mipmaps).

### M010 — NPCs B: 3 weitere Spezies (SK_NPC_Quobb, SK_NPC_Xirrik, SK_NPC_Sorvak)
- datum: 2026-11-02
- von: pipeline
- an: team
- betrifft: `SK_NPC_Quobb.fbx`, `SK_NPC_Xirrik.fbx`, `SK_NPC_Sorvak.fbx` (Branch `assets/npc-b`)
- status: erledigt
- meldung: Die übrigen 3 der 5 NPC-Spezies (Max: NPCs mit Skelett), gleiche Pipeline wie M009 (`assets/npc-a`).
  Auch für diese Figuren gibt es **keine Einträge in `ASSET-REQUESTS.md`**; wenn ihr sie verfolgen wollt, legt bitte
  Einträge an (Pfad, Größe, Clips).
  - Dateien:
    - `Assets/_Project/Art/Models/Characters/SK_NPC_Quobb.fbx` (Amphibie, 1854 Tris, 1 Slot `Body`, 1,25 m bis zu den Augen)
    - `SK_NPC_Xirrik.fbx` (Insektoid, 1712 Tris, 1 Slot `Body`, 1,91 m mit Fühlern)
    - `SK_NPC_Sorvak.fbx` (Reptiloid, 1978 Tris, 1 Slot `Body`, 1,97 m mit Hörnern, Schwanz 0,8 m nach hinten)
    - Texturen: `Assets/_Project/Art/Textures/Characters/T_NPC_<Spezies>_Albedo.png` (128 x 128, Point)
    - `.blend` unter `ArtSource/Models/Characters/`
    - Vorschauen: `docs/asset-previews/SK_NPC_*.png`, `_Turn`, `_Clips` und `lineup_npc-b.png` (alle fünf am
      SM_Counter_Desk, der Insektoid am Schalter).
  - **Rig:** wie M009 (Unity-Humanoid-Namen, T-Pose = Standardpose, Fäustlinge/Krallen ohne Fingerknochen, max. 4
    Gewichte pro Vertex). Zusatzknochen, die Humanoid ignoriert und die Clips bewegen:
    - Quobb: `Ext_Throat` (Kehlsack; wird **skaliert**, nicht gedreht: beim Atmen und Reden bläht er sich auf).
    - Xirrik: `Ext_AntennaA_L/_R`, `Ext_AntennaB_L/_R` (Fühler, je 2 Glieder), `Ext_Mandible_L/_R`.
    - Sorvak: `Ext_Jaw` (Unterkiefer, öffnet sich beim Reden), `Ext_Tail1`–`Ext_Tail3` (Schwanz).
    - Falls die Zusatzknochen im Humanoid-Clip nicht mitlaufen: im Animation-Tab unter Mask → Transform die
      `Ext_`-Knochen aktiv lassen (Humanoid-Clips übernehmen sonst nur die Muskelkurven).
  - **Clips:** Takes `Idle`, `Walk`, `Talk` in derselben FBX, 30 fps, alle loopen ("Loop Time" an).
    - Längen: Quobb 2,4 / 1,6 / 2,0 s, Xirrik 2,4 / 1,6 / 2,0 s, Sorvak 2,4 / 1,2 / 2,4 s (Quobb und Xirrik: der
      Walk-Take enthält zwei Schritte).
    - Walk auf der Stelle; Agent-Geschwindigkeit: Quobb 0,63 m/s, Xirrik 0,96 m/s, Sorvak 0,83 m/s.
    - Talk: Gesten zum Schalter hin, Füße bleiben stehen.
  - **Ausrichtung und Import:** wie M009 (Blick nach Unity +Z, Pivot unten mittig; Rig "Humanoid", Avatar "Create From
    This Model"; Material `M_NPC_<Spezies>` mit der Albedo, Point, keine Mipmaps).
- antwort: max/Claude, 03.11.: Integriert wie M012, alle 5 Avatare gültig, Ext_-Kurven (Kehlsack, Fühler, Mandibeln, Kiefer, Schwanz) in allen Clips vorhanden.

### M013 — Nachbesserungen aus M008: Abflugtafel, Pflanzkübel, Kiosk-Tresen
- datum: 2026-11-03
- von: pipeline
- an: team
- betrifft: SM_Hall_DepartureBoard, SM_Hall_Planter, SM_Kiosk_Counter
- status: erledigt
- meldung: Die drei rework-Punkte aus M008 sind umgesetzt, die Einträge stehen auf `Vorschau`:
  1. `SM_Hall_DepartureBoard` (Branch `assets/P04-hall`): Tafel unverändert (Höhe 3,2 m, Größe wie geliefert); die drei
     Kabel laufen jetzt bis in einen Deckenflansch auf 6,0 m. Modell 2,0 x 2,0 x 6,0 m, 872 Tris.
  2. `SM_Hall_Planter` (Branch `assets/P04-extras`, dort wurde er geliefert): neue `T_Hall_Planter_Emission.png`
     (64 x 64): UV-Röhre und Fensterstreifen der Lampe leuchten Neonviolett, sonst alles schwarz. Modell unverändert
     (498 Tris); Material: Emission-Map + Emission-Farbe weiß, die Stärke nach Geschmack.
  3. `SM_Kiosk_Counter` (Branch `assets/P11-P13-kiosk-drone`): Tresen bleibt 1,0 m. Darüber zwei Führungsschienen an
     den Kanten der 1,6 m breiten Durchreiche, das Rollgitter halb heruntergelassen (2,0 -> 1,5 m) und oben der
     Gitterkasten (2,0-2,4 m) mit dem violetten Leuchtschild 'GADGETS & SUPPLIES' auf 2,03-2,17 m - genau der Streifen,
     den man aus der Halle unter dem Sturz (2,2 m) sieht. Modell 0,8 x 2,8 x 2,4 m, 588 Tris. Das alte Schild in der
     Vitrine und das Gitter vor der Frontscheibe sind weg. Collider bitte auf die neue Höhe anpassen.
- antwort: max, 04.11.: Alle drei gemergt (PR #10-#12), eingebaut und auf `final`. Kiosk- und Pflanzkübel-Modell stehen jetzt in der Station, der Kiosk-Collider passt zur Höhe von 2,4 m. Emission der Abflugtafel ist jetzt auch aktiv.

### M014 — Angebot: Katzenbild für Zimmer 103 (SM_Motel_CatPainting)
- datum: 2026-11-03
- von: pipeline
- an: team
- betrifft: SM_Motel_CatPainting (neu, steht nicht auf der Request-Liste)
- status: erledigt
- meldung: Ein Extra für das Spielerzimmer 103: ein gerahmtes Gemälde von Max' eigener Katze (Branch
  `assets/P04-motel-cat-painting`). Wenn ihr es nicht wollt, einfach den Branch liegen lassen.
  - **Modell:** `SM_Motel_CatPainting`, 0,53 x 0,045 x 0,63 m (Leinwand 0,40 x 0,50 m), 34 Tris, 1 Slot `Painting`.
    Pivot `center` wie bei `SM_Prop_WallClock` (mittlere Tiefe: an die Wand mit 0,0225 m Abstand setzen). Die Rückseite
    ist offen, also nur an eine Wand hängen.
  - **Textur:** `T_Motel_CatPainting_Albedo.png`, 128 x 128, nur Palettenfarben, keine Emission. Import Point, keine
    Mipmaps. Die Leinwand hat 160 px/m, der Rahmen 120-200 px/m (Nahsicht-Dichte nach STYLE.md).
  - **Look:** Rahmen aus dunklem Fake-Holz mit Messinglippe. Das Bild ist ein gemalter brauner Tigerkater vor
    orangem Grund: der warme Gegenpol zum Teal und Giftgrün im Motel.
  - **Platzierung (Vorschlag):** über dem Kopfende des Betts, Bildmitte etwa 1,3 m über dem Boden
    (`docs/asset-previews/lineup_P04-motel-cat-painting.png`).
- antwort: max, 04.11.: Angenommen, danke! Gemergt (PR #13) und in Zimmer 103 über dem Bett aufgehängt; Eintrag in `ASSET-REQUESTS.md` nachgetragen (final).

### M015 — Außenstation 2.0: Hülle und Verbindungsteile (14 neue Einträge, P39)
- datum: 2026-11-04
- von: max
- an: pipeline
- betrifft: `ASSET-REQUESTS.md`, Paket P39 (alle `SM_Ext_*` mit `package: P39`)
- status: erledigt
- meldung: Die Station hat von außen keine Hülle (man sieht die Rückseiten der Innenwände), und Fachwerk, Solarflügel und
  Antenne schweben unverbunden herum. Neu, alle P1 bis 12.11.:
  1. Hülle: `SM_Ext_HullPanel_4x4`, `_Ribbed`, `SM_Ext_HullEdge_4`, `SM_Ext_HullCorner`, `SM_Ext_RoofCap_4x4`, `SM_Ext_HangarRib_Outer`.
  2. Verbindungen: `SM_Ext_TrussNode`, `SM_Ext_DockingCollar`, `SM_Ext_SolarMast`, `SM_Ext_AntennaBase`.
  3. Greebles: `SM_Ext_Greeble_VentBox`, `SM_Ext_Greeble_Tank`, `SM_Ext_Pipe_Large_4`, `SM_Ext_Pipe_Large_Elbow`.
  Stil wie die KESTREL-Habmodule (die gefallen Max sehr), niedrige Texeldichte. Hüllplatten müssen nahtlos kacheln.
  Reihenfolge: Hülle zuerst, dann Verbindungen, dann Greebles. Der Builder setzt alle automatisch, sobald die Dateien da sind.
- antwort:
- antwort: max/Claude, 05.11.: Alle Lieferungen vom 04.11. (P39, P06, P07, P38) freigegeben, gemergt, eingebaut und neu gebacken (P42). Die Branches hatten keine Pull Requests und lagen deshalb unbemerkt da: Bitte künftig einen PR öffnen oder hier melden, dass ein Branch bereitliegt.

