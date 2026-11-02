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
- status: offen
- meldung: Die Branches `assets/P00-sample`, `assets/P04-station-kit-a`, `assets/P05-counter-a` kamen an, aber ohne PR — mit dem
  Deploy-Key kann man pushen, aber keine PRs über die GitHub-API anlegen. Max (bzw. sein Claude) hat die PRs #1–#3 selbst
  angelegt und gemergt; alle drei sind integriert (Materialien, Importeinstellungen) und auf `final`.
  Bis das gelöst ist: Branch pushen reicht, Max legt den PR an. Dauerhafte Lösung: ein fein abgestimmter Token nur für dieses
  Repo mit "Pull requests: write", oder die Pipeline meldet fertige Branches per Push-Nachricht.
- antwort:

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
- status: offen
- meldung: Der Eintrag verlangt das Tor mit 16 x 7 m, so ist es im Branch `assets/P04-hangar` gebaut. Der neue Grundriss
  (Station 2.0 auf main) hat aber eine 28 x 12 m große Hangar-Öffnung mit Emitter-Segmenten (P23). Bitte den Eintrag
  auf die richtige Größe setzen (dann `status: rework`) oder schreiben, dass das Tor nur einen Teil der Öffnung füllt.
- antwort:

