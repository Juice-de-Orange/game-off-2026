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

### M009 — Außenwelt A (P24): Achsen von Träger und Solarflügel, Schriftzug, Antennenlicht
- datum: 2026-11-02
- von: pipeline
- an: team
- betrifft: SM_Ext_TrussSegment, SM_Ext_SolarArray, SM_Ext_HabModule, SM_Ext_Antenna (`SkyExteriorBuilder.cs`)
- status: offen
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
- antwort:
