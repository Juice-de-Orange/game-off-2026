# Asset-Anforderungen

Alle noch benötigten Assets. Wer beim Bauen eines Pakets ein Asset braucht, trägt es hier ein, legt den
Platzhalter nach den Regeln in `docs/ASSETS.md` an und baut weiter.
Max' Asset-Pipeline liest diese Datei und `docs/STYLE.md` direkt von `main` und arbeitet die Liste ab.

## Format
Pro Asset eine Überschrift `### <Dateiname>` und darunter **ein YAML-Block**. Neue Einträge unten anhängen.
Ab ca. 150 Einträgen wird die Datei pro Bereich aufgeteilt.

| Feld | Bedeutung |
|---|---|
| `path` | finaler Pfad unter `Assets/_Project/` |
| `package` | Paket aus `docs/team/PAKETE.md` |
| `scene` | Raum/Szene, in dem das Asset vorkommt (Bar, Hangar, Counter, Motel, Diner, UI ...) |
| `status` | `offen` → `in Arbeit` → `Vorschau` → `final`; bei Nachbesserung `rework` |
| `priority` | `P1` = Muss, `P2` = Soll, `P3` = Kann |
| `needed_by` | Datum, bis wann das finale Asset gebraucht wird |
| `size_m` | `[Breite, Tiefe, Höhe]` in Metern |
| `pivot` | `bottom_center`, bei Türen/Klappen `hinge_left` / `hinge_right` |
| `tris_max` | Polygon-Budget (Props ≤ 1000, Charaktere ≤ 3000) |
| `materials` | Namen der Material-Slots im FBX (1, höchstens 2) |
| `textures` | Pfade der Texturen mit Größe |
| `texel_px_per_m` | 64 (Welt) oder 128 (Nahsicht am Schalter) |
| `palette` | `docs/style/palette.png` |
| `look` | wie es final aussehen soll |
| `interaction` | `none` oder was der Spieler damit tut (öffnen, drücken ...) |
| `variants` | weitere Varianten (eigene Dateinamen) |
| `animations` | benötigte einfache Animationen (`A_...`) |
| `collision` | `prefab` (Collider im Prefab, Standard) oder `col_mesh` (zweites Mesh `_Col`) |
| `refs` | Referenzbilder unter `docs/style/refs/` |
| `placeholder` | wie der Platzhalter aussieht (Form, Größe, Pivot, Material-Slots) |
| `feedback` | datierte Zeilen bei Nachbesserung: `"2026-11-10 max: Leuchtstreifen zu hell"` |

Ablauf bei Nachbesserung: Zeile in `feedback` ergänzen und `status: rework` setzen. Derselbe Eintrag bleibt;
ein neuer Eintrag nur für eine zusätzliche Variante.

## Vorlage
```yaml
path: Assets/_Project/Art/Models/<Bereich>/<Datei>
package: Pxx
scene: <Raum>
status: offen            # offen | in Arbeit | Vorschau | rework | final
priority: P1             # P1 Muss | P2 Soll | P3 Kann
needed_by: 2026-11-xx
size_m: [0.0, 0.0, 0.0]  # Breite x Tiefe x Höhe
pivot: bottom_center
tris_max: 1000
materials: [<Slot>]
textures:
  - Assets/_Project/Art/Textures/<Bereich>/T_<...>_Albedo.png   # <Größe>, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  <Beschreibung>
interaction: none
variants: []
animations: []
collision: prefab
refs: []
placeholder: <Form, Größe, Pivot, Material-Slots>
feedback: []
```

## Anforderungen

### SM_Bar_Counter.fbx
*(Theke ist 3 m lang; in der Bar stehen zwei Stück hintereinander, X 14..15, Z 1..7 laut `docs/STATION.md`.)*
```yaml
path: Assets/_Project/Art/Models/Bar/SM_Bar_Counter.fbx
package: P04
scene: Bar
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [3.0, 0.7, 1.1]
pivot: bottom_center
tris_max: 1000
materials: [Counter]
textures:
  - Assets/_Project/Art/Textures/Bar/T_Bar_Counter_Albedo.png   # 256x128, Point
  - Assets/_Project/Art/Textures/Bar/T_Bar_Counter_Emission.png # 64x64, Leuchtstreifen
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Abgenutzter Raumstations-Bartresen: dunkle Kunstholz-Front mit Metallkanten,
  Tresenplatte aus zerkratztem Metall, schmaler Leuchtstreifen (Emission, warmes Orange)
  unter der Kante, Fußleiste mit Dellen. Gäste-Seite vorne (-Z), Barkeeper-Seite hinten
  offen mit zwei Fächern.
interaction: none
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 3.0 x 0.7 x 1.1 m, Pivot unten mittig, Material-Slot "Counter"
feedback: []
```

### SM_Station_Floor_2x2.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Floor_2x2.fbx
package: P04
scene: alle
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [2.0, 2.0, 0.1]
pivot: bottom_center
tris_max: 1000
materials: [Floor]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Floor_2x2_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Bodenplatte 2x2 m: abgenutzte Metallplatten mit Nietreihen und Fugen, dunkles Grau (#2C323C/#444B55), Kratzspuren in Laufrichtung, Schmutz in den Fugen. Kachelt nahtlos in alle Richtungen.
interaction: none
variants:
  - SM_Station_Floor_2x2_Grate (Gitterrost mit dunklem Hohlraum darunter, für Hangar und Servicegang)
animations: []
collision: prefab
refs: [docs/style/refs/mw_cargo_hold_01.webp, docs/style/refs/ai_corridor_cargo_01.webp]
placeholder: Box 2.0 x 2.0 x 0.1 m, Pivot bottom_center, Material-Slots [Floor]
feedback: []
```

### SM_Station_Wall_2x3.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Wall_2x3.fbx
package: P04
scene: alle
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [2.0, 0.2, 3.0]
pivot: bottom_center
tris_max: 1000
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_2x3_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Wandmodul 2 m breit, 3 m hoch: Industrie-Paneele in Hellgrau/Beige (#8B9096/#B7B9B5), unten 0,3 m dunkle Fußleiste mit Dellen, oben Kabelkanal, horizontale Paneelfugen, gelegentlich Warnaufkleber. Kachelt nahtlos seitlich und übereinander (Halle 6 m, Hangar 9 m).
interaction: none
variants:
  - SM_Station_Wall_2x3_Padded (gepolsterte, gesteppte Paneele in Beige: Motel, Schalter)
  - SM_Station_Wall_2x3_Panel (warme Kunstholz-Paneele #5E3A24: Bar, Diner)
animations: []
collision: prefab
refs: [docs/style/refs/ai_corridor_padded_01.webp, docs/style/refs/ai_room_warm_panels_01.webp]
placeholder: Box 2.0 x 0.2 x 3.0 m, Pivot bottom_center, Material-Slots [Wall]
feedback: []
```

### SM_Station_Wall_Door_2x3.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Wall_Door_2x3.fbx
package: P04
scene: alle
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [2.0, 0.2, 3.0]
pivot: bottom_center
tris_max: 1000
materials: [Wall, Frame]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_Door_2x3_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_Door_2x3_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Wandmodul mit Türöffnung 1,6 x 2,2 m mittig: dicker Metallrahmen mit gelb-schwarzen Warnstreifen an den Kanten, kleine Statuslampe über dem Sturz (Emission). Passt zu SM_Station_Wall_2x3.
interaction: none
variants:
  - SM_Station_Wall_Door_2x3_Narrow (Öffnung 1,2 x 2,2 m für Personaltüren und Motelzimmer)
animations: []
collision: prefab
refs: [docs/style/refs/ai_door_steam_01.webp]
placeholder: Box 2.0 x 0.2 x 3.0 m, Pivot bottom_center, Material-Slots [Wall, Frame]
feedback: []
```

### SM_Station_Door_Slide.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Door_Slide.fbx
package: P04
scene: alle
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [1.6, 0.15, 2.2]
pivot: bottom_center
tris_max: 1000
materials: [Door]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Door_Slide_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Schiebetür 1,6 x 2,2 m: schweres Schott mit diagonaler Verstärkung, kleines Sichtfenster, Griffmulde, abgeplatzter Lack. Gleitet seitlich in die Wand.
interaction: öffnet/schließt seitlich (Skript schiebt das Objekt, keine Animation im FBX)
variants:
  - SM_Station_Door_Slide_Narrow (1,2 x 2,2 m)
animations: []
collision: prefab
refs: [docs/style/refs/ai_door_steam_01.webp]
placeholder: Box 1.6 x 0.15 x 2.2 m, Pivot bottom_center, Material-Slots [Door]
feedback: []
```

### SM_Station_Ceiling_2x2.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Ceiling_2x2.fbx
package: P04
scene: alle
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [2.0, 2.0, 0.2]
pivot: bottom_center
tris_max: 1000
materials: [Ceiling]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Ceiling_2x2_Albedo.png   # 128x128, Point
  - Assets/_Project/Art/Textures/Station/T_Station_Ceiling_2x2_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Deckenplatte 2x2 m: Rasterdecke mit Lüftungsgitter, Rohrstück, dunkelgrau.
interaction: none
variants:
  - SM_Station_Ceiling_2x2_Light (mit eingelassener Neonröhre, kaltes Weiß als Emission)
animations: []
collision: prefab
refs: [docs/style/refs/ai_corridor_padded_01.webp]
placeholder: Box 2.0 x 2.0 x 0.2 m, Pivot bottom_center, Material-Slots [Ceiling]
feedback: []
```

### SM_Station_Pillar.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Pillar.fbx
package: P04
scene: Halle, Hangar
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [0.6, 0.6, 3.0]
pivot: bottom_center
tris_max: 1000
materials: [Pillar]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Pillar_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Stützpfeiler 0,6 x 0,6 x 3 m: Stahlträger mit Rohrleitungen und Kabelbündeln, Warnstreifen unten. Stapelbar.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/real_hangar_shuttle_01.webp]
placeholder: Box 0.6 x 0.6 x 3.0 m, Pivot bottom_center, Material-Slots [Pillar]
feedback: []
```

### SM_Station_Window_Panorama.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Window_Panorama.fbx
package: P04
scene: Halle
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [8.0, 0.4, 4.5]
pivot: bottom_center
tris_max: 1000
materials: [Frame, Glass]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Window_Panorama_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Panoramafenster 8 m breit, Glas 0,8-4,5 m hoch: dicker Metallrahmen mit 3 senkrechten Streben, Nieten, Kondensflecken am Rand. Glas als eigener Material-Slot (transparent).
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_corridor_window_01.webp, docs/style/refs/ai_window_docking_ship_01.webp]
placeholder: Box 8.0 x 0.4 x 4.5 m, Pivot bottom_center, Material-Slots [Frame, Glass]
feedback: []
```

### SM_Station_Window_Small.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Window_Small.fbx
package: P04
scene: Schalter
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [4.0, 0.3, 2.2]
pivot: bottom_center
tris_max: 1000
materials: [Frame, Glass]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Window_Small_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Sichtfenster 4 m breit (Glas 0,9-2,2 m): Panzerglas mit Metallrahmen. Variante mit Dokument-Durchreiche unten mittig (Schublade 0,5 m breit) für die Scheibe zur Prüfkammer.
interaction: none
variants:
  - SM_Station_Window_Small_Slot (mit Durchreiche-Schublade)
animations: []
collision: prefab
refs: [docs/style/refs/real_customs_booth_01.webp, docs/style/refs/ai_counter_terminal_01.webp]
placeholder: Box 4.0 x 0.3 x 2.2 m, Pivot bottom_center, Material-Slots [Frame, Glass]
feedback: []
```

### SM_Station_Porthole.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Porthole.fbx
package: P04
scene: Motel
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [0.9, 0.3, 0.9]
pivot: center
tris_max: 400
materials: [Frame, Glass]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Porthole_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Rundes Bullauge, Glas Ø 0,6 m, 8 Segmente, breiter Ring mit 8 Bolzen.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/real_crew_quarters_01.webp]
placeholder: Box 0.9 x 0.3 x 0.9 m, Pivot center, Material-Slots [Frame, Glass]
feedback: []
```

### SM_Station_Hatch_Airlock.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Hatch_Airlock.fbx
package: P04
scene: Prüfkammer
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [1.6, 0.4, 2.2]
pivot: bottom_center
tris_max: 1000
materials: [Hatch]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Hatch_Airlock_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Station/T_Station_Hatch_Airlock_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Außenschott der Prüfkammer (= Luftschleuse) ins All: massiver Rahmen mit roten Warnstreifen, Rotationsverriegelung in der Mitte, Schrift 'AIRLOCK - VACUUM BEYOND', rote Warnlampe darüber (Emission). Bedrohlich.
interaction: öffnet beim Luftschleusen-Knopf (Skript)
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_door_steam_01.webp, docs/style/refs/ai_suit_room_01.webp]
placeholder: Box 1.6 x 0.4 x 2.2 m, Pivot bottom_center, Material-Slots [Hatch]
feedback: []
```

### SM_Hangar_Door_Big.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_Door_Big.fbx
package: P04
scene: Hangar
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [16.0, 1.0, 7.0]
pivot: bottom_center
tris_max: 1500
materials: [Frame, Field]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_Door_Big_Albedo.png   # 512x256, Point
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_Door_Big_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Großes Hangartor 16 x 7 m zum All: Rahmen mit Warnstreifen und Positionslichtern, offen. Das Kraftfeld ist ein eigener Slot (halbtransparent bläulich, Emission).
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/real_hangar_shuttle_01.webp, docs/style/refs/ai_cockpit_view_landing_01.webp]
placeholder: Box 16.0 x 1.0 x 7.0 m, Pivot bottom_center, Material-Slots [Frame, Field]
feedback: []
```

### SM_Counter_Desk.fbx
```yaml
path: Assets/_Project/Art/Models/Counter/SM_Counter_Desk.fbx
package: P05
scene: Schalter
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [1.2, 4.0, 0.85]
pivot: bottom_center
tris_max: 1500
materials: [Desk, Screen]
textures:
  - Assets/_Project/Art/Textures/Counter/T_Counter_Desk_Albedo.png   # 512x512, Point
  - Assets/_Project/Art/Textures/Counter/T_Counter_Desk_Emission.png   # 64x64, Point
texel_px_per_m: 128
palette: docs/style/palette.png
look: >
  Arbeitstisch des Beamten entlang der Scheibe: 4 m lang, beige Kunststoff-Arbeitsplatte mit Metallkante, eingebautes CRT-Terminal (grüne Schrift) links, Ablagefach, Kabel. Mitte frei als Dokumentenfläche (ca. 1,2 x 0,8 m, hell und ruhig für 2D-Karten).
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_counter_console_01.webp, docs/style/refs/ai_desk_office_01.webp, docs/style/refs/ai_counter_terminal_01.webp]
placeholder: Box 1.2 x 4.0 x 0.85 m, Pivot bottom_center, Material-Slots [Desk, Screen]
feedback: []
```

### SM_Counter_Chair.fbx
```yaml
path: Assets/_Project/Art/Models/Counter/SM_Counter_Chair.fbx
package: P05
scene: Schalter
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [0.6, 0.6, 1.0]
pivot: bottom_center
tris_max: 800
materials: [Chair]
textures:
  - Assets/_Project/Art/Textures/Counter/T_Counter_Chair_Albedo.png   # 128x128, Point
texel_px_per_m: 128
palette: docs/style/palette.png
look: >
  Bürostuhl: gepolstert, Kunstleder rissig, Gasfeder, 5 Rollen.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_desk_office_01.webp, docs/style/refs/ai_cockpit_chair_01.webp]
placeholder: Box 0.6 x 0.6 x 1.0 m, Pivot bottom_center, Material-Slots [Chair]
feedback: []
```

### SM_Counter_Button_Arrest.fbx
```yaml
path: Assets/_Project/Art/Models/Counter/SM_Counter_Button_Arrest.fbx
package: P05
scene: Schalter
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [0.15, 0.15, 0.08]
pivot: bottom_center
tris_max: 300
materials: [Button]
textures:
  - Assets/_Project/Art/Textures/Counter/T_Counter_Button_Arrest_Albedo.png   # 64x64, Point
  - Assets/_Project/Art/Textures/Counter/T_Counter_Button_Arrest_Emission.png   # 64x64, Point
texel_px_per_m: 128
palette: docs/style/palette.png
look: >
  Großer Pilzknopf 'ARREST' in Bernstein (#FFD36B) auf schwarzem Sockel mit Beschriftung.
interaction: drücken (Knopf fährt 1 cm ein, Skript)
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_counter_console_01.webp, docs/style/refs/ai_desk_office_01.webp, docs/style/refs/ai_counter_terminal_01.webp]
placeholder: Box 0.15 x 0.15 x 0.08 m, Pivot bottom_center, Material-Slots [Button]
feedback: []
```

### SM_Counter_Button_Airlock.fbx
```yaml
path: Assets/_Project/Art/Models/Counter/SM_Counter_Button_Airlock.fbx
package: P05
scene: Schalter
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [0.15, 0.15, 0.12]
pivot: bottom_center
tris_max: 400
materials: [Button]
textures:
  - Assets/_Project/Art/Textures/Counter/T_Counter_Button_Airlock_Albedo.png   # 64x64, Point
  - Assets/_Project/Art/Textures/Counter/T_Counter_Button_Airlock_Emission.png   # 64x64, Point
texel_px_per_m: 128
palette: docs/style/palette.png
look: >
  Roter Knopf 'AIRLOCK' unter einer klappbaren Plexiglas-Schutzkappe, gelb-schwarzer Rahmen. Kappe als eigenes Unterobjekt mit Pivot an der Angel.
interaction: Kappe aufklappen, dann drücken
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_counter_console_01.webp, docs/style/refs/ai_desk_office_01.webp, docs/style/refs/ai_counter_terminal_01.webp]
placeholder: Box 0.15 x 0.15 x 0.12 m, Pivot bottom_center, Material-Slots [Button]
feedback: []
```

### SM_Counter_Stamp.fbx
```yaml
path: Assets/_Project/Art/Models/Counter/SM_Counter_Stamp.fbx
package: P05
scene: Schalter
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [0.08, 0.08, 0.12]
pivot: bottom_center
tris_max: 300
materials: [Stamp]
textures:
  - Assets/_Project/Art/Textures/Counter/T_Counter_Stamp_Albedo.png   # 64x64, Point
texel_px_per_m: 128
palette: docs/style/palette.png
look: >
  Handstempel mit Holzgriff und Metallfuß, grüner Fuß mit APPROVED.
interaction: greifen und auf Dokument drücken
variants:
  - SM_Counter_Stamp_Deny (roter Fuß, DENIED)
animations: []
collision: prefab
refs: [docs/style/refs/ai_counter_console_01.webp, docs/style/refs/ai_desk_office_01.webp, docs/style/refs/ai_counter_terminal_01.webp]
placeholder: Box 0.08 x 0.08 x 0.12 m, Pivot bottom_center, Material-Slots [Stamp]
feedback: []
```

### SM_Counter_DocSlot.fbx
```yaml
path: Assets/_Project/Art/Models/Counter/SM_Counter_DocSlot.fbx
package: P05
scene: Schalter
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [0.5, 0.4, 0.15]
pivot: bottom_center
tris_max: 400
materials: [Slot]
textures:
  - Assets/_Project/Art/Textures/Counter/T_Counter_DocSlot_Albedo.png   # 128x128, Point
texel_px_per_m: 128
palette: docs/style/palette.png
look: >
  Dokument-Durchreiche unter der Scheibe: Metallschublade, die zum Reisenden hin aus- und einfährt.
interaction: fährt aus/ein (Skript)
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_counter_console_01.webp, docs/style/refs/ai_desk_office_01.webp, docs/style/refs/ai_counter_terminal_01.webp]
placeholder: Box 0.5 x 0.4 x 0.15 m, Pivot bottom_center, Material-Slots [Slot]
feedback: []
```

### SM_Counter_Locker.fbx
```yaml
path: Assets/_Project/Art/Models/Counter/SM_Counter_Locker.fbx
package: P05
scene: Schalter
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [1.0, 0.5, 1.8]
pivot: bottom_center
tris_max: 600
materials: [Locker]
textures:
  - Assets/_Project/Art/Textures/Counter/T_Counter_Locker_Albedo.png   # 128x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Asservatenschrank: grauer Stahlspind, 2 Türen, Schild 'CONFISCATED', Vorhängeschloss.
interaction: öffnen (Inventar Beschlagnahmtes)
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_control_room_01.webp]
placeholder: Box 1.0 x 0.5 x 1.8 m, Pivot bottom_center, Material-Slots [Locker]
feedback: []
```

### SM_Counter_Speaker.fbx
```yaml
path: Assets/_Project/Art/Models/Counter/SM_Counter_Speaker.fbx
package: P05
scene: Schalter
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [0.4, 0.15, 0.3]
pivot: bottom_center
tris_max: 300
materials: [Speaker]
textures:
  - Assets/_Project/Art/Textures/Counter/T_Counter_Speaker_Albedo.png   # 64x64, Point
  - Assets/_Project/Art/Textures/Counter/T_Counter_Speaker_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Wandlautsprecher im Metallgitter mit kleiner Lampe (leuchtet bei Durchsage des Vorgesetzten).
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_control_room_01.webp]
placeholder: Box 0.4 x 0.15 x 0.3 m, Pivot bottom_center, Material-Slots [Speaker]
feedback: []
```

### SM_Counter_Board.fbx
```yaml
path: Assets/_Project/Art/Models/Counter/SM_Counter_Board.fbx
package: P05
scene: Schalter
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [1.0, 0.05, 0.7]
pivot: bottom_center
tris_max: 100
materials: [Board]
textures:
  - Assets/_Project/Art/Textures/Counter/T_Counter_Board_Albedo.png   # 128x128, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Pinnwand aus Kork mit Metallrahmen für das Tagesbulletin; Zettel sind eigene UI-Grafiken.
interaction: ansehen (Bulletin)
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/mw_cabin_posters_01.webp]
placeholder: Box 1.0 x 0.05 x 0.7 m, Pivot bottom_center, Material-Slots [Board]
feedback: []
```

### SM_Counter_WarningLamp.fbx
```yaml
path: Assets/_Project/Art/Models/Counter/SM_Counter_WarningLamp.fbx
package: P05
scene: Prüfkammer
status: offen
priority: P1
needed_by: 2026-11-12
size_m: [0.25, 0.25, 0.3]
pivot: top_center
tris_max: 300
materials: [Lamp]
textures:
  - Assets/_Project/Art/Textures/Counter/T_Counter_WarningLamp_Albedo.png   # 64x64, Point
  - Assets/_Project/Art/Textures/Counter/T_Counter_WarningLamp_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Rundumleuchte in Rot mit Gitterkorb an der Decke der Prüfkammer.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_door_steam_01.webp]
placeholder: Box 0.25 x 0.25 x 0.3 m, Pivot top_center, Material-Slots [Lamp]
feedback: []
```

### SM_Hall_DepartureBoard.fbx
```yaml
path: Assets/_Project/Art/Models/Hall/SM_Hall_DepartureBoard.fbx
package: P04
scene: Halle
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [2.0, 2.0, 3.5]
pivot: bottom_center
tris_max: 1200
materials: [Board, Screen]
textures:
  - Assets/_Project/Art/Textures/Hall/T_Hall_DepartureBoard_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Hall/T_Hall_DepartureBoard_Emission.png   # 128x128, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Anzeigesäule in der Hallenmitte: Vierkantsäule, an jeder Seite zwei CRT-Monitore in Bernstein mit Ankunfts-/Abflugtabellen, oben ein Schild 'KESTREL-9 TRANSIT CONTROL'. Kabel laufen in die Decke.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_lounge_terminal_01.webp, docs/style/refs/mw_corridor_signs_01.webp]
placeholder: Box 2.0 x 2.0 x 3.5 m, Pivot bottom_center, Material-Slots [Board, Screen]
feedback: []
```

### SM_Hall_Bench.fbx
```yaml
path: Assets/_Project/Art/Models/Hall/SM_Hall_Bench.fbx
package: P04
scene: Halle
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [2.8, 0.6, 0.9]
pivot: bottom_center
tris_max: 800
materials: [Bench]
textures:
  - Assets/_Project/Art/Textures/Hall/T_Hall_Bench_Albedo.png   # 128x128, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Wartebank: 4 verbundene Schalensitze aus orangem Kunststoff (#B9824A) auf Metallschiene, einer gesprungen.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/real_border_control_lanes_01.webp]
placeholder: Box 2.8 x 0.6 x 0.9 m, Pivot bottom_center, Material-Slots [Bench]
feedback: []
```

### SM_Hall_VendingMachine.fbx
```yaml
path: Assets/_Project/Art/Models/Hall/SM_Hall_VendingMachine.fbx
package: P04
scene: Halle
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [1.0, 0.8, 1.9]
pivot: bottom_center
tris_max: 600
materials: [Machine]
textures:
  - Assets/_Project/Art/Textures/Hall/T_Hall_VendingMachine_Albedo.png   # 128x256, Point
  - Assets/_Project/Art/Textures/Hall/T_Hall_VendingMachine_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Getränkeautomat: beiges Gehäuse, beleuchtete Front mit Dosenreihen (Emission), Münzschlitz, Delle von einem Tritt.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_lounge_terminal_01.webp]
placeholder: Box 1.0 x 0.8 x 1.9 m, Pivot bottom_center, Material-Slots [Machine]
feedback: []
```

### SM_Hall_Planter.fbx
```yaml
path: Assets/_Project/Art/Models/Hall/SM_Hall_Planter.fbx
package: P04
scene: Halle
status: offen
priority: P2
needed_by: 2026-11-22
size_m: [1.0, 1.0, 1.4]
pivot: bottom_center
tris_max: 600
materials: [Planter]
textures:
  - Assets/_Project/Art/Textures/Hall/T_Hall_Planter_Albedo.png   # 128x128, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Pflanzkübel aus Metall mit halb vertrockneter Hydrokultur-Pflanze unter einer kleinen UV-Lampe.
interaction: none
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 1.0 x 1.0 x 1.4 m, Pivot bottom_center, Material-Slots [Planter]
feedback: []
```

### SM_Hall_Sign.fbx
```yaml
path: Assets/_Project/Art/Models/Hall/SM_Hall_Sign.fbx
package: P04
scene: Halle
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [1.2, 0.1, 0.4]
pivot: top_center
tris_max: 200
materials: [Sign]
textures:
  - Assets/_Project/Art/Textures/Hall/T_Hall_Sign_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Hall/T_Hall_Sign_Emission.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Hängendes Wegweiser-Schild: Leuchtkasten mit Pfeil und Text (Orbitron, Großbuchstaben). Text als Texturatlas mit 6 Zeilen: MOTEL, DINER, BAR, KIOSK, HANGAR - STAFF ONLY, CUSTOMS.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/mw_corridor_signs_01.webp]
placeholder: Box 1.2 x 0.1 x 0.4 m, Pivot top_center, Material-Slots [Sign]
feedback: []
```

### SM_Hangar_Shuttle.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_Shuttle.fbx
package: P04
scene: Hangar
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [6.0, 9.0, 3.5]
pivot: bottom_center
tris_max: 3000
materials: [Hull, Detail]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_Shuttle_Albedo.png   # 512x512, Point
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_Shuttle_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Kleines Personenshuttle auf dem Landefeld: klobig, kastenförmig, abgeplatzter weißer Lack mit Rost, Landebeine, Heckrampe offen (Reisende kommen heraus), Cockpitfenster dunkel, Seriennummer am Rumpf.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/real_hangar_shuttle_01.webp, docs/style/refs/mw_cargo_hold_01.webp, docs/style/refs/ai_window_docking_ship_01.webp]
placeholder: Box 6.0 x 9.0 x 3.5 m, Pivot bottom_center, Material-Slots [Hull, Detail]
feedback: []
```

### SM_Hangar_DronePad.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_DronePad.fbx
package: P13
scene: Hangar
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [3.0, 3.0, 0.2]
pivot: bottom_center
tris_max: 600
materials: [Pad]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_DronePad_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_DronePad_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Landeplatte der Frachtdrohne: Gitterplatte mit gelbem Landekreuz und Randlichtern (Emission), daneben ein Terminal-Pfosten mit CRT.
interaction: "Terminal - Paket an Familie schicken"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/real_hangar_shuttle_01.webp, docs/style/refs/mw_cargo_hold_01.webp, docs/style/refs/ai_window_docking_ship_01.webp]
placeholder: Box 3.0 x 3.0 x 0.2 m, Pivot bottom_center, Material-Slots [Pad]
feedback: []
```

### SM_Hangar_CargoDrone.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_CargoDrone.fbx
package: P13
scene: Hangar
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [1.6, 1.6, 0.8]
pivot: bottom_center
tris_max: 1500
materials: [Drone]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_CargoDrone_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Frachtdrohne: kastenförmiger Körper mit 4 ummantelten Rotoren, Frachtklappe unten, Warnstreifen.
interaction: hebt ab (Skript)
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/real_hangar_shuttle_01.webp, docs/style/refs/mw_cargo_hold_01.webp, docs/style/refs/ai_window_docking_ship_01.webp]
placeholder: Box 1.6 x 1.6 x 0.8 m, Pivot bottom_center, Material-Slots [Drone]
feedback: []
```

### SM_Hangar_Crate.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_Crate.fbx
package: P04
scene: Hangar
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [1.2, 1.2, 1.0]
pivot: bottom_center
tris_max: 300
materials: [Crate]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_Crate_Albedo.png   # 128x128, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Frachtkiste: Metallkanten, Seitenwände aus olivgrünem Kunststoff, Schablonenschrift, Griffe.
interaction: none
variants:
  - SM_Hangar_Crate_Small (0,6 x 0,6 x 0,5 m)
animations: []
collision: prefab
refs: [docs/style/refs/ai_corridor_cargo_01.webp, docs/style/refs/mw_props_crate_01.webp]
placeholder: Box 1.2 x 1.2 x 1.0 m, Pivot bottom_center, Material-Slots [Crate]
feedback: []
```

### SM_Hangar_Barrier.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_Barrier.fbx
package: P04
scene: Hangar
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [2.0, 0.3, 1.0]
pivot: bottom_center
tris_max: 300
materials: [Barrier]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_Barrier_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Absperrgitter-Segment für die Warteschlange: zwei Pfosten, Gurtband gelb-schwarz.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/real_border_control_lanes_01.webp]
placeholder: Box 2.0 x 0.3 x 1.0 m, Pivot bottom_center, Material-Slots [Barrier]
feedback: []
```

### SM_Hangar_Tank.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_Tank.fbx
package: P04
scene: Hangar
status: offen
priority: P2
needed_by: 2026-11-22
size_m: [1.4, 1.4, 3.5]
pivot: bottom_center
tris_max: 800
materials: [Tank]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_Tank_Albedo.png   # 128x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Stehender Treibstofftank: Zylinder (8 Segmente) mit Stahlbändern, Ventilen und Rohren, rote Gefahrenraute.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/real_hangar_shuttle_01.webp, docs/style/refs/mw_cargo_hold_01.webp, docs/style/refs/ai_window_docking_ship_01.webp]
placeholder: Box 1.4 x 1.4 x 3.5 m, Pivot bottom_center, Material-Slots [Tank]
feedback: []
```

### SM_Motel_Bed.fbx
```yaml
path: Assets/_Project/Art/Models/Motel/SM_Motel_Bed.fbx
package: P04
scene: Motel
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [1.0, 2.0, 0.6]
pivot: bottom_center
tris_max: 600
materials: [Bed]
textures:
  - Assets/_Project/Art/Textures/Motel/T_Motel_Bed_Albedo.png   # 128x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Schmales Klappbett aus Metallrahmen, dünne Matratze, graue Wolldecke zerwühlt, flaches Kissen.
interaction: schlafen (beendet den Tag)
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/real_crew_quarters_01.webp, docs/style/refs/mw_cabin_posters_01.webp, docs/style/refs/ai_cockpit_chair_01.webp]
placeholder: Box 1.0 x 2.0 x 0.6 m, Pivot bottom_center, Material-Slots [Bed]
feedback: []
```

### SM_Motel_Table.fbx
```yaml
path: Assets/_Project/Art/Models/Motel/SM_Motel_Table.fbx
package: P04
scene: Motel
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [0.8, 0.6, 0.75]
pivot: bottom_center
tris_max: 400
materials: [Table]
textures:
  - Assets/_Project/Art/Textures/Motel/T_Motel_Table_Albedo.png   # 128x128, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Kleiner Wandklapptisch mit Hocker. Platz für Brief und Bulletin (eigene UI-Grafiken).
interaction: Brief lesen
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/real_crew_quarters_01.webp, docs/style/refs/mw_cabin_posters_01.webp, docs/style/refs/ai_cockpit_chair_01.webp]
placeholder: Box 0.8 x 0.6 x 0.75 m, Pivot bottom_center, Material-Slots [Table]
feedback: []
```

### SM_Motel_Locker.fbx
```yaml
path: Assets/_Project/Art/Models/Motel/SM_Motel_Locker.fbx
package: P04
scene: Motel
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [0.6, 0.5, 1.9]
pivot: bottom_center
tris_max: 300
materials: [Locker]
textures:
  - Assets/_Project/Art/Textures/Motel/T_Motel_Locker_Albedo.png   # 128x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Schmaler Wandspind mit Lüftungsschlitzen, Familienfoto auf die Tür geklebt.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/real_crew_quarters_01.webp, docs/style/refs/mw_cabin_posters_01.webp, docs/style/refs/ai_cockpit_chair_01.webp]
placeholder: Box 0.6 x 0.5 x 1.9 m, Pivot bottom_center, Material-Slots [Locker]
feedback: []
```

### SM_Motel_TV.fbx
```yaml
path: Assets/_Project/Art/Models/Motel/SM_Motel_TV.fbx
package: P04
scene: Motel
status: offen
priority: P2
needed_by: 2026-11-22
size_m: [0.5, 0.4, 0.4]
pivot: bottom_center
tris_max: 400
materials: [TV]
textures:
  - Assets/_Project/Art/Textures/Motel/T_Motel_TV_Albedo.png   # 64x64, Point
  - Assets/_Project/Art/Textures/Motel/T_Motel_TV_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Kleiner CRT-Fernseher auf Wandhalterung, Bildschirm flimmert (Emission).
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_terminal_crt_01.webp]
placeholder: Box 0.5 x 0.4 x 0.4 m, Pivot bottom_center, Material-Slots [TV]
feedback: []
```

### SM_Motel_Reception.fbx
```yaml
path: Assets/_Project/Art/Models/Motel/SM_Motel_Reception.fbx
package: P04
scene: Motel
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [1.2, 0.5, 1.2]
pivot: bottom_center
tris_max: 500
materials: [Terminal]
textures:
  - Assets/_Project/Art/Textures/Motel/T_Motel_Reception_Albedo.png   # 128x128, Point
  - Assets/_Project/Art/Textures/Motel/T_Motel_Reception_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Rezeptions-Terminal am Anfang des Motelgangs: Wandkasten mit CRT (Giftgrün), Kartenschlitz, Schild 'SPACE MOTEL - ROOMS BY THE CYCLE'.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_terminal_crt_01.webp, docs/style/refs/ai_lounge_terminal_01.webp]
placeholder: Box 1.2 x 0.5 x 1.2 m, Pivot bottom_center, Material-Slots [Terminal]
feedback: []
```

### SM_Motel_DoorSign.fbx
```yaml
path: Assets/_Project/Art/Models/Motel/SM_Motel_DoorSign.fbx
package: P04
scene: Motel
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [0.3, 0.05, 0.15]
pivot: center
tris_max: 50
materials: [Sign]
textures:
  - Assets/_Project/Art/Textures/Motel/T_Motel_DoorSign_Albedo.png   # 64x64, Point
  - Assets/_Project/Art/Textures/Motel/T_Motel_DoorSign_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Zimmernummernschild mit grüner Leuchtziffer; Texturatlas mit 101, 102, 103.
interaction: none
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.3 x 0.05 x 0.15 m, Pivot center, Material-Slots [Sign]
feedback: []
```

### SM_Bar_Stool.fbx
```yaml
path: Assets/_Project/Art/Models/Bar/SM_Bar_Stool.fbx
package: P04
scene: Bar, Diner
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [0.4, 0.4, 0.75]
pivot: bottom_center
tris_max: 300
materials: [Stool]
textures:
  - Assets/_Project/Art/Textures/Bar/T_Bar_Stool_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Barhocker: Chromrohr, rundes Polster in Dunkelrot (#7A1F2B), Fußring.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_mess_table_02.webp, docs/style/refs/mw_lounge_red_01.webp, docs/style/refs/mw_dialogue_counter_01.webp]
placeholder: Box 0.4 x 0.4 x 0.75 m, Pivot bottom_center, Material-Slots [Stool]
feedback: []
```

### SM_Bar_Table.fbx
```yaml
path: Assets/_Project/Art/Models/Bar/SM_Bar_Table.fbx
package: P04
scene: Bar
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [0.8, 0.8, 1.1]
pivot: bottom_center
tris_max: 300
materials: [Table]
textures:
  - Assets/_Project/Art/Textures/Bar/T_Bar_Table_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Runder Stehtisch (8 Segmente) mit Metallfuß, Platte mit Brandflecken und Ringen.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_mess_table_02.webp, docs/style/refs/mw_lounge_red_01.webp, docs/style/refs/mw_dialogue_counter_01.webp]
placeholder: Box 0.8 x 0.8 x 1.1 m, Pivot bottom_center, Material-Slots [Table]
feedback: []
```

### SM_Bar_Shelf.fbx
```yaml
path: Assets/_Project/Art/Models/Bar/SM_Bar_Shelf.fbx
package: P04
scene: Bar
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [3.0, 0.3, 2.2]
pivot: bottom_center
tris_max: 1500
materials: [Shelf, Bottles]
textures:
  - Assets/_Project/Art/Textures/Bar/T_Bar_Shelf_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Bar/T_Bar_Shelf_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Flaschenregal hinter der Theke: 3 Böden mit bunten Flaschen (als Low-Poly-Gruppen), hinterleuchtet in Orange (Emission).
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_mess_table_02.webp, docs/style/refs/mw_lounge_red_01.webp, docs/style/refs/mw_dialogue_counter_01.webp]
placeholder: Box 3.0 x 0.3 x 2.2 m, Pivot bottom_center, Material-Slots [Shelf, Bottles]
feedback: []
```

### SM_Bar_NeonSign.fbx
```yaml
path: Assets/_Project/Art/Models/Bar/SM_Bar_NeonSign.fbx
package: P04
scene: Bar
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [2.0, 0.1, 0.6]
pivot: center
tris_max: 200
materials: [Neon]
textures:
  - Assets/_Project/Art/Textures/Bar/T_Bar_NeonSign_Albedo.png   # 128x64, Point
  - Assets/_Project/Art/Textures/Bar/T_Bar_NeonSign_Emission.png   # 128x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Neonschild in der Bar: Schriftzug 'THE LAST ORBIT' in Neonviolett (#C27BD6) mit orangem Cocktailglas.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_mess_table_02.webp, docs/style/refs/mw_lounge_red_01.webp, docs/style/refs/mw_dialogue_counter_01.webp]
placeholder: Box 2.0 x 0.1 x 0.6 m, Pivot center, Material-Slots [Neon]
feedback: []
```

### SM_Kiosk_Counter.fbx
```yaml
path: Assets/_Project/Art/Models/Kiosk/SM_Kiosk_Counter.fbx
package: P11
scene: Kiosk
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [0.8, 2.8, 1.0]
pivot: bottom_center
tris_max: 800
materials: [Counter, Glass]
textures:
  - Assets/_Project/Art/Textures/Kiosk/T_Kiosk_Counter_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Kiosk/T_Kiosk_Counter_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Verkaufstheke in der Durchreiche: Metallthekenplatte, Glasvitrine mit Gadgets, Rollgitter oben (halb hochgezogen), Schild 'GADGETS & SUPPLIES' in Violett.
interaction: kaufen (Shop-UI)
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_workshop_bench_01.webp, docs/style/refs/ai_counter_console_01.webp]
placeholder: Box 0.8 x 2.8 x 1.0 m, Pivot bottom_center, Material-Slots [Counter, Glass]
feedback: []
```

### SM_Kiosk_Shelf.fbx
```yaml
path: Assets/_Project/Art/Models/Kiosk/SM_Kiosk_Shelf.fbx
package: P11
scene: Kiosk
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [0.4, 2.8, 2.0]
pivot: bottom_center
tris_max: 1000
materials: [Shelf]
textures:
  - Assets/_Project/Art/Textures/Kiosk/T_Kiosk_Shelf_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Wandregal voller Kartons, Ersatzteile, Batterien und Geräte in Beige; einige Preisschilder.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_workshop_bench_01.webp]
placeholder: Box 0.4 x 2.8 x 2.0 m, Pivot bottom_center, Material-Slots [Shelf]
feedback: []
```

### SM_Diner_Counter.fbx
```yaml
path: Assets/_Project/Art/Models/Diner/SM_Diner_Counter.fbx
package: P04
scene: Diner
status: offen
priority: P2
needed_by: 2026-11-22
size_m: [6.0, 0.7, 1.1]
pivot: bottom_center
tris_max: 1200
materials: [Counter]
textures:
  - Assets/_Project/Art/Textures/Diner/T_Diner_Counter_Albedo.png   # 256x128, Point
  - Assets/_Project/Art/Textures/Diner/T_Diner_Counter_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Diner-Tresen: Front aus Chrom-Riffelblech, Platte cremefarben (#E2DDCF), Kaffeemaschine und Tortenhaube, Bernstein-Licht.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_mess_table_01.webp, docs/style/refs/ai_mess_table_02.webp]
placeholder: Box 6.0 x 0.7 x 1.1 m, Pivot bottom_center, Material-Slots [Counter]
feedback: []
```

### SM_Diner_Booth.fbx
```yaml
path: Assets/_Project/Art/Models/Diner/SM_Diner_Booth.fbx
package: P04
scene: Diner
status: offen
priority: P2
needed_by: 2026-11-22
size_m: [2.2, 1.4, 1.2]
pivot: bottom_center
tris_max: 900
materials: [Booth]
textures:
  - Assets/_Project/Art/Textures/Diner/T_Diner_Booth_Albedo.png   # 128x128, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Sitznische: zwei gepolsterte Bänke (Kunstleder Bernstein/Braun, geflickt) und Tisch dazwischen, Serviettenspender.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_mess_table_01.webp, docs/style/refs/ai_mess_table_02.webp]
placeholder: Box 2.2 x 1.4 x 1.2 m, Pivot bottom_center, Material-Slots [Booth]
feedback: []
```

### SM_Diner_MenuBoard.fbx
```yaml
path: Assets/_Project/Art/Models/Diner/SM_Diner_MenuBoard.fbx
package: P04
scene: Diner
status: offen
priority: P2
needed_by: 2026-11-22
size_m: [2.0, 0.1, 0.8]
pivot: center
tris_max: 100
materials: [Board]
textures:
  - Assets/_Project/Art/Textures/Diner/T_Diner_MenuBoard_Albedo.png   # 256x128, Point
  - Assets/_Project/Art/Textures/Diner/T_Diner_MenuBoard_Emission.png   # 256x128, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Menütafel mit Leuchtbuchstaben in Bernstein: Gerichte wie 'PROTEIN LOAF', 'RECYCLED COFFEE', Preise in Credits.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_mess_table_01.webp, docs/style/refs/ai_mess_table_02.webp]
placeholder: Box 2.0 x 0.1 x 0.8 m, Pivot center, Material-Slots [Board]
feedback: []
```

### T_Sky_Space.png
```yaml
path: Assets/_Project/Art/Textures/Sky/T_Sky_Space.png
package: P04
scene: Halle, Hangar, Prüfkammer (sichtbar durch Fenster und Hangartor)
status: offen
priority: P1
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Sky/T_Sky_Space.png   # 2048x1024 equirectangular, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Weltraum-Panorama (equirectangular): dunkles Blauschwarz (#0D0F14/#1B1F27) mit spärlichen Pixel-Sternen, ein schwacher
  violett-cyanfarbener Nebel und ein großer, kalter Heimatplanet unten im Bild (sichtbar durch das Panoramafenster nach Norden),
  dessen Nachtseite ein paar warme Stadtlichter zeigt. Kein Bloom, harte Pixel.
interaction: none
variants: []
animations: []
collision: none
refs: [docs/style/refs/ai_station_exterior_01.webp, docs/style/refs/ai_window_docking_ship_01.webp]
placeholder: PNG 2048x1024, schwarz mit weißen Einzelpixeln
feedback: []
```
