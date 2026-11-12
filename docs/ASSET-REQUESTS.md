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
status: final
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
status: final
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
status: final
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
status: final
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
status: final
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
status: final
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
status: final
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
status: final
priority: P3
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
feedback:
  - "2026-11-02 max: ENTFÄLLT - die Aussichtsplattform (SM_Station_Window_Curved) ersetzt das Panoramafenster. Bitte nicht bauen."
```

### SM_Station_Window_Small.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Window_Small.fbx
package: P04
scene: Schalter
status: final
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
status: final
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
status: final
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
feedback:
  - "2026-11-02 max: Schott klappt nach außen ins All weg (M005 Punkt 5) - Pivot hinge_left an der Nordkante, geschlossen bündig in der Wand."
```

### SM_Hangar_Door_Big.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_Door_Big.fbx
package: P04
scene: Hangar
status: final
priority: P3
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
feedback:
  - "2026-11-02 max: ENTFÄLLT (M004) - Barriere + Emitter bilden die Öffnung, wird nicht verbaut."
```

### SM_Counter_Desk.fbx
```yaml
path: Assets/_Project/Art/Models/Counter/SM_Counter_Desk.fbx
package: P05
scene: Schalter
status: final
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
status: final
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
status: final
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
status: final
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
status: final
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
status: final
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
status: final
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
status: final
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
status: final
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
status: final
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
status: final
priority: P1
needed_by: 2026-11-16
size_m: [2.0, 2.0, 6.0]
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
feedback:
  - "2026-11-03 max: (M008/1) Halle ist 6 m hoch - Halteseile bis zur Decke auf 6 m verlaengern, Tafel selbst bleibt in Hoehe und Groesse wie geliefert."
```

### SM_Hall_Bench.fbx
```yaml
path: Assets/_Project/Art/Models/Hall/SM_Hall_Bench.fbx
package: P04
scene: Halle
status: final
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
status: final
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
status: final
priority: P2
needed_by: 2026-11-22
size_m: [1.0, 1.0, 1.4]
pivot: bottom_center
tris_max: 600
materials: [Planter]
textures:
  - Assets/_Project/Art/Textures/Hall/T_Hall_Planter_Albedo.png   # 128x128, Point
  - Assets/_Project/Art/Textures/Hall/T_Hall_Planter_Emission.png   # 64x64, Point
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
feedback:
  - "2026-11-03 max: (M008/3) Violettes Pflanzenlicht soll leuchten - Emission-Textur nachliefern, Modell bleibt."
```

### SM_Hall_Sign.fbx
```yaml
path: Assets/_Project/Art/Models/Hall/SM_Hall_Sign.fbx
package: P04
scene: Halle
status: final
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
status: final
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
feedback:
  - "2026-11-02 max: Heckrampe als eigenes Unterobjekt 'Ramp' mit Pivot an der Angel (klappt per Skript hoch/runter, P25). Rest bleibt."
```

### SM_Hangar_DronePad.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_DronePad.fbx
package: P13
scene: Hangar
status: final
priority: P1
needed_by: 2026-11-16
size_m: [3.0, 3.0, 1.42]
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
feedback:
  - "2026-11-03 max: (M011) Hoehe mit Terminal-Pfosten 1,42 m angenommen, Lieferung passt."
```

### SM_Hangar_CargoDrone.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_CargoDrone.fbx
package: P13
scene: Hangar
status: final
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
status: final
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
status: final
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
status: final
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
status: final
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
status: final
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
status: final
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
feedback:
  - "2026-11-09 max: Auf jedem Spind steht fest die Nummer 103, auch in fremden Zimmern (Issue #29). Bitte die Nummer aus der Textur nehmen."
```

### SM_Motel_TV.fbx
```yaml
path: Assets/_Project/Art/Models/Motel/SM_Motel_TV.fbx
package: P04
scene: Motel
status: final
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
status: final
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
status: final
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
status: final
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
status: final
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
status: final
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
status: final
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
status: final
priority: P1
needed_by: 2026-11-16
size_m: [0.8, 2.8, 2.4]
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
feedback:
  - "2026-11-03 max: (M008/5) Tresen bleibt 1,0 m hoch; Rollgitter schliesst die Oeffnung darueber, violettes Schild haengt oben - Gesamthoehe 2,4 m."
  - "2026-11-08 max: (P90, Issue #21) Rollgitter aus dem Modell nehmen: Die Öffnung über der Theke bleibt frei, oben nur Blende und Schild. Das Gitter kommt als eigenes Modell SM_Kiosk_Shutter und ist entweder ganz offen oder ganz zu."
```

### SM_Kiosk_Shelf.fbx
```yaml
path: Assets/_Project/Art/Models/Kiosk/SM_Kiosk_Shelf.fbx
package: P11
scene: Kiosk
status: final
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
status: final
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
status: final
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
status: final
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
status: final
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
feedback:
  - "2026-11-02 max: Nur noch Sterne + Nebel (violett/cyan, dezent) + ferner Asteroidengürtel-Streifen, KEIN Planet - Sonne und Planeten kommen als eigene Objekte (P24)"
```

### SM_Station_Window_Curved.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Window_Curved.fbx
package: P22
scene: Aussichtsplattform
status: final
priority: P1
needed_by: 2026-11-09
size_m: [1.95, 0.4, 6.4]
pivot: bottom_center
tris_max: 1000
materials: [Frame, Glass]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Window_Curved_Albedo.png   # 256x512, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Ein Segment der halbrunden Glasfront der Aussichtsplattform (8 Segmente ergeben den Halbkreis, r = 5 m). Dicker, abgenutzter Metallrahmen mit Nieten, unten 0,5 m geschlossene Brüstung, darüber Glas bis 5,8 m, oben Rahmen. Seitliche Pfosten breit genug, dass zwei Segmente im Winkel von 22,5 Grad nahtlos aneinanderstoßen. Glas als eigener Slot.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_corridor_window_01.webp, docs/style/refs/ai_window_docking_ship_01.webp]
placeholder: Box 1.95 x 0.4 x 6.4 m, Pivot bottom_center, Material-Slots [Frame, Glass]
feedback: []
```

### SM_Station_Floor_DeckRound.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Floor_DeckRound.fbx
package: P22
scene: Aussichtsplattform
status: final
priority: P1
needed_by: 2026-11-09
size_m: [10.0, 5.0, 0.1]
pivot: bottom_center
tris_max: 400
materials: [Floor]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Floor_DeckRound_Albedo.png   # 512x256, Point
  - Assets/_Project/Art/Textures/Station/T_Station_Floor_DeckRound_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Boden der Aussichtsplattform: Halbkreis r = 5 m (16 Segmente), gerade Kante an der Südseite (lokal -Z). Gleiche Metallplatten wie SM_Station_Floor_2x2, radial verlegt, mit einem Leuchtstreifen-Bogen 0,3 m vor dem Glas.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/mw_cargo_hold_01.webp]
placeholder: Box 10.0 x 5.0 x 0.1 m, Pivot bottom_center, Material-Slots [Floor]
feedback: []
```

### SM_Station_Ceiling_DeckRound.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Ceiling_DeckRound.fbx
package: P22
scene: Aussichtsplattform
status: final
priority: P1
needed_by: 2026-11-09
size_m: [10.0, 5.0, 0.3]
pivot: bottom_center
tris_max: 600
materials: [Ceiling]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Ceiling_DeckRound_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Station/T_Station_Ceiling_DeckRound_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Decke der Aussichtsplattform: Halbkreis r = 5 m, leicht gewölbt, Rippen von der Mitte zum Glasrand, ein paar kleine Spots.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 10.0 x 5.0 x 0.3 m, Pivot bottom_center, Material-Slots [Ceiling]
feedback: []
```

### SM_Station_Steps_2m.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Steps_2m.fbx
package: P22
scene: Aussichtsplattform
status: final
priority: P1
needed_by: 2026-11-09
size_m: [2.0, 0.8, 0.36]
pivot: bottom_center
tris_max: 200
materials: [Steps]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Steps_2m_Albedo.png   # 128x128, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Zwei Stufen à 0,18 m hoch und 0,4 m tief, 2 m breit, Metall mit gelb-schwarzer Kante an jeder Stufe und Gitterrost-Trittflächen. Oberkante bündig mit dem Hallenboden (Pivot unten mittig an der unteren Stufe, lokal +Z = oben/Halle).
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 2.0 x 0.8 x 0.36 m, Pivot bottom_center, Material-Slots [Steps]
feedback: []
```

### SM_Station_Wall_Window_2x3.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Wall_Window_2x3.fbx
package: P22
scene: Bar
status: final
priority: P1
needed_by: 2026-11-09
size_m: [2.0, 0.2, 3.0]
pivot: bottom_center
tris_max: 1000
materials: [Wall, Glass]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_Window_2x3_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Wandmodul 2 x 3 m wie SM_Station_Wall_2x3, mit Fenster von 0,9 bis 2,4 m Höhe über die volle Breite (Rahmen 0,1 m), dickes Glas als eigener Slot. Kachelt seitlich zu einem Fensterband.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_corridor_window_01.webp]
placeholder: Box 2.0 x 0.2 x 3.0 m, Pivot bottom_center, Material-Slots [Wall, Glass]
feedback: []
```

### SM_Hangar_Wall_4x4.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_Wall_4x4.fbx
package: P22
scene: Hangar
status: final
priority: P1
needed_by: 2026-11-09
size_m: [4.0, 0.3, 4.0]
pivot: bottom_center
tris_max: 1000
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_Wall_4x4_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Hangar-Wandmodul 4 x 4 m: grobe Industrieplatten mit senkrechten Rippen, Nieten, Ölspuren, Nummern-Schablonen, ab und zu Kabeltrassen. Kachelt seitlich und übereinander (Hangar 16 m hoch = 4 Reihen).
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/real_hangar_shuttle_01.webp, docs/style/refs/mw_cargo_hold_01.webp]
placeholder: Box 4.0 x 0.3 x 4.0 m, Pivot bottom_center, Material-Slots [Wall]
feedback:
  - "2026-11-09 max: Die mittlere senkrechte Rippe jeder Kachel (x +-0,12 m) wirkt im Spiel wie ein schwarzer Streifen und flackert beim Bewegen (Issue #17). Bitte die Rippe in der Plattenmitte entfernen oder in der hellen Stahlfarbe der Randrippen einfärben; Textur-Spalte um u 0,91 bis 0,97 aufhellen, keine schmalen Spalten mit hohem Kontrast."
```

### SM_Hangar_Floor_4x4.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_Floor_4x4.fbx
package: P22
scene: Hangar
status: final
priority: P1
needed_by: 2026-11-09
size_m: [4.0, 4.0, 0.15]
pivot: bottom_center
tris_max: 1000
materials: [Floor]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_Floor_4x4_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Hangar-Bodenplatte 4 x 4 m: schwerer Stahlboden mit Reifenspuren, Ölflecken, Bodenmarkierungen. Varianten mit gelber Landekreis-Markierung und Pfeilen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/real_hangar_shuttle_01.webp]
placeholder: Box 4.0 x 4.0 x 0.15 m, Pivot bottom_center, Material-Slots [Floor]
feedback: []
```

### SM_Hangar_Ceiling_4x4.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_Ceiling_4x4.fbx
package: P22
scene: Hangar
status: final
priority: P1
needed_by: 2026-11-09
size_m: [4.0, 4.0, 1.0]
pivot: bottom_center
tris_max: 800
materials: [Ceiling]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_Ceiling_4x4_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_Ceiling_4x4_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Hangar-Deckenmodul 4 x 4 m: Stahlfachwerk mit Lampenkasten in der Mitte (Emission, kaltes Weiß), Kabel, Kettenhaken.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 4.0 x 4.0 x 1.0 m, Pivot bottom_center, Material-Slots [Ceiling]
feedback: []
```

### SM_Hangar_BarrierEmitter.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_BarrierEmitter.fbx
package: P23
scene: Hangar
status: final
priority: P1
needed_by: 2026-11-09
size_m: [4.0, 1.2, 1.0]
pivot: bottom_center
tris_max: 1000
materials: [Emitter]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_BarrierEmitter_Albedo.png   # 256x128, Point
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_BarrierEmitter_Emission.png   # 128x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Emitter-Segment für die Energiebarriere, 4 m lang (7 Stück oben, 7 unten, gedreht an den Seiten): klobiger Metallkasten mit Kühlrippen, Kabeln und einer durchgehenden Linse an der Innenseite, die blau leuchtet (Emission). Warnschilder 'FORCE FIELD - DO NOT TOUCH'.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/real_hangar_shuttle_01.webp]
placeholder: Box 4.0 x 1.2 x 1.0 m, Pivot bottom_center, Material-Slots [Emitter]
feedback: []
```

### SM_Hangar_Scaffold.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_Scaffold.fbx
package: P26
scene: Hangar
status: final
priority: P2
needed_by: 2026-11-16
size_m: [4.0, 2.0, 5.0]
pivot: bottom_center
tris_max: 1200
materials: [Scaffold]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_Scaffold_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Wartungsgerüst mit zwei Ebenen, Gitterrosten, Geländer, Leiter, ein paar Werkzeugkisten. Gelb lackiert, abgeplatzt.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/real_hangar_shuttle_01.webp]
placeholder: Box 4.0 x 2.0 x 5.0 m, Pivot bottom_center, Material-Slots [Scaffold]
feedback: []
```

### SM_Hangar_CraneRail.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_CraneRail.fbx
package: P26
scene: Hangar
status: final
priority: P2
needed_by: 2026-11-16
size_m: [36.0, 2.0, 1.5]
pivot: top_center
tris_max: 800
materials: [Crane]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_CraneRail_Albedo.png   # 256x128, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Laufkran-Schiene quer durch den Hangar unter der Decke mit Laufkatze und hängendem Haken.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 36.0 x 2.0 x 1.5 m, Pivot top_center, Material-Slots [Crane]
feedback: []
```

### SM_Ext_HabModule.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_HabModule.fbx
package: P24
scene: Außen (sichtbar durch Fenster + Startbildschirm)
status: final
priority: P2
needed_by: 2026-11-13
size_m: [12.0, 12.0, 30.0]
pivot: center
tris_max: 1500
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HabModule_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HabModule_Emission.png   # 128x128, Point
texel_px_per_m: 8
palette: docs/style/palette.png
look: >
  Wohnmodul der Station von außen: liegender Zylinder (12 Segmente), Rumpfplatten in Beige/Grau mit Dreck- und Brandspuren, kleine erleuchtete Fensterreihen (Emission, warm), Logo-freie Schriftzüge 'KESTREL-9 / HAB 2'.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_station_exterior_01.webp]
placeholder: Box 12.0 x 12.0 x 30.0 m, Pivot center, Material-Slots [Hull]
feedback: []
```

### SM_Ext_TrussSegment.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_TrussSegment.fbx
package: P24
scene: Außen (sichtbar durch Fenster + Startbildschirm)
status: final
priority: P2
needed_by: 2026-11-13
size_m: [3.0, 3.0, 20.0]
pivot: center
tris_max: 600
materials: [Truss]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_TrussSegment_Albedo.png   # 128x128, Point
texel_px_per_m: 8
palette: docs/style/palette.png
look: >
  Fachwerk-Träger, der Module verbindet, mit Rohrleitungen und Kabeln.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 3.0 x 3.0 x 20.0 m, Pivot center, Material-Slots [Truss]
feedback: []
```

### SM_Ext_SolarArray.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_SolarArray.fbx
package: P24
scene: Außen (sichtbar durch Fenster + Startbildschirm)
status: final
priority: P2
needed_by: 2026-11-13
size_m: [30.0, 1.0, 10.0]
pivot: center
tris_max: 400
materials: [Solar]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_SolarArray_Albedo.png   # 256x128, Point
texel_px_per_m: 8
palette: docs/style/palette.png
look: >
  Großer Solarflügel: Gitter aus dunkelblauen Zellen mit Metallrahmen, ein paar Zellen fehlen/zersplittert.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 30.0 x 1.0 x 10.0 m, Pivot center, Material-Slots [Solar]
feedback: []
```

### SM_Ext_Antenna.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_Antenna.fbx
package: P24
scene: Außen (sichtbar durch Fenster + Startbildschirm)
status: final
priority: P2
needed_by: 2026-11-13
size_m: [4.0, 4.0, 18.0]
pivot: bottom_center
tris_max: 800
materials: [Antenna]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_Antenna_Albedo.png   # 128x128, Point
  - Assets/_Project/Art/Textures/Exterior/T_Ext_Antenna_Emission.png   # 64x64, Point
texel_px_per_m: 8
palette: docs/style/palette.png
look: >
  Antennenmast mit zwei Schüsseln, Gitterkorb und rotem Blinklicht oben (Emission).
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 4.0 x 4.0 x 18.0 m, Pivot bottom_center, Material-Slots [Antenna]
feedback: []
```

### SM_Ext_DockingArm.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_DockingArm.fbx
package: P24
scene: Außen (sichtbar durch Fenster + Startbildschirm)
status: final
priority: P2
needed_by: 2026-11-13
size_m: [6.0, 25.0, 6.0]
pivot: center
tris_max: 1000
materials: [Dock]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_DockingArm_Albedo.png   # 256x128, Point
  - Assets/_Project/Art/Textures/Exterior/T_Ext_DockingArm_Emission.png   # 64x64, Point
texel_px_per_m: 8
palette: docs/style/palette.png
look: >
  Andockarm mit Gelenk und Schleusen-Kopf; daran parkt ein Schiff. Positionslichter (Emission, grün/rot).
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 6.0 x 25.0 x 6.0 m, Pivot center, Material-Slots [Dock]
feedback: []
```

### SM_Ext_HangarMouth.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_HangarMouth.fbx
package: P24
scene: Außen (sichtbar durch Fenster + Startbildschirm)
status: final
priority: P2
needed_by: 2026-11-13
size_m: [36.0, 10.0, 18.0]
pivot: bottom_center
tris_max: 1200
materials: [Frame]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HangarMouth_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HangarMouth_Emission.png   # 128x64, Point
texel_px_per_m: 8
palette: docs/style/palette.png
look: >
  Außenrahmen um die Hangaröffnung: massive Einfassung mit Warnstreifen, Positionslichtern, Anflugbefeuerung (Emission), Schriftzug 'CUSTOMS - ALL TRAFFIC STOP'.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 36.0 x 10.0 x 18.0 m, Pivot bottom_center, Material-Slots [Frame]
feedback: []
```

### SM_Ext_DeckDome.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_DeckDome.fbx
package: P24
scene: Außen (sichtbar durch Fenster + Startbildschirm)
status: final
priority: P2
needed_by: 2026-11-13
size_m: [12.0, 7.0, 8.0]
pivot: bottom_center
tris_max: 1000
materials: [Hull, Glass]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_DeckDome_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Exterior/T_Ext_DeckDome_Emission.png   # 64x64, Point
texel_px_per_m: 8
palette: docs/style/palette.png
look: >
  Außenhülle der Aussichtsplattform: halbrunde Glasfront mit Rahmen von außen, Rumpfkranz drumherum, Licht dringt nach außen (Emission).
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 12.0 x 7.0 x 8.0 m, Pivot bottom_center, Material-Slots [Hull, Glass]
feedback: []
```

### SM_Ext_RadiatorPanel.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_RadiatorPanel.fbx
package: P24
scene: Außen (sichtbar durch Fenster + Startbildschirm)
status: final
priority: P3
needed_by: 2026-11-16
size_m: [2.0, 12.0, 8.0]
pivot: center
tris_max: 200
materials: [Radiator]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_RadiatorPanel_Albedo.png   # 128x128, Point
  - Assets/_Project/Art/Textures/Exterior/T_Ext_RadiatorPanel_Emission.png   # 64x64, Point
texel_px_per_m: 8
palette: docs/style/palette.png
look: >
  Kühlpaneel, glüht an den Rändern leicht orange (Emission).
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 2.0 x 12.0 x 8.0 m, Pivot center, Material-Slots [Radiator]
feedback: []
```

### SM_Ship_Freighter.fbx
```yaml
path: Assets/_Project/Art/Models/Ships/SM_Ship_Freighter.fbx
package: P25
scene: Weltraum vor den Fenstern
status: final
priority: P2
needed_by: 2026-11-13
size_m: [40.0, 160.0, 30.0]
pivot: center
tris_max: 2500
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Ships/T_Ship_Freighter_Albedo.png   # 512x512, Point
  - Assets/_Project/Art/Textures/Ships/T_Ship_Freighter_Emission.png   # 256x256, Point
texel_px_per_m: 4
palette: docs/style/palette.png
look: >
  Großer Frachter (fliegt langsam an den Fenstern vorbei): langer Rücken aus Containermodulen, Brückenturm hinten, Triebwerke mit blauem Glühen (Emission), Positionslichter, viele kleine erleuchtete Fenster. Abgenutzter Industrie-Look.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_window_docking_ship_01.webp, docs/style/refs/ai_station_exterior_01.webp]
placeholder: Box 40.0 x 160.0 x 30.0 m, Pivot center, Material-Slots [Hull]
feedback: []
```

### SM_Ship_Tanker.fbx
```yaml
path: Assets/_Project/Art/Models/Ships/SM_Ship_Tanker.fbx
package: P25
scene: Weltraum vor den Fenstern
status: final
priority: P2
needed_by: 2026-11-13
size_m: [30.0, 120.0, 30.0]
pivot: center
tris_max: 2000
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Ships/T_Ship_Tanker_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Ships/T_Ship_Tanker_Emission.png   # 128x128, Point
texel_px_per_m: 4
palette: docs/style/palette.png
look: >
  Tanker mit drei kugelförmigen Tanks hintereinander an einem Rückgrat, Warnmarkierungen, kleine Brücke vorne.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 30.0 x 120.0 x 30.0 m, Pivot center, Material-Slots [Hull]
feedback: []
```

### SM_Ship_Patrol.fbx
```yaml
path: Assets/_Project/Art/Models/Ships/SM_Ship_Patrol.fbx
package: P25
scene: Weltraum vor den Fenstern
status: final
priority: P3
needed_by: 2026-11-16
size_m: [15.0, 35.0, 8.0]
pivot: center
tris_max: 1500
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Ships/T_Ship_Patrol_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Ships/T_Ship_Patrol_Emission.png   # 64x64, Point
texel_px_per_m: 8
palette: docs/style/palette.png
look: >
  Zoll-Patrouillenschiff: schnittig aber klobig, Suchscheinwerfer, rote/blaue Blinklichter (Emission), Aufschrift 'CUSTOMS'.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 15.0 x 35.0 x 8.0 m, Pivot center, Material-Slots [Hull]
feedback: []
```

### SM_Ship_TrafficSmall.fbx
```yaml
path: Assets/_Project/Art/Models/Ships/SM_Ship_TrafficSmall.fbx
package: P25
scene: Weltraum vor den Fenstern
status: final
priority: P3
needed_by: 2026-11-16
size_m: [4.0, 8.0, 2.5]
pivot: center
tris_max: 300
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Ships/T_Ship_TrafficSmall_Albedo.png   # 64x64, Point
  - Assets/_Project/Art/Textures/Ships/T_Ship_TrafficSmall_Emission.png   # 64x64, Point
texel_px_per_m: 8
palette: docs/style/palette.png
look: >
  Kleines Taxi-/Pendelschiff für den Hintergrundverkehr, sehr einfach, Triebwerksglühen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 4.0 x 8.0 x 2.5 m, Pivot center, Material-Slots [Hull]
feedback: []
```

### SM_Prop_PipeBundle_2m.fbx
```yaml
path: Assets/_Project/Art/Models/Props/SM_Prop_PipeBundle_2m.fbx
package: P26
scene: alle
status: final
priority: P2
needed_by: 2026-11-16
size_m: [2.0, 0.4, 0.5]
pivot: bottom_center
tris_max: 300
materials: [Pipe]
textures:
  - Assets/_Project/Art/Textures/Props/T_Prop_PipeBundle_2m_Albedo.png   # 128x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Drei bis vier parallele Rohre mit Schellen, für Wände und Decken, kachelt in Längsrichtung.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 2.0 x 0.4 x 0.5 m, Pivot bottom_center, Material-Slots [Pipe]
feedback: []
```

### SM_Prop_PipeElbow.fbx
```yaml
path: Assets/_Project/Art/Models/Props/SM_Prop_PipeElbow.fbx
package: P26
scene: alle
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.6, 0.6, 0.5]
pivot: bottom_center
tris_max: 300
materials: [Pipe]
textures:
  - Assets/_Project/Art/Textures/Props/T_Prop_PipeElbow_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Bogenstück zum Rohrbündel (90 Grad).
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.6 x 0.6 x 0.5 m, Pivot bottom_center, Material-Slots [Pipe]
feedback: []
```

### SM_Prop_CableBundle.fbx
```yaml
path: Assets/_Project/Art/Models/Props/SM_Prop_CableBundle.fbx
package: P26
scene: alle
status: final
priority: P2
needed_by: 2026-11-16
size_m: [2.0, 0.2, 0.3]
pivot: bottom_center
tris_max: 200
materials: [Cable]
textures:
  - Assets/_Project/Art/Textures/Props/T_Prop_CableBundle_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Durchhängende Kabelbündel mit Kabelbindern, schwarz/rot/gelb.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 2.0 x 0.2 x 0.3 m, Pivot bottom_center, Material-Slots [Cable]
feedback: []
```

### SM_Prop_VentGrate.fbx
```yaml
path: Assets/_Project/Art/Models/Props/SM_Prop_VentGrate.fbx
package: P26
scene: alle
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.8, 0.1, 0.5]
pivot: bottom_center
tris_max: 100
materials: [Vent]
textures:
  - Assets/_Project/Art/Textures/Props/T_Prop_VentGrate_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Lüftungsgitter für die Wand, verstaubt, Lamellen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.8 x 0.1 x 0.5 m, Pivot bottom_center, Material-Slots [Vent]
feedback: []
```

### SM_Prop_JunctionBox.fbx
```yaml
path: Assets/_Project/Art/Models/Props/SM_Prop_JunctionBox.fbx
package: P26
scene: alle
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.5, 0.2, 0.6]
pivot: bottom_center
tris_max: 200
materials: [Box]
textures:
  - Assets/_Project/Art/Textures/Props/T_Prop_JunctionBox_Albedo.png   # 64x64, Point
  - Assets/_Project/Art/Textures/Props/T_Prop_JunctionBox_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Verteilerkasten mit Warnaufkleber, Kontrolllämpchen (Emission), Kabel raus.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.5 x 0.2 x 0.6 m, Pivot bottom_center, Material-Slots [Box]
feedback: []
```

### SM_Prop_FireExtinguisher.fbx
```yaml
path: Assets/_Project/Art/Models/Props/SM_Prop_FireExtinguisher.fbx
package: P26
scene: alle
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.25, 0.2, 0.7]
pivot: bottom_center
tris_max: 300
materials: [Extinguisher]
textures:
  - Assets/_Project/Art/Textures/Props/T_Prop_FireExtinguisher_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Feuerlöscher in Wandhalterung mit Schild.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.25 x 0.2 x 0.7 m, Pivot bottom_center, Material-Slots [Extinguisher]
feedback: []
```

### SM_Prop_TrashBin.fbx
```yaml
path: Assets/_Project/Art/Models/Props/SM_Prop_TrashBin.fbx
package: P26
scene: Halle, Bar, Diner
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.5, 0.5, 0.9]
pivot: bottom_center
tris_max: 250
materials: [Bin]
textures:
  - Assets/_Project/Art/Textures/Props/T_Prop_TrashBin_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Metallmülleimer mit Deckel, verbeult, Aufkleber.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.5 x 0.5 x 0.9 m, Pivot bottom_center, Material-Slots [Bin]
feedback: []
```

### SM_Prop_TrashPile.fbx
```yaml
path: Assets/_Project/Art/Models/Props/SM_Prop_TrashPile.fbx
package: P26
scene: Gänge, Hangar
status: final
priority: P2
needed_by: 2026-11-16
size_m: [1.2, 0.8, 0.3]
pivot: bottom_center
tris_max: 400
materials: [Trash]
textures:
  - Assets/_Project/Art/Textures/Props/T_Prop_TrashPile_Albedo.png   # 128x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Kleiner Haufen aus Verpackungen, Dosen, Papier in einer Ecke.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 1.2 x 0.8 x 0.3 m, Pivot bottom_center, Material-Slots [Trash]
feedback: []
```

### SM_Prop_SodaCan.fbx
```yaml
path: Assets/_Project/Art/Models/Props/SM_Prop_SodaCan.fbx
package: P26
scene: überall
status: final
priority: P3
needed_by: 2026-11-16
size_m: [0.07, 0.07, 0.12]
pivot: bottom_center
tris_max: 60
materials: [Can]
textures:
  - Assets/_Project/Art/Textures/Props/T_Prop_SodaCan_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Getränkedose (erfundene Marke 'NOVA COLA'), auch zerdrückt als Variante.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.07 x 0.07 x 0.12 m, Pivot bottom_center, Material-Slots [Can]
feedback: []
```

### SM_Prop_PlantDead.fbx
```yaml
path: Assets/_Project/Art/Models/Props/SM_Prop_PlantDead.fbx
package: P26
scene: Halle, Motel
status: final
priority: P3
needed_by: 2026-11-16
size_m: [0.5, 0.5, 1.0]
pivot: bottom_center
tris_max: 400
materials: [Plant]
textures:
  - Assets/_Project/Art/Textures/Props/T_Prop_PlantDead_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Topfpflanze, halb vertrocknet, Plastiktopf.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.5 x 0.5 x 1.0 m, Pivot bottom_center, Material-Slots [Plant]
feedback: []
```

### SM_Prop_CleaningBot.fbx
```yaml
path: Assets/_Project/Art/Models/Props/SM_Prop_CleaningBot.fbx
package: P26
scene: Halle
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.6, 0.6, 0.4]
pivot: bottom_center
tris_max: 500
materials: [Bot]
textures:
  - Assets/_Project/Art/Textures/Props/T_Prop_CleaningBot_Albedo.png   # 64x64, Point
  - Assets/_Project/Art/Textures/Props/T_Prop_CleaningBot_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Kleiner runder Putzroboter (fährt per Skript durch die Halle): beige Kunststoffschale, ein Bürstenkranz unten, gelbe Rundumleuchte (Emission), Delle.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.6 x 0.6 x 0.4 m, Pivot bottom_center, Material-Slots [Bot]
feedback: []
```

### SM_Prop_CoinTelescope.fbx
```yaml
path: Assets/_Project/Art/Models/Props/SM_Prop_CoinTelescope.fbx
package: P26
scene: Aussichtsplattform
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.5, 0.6, 1.5]
pivot: bottom_center
tris_max: 600
materials: [Telescope]
textures:
  - Assets/_Project/Art/Textures/Props/T_Prop_CoinTelescope_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Münzfernrohr auf Säule, Münzschlitz, 'VIEW - 1 CREDIT'.
interaction: "ansehen (später Zoom-Blick auf den Heimatplaneten)"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.5 x 0.6 x 1.5 m, Pivot bottom_center, Material-Slots [Telescope]
feedback: []
```

### SM_Prop_DeckBench.fbx
```yaml
path: Assets/_Project/Art/Models/Props/SM_Prop_DeckBench.fbx
package: P26
scene: Aussichtsplattform
status: final
priority: P2
needed_by: 2026-11-16
size_m: [2.4, 0.6, 0.5]
pivot: bottom_center
tris_max: 300
materials: [Bench]
textures:
  - Assets/_Project/Art/Textures/Props/T_Prop_DeckBench_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Niedrige gepolsterte Bank ohne Lehne, Kunstleder rissig, für die Plattform.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 2.4 x 0.6 x 0.5 m, Pivot bottom_center, Material-Slots [Bench]
feedback: []
```

### SM_Prop_NeonSign_Open.fbx
```yaml
path: Assets/_Project/Art/Models/Props/SM_Prop_NeonSign_Open.fbx
package: P26
scene: Bar, Diner, Kiosk
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.8, 0.05, 0.3]
pivot: center
tris_max: 100
materials: [Neon]
textures:
  - Assets/_Project/Art/Textures/Props/T_Prop_NeonSign_Open_Albedo.png   # 64x32, Point
  - Assets/_Project/Art/Textures/Props/T_Prop_NeonSign_Open_Emission.png   # 64x32, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Kleines Leuchtschild 'OPEN' (ein Buchstabe defekt, flackert per Skript).
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.8 x 0.05 x 0.3 m, Pivot center, Material-Slots [Neon]
feedback: []
```

### SM_Prop_WallClock.fbx
```yaml
path: Assets/_Project/Art/Models/Props/SM_Prop_WallClock.fbx
package: P26
scene: Halle, Schalter
status: final
priority: P3
needed_by: 2026-11-16
size_m: [0.5, 0.08, 0.5]
pivot: center
tris_max: 100
materials: [Clock]
textures:
  - Assets/_Project/Art/Textures/Props/T_Prop_WallClock_Albedo.png   # 64x64, Point
  - Assets/_Project/Art/Textures/Props/T_Prop_WallClock_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Digitale Stationsuhr mit roter Segmentanzeige (Emission) 'SHIFT 07:42'.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.5 x 0.08 x 0.5 m, Pivot center, Material-Slots [Clock]
feedback: []
```

### SM_Prop_PASpeaker.fbx
```yaml
path: Assets/_Project/Art/Models/Props/SM_Prop_PASpeaker.fbx
package: P26
scene: Halle, Gänge
status: final
priority: P3
needed_by: 2026-11-16
size_m: [0.4, 0.3, 0.3]
pivot: top_center
tris_max: 200
materials: [Speaker]
textures:
  - Assets/_Project/Art/Textures/Props/T_Prop_PASpeaker_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Trichterlautsprecher an der Decke (Durchsagen).
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.4 x 0.3 x 0.3 m, Pivot top_center, Material-Slots [Speaker]
feedback: []
```

### SM_Prop_CeilingFan.fbx
```yaml
path: Assets/_Project/Art/Models/Props/SM_Prop_CeilingFan.fbx
package: P26
scene: Bar, Diner
status: final
priority: P3
needed_by: 2026-11-16
size_m: [1.2, 1.2, 0.4]
pivot: top_center
tris_max: 300
materials: [Fan]
textures:
  - Assets/_Project/Art/Textures/Props/T_Prop_CeilingFan_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Langsam drehender Deckenventilator, 4 Blätter als eigenes Unterobjekt (dreht per Skript).
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 1.2 x 1.2 x 0.4 m, Pivot top_center, Material-Slots [Fan]
feedback: []
```

### SM_Prop_MopBucket.fbx
```yaml
path: Assets/_Project/Art/Models/Props/SM_Prop_MopBucket.fbx
package: P26
scene: Gänge
status: final
priority: P3
needed_by: 2026-11-16
size_m: [0.5, 0.4, 1.2]
pivot: bottom_center
tris_max: 300
materials: [Mop]
textures:
  - Assets/_Project/Art/Textures/Props/T_Prop_MopBucket_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Wischeimer mit Mopp, Warnschild 'WET FLOOR' daneben als Variante.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.5 x 0.4 x 1.2 m, Pivot bottom_center, Material-Slots [Mop]
feedback: []
```

### T_Planet_GasGiant_Albedo.png
```yaml
path: Assets/_Project/Art/Textures/Sky/T_Planet_GasGiant_Albedo.png
package: P24
scene: Himmel
status: final
priority: P2
needed_by: 2026-11-13
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Sky/T_Planet_GasGiant_Albedo.png   # 1024x512, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Equirectangular-Textur eines Gasriesen: Bänder in gedämpftem Orange, Ocker und Violett, ein großer Sturmwirbel, harte Pixel, Palette.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: PNG 1024x512
feedback: []
```

### T_Planet_Ring.png
```yaml
path: Assets/_Project/Art/Textures/Sky/T_Planet_Ring.png
package: P24
scene: Himmel
status: final
priority: P2
needed_by: 2026-11-13
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Sky/T_Planet_Ring.png   # 512x64, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Ringtextur (radial von innen nach außen, mit Alpha): mehrere Ringe mit Lücken, staubig beige/grau.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: PNG 512x64
feedback: []
```

### T_Planet_Home_Albedo.png
```yaml
path: Assets/_Project/Art/Textures/Sky/T_Planet_Home_Albedo.png
package: P24
scene: Himmel
status: final
priority: P1
needed_by: 2026-11-13
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Sky/T_Planet_Home_Albedo.png   # 1024x512, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Equirectangular-Heimatplanet: kalte Ozeane, Eis an den Polen, karge graubraune Kontinente, Wolkenbänder, harte Pixel.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: PNG 1024x512
feedback:
  - "2026-11-08 max: (P29) Neu und größer, 2048x1024: klar erkennbare Kontinente mit Küstenlinien und Gebirgen, durchs Fernrohr muss man hineinzoomen können. Eine große Küstenregion auf der der Station zugewandten Seite (Längengrad 0, etwa 25° Nord) ist die Heimatregion."
```

### T_Planet_Home_Emission.png
```yaml
path: Assets/_Project/Art/Textures/Sky/T_Planet_Home_Emission.png
package: P24
scene: Himmel
status: final
priority: P1
needed_by: 2026-11-13
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Sky/T_Planet_Home_Emission.png   # 1024x512, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Nachtlichter der Städte zur Albedo passend (warmes Gelb, schwarzer Hintergrund) - zeigt, wo die Familie lebt.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: PNG 1024x512
feedback:
  - "2026-11-08 max: (P29) Neu, 2048x1024, passend zu den Kontinenten: Stadtlichter als Netze entlang von Küsten und Straßen, große Städte erkennbar. Die Heimatstadt liegt bei Längengrad 0, etwa 25° Nord. Dort ein eigener zusammenhängender heller Fleck, damit ihre Lichter später je nach Familienzustand flackern oder ausgehen können."
```

### T_Sun_Glow.png
```yaml
path: Assets/_Project/Art/Textures/Sky/T_Sun_Glow.png
package: P24
scene: Himmel
status: final
priority: P1
needed_by: 2026-11-13
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Sky/T_Sun_Glow.png   # 256x256, Bilinear
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Sonnenscheibe mit weichem Glühen und vier kurzen Strahlen (Alpha), warmweiß, für ein Billboard.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: PNG 256x256
feedback: []
```

### T_Planet_Moon_Albedo.png
```yaml
path: Assets/_Project/Art/Textures/Sky/T_Planet_Moon_Albedo.png
package: P24
scene: Himmel
status: final
priority: P3
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Sky/T_Planet_Moon_Albedo.png   # 512x256, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Kleiner grauer Mond mit Kratern, equirectangular.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: PNG 512x256
feedback: []
```

### T_Decal_Signs.png
```yaml
path: Assets/_Project/Art/Textures/Decals/T_Decal_Signs.png
package: P26
scene: überall
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Decals/T_Decal_Signs.png   # 512x512, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Atlas 4x4 mit Stationsschildern (Englisch, Großbuchstaben, Orbitron-artig, erfunden): 'STAFF ONLY', 'CUSTOMS', 'HANGAR', 'MOTEL', 'BAR', 'DINER', 'AIRLOCK', 'WARNING - VACUUM', Pfeile, Deck-Nummern, Fluchtweg-Schilder.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: PNG 512x512
feedback: []
```

### T_Decal_Posters.png
```yaml
path: Assets/_Project/Art/Textures/Decals/T_Decal_Posters.png
package: P26
scene: Motel, Bar, Halle
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Decals/T_Decal_Posters.png   # 512x512, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Atlas 2x2 mit Plakaten: Propaganda 'A GOOD OFFICER IS A WATCHFUL OFFICER', Reisewerbung für den Heimatplaneten, Fahndungsplakat 'WANTED', Bar-Werbung für 'NOVA COLA'.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: PNG 512x512
feedback: []
```

### T_Decal_Grime.png
```yaml
path: Assets/_Project/Art/Textures/Decals/T_Decal_Grime.png
package: P26
scene: überall
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Decals/T_Decal_Grime.png   # 512x512, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Atlas 4x4 mit Schmutz-Decals (Alpha): Wasserflecken, Rostläufer, Ölflecken, Kratzspuren, kleine Graffiti-Tags, Fußabdrücke.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: PNG 512x512
feedback: []
```

### SM_Station_Door_Slide_Narrow.fbx
*(Variante, von der Pipeline gebaut und geliefert; Eintrag nachgezogen, M005)*
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Door_Slide_Narrow.fbx
package: P27
scene: Servicegang, Motel
status: final
priority: P1
needed_by: 2026-11-09
size_m: [1.2, 0.14, 2.2]
pivot: bottom_center
tris_max: 1000
materials: [Door]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Door_Slide_Narrow_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Schmale Schiebetür 1,2 x 2,2 m für Personaltüren und Motelzimmer, gleicher Stil wie SM_Station_Door_Slide.
interaction: "öffnet automatisch (P27)"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_door_steam_01.webp]
placeholder: Box 1.2 x 0.14 x 2.2 m, Pivot bottom_center, Material-Slots [Door]
feedback: []
```

### SM_Station_Window_Small_Slot.fbx
*(Variante, von der Pipeline gebaut und geliefert; Eintrag nachgezogen, M005)*
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Window_Small_Slot.fbx
package: P05
scene: Schalter
status: final
priority: P1
needed_by: 2026-11-09
size_m: [4.0, 0.3, 2.2]
pivot: bottom_center
tris_max: 1000
materials: [Frame, Glass]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Window_Small_Slot_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Panzerglas-Scheibe zur Prüfkammer mit Dokument-Durchreiche unten mittig.
interaction: "Schublade fährt aus/ein (P06)"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/real_customs_booth_01.webp]
placeholder: Box 4.0 x 0.3 x 2.2 m, Pivot bottom_center, Material-Slots [Frame, Glass]
feedback: []
```

### SM_Station_Lintel_4x08.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Lintel_4x08.fbx
package: P22
scene: Schalter
status: final
priority: P1
needed_by: 2026-11-09
size_m: [4.0, 0.3, 0.8]
pivot: bottom_center
tris_max: 200
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Lintel_4x08_Albedo.png   # 256x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Sturzstreifen über den 4-m-Fenstern (2,2 bis 3,0 m): Wandpaneel passend zu SM_Station_Wall_2x3 mit Kabelkanal oben.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 4.0 x 0.3 x 0.8 m, Pivot bottom_center, Material-Slots [Wall]
feedback: []
```

### SM_Station_Wall_Porthole_2x3.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Wall_Porthole_2x3.fbx
package: P22
scene: Motel
status: final
priority: P1
needed_by: 2026-11-09
size_m: [2.0, 0.2, 3.0]
pivot: bottom_center
tris_max: 400
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_Porthole_2x3_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Wandmodul wie SM_Station_Wall_2x3 (Polster-Variante fürs Motel) mit Achteck-Loch für SM_Station_Porthole, Mitte auf 1,5 m.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 2.0 x 0.2 x 3.0 m, Pivot bottom_center, Material-Slots [Wall]
feedback: []
```

### SM_Station_Wall_Door_2x3_Narrow.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Wall_Door_2x3_Narrow.fbx
package: P27
scene: Servicegang, Motel
status: final
priority: P1
needed_by: 2026-11-09
size_m: [2.0, 0.2, 3.0]
pivot: bottom_center
tris_max: 1000
materials: [Wall, Frame]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_Door_2x3_Narrow_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_Door_2x3_Narrow_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Wandmodul mit 1,2 x 2,2 m Türöffnung mittig, Rahmen mit Warnstreifen und Statuslampe (Emission), Nuten für SM_Station_Door_Slide_Narrow.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_door_steam_01.webp]
placeholder: Box 2.0 x 0.2 x 3.0 m, Pivot bottom_center, Material-Slots [Wall, Frame]
feedback: []
```

### SM_Item_PillBottle.fbx
```yaml
path: Assets/_Project/Art/Models/Items/SM_Item_PillBottle.fbx
package: P32
scene: Schalter (Asservatenschrank), Hangar-Kisten
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.06, 0.06, 0.1]
pivot: bottom_center
tris_max: 300
materials: [Item]
textures:
  - Assets/_Project/Art/Textures/Items/T_Item_PillBottle_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Pillendose ohne Etikett, Kapseln sichtbar (verbotene Medizin). Liegt im Asservatenschrank und in durchsuchbaren Kisten; wird in die Hand genommen.
interaction: "aufheben, verkaufen, nach Hause schicken"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.06 x 0.06 x 0.1 m, Pivot bottom_center, Material-Slots [Item]
feedback: []
```

### SM_Item_Liquor.fbx
```yaml
path: Assets/_Project/Art/Models/Items/SM_Item_Liquor.fbx
package: P32
scene: Schalter (Asservatenschrank), Hangar-Kisten
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.09, 0.09, 0.3]
pivot: bottom_center
tris_max: 300
materials: [Item]
textures:
  - Assets/_Project/Art/Textures/Items/T_Item_Liquor_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Flasche Schwarzbrand mit Pfropfen. Liegt im Asservatenschrank und in durchsuchbaren Kisten; wird in die Hand genommen.
interaction: "aufheben, verkaufen, nach Hause schicken"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.09 x 0.09 x 0.3 m, Pivot bottom_center, Material-Slots [Item]
feedback: []
```

### SM_Item_WeaponParts.fbx
```yaml
path: Assets/_Project/Art/Models/Items/SM_Item_WeaponParts.fbx
package: P32
scene: Schalter (Asservatenschrank), Hangar-Kisten
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.35, 0.12, 0.08]
pivot: bottom_center
tris_max: 300
materials: [Item]
textures:
  - Assets/_Project/Art/Textures/Items/T_Item_WeaponParts_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Gewickeltes Bündel mit Waffenteilen (Lauf, Griff sichtbar). Liegt im Asservatenschrank und in durchsuchbaren Kisten; wird in die Hand genommen.
interaction: "aufheben, verkaufen, nach Hause schicken"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.35 x 0.12 x 0.08 m, Pivot bottom_center, Material-Slots [Item]
feedback: []
```

### SM_Item_DataChip.fbx
```yaml
path: Assets/_Project/Art/Models/Items/SM_Item_DataChip.fbx
package: P32
scene: Schalter (Asservatenschrank), Hangar-Kisten
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.08, 0.05, 0.01]
pivot: bottom_center
tris_max: 300
materials: [Item]
textures:
  - Assets/_Project/Art/Textures/Items/T_Item_DataChip_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Datenchip in Plastikhülle, grün leuchtende Kante (Emission). Liegt im Asservatenschrank und in durchsuchbaren Kisten; wird in die Hand genommen.
interaction: "aufheben, verkaufen, nach Hause schicken"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.08 x 0.05 x 0.01 m, Pivot bottom_center, Material-Slots [Item]
feedback: []
```

### SM_Item_CashBundle.fbx
```yaml
path: Assets/_Project/Art/Models/Items/SM_Item_CashBundle.fbx
package: P32
scene: Schalter (Asservatenschrank), Hangar-Kisten
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.16, 0.07, 0.04]
pivot: bottom_center
tris_max: 300
materials: [Item]
textures:
  - Assets/_Project/Art/Textures/Items/T_Item_CashBundle_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Geldbündel fremder Währung mit Banderole. Liegt im Asservatenschrank und in durchsuchbaren Kisten; wird in die Hand genommen.
interaction: "aufheben, verkaufen, nach Hause schicken"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.16 x 0.07 x 0.04 m, Pivot bottom_center, Material-Slots [Item]
feedback: []
```

### SM_Item_AlienSpecimen.fbx
```yaml
path: Assets/_Project/Art/Models/Items/SM_Item_AlienSpecimen.fbx
package: P32
scene: Schalter (Asservatenschrank), Hangar-Kisten
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.12, 0.12, 0.2]
pivot: bottom_center
tris_max: 300
materials: [Item]
textures:
  - Assets/_Project/Art/Textures/Items/T_Item_AlienSpecimen_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Glasbehälter mit seltsamem Organismus in Flüssigkeit (Emission, schwach). Liegt im Asservatenschrank und in durchsuchbaren Kisten; wird in die Hand genommen.
interaction: "aufheben, verkaufen, nach Hause schicken"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.12 x 0.12 x 0.2 m, Pivot bottom_center, Material-Slots [Item]
feedback: []
```

### UI_Telescope_Overlay.png
```yaml
path: Assets/_Project/Art/UI/UI_Telescope_Overlay.png
package: P29
scene: UI
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/UI_Telescope_Overlay.png   # 1024x1024 Alpha, Bilinear
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Okular-Maske des Münzfernrohrs: runder Ausschnitt mit leicht verkratztem Glas, schwarzer Rand mit Gummiwulst, kleine Fadenkreuz-Markierungen und ein Zählwerk 'TIME 20' unten. Mitte transparent.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: PNG 1024x1024 Alpha
feedback: []
```

### UI_Family_Photo.png
```yaml
path: Assets/_Project/Art/UI/UI_Family_Photo.png
package: P29
scene: Motel (Spind), UI
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/UI_Family_Photo.png   # 512x256 Atlas, Bilinear
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Atlas mit drei Fassungen desselben Familienfotos (Partner/in, zwei Kinder, ein Großelternteil vor einem Wohnblock): links intakt und farbig, Mitte verblasst mit Knick, rechts eingerissen und fleckig. Erfundene Personen, Retro-Fotostil.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: PNG 512x256 Atlas
feedback: []
```

### SFX_Door_Open.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Door_Open.ogg
package: P27
scene: alle Türen
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Schwere Schiebetür öffnet: kurzes pneumatisches Zischen + metallisches Gleiten, ca. 0,5 s. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), OGG mono 44,1 kHz, "Decompress On Load".
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: keiner (Tür spielt ohne Clip einfach keinen Ton)
feedback: []
```

### SFX_Door_Close.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Door_Close.ogg
package: P27
scene: alle Türen
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Schiebetür schließt: Gleiten + dumpfes Einrasten, ca. 0,5 s. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), OGG mono 44,1 kHz, "Decompress On Load".
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: keiner (Tür spielt ohne Clip einfach keinen Ton)
feedback: []
```

### SFX_Door_Locked.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Door_Locked.ogg
package: P27
scene: alle Türen
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Verriegelte Tür: kurzer, unfreundlicher Summer/Doppel-Piep, ca. 0,3 s. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), OGG mono 44,1 kHz, "Decompress On Load".
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: keiner (Tür spielt ohne Clip einfach keinen Ton)
feedback: []
```

### SM_Station_Wall_2x3_Padded.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Wall_2x3_Padded.fbx
package: P21
scene: Motel, Schalter
status: final
priority: P1
needed_by: 2026-11-09
size_m: [2.0, 0.2, 3.0]
pivot: bottom_center
tris_max: 600
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_2x3_Padded_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Gleiches Raster wie SM_Station_Wall_2x3 (2 x 3 m, 0,2 m tief, Vorderseite lokal -Z), kachelt nahtlos mit der Grundwand und den anderen Varianten. Gesteppte, gepolsterte Paneele in Beige/Creme (wie Raumschiff-Kabinen der 70er), einzelne Nähte geplatzt, Fußleiste dunkel.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_corridor_padded_01.webp, docs/style/refs/real_crew_quarters_01.webp]
placeholder: Box 2.0 x 0.2 x 3.0 m, Pivot bottom_center, Material-Slots [Wall]
feedback: []
```

### SM_Station_Wall_2x3_Panel.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Wall_2x3_Panel.fbx
package: P21
scene: Bar, Diner
status: final
priority: P1
needed_by: 2026-11-09
size_m: [2.0, 0.2, 3.0]
pivot: bottom_center
tris_max: 600
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_2x3_Panel_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Gleiches Raster wie SM_Station_Wall_2x3 (2 x 3 m, 0,2 m tief, Vorderseite lokal -Z), kachelt nahtlos mit der Grundwand und den anderen Varianten. Warme Kunstholz-Paneele (#5E3A24/#8A5A34) mit Metallleisten, unten Kickplatte aus Riffelblech, Brandflecken.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_room_warm_panels_01.webp, docs/style/refs/ai_mess_table_02.webp]
placeholder: Box 2.0 x 0.2 x 3.0 m, Pivot bottom_center, Material-Slots [Wall]
feedback: []
```

### SM_Station_Wall_2x3_Pipes.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Wall_2x3_Pipes.fbx
package: P21
scene: Gänge, Hangar-Nähe, Schalter
status: final
priority: P1
needed_by: 2026-11-09
size_m: [2.0, 0.35, 3.0]
pivot: bottom_center
tris_max: 900
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_2x3_Pipes_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Gleiches Raster wie SM_Station_Wall_2x3 (2 x 3 m, 0,2 m tief, Vorderseite lokal -Z), kachelt nahtlos mit der Grundwand und den anderen Varianten. Wand mit sichtbaren Rohrleitungen davor (3 Rohre waagerecht + 1 senkrecht mit Ventilrad), Schellen, Rost an Muffen. Rohre ragen bis 0,15 m in den Raum.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_corridor_cargo_01.webp]
placeholder: Box 2.0 x 0.35 x 3.0 m, Pivot bottom_center, Material-Slots [Wall]
feedback: []
```

### SM_Station_Wall_2x3_Vent.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Wall_2x3_Vent.fbx
package: P21
scene: alle
status: final
priority: P1
needed_by: 2026-11-09
size_m: [2.0, 0.25, 3.0]
pivot: bottom_center
tris_max: 700
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_2x3_Vent_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Gleiches Raster wie SM_Station_Wall_2x3 (2 x 3 m, 0,2 m tief, Vorderseite lokal -Z), kachelt nahtlos mit der Grundwand und den anderen Varianten. Wand mit großem Lüftungsgitter (Lamellen, Staubfahne darunter) und kleinem Warnschild 'AIR RETURN'.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 2.0 x 0.25 x 3.0 m, Pivot bottom_center, Material-Slots [Wall]
feedback: []
```

### SM_Station_Wall_2x3_Damaged.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Wall_2x3_Damaged.fbx
package: P21
scene: Gänge, Motel, Hangar-Nähe
status: final
priority: P1
needed_by: 2026-11-09
size_m: [2.0, 0.25, 3.0]
pivot: bottom_center
tris_max: 800
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_2x3_Damaged_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Gleiches Raster wie SM_Station_Wall_2x3 (2 x 3 m, 0,2 m tief, Vorderseite lokal -Z), kachelt nahtlos mit der Grundwand und den anderen Varianten. Abgenutzte Wand: ein Paneel fehlt (Kabel und Isolierung sichtbar), Delle, Wasserfleck, provisorisch mit Klebeband geflickt.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 2.0 x 0.25 x 3.0 m, Pivot bottom_center, Material-Slots [Wall]
feedback: []
```

### SM_Station_Wall_2x3_Screen.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Wall_2x3_Screen.fbx
package: P21
scene: Halle, Bar, Gänge
status: final
priority: P1
needed_by: 2026-11-09
size_m: [2.0, 0.25, 3.0]
pivot: bottom_center
tris_max: 700
materials: [Wall, Screen]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_2x3_Screen_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_2x3_Screen_Emission.png   # 128x128, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Gleiches Raster wie SM_Station_Wall_2x3 (2 x 3 m, 0,2 m tief, Vorderseite lokal -Z), kachelt nahtlos mit der Grundwand und den anderen Varianten. Wand mit eingebautem CRT-Infobildschirm (Emission, Bernstein: 'KEEP PAPERS READY', Uhrzeit) und Kabelkanal.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 2.0 x 0.25 x 3.0 m, Pivot bottom_center, Material-Slots [Wall, Screen]
feedback: []
```

### SM_Station_Wall_2x3_Lockers.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Wall_2x3_Lockers.fbx
package: P21
scene: Servicegang, Motelgang, Hangar-Nähe
status: final
priority: P1
needed_by: 2026-11-09
size_m: [2.0, 0.45, 3.0]
pivot: bottom_center
tris_max: 900
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_2x3_Lockers_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Gleiches Raster wie SM_Station_Wall_2x3 (2 x 3 m, 0,2 m tief, Vorderseite lokal -Z), kachelt nahtlos mit der Grundwand und den anderen Varianten. Wand mit eingebauter Spindreihe (4 schmale Spinde, ein Spind halb offen, Nummernschilder), ragt 0,25 m in den Raum.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 2.0 x 0.45 x 3.0 m, Pivot bottom_center, Material-Slots [Wall]
feedback: []
```

### SM_Station_Wall_2x1.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Wall_2x1.fbx
package: P21
scene: Bar (4 m hohe Wände)
status: final
priority: P1
needed_by: 2026-11-09
size_m: [2.0, 0.2, 1.0]
pivot: bottom_center
tris_max: 200
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_2x1_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Füllstück 2 x 1 m über der 3-m-Wand in 4 m hohen Räumen (Bar): Kabelkanal, Lüftungsschlitze, passt oben an alle Wandvarianten.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 2.0 x 0.2 x 1.0 m, Pivot bottom_center, Material-Slots [Wall]
feedback: []
```

### SM_Station_Floor_2x2_Grate.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Floor_2x2_Grate.fbx
package: P21
scene: Servicegang, Hangar-Ränder
status: final
priority: P1
needed_by: 2026-11-09
size_m: [2.0, 2.0, 0.1]
pivot: bottom_center
tris_max: 400
materials: [Floor]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Floor_2x2_Grate_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Gitterrost-Bodenplatte 2 x 2 m, darunter dunkler Hohlraum mit Kabeln/Rohren (sichtbar durch die Gitter), kachelt mit SM_Station_Floor_2x2.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/mw_cargo_hold_01.webp]
placeholder: Box 2.0 x 2.0 x 0.1 m, Pivot bottom_center, Material-Slots [Floor]
feedback: []
```

### SM_Station_Ceiling_2x2_Light.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Ceiling_2x2_Light.fbx
package: P21
scene: alle
status: final
priority: P1
needed_by: 2026-11-09
size_m: [2.0, 2.0, 0.2]
pivot: bottom_center
tris_max: 400
materials: [Ceiling]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Ceiling_2x2_Light_Albedo.png   # 128x128, Point
  - Assets/_Project/Art/Textures/Station/T_Station_Ceiling_2x2_Light_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Deckenplatte wie SM_Station_Ceiling_2x2 mit eingelassener Leuchte (ovale Leuchtwanne mit runden Enden, kaltweiße Emission).
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 2.0 x 2.0 x 0.2 m, Pivot bottom_center, Material-Slots [Ceiling]
feedback: []
```

### SM_Station_Ceiling_Arch_2x2.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Ceiling_Arch_2x2.fbx
package: P21
scene: Servicegang, Motelgang (2 m breite Gänge)
status: final
priority: P1
needed_by: 2026-11-09
size_m: [2.0, 2.0, 0.7]
pivot: bottom_center
tris_max: 600
materials: [Ceiling]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Ceiling_Arch_2x2_Albedo.png   # 256x128, Point
  - Assets/_Project/Art/Textures/Station/T_Station_Ceiling_Arch_2x2_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Teil der 'runderen Station' (Max, 02.11.): weiche Rundungen statt harter Kanten, Rundungen mit 8-12 Segmenten. Gewölbtes Deckensegment für 2 m breite Gänge: Halbtonne quer zur Laufrichtung (lokale X = Gangbreite), Scheitel 0,7 m über den Wandoberkanten, Rippen alle 2 m, eine Leuchtleiste im Scheitel (Emission). Pivot unten mittig auf Wandoberkante.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_corridor_padded_01.webp]
placeholder: Box 2.0 x 2.0 x 0.7 m, Pivot bottom_center, Material-Slots [Ceiling]
feedback: []
```

### SM_Station_Wall_CornerRound.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Wall_CornerRound.fbx
package: P21
scene: alle Räume (Innenecken)
status: final
priority: P1
needed_by: 2026-11-09
size_m: [0.6, 0.6, 3.0]
pivot: bottom_center
tris_max: 300
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_CornerRound_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Teil der 'runderen Station' (Max, 02.11.): weiche Rundungen statt harter Kanten, Rundungen mit 8-12 Segmenten. Gerundete Innenecke (Viertelkreis-Hohlkehle r 0,5 m) für jede Raumecke, gleiche Paneeloptik wie die Wand. Pivot unten an der Raumecke; die Rundung zeigt in lokale +X und +Z (der Raum liegt bei +X/+Z). Stapelbar für hohe Räume.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.6 x 0.6 x 3.0 m, Pivot bottom_center, Material-Slots [Wall]
feedback: []
```

### SM_Station_Pillar_Round.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Pillar_Round.fbx
package: P21
scene: Halle, Plattform-Öffnung, Hangar
status: final
priority: P1
needed_by: 2026-11-09
size_m: [0.8, 0.8, 3.0]
pivot: bottom_center
tris_max: 600
materials: [Pillar]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Pillar_Round_Albedo.png   # 128x256, Point
  - Assets/_Project/Art/Textures/Station/T_Station_Pillar_Round_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Teil der 'runderen Station' (Max, 02.11.): weiche Rundungen statt harter Kanten, Rundungen mit 8-12 Segmenten. Runde Säule (Ø 0,8 m, 12 Segmente) mit Metallringen oben/unten, Kabelbündel spiralförmig, Leuchtring auf 2,4 m (Emission). Stapelbar.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 0.8 x 0.8 x 3.0 m, Pivot bottom_center, Material-Slots [Pillar]
feedback: []
```

### SM_Station_DoorTrim_Round.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_DoorTrim_Round.fbx
package: P27
scene: alle Türen
status: final
priority: P1
needed_by: 2026-11-09
size_m: [2.0, 0.3, 2.6]
pivot: bottom_center
tris_max: 500
materials: [Frame]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_DoorTrim_Round_Albedo.png   # 128x128, Point
  - Assets/_Project/Art/Textures/Station/T_Station_DoorTrim_Round_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Teil der 'runderen Station' (Max, 02.11.): weiche Rundungen statt harter Kanten, Rundungen mit 8-12 Segmenten. Aufgesetzter Türrahmen mit stark gerundeten oberen Ecken (Raumschiff-Schott-Optik), passt um die 1,6-m-Türöffnung, mit Warnstreifen und Statuslampe (Emission). Variante _Narrow für 1,2-m-Türen (eigene Datei).
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_door_steam_01.webp]
placeholder: Box 2.0 x 0.3 x 2.6 m, Pivot bottom_center, Material-Slots [Frame]
feedback: []
```

### SM_Station_DoorTrim_Round_Narrow.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_DoorTrim_Round_Narrow.fbx
package: P27
scene: Personaltüren, Motel
status: final
priority: P1
needed_by: 2026-11-09
size_m: [1.6, 0.3, 2.6]
pivot: bottom_center
tris_max: 500
materials: [Frame]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_DoorTrim_Round_Narrow_Albedo.png   # 128x128, Point
  - Assets/_Project/Art/Textures/Station/T_Station_DoorTrim_Round_Narrow_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Teil der 'runderen Station' (Max, 02.11.): weiche Rundungen statt harter Kanten, Rundungen mit 8-12 Segmenten. Wie SM_Station_DoorTrim_Round für die 1,2-m-Türöffnung.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 1.6 x 0.3 x 2.6 m, Pivot bottom_center, Material-Slots [Frame]
feedback: []
```

### SM_Station_Wall_Arch_2x3.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Wall_Arch_2x3.fbx
package: P21
scene: Halle ↔ Motelgang
status: final
priority: P1
needed_by: 2026-11-09
size_m: [2.0, 0.3, 3.0]
pivot: bottom_center
tris_max: 600
materials: [Wall, Frame]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_Arch_2x3_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_Arch_2x3_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Teil der 'runderen Station' (Max, 02.11.): weiche Rundungen statt harter Kanten, Rundungen mit 8-12 Segmenten. Wandmodul mit offenem Rundbogen (2,0 m breit, Scheitel 2,6 m), dicke Laibung mit Leuchtstreifen im Bogen (Emission). Für den offenen Durchgang Halle ↔ Motelgang.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 2.0 x 0.3 x 3.0 m, Pivot bottom_center, Material-Slots [Wall, Frame]
feedback: []
```

### SM_Station_DeckArch.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_DeckArch.fbx
package: P22
scene: Halle ↔ Aussichtsplattform
status: final
priority: P1
needed_by: 2026-11-09
size_m: [11.0, 0.8, 6.0]
pivot: bottom_center
tris_max: 1500
materials: [Frame]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_DeckArch_Albedo.png   # 512x256, Point
  - Assets/_Project/Art/Textures/Station/T_Station_DeckArch_Emission.png   # 128x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Teil der 'runderen Station' (Max, 02.11.): weiche Rundungen statt harter Kanten, Rundungen mit 8-12 Segmenten. Großer Portalbogen über der 10 m breiten Öffnung zur Aussichtsplattform: zwei runde Pfeiler, oben weiter Segmentbogen bis zur Hallendecke, Schriftzug 'OBSERVATION DECK' (Emission), Nieten, Leuchtband entlang des Bogens.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 11.0 x 0.8 x 6.0 m, Pivot bottom_center, Material-Slots [Frame]
feedback: []
```

### SM_Hall_CeilingDome.fbx
```yaml
path: Assets/_Project/Art/Models/Hall/SM_Hall_CeilingDome.fbx
package: P21
scene: Halle
status: final
priority: P1
needed_by: 2026-11-09
size_m: [8.0, 8.0, 1.5]
pivot: top_center
tris_max: 1200
materials: [Dome]
textures:
  - Assets/_Project/Art/Textures/Hall/T_Hall_CeilingDome_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Hall/T_Hall_CeilingDome_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Teil der 'runderen Station' (Max, 02.11.): weiche Rundungen statt harter Kanten, Rundungen mit 8-12 Segmenten. Flache Deckenkuppel (Ø 8 m, 1,5 m hoch) über der Anzeigesäule, Rippen strahlenförmig, Ring aus Leuchten am Rand (Emission). Pivot oben mittig (hängt von der 6-m-Decke nach unten offen).
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 8.0 x 8.0 x 1.5 m, Pivot top_center, Material-Slots [Dome]
feedback: []
```

### SM_Deck_Railing.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Deck_Railing.fbx
package: P22
scene: Aussichtsplattform
status: final
priority: P2
needed_by: 2026-11-13
size_m: [1.95, 0.15, 1.0]
pivot: bottom_center
tris_max: 400
materials: [Railing]
textures:
  - Assets/_Project/Art/Textures/Station/T_Deck_Railing_Albedo.png   # 128x64, Point
  - Assets/_Project/Art/Textures/Station/T_Deck_Railing_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Teil der 'runderen Station' (Max, 02.11.): weiche Rundungen statt harter Kanten, Rundungen mit 8-12 Segmenten. Geländer-Segment vor einem gebogenen Glassegment (passt zu SM_Station_Window_Curved, 22,5 Grad je Segment): runder Handlauf, Stützen, unten Leuchtstreifen (Emission) zum Boden.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 1.95 x 0.15 x 1.0 m, Pivot bottom_center, Material-Slots [Railing]
feedback: []
```

### SM_Hangar_RibArch.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_RibArch.fbx
package: P22
scene: Hangar
status: final
priority: P1
needed_by: 2026-11-09
size_m: [36.0, 1.2, 4.0]
pivot: top_center
tris_max: 1500
materials: [Rib]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_RibArch_Albedo.png   # 256x128, Point
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_RibArch_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Teil der 'runderen Station' (Max, 02.11.): weiche Rundungen statt harter Kanten, Rundungen mit 8-12 Segmenten. Gebogene Stahl-Rippe unter der Hangardecke über die volle Breite (36 m), Fachwerk-Profil mit Lampen und Kabeln. Steht alle 7 m; gibt dem Hangar eine gewölbte Halle statt Kistenform. Pivot oben mittig.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/real_hangar_shuttle_01.webp]
placeholder: Box 36.0 x 1.2 x 4.0 m, Pivot top_center, Material-Slots [Rib]
feedback: []
```

### SM_Motel_CatPainting.fbx
```yaml
path: Assets/_Project/Art/Models/Motel/SM_Motel_CatPainting.fbx
package: P04
scene: Motel
status: final
priority: P3
needed_by: 2026-11-04
size_m: [0.53, 0.045, 0.63]
pivot: center
tris_max: 100
materials: [Painting]
textures:
  - Assets/_Project/Art/Textures/Motel/T_Motel_CatPainting_Albedo.png   # 128x128, Point
texel_px_per_m: 160
palette: docs/style/palette.png
look: >
  Extra der Pipeline (M014), von Max angenommen: gerahmtes Gemälde von Max' Katze (brauner Tigerkater vor orangem Grund),
  dunkler Fake-Holzrahmen mit Messinglippe. Hängt in Zimmer 103 über dem Kopfende des Betts, Bildmitte 1,3 m.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: none (direkt geliefert)
feedback: []
```

### SM_Motel_DogPainting.fbx
```yaml
path: Assets/_Project/Art/Models/Motel/SM_Motel_DogPainting.fbx
package: P51
scene: Motel
status: final
priority: P3
needed_by: 2026-11-06
size_m: [0.63, 0.045, 0.53]
pivot: center
tris_max: 100
materials: [Painting]
textures:
  - Assets/_Project/Art/Textures/Motel/T_Motel_DogPainting_Albedo.png   # 128x128, Point
texel_px_per_m: 160
palette: docs/style/palette.png
look: >
  Extra der Pipeline, von Max bestellt 06.11.: gerahmtes Gemälde von Max' Hund Elo (schlafend, gestromt, vor
  Teal-Grund), gleicher dunkler Fake-Holzrahmen mit Messinglippe wie das Katzenbild. Platzierung: Zimmer 103, neben dem
  Katzenbild, gleiche Bildmitte, 20 cm Abstand.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: none (direkt geliefert)
feedback: []
```

### SM_Ext_HullPanel_4x4.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_HullPanel_4x4.fbx
package: P39
scene: Außen (Station von außen, durch Fenster und im Startbildschirm)
status: final
priority: P1
needed_by: 2026-11-12
size_m: [4.0, 0.3, 4.0]
pivot: center
tris_max: 400
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HullPanel_4x4_Albedo.png   # 128x128, Point
texel_px_per_m: 6
palette: docs/style/palette.png
look: >
  Außenhüllplatte 4 x 4 m für Wände und Dächer: Stahlplatten mit Nietreihen, Wartungsluke, Dreck- und Brandspuren, dunkles Grau mit beigen Flecken wie die KESTREL-Habmodule. Vorderseite (-Y im Blender = Unity -Z) zeigt nach außen. Kachelbar: Kanten müssen nahtlos aneinanderpassen. Wird nur aus der Ferne gesehen: niedrige Texeldichte (4-8 px/m), Browser-Downloadgröße im Blick.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/style/refs/ai_station_exterior_01.webp]
placeholder: Box 4.0 x 0.3 x 4.0 m, Pivot center, Material-Slots [Hull] (vom SkyExteriorBuilder per Code)
feedback: []
```

### SM_Ext_HullPanel_4x4_Ribbed.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_HullPanel_4x4_Ribbed.fbx
package: P39
scene: Außen (Station von außen, durch Fenster und im Startbildschirm)
status: final
priority: P1
needed_by: 2026-11-12
size_m: [4.0, 0.5, 4.0]
pivot: center
tris_max: 600
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HullPanel_4x4_Ribbed_Albedo.png   # 128x128, Point
texel_px_per_m: 6
palette: docs/style/palette.png
look: >
  Variante der Hüllplatte mit zwei waagrechten Versteifungsrippen und Kabelkanal; für große Flächen (Hangar-Seiten), damit sie nicht flach wirken. Gleiche Kanten wie SM_Ext_HullPanel_4x4. Wird nur aus der Ferne gesehen: niedrige Texeldichte (4-8 px/m), Browser-Downloadgröße im Blick.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/style/refs/ai_station_exterior_01.webp]
placeholder: Box 4.0 x 0.5 x 4.0 m, Pivot center, Material-Slots [Hull] (vom SkyExteriorBuilder per Code)
feedback: []
```

### SM_Ext_HullEdge_4.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_HullEdge_4.fbx
package: P39
scene: Außen (Station von außen, durch Fenster und im Startbildschirm)
status: final
priority: P1
needed_by: 2026-11-12
size_m: [4.0, 0.6, 0.6]
pivot: center
tris_max: 300
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HullEdge_4_Albedo.png   # 128x128, Point
texel_px_per_m: 6
palette: docs/style/palette.png
look: >
  Kantenleiste 4 m für Außenkanten der Hülle (Wand trifft Dach, Wand trifft Wand): abgerundetes Profil (8 Segmente) mit Gelb-Schwarz-Warnstreifen alle 2 m. Liegt entlang X. Wird nur aus der Ferne gesehen: niedrige Texeldichte (4-8 px/m), Browser-Downloadgröße im Blick.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/style/refs/ai_station_exterior_01.webp]
placeholder: Box 4.0 x 0.6 x 0.6 m, Pivot center, Material-Slots [Hull] (vom SkyExteriorBuilder per Code)
feedback: []
```

### SM_Ext_HullCorner.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_HullCorner.fbx
package: P39
scene: Außen (Station von außen, durch Fenster und im Startbildschirm)
status: final
priority: P1
needed_by: 2026-11-12
size_m: [0.6, 0.6, 0.6]
pivot: center
tris_max: 200
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HullCorner_Albedo.png   # 128x128, Point
texel_px_per_m: 6
palette: docs/style/palette.png
look: >
  Eckstück, wo drei Kantenleisten zusammentreffen: Kugelsegment/abgerundeter Würfel, passt an SM_Ext_HullEdge_4. Wird nur aus der Ferne gesehen: niedrige Texeldichte (4-8 px/m), Browser-Downloadgröße im Blick.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/style/refs/ai_station_exterior_01.webp]
placeholder: Box 0.6 x 0.6 x 0.6 m, Pivot center, Material-Slots [Hull] (vom SkyExteriorBuilder per Code)
feedback: []
```

### SM_Ext_RoofCap_4x4.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_RoofCap_4x4.fbx
package: P39
scene: Außen (Station von außen, durch Fenster und im Startbildschirm)
status: final
priority: P1
needed_by: 2026-11-12
size_m: [4.0, 4.0, 0.4]
pivot: bottom_center
tris_max: 500
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_RoofCap_4x4_Albedo.png   # 128x128, Point
  - Assets/_Project/Art/Textures/Exterior/T_Ext_RoofCap_4x4_Emission.png   # 64x64, Point
texel_px_per_m: 6
palette: docs/style/palette.png
look: >
  Dachplatte 4 x 4 m für die niedrigen Räume (Motel, Diner, Bar): flache Platte mit Lüftungsgittern, kleinen Kästen und einem roten Positionslicht (Emission). Kachelbar. Wird nur aus der Ferne gesehen: niedrige Texeldichte (4-8 px/m), Browser-Downloadgröße im Blick.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/style/refs/ai_station_exterior_01.webp]
placeholder: Box 4.0 x 4.0 x 0.4 m, Pivot bottom_center, Material-Slots [Hull] (vom SkyExteriorBuilder per Code)
feedback: []
```

### SM_Ext_Greeble_VentBox.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_Greeble_VentBox.fbx
package: P39
scene: Außen (Station von außen, durch Fenster und im Startbildschirm)
status: final
priority: P1
needed_by: 2026-11-12
size_m: [2.0, 1.5, 1.2]
pivot: bottom_center
tris_max: 300
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_Greeble_VentBox_Albedo.png   # 128x128, Point
texel_px_per_m: 6
palette: docs/style/palette.png
look: >
  Großes Lüftungs-/Technikgehäuse fürs Dach und die Hüllseiten: Kasten mit Lamellen, Rohranschluss, Warnaufkleber. Steht auf der Hülle. Wird nur aus der Ferne gesehen: niedrige Texeldichte (4-8 px/m), Browser-Downloadgröße im Blick.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/style/refs/ai_station_exterior_01.webp]
placeholder: Box 2.0 x 1.5 x 1.2 m, Pivot bottom_center, Material-Slots [Hull] (vom SkyExteriorBuilder per Code)
feedback: []
```

### SM_Ext_Greeble_Tank.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_Greeble_Tank.fbx
package: P39
scene: Außen (Station von außen, durch Fenster und im Startbildschirm)
status: final
priority: P1
needed_by: 2026-11-12
size_m: [2.0, 2.0, 4.0]
pivot: bottom_center
tris_max: 400
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_Greeble_Tank_Albedo.png   # 128x128, Point
texel_px_per_m: 6
palette: docs/style/palette.png
look: >
  Außentank mit Halteklammern in verblichenem Weiß, Beschriftung 'O2'; steht aufrecht oder liegt (per Rotation) auf der Hülle. Wird nur aus der Ferne gesehen: niedrige Texeldichte (4-8 px/m), Browser-Downloadgröße im Blick.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/style/refs/ai_station_exterior_01.webp]
placeholder: Box 2.0 x 2.0 x 4.0 m, Pivot bottom_center, Material-Slots [Hull] (vom SkyExteriorBuilder per Code)
feedback: []
```

### SM_Ext_Pipe_Large_4.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_Pipe_Large_4.fbx
package: P39
scene: Außen (Station von außen, durch Fenster und im Startbildschirm)
status: final
priority: P1
needed_by: 2026-11-12
size_m: [4.0, 0.8, 0.8]
pivot: center
tris_max: 200
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_Pipe_Large_4_Albedo.png   # 128x128, Point
texel_px_per_m: 6
palette: docs/style/palette.png
look: >
  Großrohr 4 m (Durchmesser 0,6 m, 10 Segmente) mit Flanschen an beiden Enden und Halteschellen; verläuft über die Hülle. Liegt entlang X. Wird nur aus der Ferne gesehen: niedrige Texeldichte (4-8 px/m), Browser-Downloadgröße im Blick.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/style/refs/ai_station_exterior_01.webp]
placeholder: Box 4.0 x 0.8 x 0.8 m, Pivot center, Material-Slots [Hull] (vom SkyExteriorBuilder per Code)
feedback: []
```

### SM_Ext_Pipe_Large_Elbow.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_Pipe_Large_Elbow.fbx
package: P39
scene: Außen (Station von außen, durch Fenster und im Startbildschirm)
status: final
priority: P1
needed_by: 2026-11-12
size_m: [1.2, 1.2, 0.8]
pivot: center
tris_max: 200
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_Pipe_Large_Elbow_Albedo.png   # 128x128, Point
texel_px_per_m: 6
palette: docs/style/palette.png
look: >
  90-Grad-Bogen zum Großrohr, gleicher Durchmesser und Flansch. Anschlüsse an +X und +Y (Blender). Wird nur aus der Ferne gesehen: niedrige Texeldichte (4-8 px/m), Browser-Downloadgröße im Blick.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/style/refs/ai_station_exterior_01.webp]
placeholder: Box 1.2 x 1.2 x 0.8 m, Pivot center, Material-Slots [Hull] (vom SkyExteriorBuilder per Code)
feedback: []
```

### SM_Ext_DockingCollar.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_DockingCollar.fbx
package: P39
scene: Außen (Station von außen, durch Fenster und im Startbildschirm)
status: final
priority: P1
needed_by: 2026-11-12
size_m: [8.0, 8.0, 3.0]
pivot: center
tris_max: 500
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_DockingCollar_Albedo.png   # 128x128, Point
texel_px_per_m: 6
palette: docs/style/palette.png
look: >
  Andockflansch, an dem ein Truss-Arm seitlich auf ein Habmodul (Zylinder, Durchmesser 12 m) trifft: Unterseite gewölbt passend zum Zylinder (Radius 6 m), oben eine quadratische Aufnahme für den 3 x 3 m Truss-Querschnitt, dazwischen Verstrebungen und Kabel. Truss-Richtung = +Z (oben). Wird nur aus der Ferne gesehen: niedrige Texeldichte (4-8 px/m), Browser-Downloadgröße im Blick.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/style/refs/ai_station_exterior_01.webp]
placeholder: Box 8.0 x 8.0 x 3.0 m, Pivot center, Material-Slots [Hull] (vom SkyExteriorBuilder per Code)
feedback: []
```

### SM_Ext_TrussNode.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_TrussNode.fbx
package: P39
scene: Außen (Station von außen, durch Fenster und im Startbildschirm)
status: final
priority: P1
needed_by: 2026-11-12
size_m: [4.0, 4.0, 4.0]
pivot: center
tris_max: 400
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_TrussNode_Albedo.png   # 128x128, Point
  - Assets/_Project/Art/Textures/Exterior/T_Ext_TrussNode_Emission.png   # 64x64, Point
texel_px_per_m: 6
palette: docs/style/palette.png
look: >
  Knotenwürfel für das Fachwerk: nimmt an allen 6 Seiten ein SM_Ext_TrussSegment (3 x 3 m Querschnitt) auf, Gitterstruktur mit Positionslicht. Dient als Verbindung Truss-Stationswand und Truss-Truss. Wird nur aus der Ferne gesehen: niedrige Texeldichte (4-8 px/m), Browser-Downloadgröße im Blick.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/style/refs/ai_station_exterior_01.webp]
placeholder: Box 4.0 x 4.0 x 4.0 m, Pivot center, Material-Slots [Hull] (vom SkyExteriorBuilder per Code)
feedback: []
```

### SM_Ext_SolarMast.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_SolarMast.fbx
package: P39
scene: Außen (Station von außen, durch Fenster und im Startbildschirm)
status: final
priority: P1
needed_by: 2026-11-12
size_m: [2.0, 2.0, 8.0]
pivot: bottom_center
tris_max: 300
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_SolarMast_Albedo.png   # 128x128, Point
texel_px_per_m: 6
palette: docs/style/palette.png
look: >
  Mast mit Drehgelenk (Gimbal) oben, trägt einen SM_Ext_SolarArray; unten Flansch, der auf ein TrussNode oder das Ende eines Truss passt. Wird nur aus der Ferne gesehen: niedrige Texeldichte (4-8 px/m), Browser-Downloadgröße im Blick.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/style/refs/ai_station_exterior_01.webp]
placeholder: Box 2.0 x 2.0 x 8.0 m, Pivot bottom_center, Material-Slots [Hull] (vom SkyExteriorBuilder per Code)
feedback: []
```

### SM_Ext_AntennaBase.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_AntennaBase.fbx
package: P39
scene: Außen (Station von außen, durch Fenster und im Startbildschirm)
status: final
priority: P1
needed_by: 2026-11-12
size_m: [4.0, 4.0, 1.5]
pivot: bottom_center
tris_max: 300
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_AntennaBase_Albedo.png   # 128x128, Point
texel_px_per_m: 6
palette: docs/style/palette.png
look: >
  Sockelplatte für SM_Ext_Antenna auf dem Hallendach: verstrebter Fuß mit Kabelkanälen und Wartungsleiter-Ansatz. Wird nur aus der Ferne gesehen: niedrige Texeldichte (4-8 px/m), Browser-Downloadgröße im Blick.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/style/refs/ai_station_exterior_01.webp]
placeholder: Box 4.0 x 4.0 x 1.5 m, Pivot bottom_center, Material-Slots [Hull] (vom SkyExteriorBuilder per Code)
feedback: []
```

### SM_Ext_HangarRib_Outer.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_HangarRib_Outer.fbx
package: P39
scene: Außen (Station von außen, durch Fenster und im Startbildschirm)
status: final
priority: P1
needed_by: 2026-11-12
size_m: [1.0, 2.0, 17.0]
pivot: bottom_center
tris_max: 300
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HangarRib_Outer_Albedo.png   # 128x128, Point
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HangarRib_Outer_Emission.png   # 64x64, Point
texel_px_per_m: 6
palette: docs/style/palette.png
look: >
  Äußere Tragrippe für die Hangar-Seitenwände (alle 7 m, passend zu den inneren SM_Hangar_RibArch): Stahlträger 17 m hoch, oben abgewinkelt aufs Dach, mit Positionslichtern. Wird nur aus der Ferne gesehen: niedrige Texeldichte (4-8 px/m), Browser-Downloadgröße im Blick.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/style/refs/ai_station_exterior_01.webp]
placeholder: Box 1.0 x 2.0 x 17.0 m, Pivot bottom_center, Material-Slots [Hull] (vom SkyExteriorBuilder per Code)
feedback: []
```

### T_Stamp_Approved.png
```yaml
path: Assets/_Project/Art/Textures/Counter/T_Stamp_Approved.png
package: P38
scene: Schalter
status: final
priority: P2
needed_by: 2026-11-12
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Counter/T_Stamp_Approved.png   # 128x128, Point, mit Alpha
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Stempelabdruck 'APPROVED' in grüner Stempelfarbe auf transparentem Grund: Rahmen, Schriftzug, kleines Stationssymbol
  (KESTREL-9), ungleichmäßig eingefärbt mit ausgefransten Kanten wie ein echter Gummistempel. Wird als Quad auf Dokumente gelegt.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: PNG 128x128 (Rahmen + Text, per Skript erzeugt)
feedback: []
```

### T_Stamp_Denied.png
```yaml
path: Assets/_Project/Art/Textures/Counter/T_Stamp_Denied.png
package: P38
scene: Schalter
status: final
priority: P2
needed_by: 2026-11-12
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Counter/T_Stamp_Denied.png   # 128x128, Point, mit Alpha
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Stempelabdruck 'DENIED' in roter Stempelfarbe auf transparentem Grund, gleicher Aufbau wie T_Stamp_Approved
  (Rahmen, Schriftzug, Stationssymbol, ausgefranste Gummistempel-Kanten).
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: PNG 128x128 (Rahmen + Text, per Skript erzeugt)
feedback: []
```

### T_Doc_IdCard.png
```yaml
path: Assets/_Project/Art/Textures/Documents/T_Doc_IdCard.png
package: P06
scene: Schalter (Dokumente, gelesen in voller Auflösung)
status: final
priority: P1
needed_by: 2026-11-12
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Documents/T_Doc_IdCard.png   # 512x341, Bilinear (Dokumente werden scharf gezeichnet, nicht Point)
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Hintergrund des Ausweises (12 x 8 cm, Querformat): Plastikkarten-Look in Petrol/Mintgrau, oben ein Kopfband (16 % der Höhe) in Petrol für den Titel, feine Guilloche-Sicherheitslinien, kleines KESTREL-9-Hologramm unten rechts. KEINE Feldbeschriftungen und KEINE Werte: Titel, Labels und Daten setzt das Spiel als scharfen Text darüber (Felder links Foto 32x40 mm, rechts Textspalte).
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: PNG 512x341 (per Skript erzeugt)
feedback: []
```

### T_Doc_EntryPermit.png
```yaml
path: Assets/_Project/Art/Textures/Documents/T_Doc_EntryPermit.png
package: P06
scene: Schalter (Dokumente, gelesen in voller Auflösung)
status: final
priority: P1
needed_by: 2026-11-12
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Documents/T_Doc_EntryPermit.png   # 512x717, Bilinear (Dokumente werden scharf gezeichnet, nicht Point)
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Hintergrund der Einreiseerlaubnis (15 x 21 cm, Hochformat): leicht vergilbtes Behördenpapier, Kopfband (9 % der Höhe) in Rostrot für den Titel, Rahmenlinie, Guilloche-Wasserzeichen, leerer Siegelkreis unten rechts, Fotofeld oben rechts nur als feiner Rahmen. KEINE Texte außer evtl. einem winzigen Formularcode; Labels und Daten setzt das Spiel.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: PNG 512x717 (per Skript erzeugt)
feedback: []
```

### T_Doc_CargoManifest.png
```yaml
path: Assets/_Project/Art/Textures/Documents/T_Doc_CargoManifest.png
package: P06
scene: Schalter (Dokumente, gelesen in voller Auflösung)
status: final
priority: P1
needed_by: 2026-11-12
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Documents/T_Doc_CargoManifest.png   # 512x731, Bilinear (Dokumente werden scharf gezeichnet, nicht Point)
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Hintergrund der Frachtliste (21 x 30 cm, Hochformat): Durchschlagpapier/Formular in kühlem Grauweiß, Kopfband (9 % der Höhe) in Dunkelgrau, Linien für die Warenliste ab 40 % der Höhe, Lochrand links, Kaffeefleck. KEINE Feldtexte; Labels und Daten setzt das Spiel.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: PNG 512x731 (per Skript erzeugt)
feedback: []
```

### T_Doc_Photo_Placeholder.png
```yaml
path: Assets/_Project/Art/Textures/Documents/T_Doc_Photo_Placeholder.png
package: P06
scene: Schalter (Dokumente, gelesen in voller Auflösung)
status: final
priority: P1
needed_by: 2026-11-12
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Documents/T_Doc_Photo_Placeholder.png   # 128x160, Bilinear (Dokumente werden scharf gezeichnet, nicht Point)
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Neutrales Passfoto-Platzhalterbild (grauer Hintergrund, Silhouette), solange Reisende (P07/P15) noch keine eigenen Porträts haben.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: PNG 128x160 (per Skript erzeugt)
feedback: []
```

### T_Doc_Citation.png
```yaml
path: Assets/_Project/Art/Textures/Documents/T_Doc_Citation.png
package: P07
scene: Schalter (Verwarnzettel aus dem Drucker)
status: final
priority: P2
needed_by: 2026-11-12
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Documents/T_Doc_Citation.png   # 256x448, Bilinear
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Hintergrund des Verwarnzettels (8 x 14 cm): Thermodruckpapier, leicht rosa-weiß, oben ein rotes Kopfband (13 % der Höhe) mit
  Platz für den Titel, unten gezackte Abrisskante, schwache Linien. KEINE Texte; Titel, Grund und Strafe setzt das Spiel.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: PNG 256x448 (per Skript erzeugt)
feedback: []
```

### SM_Counter_Printer.fbx
```yaml
path: Assets/_Project/Art/Models/Counter/SM_Counter_Printer.fbx
package: P07
scene: Schalter
status: final
priority: P2
needed_by: 2026-11-12
size_m: [0.22, 0.18, 0.08]
pivot: bottom_center
tris_max: 400
materials: [Printer]
textures:
  - Assets/_Project/Art/Textures/Counter/T_Counter_Printer_Albedo.png   # 128x128, Point
texel_px_per_m: 128
palette: docs/style/palette.png
look: >
  Kleiner Belegdrucker auf dem Schaltertisch (klobig, Alien-Isolation-Stil), beige-graues Gehäuse, Papierschlitz vorne (Unity -Z)
  auf 6 cm Höhe, rote Status-LED, Aufkleber 'CITATIONS'. Aus dem Schlitz kommen die Verwarnzettel.
interaction: "druckt Verwarnzettel (Skript)"
variants: []
animations: []
collision: prefab
refs: []
placeholder: graue Box 0.22 x 0.08 x 0.18 m vom DocumentPrefabBuilder
feedback: []
```

### SM_NPC_Holly.fbx
```yaml
path: Assets/_Project/Art/Models/Characters/SM_NPC_Holly.fbx
package: P52
scene: Kiosk
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.55, 0.45, 1.7]
pivot: bottom_center
tris_max: 3000
materials: [Body]
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Holly_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Holly, die Kioskbesitzerin: ältere Frau hinter der Kiosk-Durchreiche, abgetragene Arbeitskleidung, freundlich und müde.
  Statische Figur ohne Rig in einer ruhigen Haltung, im selben Stil wie die Reisenden SK_NPC_*.
  Blickrichtung in Unity lokal +Z: der EconomyBuilder dreht die Figur danach. Hitbox und Handel hängen am Prefab.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Graue Box aus GameOff/Build Economy into Main (EconomyBuilder), kein FBX am Pfad. Sobald das FBX da ist, setzt der Builder es ein.
feedback: []
```

### SM_NPC_Barkeeper.fbx
```yaml
path: Assets/_Project/Art/Models/Characters/SM_NPC_Barkeeper.fbx
package: P52
scene: Bar
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.8, 0.6, 1.9]
pivot: bottom_center
tris_max: 3000
materials: [Body]
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Barkeeper_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Barkeeper: vierarmiges Alien hinter der Bar-Theke, mit Atemmaske und Schlauch zu einem Rucksack.
  Statische Figur ohne Rig in einer ruhigen Haltung, im selben Stil wie die Reisenden SK_NPC_*.
  Blickrichtung in Unity lokal +Z: der EconomyBuilder dreht die Figur danach. Hitbox und Handel hängen am Prefab.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Graue Box aus GameOff/Build Economy into Main (EconomyBuilder), kein FBX am Pfad. Sobald das FBX da ist, setzt der Builder es ein.
feedback: []
```

### SM_NPC_Dealer.fbx
```yaml
path: Assets/_Project/Art/Models/Characters/SM_NPC_Dealer.fbx
package: P52
scene: Bar
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.6, 0.6, 1.3]
pivot: bottom_center
tris_max: 3000
materials: [Body]
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Dealer_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Dealer vom Schwarzmarkt: sitzt shady auf einem Barhocker (Hocker gehört nicht zum Modell, Pivot = Sitzfläche, 0,45 m über dem Boden),
  dunkle, abgewetzte Kleidung mit Kapuze, Gesicht im Schatten.
  Statische Figur ohne Rig in einer ruhigen Haltung, im selben Stil wie die Reisenden SK_NPC_*.
  Blickrichtung in Unity lokal +Z: der EconomyBuilder dreht die Figur danach. Hitbox und Handel hängen am Prefab.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Graue Box aus GameOff/Build Economy into Main (EconomyBuilder), kein FBX am Pfad. Sobald das FBX da ist, setzt der Builder es ein.
feedback: []
```

### SM_Counter_WindowWall_6x3.fbx
```yaml
path: Assets/_Project/Art/Models/Counter/SM_Counter_WindowWall_6x3.fbx
package: P60
scene: Counter
status: final
priority: P1
needed_by: 2026-11-16
size_m: [6.0, 0.2, 3.0]
pivot: bottom_center
tris_max: 500
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Counter/T_Counter_WindowWall_Albedo.png   # 256x128, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Vorderwand des Schalterraums, Seite des Beamten, die wichtigste Wand im Spiel: Der Spieler sitzt die ganze Schicht davor. 6 m breit, 3 m hoch, 0,2 m dick. Öffnung für das vorhandene Schalterfenster: mittig 4 m breit, vom Boden bis 2,2 m hoch, also 1 m Wand links und rechts und 0,8 m Sturz. Gepolsterte Behördenwand wie SM_Station_Wall_2x3_Padded, kühles Blaugrau, abgenutzt, Nietenreihe um die Öffnung, kleines Warnschild oben seitlich. Der Sturz soll nicht leer wirken. Ausrichtung wie SM_Station_Wall_2x3, die Sichtseite ist die Vorderseite; der Builder dreht das Stück.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Fünf flache blaue Blöcke `Wall_X-0,1_*` (M_Graybox_Counter) aus dem StationGrayboxBuilder um das Schalterfenster. Maße wie oben; Kollision bleibt bei den Blöcken.
feedback: []
```

### SM_Lock_WindowWall_6x3.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Lock_WindowWall_6x3.fbx
package: P60
scene: InspectionLock
status: final
priority: P1
needed_by: 2026-11-16
size_m: [6.0, 0.2, 3.0]
pivot: bottom_center
tris_max: 500
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Station/T_Lock_WindowWall_Albedo.png   # 256x128, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Dieselbe Wand von der Seite der Prüfkammer: gleiche Maße und gleiche Öffnung wie SM_Counter_WindowWall_6x3 (6 x 3 m, Öffnung 4 m breit, 2,2 m hoch). Sicherheitsbereich: dunkles, verkratztes Metall, rot-gelbe Warnstreifen um die Öffnung, im Stil von SM_Station_Hatch_Airlock. Der Spieler sieht sie vom Schalter nur durch das Fenster, die Reisenden stehen davor. Passt zum roten Licht der Kammer.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Fünf flache rote Blöcke `Wall_X0,1_*` (M_Graybox_Danger) aus dem StationGrayboxBuilder. Maße wie oben; Kollision bleibt bei den Blöcken.
feedback: []
```

### SM_Hall_WallEnd_1x6.fbx
```yaml
path: Assets/_Project/Art/Models/Hall/SM_Hall_WallEnd_1x6.fbx
package: P60
scene: Hall
status: final
priority: P2
needed_by: 2026-11-16
size_m: [1.0, 0.2, 6.0]
pivot: bottom_center
tris_max: 200
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Hall/T_Hall_WallEnd_Albedo.png   # 64x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Schmales, hohes Wandstück der Halle, 1 m breit und 6 m hoch: schließt die Nordwand links und rechts neben dem breiten Durchgang ab. Wird zweimal gesetzt, einmal gespiegelt. Muss zu den Wandmodulen der Halle passen: gleiche Plattenteilung auf 3 m Höhe, oben eine vertikale Rippe oder ein Rohr, damit das Stück nicht leer wirkt.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vier neutrale graue Blöcke `Wall_Z5,9_*` (M_Graybox_Neutral) bei x = ±5,5. Kollision bleibt bei den Blöcken.
feedback: []
```

### SM_Station_LightStrip.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_LightStrip.fbx
package: P60
scene: alle Räume
status: final
priority: P2
needed_by: 2026-11-16
size_m: [1.4, 0.2, 0.06]
pivot: bottom_center
tris_max: 60
materials: [Frame, Glow]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_LightStrip_Albedo.png   # 64x16, Point
  - Assets/_Project/Art/Textures/Station/T_Station_LightStrip_Emission.png   # 64x16, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Einfache Deckenleuchte: schmaler Metallrahmen mit Leuchtstreifen, 1,4 m lang. Wird 49-mal gesetzt, also sehr sparsam. Der Slot Glow muss weiß bis hellgrau sein: Der Builder färbt ihn je Raum ein (blau, orange, grün, violett ...). Die Oberseite sieht niemand und braucht keine Dreiecke.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Leuchtende Quader `<Raum>_Strip_*` (M_Graybox_Emit_*) in der Gruppe LightFixtures. Pivot unten mittig direkt an der Decke.
feedback: []
```

### SM_Station_LockLamp.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_LockLamp.fbx
package: P60
scene: Motel
status: final
priority: P3
needed_by: 2026-11-16
size_m: [0.12, 0.44, 0.08]
pivot: center
tris_max: 40
materials: [Lamp]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_LockLamp_Albedo.png   # 16x16, Point
  - Assets/_Project/Art/Textures/Station/T_Station_LockLamp_Emission.png   # 16x16, Point
texel_px_per_m: 128
palette: docs/style/palette.png
look: >
  Kleine rote Statuslampe über einer verriegelten Tür. Geht durch die Wand und ist von beiden Seiten sichtbar: 12 cm breit, 8 cm hoch, 44 cm tief. Kleiner Metallrahmen, rotes Glas mit Emission.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Roter leuchtender Quader `LockedLamp` an den Türen von Zimmer 101 und 102.
feedback: []
```

### T_Ext_HullFiller_A_Albedo.png
```yaml
path: Assets/_Project/Art/Textures/Exterior/T_Ext_HullFiller_A_Albedo.png
package: P60
scene: Exterior
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HullFiller_A_Albedo.png   # 128x128, nahtlos kachelnd, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Kachelnde Außenhaut für alle Hüllflächen, die keine ganze 4-m-Platte sind: die 137 Platten der Unterseite und die Reststücke rund um Öffnungen und Kanten. Eine Kachel entspricht 4 m. Gleicher Stil, Farbton und gleiche Texeldichte wie T_Ext_HullPanel_4x4_Albedo, aber ruhiger und ohne Luken, damit bei Wiederholung und schmalen Streifen kein Muster auffällt. Muss nahtlos kacheln.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Einfarbige Blöcke mit M_Graybox_Hull_A in der Gruppe Hull, rund 130 Stück.
feedback: []
```

### T_Ext_HullFiller_B_Albedo.png
```yaml
path: Assets/_Project/Art/Textures/Exterior/T_Ext_HullFiller_B_Albedo.png
package: P60
scene: Exterior
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HullFiller_B_Albedo.png   # 128x128, nahtlos kachelnd, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Zweite Variante von T_Ext_HullFiller_A_Albedo, etwas heller und mit mehr Brandspuren. Beide werden gemischt, damit die Flächen nicht einheitlich wirken. Gleiche Dichte, muss nahtlos kacheln.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Einfarbige Blöcke mit M_Graybox_Hull_B in der Gruppe Hull, rund 60 Stück.
feedback: []
```

### T_Doc_Photo_Human.png
```yaml
path: Assets/_Project/Art/Textures/Documents/T_Doc_Photo_Human.png
package: P60
scene: Counter
status: final
priority: P1
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Documents/T_Doc_Photo_Human.png   # 128x160, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Passfoto für Dokumente: Mensch frontal von vorn, Kopf und Schultern, neutraler heller Behördenhintergrund, leicht vergilbt und grobkörnig. Gerendert aus SK_NPC_Human (Gesicht und Kleidung müssen dazu passen), damit der Spieler Foto und Figur in der Kammer vergleichen kann. Gleicher Bildausschnitt bei allen fünf Arten, keine Schrift im Bild.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Graue Silhouette T_Doc_Photo_Placeholder.png (128x160) auf allen Dokumenten aller Reisenden. Liefern unter neuem Namen, die alte Datei bleibt.
feedback: []
```

### T_Doc_Photo_Vellani.png
```yaml
path: Assets/_Project/Art/Textures/Documents/T_Doc_Photo_Vellani.png
package: P60
scene: Counter
status: final
priority: P1
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Documents/T_Doc_Photo_Vellani.png   # 128x160, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Passfoto für Dokumente: Vellani frontal von vorn, Kopf und Schultern, neutraler heller Behördenhintergrund, leicht vergilbt und grobkörnig. Gerendert aus SK_NPC_Vellani (Gesicht und Kleidung müssen dazu passen), damit der Spieler Foto und Figur in der Kammer vergleichen kann. Gleicher Bildausschnitt bei allen fünf Arten, keine Schrift im Bild.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Graue Silhouette T_Doc_Photo_Placeholder.png (128x160) auf allen Dokumenten aller Reisenden. Liefern unter neuem Namen, die alte Datei bleibt.
feedback: []
```

### T_Doc_Photo_Quobb.png
```yaml
path: Assets/_Project/Art/Textures/Documents/T_Doc_Photo_Quobb.png
package: P60
scene: Counter
status: final
priority: P1
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Documents/T_Doc_Photo_Quobb.png   # 128x160, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Passfoto für Dokumente: Quobb frontal von vorn, Kopf und Schultern, neutraler heller Behördenhintergrund, leicht vergilbt und grobkörnig. Gerendert aus SK_NPC_Quobb (Gesicht und Kleidung müssen dazu passen), damit der Spieler Foto und Figur in der Kammer vergleichen kann. Gleicher Bildausschnitt bei allen fünf Arten, keine Schrift im Bild.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Graue Silhouette T_Doc_Photo_Placeholder.png (128x160) auf allen Dokumenten aller Reisenden. Liefern unter neuem Namen, die alte Datei bleibt.
feedback: []
```

### T_Doc_Photo_Xirrik.png
```yaml
path: Assets/_Project/Art/Textures/Documents/T_Doc_Photo_Xirrik.png
package: P60
scene: Counter
status: final
priority: P1
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Documents/T_Doc_Photo_Xirrik.png   # 128x160, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Passfoto für Dokumente: Xirrik frontal von vorn, Kopf und Schultern, neutraler heller Behördenhintergrund, leicht vergilbt und grobkörnig. Gerendert aus SK_NPC_Xirrik (Gesicht und Kleidung müssen dazu passen), damit der Spieler Foto und Figur in der Kammer vergleichen kann. Gleicher Bildausschnitt bei allen fünf Arten, keine Schrift im Bild.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Graue Silhouette T_Doc_Photo_Placeholder.png (128x160) auf allen Dokumenten aller Reisenden. Liefern unter neuem Namen, die alte Datei bleibt.
feedback: []
```

### T_Doc_Photo_Sorvak.png
```yaml
path: Assets/_Project/Art/Textures/Documents/T_Doc_Photo_Sorvak.png
package: P60
scene: Counter
status: final
priority: P1
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Documents/T_Doc_Photo_Sorvak.png   # 128x160, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Passfoto für Dokumente: Sorvak frontal von vorn, Kopf und Schultern, neutraler heller Behördenhintergrund, leicht vergilbt und grobkörnig. Gerendert aus SK_NPC_Sorvak (Gesicht und Kleidung müssen dazu passen), damit der Spieler Foto und Figur in der Kammer vergleichen kann. Gleicher Bildausschnitt bei allen fünf Arten, keine Schrift im Bild.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Graue Silhouette T_Doc_Photo_Placeholder.png (128x160) auf allen Dokumenten aller Reisenden. Liefern unter neuem Namen, die alte Datei bleibt.
feedback: []
```

### SFX_PA_Chime.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_PA_Chime.ogg
package: P60
scene: Station
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Durchsage-Gong der Station vor Ansagen: zwei kurze, leicht verrauschte Töne aus einem alten Lautsprecher, ca. 1 s. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono. Keine Stimme.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Synthetischer Testton AudioManager.PlayPreview(Voice) bei jeder Durchsage. Die gesprochenen Durchsagen selbst kommen nicht von der Pipeline.
feedback: []
```

### SFX_Vendor_Greet.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Vendor_Greet.ogg
package: P60
scene: Kiosk, Bar
status: final
priority: P3
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Kurzes, sprachähnliches Murmeln beim Ansprechen eines Händlers, ca. 0,5 s, keine verständlichen Wörter, leicht verfremdet, damit es zu Mensch und Alien passt. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Synthetischer Testton AudioManager.PlayPreview(Voice) in Vendor.OnInteract.
feedback: []
```

### SFX_Stamp_Approve.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Stamp_Approve.ogg
package: P61
scene: Counter
status: final
priority: P1
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Schwerer Gummistempel knallt auf Papier, trocken und befriedigend, ca. 0,3 s. Etwas heller als SFX_Stamp_Deny. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, die Stelle ist bisher stumm. Der Einbau folgt in P61.
feedback: []
```

### SFX_Stamp_Deny.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Stamp_Deny.ogg
package: P61
scene: Counter
status: final
priority: P1
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Derselbe Stempel, dumpfer und härter, ca. 0,3 s. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, die Stelle ist bisher stumm. Der Einbau folgt in P61.
feedback: []
```

### SFX_Paper_Slide.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Paper_Slide.ogg
package: P61
scene: Counter
status: final
priority: P1
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Papier wird über einen Metalltisch geschoben, kurz und leise, ca. 0,3 s. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, die Stelle ist bisher stumm. Der Einbau folgt in P61.
feedback: []
```

### SFX_Paper_Pickup.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Paper_Pickup.ogg
package: P61
scene: Counter
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Blatt wird angehoben und zum Lesen hochgehalten, leises Rascheln, ca. 0,3 s. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, die Stelle ist bisher stumm. Der Einbau folgt in P61.
feedback: []
```

### SFX_Drawer_Open.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Drawer_Open.ogg
package: P61
scene: Counter
status: final
priority: P1
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Durchreiche-Schublade aus Metall fährt aus: Schieben auf Schienen mit Anschlag, ca. 0,6 s. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, die Stelle ist bisher stumm. Der Einbau folgt in P61.
feedback: []
```

### SFX_Drawer_Close.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Drawer_Close.ogg
package: P61
scene: Counter
status: final
priority: P1
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Dieselbe Schublade fährt zurück und rastet ein, ca. 0,6 s. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, die Stelle ist bisher stumm. Der Einbau folgt in P61.
feedback: []
```

### SFX_Printer_Citation.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Printer_Citation.ogg
package: P61
scene: Counter
status: final
priority: P1
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Alter Nadeldrucker spuckt einen Verwarnzettel aus: kurzes Rattern und Abreißen, ca. 1,2 s. Soll leicht unangenehm sein. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, die Stelle ist bisher stumm. Der Einbau folgt in P61.
feedback: []
```

### SFX_Button_Arrest.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Button_Arrest.ogg
package: P61
scene: Counter
status: final
priority: P1
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Großer Pilzknopf wird gedrückt, gefolgt von einem kurzen Alarm-Blip, ca. 0,6 s. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, die Stelle ist bisher stumm. Der Einbau folgt in P61.
feedback: []
```

### SFX_Cover_Flip.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Cover_Flip.ogg
package: P61
scene: Counter
status: final
priority: P1
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Metallene Schutzklappe klappt auf oder zu, ein trockenes Klacken, ca. 0,2 s. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, die Stelle ist bisher stumm. Der Einbau folgt in P61.
feedback: []
```

### SFX_Airlock_Fire.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Airlock_Fire.ogg
package: P61
scene: InspectionLock
status: final
priority: P1
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Schleuse wird ausgelöst: schweres Verriegeln, dann Druckabfall und Zischen, das in Stille abreißt, ca. 2,5 s. Der härteste Sound im Spiel. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, die Stelle ist bisher stumm. Der Einbau folgt in P61.
feedback: []
```

### SFX_Footstep_Metal_01.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Footstep_Metal_01.ogg
package: P61
scene: alle Räume
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Ein Schritt mit Arbeitsstiefel auf Metallboden, leise, ca. 0,2 s. Einzeln, ohne Hall. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, die Stelle ist bisher stumm. Der Einbau folgt in P61.
feedback: []
```

### SFX_Footstep_Metal_02.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Footstep_Metal_02.ogg
package: P61
scene: alle Räume
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Zweite Variante von SFX_Footstep_Metal_01, gleiche Lautstärke, ca. 0,2 s. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, die Stelle ist bisher stumm. Der Einbau folgt in P61.
feedback: []
```

### SFX_Footstep_Metal_03.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Footstep_Metal_03.ogg
package: P61
scene: alle Räume
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Dritte Variante von SFX_Footstep_Metal_01, gleiche Lautstärke, ca. 0,2 s. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, die Stelle ist bisher stumm. Der Einbau folgt in P61.
feedback: []
```

### SFX_UI_Click.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_UI_Click.ogg
package: P61
scene: UI
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Kurzer, mechanischer Klick für Menü und Fenster, ca. 0,1 s. Passt zu alter Behördentechnik, kein moderner Pling. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, die Stelle ist bisher stumm. Der Einbau folgt in P61.
feedback: []
```

### SFX_Trade_Buy.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Trade_Buy.ogg
package: P61
scene: Kiosk, Bar
status: final
priority: P3
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Kauf abgeschlossen: Münzen oder Kassenklacken, ca. 0,4 s. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, die Stelle ist bisher stumm. Der Einbau folgt in P61.
feedback: []
```

### SFX_Amb_Station_Loop.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Amb_Station_Loop.ogg
package: P61
scene: Halle, Schalter, Motel
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Grundton der Station: tiefes Lüftungsbrummen mit seltenem Knacken im Metall. Nahtlos loopend, höchstens 30 s, weil die Downloadgröße zählt. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, die Stelle ist bisher stumm. Der Einbau folgt in P61.
feedback:
  - "2026-11-06 martin: Liefert Martin selbst, Pipeline nicht mehr nötig. Freesound 77838 (Jovica, CC BY 4.0), ruhiger Teil 51 s als nahtloser Loop, stereo, 486 KB. Liegt schon unter dem Zielpfad, Quelle in ArtSource/Audio/. Länge und Stereo sind Martins Entscheidung (statt höchstens 30 s mono). Max: bitte Status setzen und beim Einbau in P61 nach Gehör prüfen."
  - "2026-11-06 max: Max hat entschieden, die synthetische Station-Atmo aus P61 (W1067, Stil der Türsounds, 29 s mono, Loop rund) ersetzt die Freesound-Datei. Damit entfallen Quelle (Jovica FLAC/WAV) und die CREDITS-Zeile; die .meta (GUID) bleibt, PF_Ambience zeigt also ohne Änderung auf die neue Datei."
```

### SFX_Amb_Bar_Loop.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Amb_Bar_Loop.ogg
package: P61
scene: Bar
status: final
priority: P3
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Raumklang der Bar: Stimmengemurmel ohne verständliche Wörter, gelegentlich Gläser. Nahtlos loopend, höchstens 30 s. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, die Stelle ist bisher stumm. Der Einbau folgt in P61.
feedback: []
```

### SFX_Amb_Hangar_Loop.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Amb_Hangar_Loop.ogg
package: P61
scene: Hangar
status: final
priority: P3
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Raumklang des Hangars: große Halle, entfernte Maschinen, leises Summen des Kraftfelds. Nahtlos loopend, höchstens 30 s. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, die Stelle ist bisher stumm. Der Einbau folgt in P61.
feedback: []
```

### SM_Item_DocScanner.fbx
```yaml
path: Assets/_Project/Art/Models/Items/SM_Item_DocScanner.fbx
package: P16
scene: Kiosk, Schalter (auf dem Tisch)
status: final
priority: P1
needed_by: 2026-11-16
size_m: [0.12, 0.18, 0.05]
pivot: bottom_center
tris_max: 400
materials: [Item]
textures:
  - Assets/_Project/Art/Textures/Items/T_Item_DocScanner_Albedo.png   # 64x64, Point
  - Assets/_Project/Art/Textures/Items/T_Item_DocScanner_Emission.png   # 64x64, Point
texel_px_per_m: 128
palette: docs/style/palette.png
look: >
  Dokumentenscanner: klobiges Handgerät mit Griff, kleinem grünen Röhrenschirm und Abtastleiste, abgenutztes beiges Plastik, passend zu SM_Counter_Printer und dem CRT am Schalter. Liegt nach dem Kauf auf dem Schaltertisch. Der Schirm leuchtet über die Emission.
interaction: "kaufen, am Schalter benutzen"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Noch kein Modell, der Scanner existiert nur als Gegenstand Item_DocumentScanner im Kiosk.
feedback: []
```

### SM_Item_UVLamp.fbx
```yaml
path: Assets/_Project/Art/Models/Items/SM_Item_UVLamp.fbx
package: P62
scene: Schalter (auf dem Tisch)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.06, 0.18, 0.06]
pivot: bottom_center
tris_max: 300
materials: [Item]
textures:
  - Assets/_Project/Art/Textures/Items/T_Item_UVLamp_Albedo.png   # 64x64, Point
  - Assets/_Project/Art/Textures/Items/T_Item_UVLamp_Emission.png   # 64x64, Point
texel_px_per_m: 128
palette: docs/style/palette.png
look: >
  UV-Handlampe: kurzer Metallzylinder mit Griffrillen und violetter Leuchtröhre vorn, Behördenausstattung, verkratzt. Steht auf dem Schaltertisch. Die Röhre leuchtet violett über die Emission.
interaction: "am Schalter benutzen"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Noch kein Modell und kein Gegenstand, kommt mit P62.
feedback: []
```

### SM_Item_ClownShoes.fbx
```yaml
path: Assets/_Project/Art/Models/Items/SM_Item_ClownShoes.fbx
package: P63
scene: Asservaten, Inventar
status: final
priority: P3
needed_by: 2026-11-16
size_m: [0.3, 0.42, 0.14]
pivot: bottom_center
tris_max: 500
materials: [Item]
textures:
  - Assets/_Project/Art/Textures/Items/T_Item_ClownShoes_Albedo.png   # 64x64, Point
texel_px_per_m: 128
palette: docs/style/palette.png
look: >
  Paar riesiger Clownsschuhe mit runden Kappen, rot und gelb, abgewetzt und völlig fehl am Platz in der Station. Der einzige fröhliche Gegenstand im Spiel. Beide Schuhe in einem Modell, nebeneinander stehend.
interaction: "beschlagnahmen, anziehen, ausziehen"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Noch kein Modell und kein Gegenstand, kommt mit P63.
feedback: []
```

### SFX_ClownShoe_Squeak_01.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_ClownShoe_Squeak_01.ogg
package: P63
scene: alle Räume
status: final
priority: P3
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Quietschender Schritt eines Clownsschuhs, wie ein Gummitier, ca. 0,25 s. Mono, OGG.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton. Der Einbau folgt in P63.
feedback: []
```

### SFX_ClownShoe_Squeak_02.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_ClownShoe_Squeak_02.ogg
package: P63
scene: alle Räume
status: final
priority: P3
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Zweite Variante von SFX_ClownShoe_Squeak_01, etwas höher, ca. 0,25 s. Mono, OGG.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton. Der Einbau folgt in P63.
feedback: []
```

### SFX_ClownShoe_Squeak_03.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_ClownShoe_Squeak_03.ogg
package: P63
scene: alle Räume
status: final
priority: P3
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Dritte Variante von SFX_ClownShoe_Squeak_01, etwas tiefer, ca. 0,25 s. Mono, OGG.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton. Der Einbau folgt in P63.
feedback: []
```

### SM_Station_Hab_Shell_4m.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Hab_Shell_4m.fbx
package: P53
scene: Wohnmodul Hab_West (innen) und Druckröhre vom Motelgang
status: final
priority: P2
needed_by: 2026-11-13
size_m: [10.4, 4.0, 3.59]
pivot: bottom_center
tris_max: 140
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Hab_Shell_4m_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Extra der Pipeline, von Max zum Bau freigegeben 06.11.: 4-m-Scheibe der Innenhülle des Wohnmoduls: zu beiden Seiten Sockel, schräge Hüllverkleidung (Rückwand der Räume), obere Facette und Raumdecke auf 3,0 m, dazwischen das Flurgewölbe bis 3,59 m. Stationsgrau mit Rippen und Fugen wie das Kit. Sechs Stück hintereinander (Z 14 … 38) füllen das Deck zwischen den Schotten; Pivot Flurmitte auf dem Deck (Y 7,42), die Scheibe läuft entlang lokal Z. Platzierung: docs/station/wohnmodul.md.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/station/wohnmodul_plan.png]
placeholder: none (direkt geliefert)
feedback: []
```

### SM_Station_Hab_Bulkhead.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Hab_Bulkhead.fbx
package: P53
scene: Wohnmodul Hab_West (innen) und Druckröhre vom Motelgang
status: final
priority: P2
needed_by: 2026-11-13
size_m: [10.7, 0.2, 3.7]
pivot: bottom_center
tris_max: 80
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Hab_Bulkhead_Albedo.png   # 128x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Extra der Pipeline, von Max zum Bau freigegeben 06.11.: Schott am Nord- und Südende des Decks: 12-Eck-Platte (Apothem 5,35) mit Randrippen, beidseitig, und dem 2 m breiten Türschlitz des Flurs, den eine Kit-Türwand (Süd) bzw. die Screen-Wand (Nord) füllt. Pivot Unterkante Mitte. Platzierung: docs/station/wohnmodul.md.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/station/wohnmodul_plan.png]
placeholder: none (direkt geliefert)
feedback: []
```

### SM_Station_Hab_LightStrip_2m.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Hab_LightStrip_2m.fbx
package: P53
scene: Wohnmodul Hab_West (innen) und Druckröhre vom Motelgang
status: final
priority: P2
needed_by: 2026-11-13
size_m: [2.0, 0.2, 0.06]
pivot: top_center
tris_max: 12
materials: [Light]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Hab_LightStrip_2m_Albedo.png   # 32x32, Point
  - Assets/_Project/Art/Textures/Station/T_Station_Hab_LightStrip_2m_Emission.png   # 32x32, Point
texel_px_per_m: 16
palette: docs/style/palette.png
look: >
  Extra der Pipeline, von Max zum Bau freigegeben 06.11.: Deckenleuchtstreifen 2 m: flache Leiste mit Linse nach unten, grünes Leuchten wie im Motelgang (Emission). Pivot oben Mitte (an der Decke); 24 Stück im Flur und in den Räumen. Platzierung: docs/station/wohnmodul.md.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/station/wohnmodul_plan.png]
placeholder: none (direkt geliefert)
feedback: []
```

### SM_Station_HabTube_Frame.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_HabTube_Frame.fbx
package: P53
scene: Wohnmodul Hab_West (innen) und Druckröhre vom Motelgang
status: final
priority: P2
needed_by: 2026-11-13
size_m: [2.9, 2.5, 2.45]
pivot: bottom_center
tris_max: 140
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_HabTube_Frame_Albedo.png   # 128x128, Point
  - Assets/_Project/Art/Textures/Station/T_Station_HabTube_Frame_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Extra der Pipeline, von Max zum Bau freigegeben 06.11.: Druckrahmen hinter der Türwand am Westende des Motelgangs: Laibung und Sturz der 1,2 × 2,2-m-Öffnung, Rahmenplatte mit Warnstreifen und das 2,05-m-Podest bis zur ersten Treppe. Pivot Bodenmitte an der Wandrückseite, die Röhre läuft entlang lokal +Z. Platzierung: docs/station/wohnmodul.md.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/station/wohnmodul_plan.png]
placeholder: none (direkt geliefert)
feedback: []
```

### SM_Station_HabTube_Stair_2m.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_HabTube_Stair_2m.fbx
package: P53
scene: Wohnmodul Hab_West (innen) und Druckröhre vom Motelgang
status: final
priority: P2
needed_by: 2026-11-13
size_m: [2.9, 2.0, 3.687]
pivot: bottom_center
tris_max: 160
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_HabTube_Stair_2m_Albedo.png   # 128x64, Point
  - Assets/_Project/Art/Textures/Station/T_Station_HabTube_Stair_2m_Emission.png   # 64x32, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Extra der Pipeline, von Max zum Bau freigegeben 06.11.: Treppensegment der Druckröhre (12-Eck, lichte Weite 2,9 m): 2 m lang, 7 Stufen à 0,177 m (1,24 m Steigung), Kopfhöhe 2,27 m, Stufen mit gelber Trittkante und Rillen, Rippen. Sechs Stück hintereinander überwinden 7,42 m. Pivot Bodenmitte am Segmentanfang. Platzierung: docs/station/wohnmodul.md.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/station/wohnmodul_plan.png]
placeholder: none (direkt geliefert)
feedback: []
```

### SM_Station_HabTube_Corner.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_HabTube_Corner.fbx
package: P53
scene: Wohnmodul Hab_West (innen) und Druckröhre vom Motelgang
status: final
priority: P2
needed_by: 2026-11-13
size_m: [3.45, 3.45, 2.45]
pivot: bottom_center
tris_max: 160
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_HabTube_Corner_Albedo.png   # 128x64, Point
  - Assets/_Project/Art/Textures/Station/T_Station_HabTube_Corner_Emission.png   # 64x32, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Extra der Pipeline, von Max zum Bau freigegeben 06.11.: 90°-Bogen der Druckröhre (R 2 m, nach rechts = Norden) am oberen Treppenende, gleicher Querschnitt wie Straight_2m. Pivot Bodenmitte am Bogenanfang. Platzierung: docs/station/wohnmodul.md.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/station/wohnmodul_plan.png]
placeholder: none (direkt geliefert)
feedback: []
```

### SM_Station_HabTube_Straight_2m.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_HabTube_Straight_2m.fbx
package: P53
scene: Wohnmodul Hab_West (innen) und Druckröhre vom Motelgang
status: final
priority: P2
needed_by: 2026-11-13
size_m: [2.9, 2.0, 2.45]
pivot: bottom_center
tris_max: 120
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_HabTube_Straight_2m_Albedo.png   # 128x64, Point
  - Assets/_Project/Art/Textures/Station/T_Station_HabTube_Straight_2m_Emission.png   # 64x32, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Extra der Pipeline, von Max zum Bau freigegeben 06.11.: Gerades Segment der Druckröhre, 2 m: 12-Eck-Querschnitt (lichte Weite 2,9 m, Höhe 2,45 m), flacher Boden 2,2 m breit, Rippen, grüner Leuchtstreifen im Scheitel (Emission). Pivot Bodenmitte am Segmentanfang. Platzierung: docs/station/wohnmodul.md.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/station/wohnmodul_plan.png]
placeholder: none (direkt geliefert)
feedback: []
```

### SM_Station_HabTube_Collar.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_HabTube_Collar.fbx
package: P53
scene: Wohnmodul Hab_West (innen) und Druckröhre vom Motelgang
status: final
priority: P2
needed_by: 2026-11-13
size_m: [2.9, 3.0, 2.45]
pivot: bottom_center
tris_max: 160
materials: [Wall]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_HabTube_Collar_Albedo.png   # 128x128, Point
  - Assets/_Project/Art/Textures/Station/T_Station_HabTube_Collar_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Extra der Pipeline, von Max zum Bau freigegeben 06.11.: Einstieg der Röhre durch die Südkappe des Moduls bis zum Schott: Röhre mit Warnringen an beiden Enden, Schottplatte und Türschwelle (Z 11 … 14). Pivot Bodenmitte an der Südkappe (Z 11). Platzierung: docs/station/wohnmodul.md.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/station/wohnmodul_plan.png]
placeholder: none (direkt geliefert)
feedback: []
```

### SM_Ext_HabTube_Flange.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_HabTube_Flange.fbx
package: P53
scene: Außen (Station von außen, durch Fenster und im Startbildschirm)
status: final
priority: P2
needed_by: 2026-11-13
size_m: [3.8, 2.15, 3.8]
pivot: bottom_center
tris_max: 90
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HabModule_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HabModule_Emission.png   # 128x128, Point
texel_px_per_m: 8
palette: docs/style/palette.png
look: >
  Extra der Pipeline, von Max zum Bau freigegeben 06.11.: Flansch der Druckröhre an der Außenhülle des Motels (Warnring, Apothem 1,9) und die Röhre bis über das Podest; nutzt die Texturen von SM_Ext_HabModule (gleiche Pixel, keine neue Textur): Slot Hull auf M_Ext_HabModule legen. Platzierung: docs/station/wohnmodul.md.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/station/wohnmodul_plan.png]
placeholder: none (direkt geliefert)
feedback: []
```

### SM_Ext_HabTube_Stair_2m.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_HabTube_Stair_2m.fbx
package: P53
scene: Außen (Station von außen, durch Fenster und im Startbildschirm)
status: final
priority: P2
needed_by: 2026-11-13
size_m: [3.2, 2.0, 4.437]
pivot: bottom_center
tris_max: 40
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HabModule_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HabModule_Emission.png   # 128x128, Point
texel_px_per_m: 8
palette: docs/style/palette.png
look: >
  Extra der Pipeline, von Max zum Bau freigegeben 06.11.: Außenhaut eines Treppensegments der Röhre (12-Eck, Apothem 1,6, geschert); nutzt die Texturen von SM_Ext_HabModule (gleiche Pixel, keine neue Textur): Slot Hull auf M_Ext_HabModule legen. Platzierung: docs/station/wohnmodul.md.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/station/wohnmodul_plan.png]
placeholder: none (direkt geliefert)
feedback: []
```

### SM_Ext_HabTube_Corner.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_HabTube_Corner.fbx
package: P53
scene: Außen (Station von außen, durch Fenster und im Startbildschirm)
status: final
priority: P2
needed_by: 2026-11-13
size_m: [3.6, 3.6, 3.2]
pivot: bottom_center
tris_max: 170
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HabModule_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HabModule_Emission.png   # 128x128, Point
texel_px_per_m: 8
palette: docs/style/palette.png
look: >
  Extra der Pipeline, von Max zum Bau freigegeben 06.11.: Außenhaut des 90°-Bogens der Röhre (sechs 15°-Segmente); nutzt die Texturen von SM_Ext_HabModule (gleiche Pixel, keine neue Textur): Slot Hull auf M_Ext_HabModule legen. Platzierung: docs/station/wohnmodul.md.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/station/wohnmodul_plan.png]
placeholder: none (direkt geliefert)
feedback: []
```

### SM_Ext_HabTube_Straight_2m.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_HabTube_Straight_2m.fbx
package: P53
scene: Außen (Station von außen, durch Fenster und im Startbildschirm)
status: final
priority: P2
needed_by: 2026-11-13
size_m: [3.2, 2.0, 3.2]
pivot: bottom_center
tris_max: 40
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HabModule_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HabModule_Emission.png   # 128x128, Point
texel_px_per_m: 8
palette: docs/style/palette.png
look: >
  Extra der Pipeline, von Max zum Bau freigegeben 06.11.: Außenhaut eines geraden 2-m-Segments der Röhre (12-Eck, Apothem 1,6); nutzt die Texturen von SM_Ext_HabModule (gleiche Pixel, keine neue Textur): Slot Hull auf M_Ext_HabModule legen. Platzierung: docs/station/wohnmodul.md.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/station/wohnmodul_plan.png]
placeholder: none (direkt geliefert)
feedback: []
```

### SM_Ext_HabModule_Entry.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_HabModule_Entry.fbx
package: P53
scene: Außen (Station von außen, durch Fenster und im Startbildschirm)
status: final
priority: P2
needed_by: 2026-11-13
size_m: [12.0, 12.0, 30.0]
pivot: center
tris_max: 1500
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HabModule_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Exterior/T_Ext_HabModule_Emission.png   # 128x128, Point
texel_px_per_m: 8
palette: docs/style/palette.png
look: >
  Extra der Pipeline, von Max zum Bau freigegeben 06.11.: SM_Ext_HabModule mit geöffneter Südkappe für die Röhre: statt der Andockluke ein Ring vom Flansch (Apothem 2,8) zur Röhre (1,6); sonst gleich, ersetzt das Modul an Hab_West (gleiche Transformation); nutzt die Texturen von SM_Ext_HabModule (gleiche Pixel, keine neue Textur): Slot Hull auf M_Ext_HabModule legen. Platzierung: docs/station/wohnmodul.md.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [docs/station/wohnmodul_plan.png]
placeholder: none (direkt geliefert)
feedback: []
```

### SM_Bar_WishBoard.fbx
```yaml
path: Assets/_Project/Art/Models/Bar/SM_Bar_WishBoard.fbx
package: P65
scene: Halle, neben der Bar-Tür (x 7,75, z 4,6, Blick in die Halle)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.9, 0.06, 0.6]
pivot: center
tris_max: 300
materials: [Board, Frame]
textures:
  - Assets/_Project/Art/Textures/Bar/T_Bar_WishBoard_Albedo.png   # 128x128, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Kleine Kreidetafel oder dunkles Leuchtschild der Bar "The Last Orbit" an der Wand neben der Bar-Tür. Die Schreibfläche
  (ca. 0,8 x 0,5 m, Vorderseite zeigt nach -Z) bleibt dunkel und leer: Der Text (was der Barkeeper heute haben will) kommt
  im Spiel als TextMeshPro darüber. Rahmen in Bar-Farben (Orange/Violett), etwas abgenutzt.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: Dunkler Quader 0.9 x 0.6 x 0.04 m, Pivot center (EconomyBuilder, P65)
feedback:
  - "2026-11-08 max: (P93) Wird nicht mehr verwendet: Das Schild vor der Bar ist entfallen, der Barkeeper nennt den Wunsch selbst. Nichts weiter zu tun."
```

### SM_Prop_BribeEnvelope.fbx
```yaml
path: Assets/_Project/Art/Models/Props/SM_Prop_BribeEnvelope.fbx
package: P66
scene: Schalter (Schreibtisch, links neben den Papieren)
status: final
priority: P3
needed_by: 2026-11-20
size_m: [0.18, 0.11, 0.015]
pivot: bottom_center
tris_max: 150
materials: [Envelope]
textures:
  - Assets/_Project/Art/Textures/Props/T_Prop_BribeEnvelope_Albedo.png   # 64x64, Point
texel_px_per_m: 128
palette: docs/style/palette.png
look: >
  Flacher, leicht zerknitterter Briefumschlag aus braunem Packpapier, Lasche offen, ein paar Credit-Scheine schauen
  heraus. Kleiner Bar-Stempel "THE LAST ORBIT" (Orange/Violett) in einer Ecke. Liegt flach auf dem Schalter-Tisch an
  Tagen mit Bestechung; der lesbare Zettel dazu bleibt ein eigenes Papier daneben (TextMeshPro).
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Keiner im Spiel bis zur Lieferung; der Bestechungszettel ist bis dahin nur Papier (P66)
feedback: []
```

### UI_Icon_Item_NovaCola.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png
package: P46
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  STILPROBE für die Waren-Icons (Max gibt den Stil frei, danach folgen die anderen Waren im selben Stil).
  Vorschlag: Dose "NOVA COLA" (wie SM_Prop_SodaCan) in leichter Dreiviertel-Ansicht, Pixel-Art mit ca. 2 px dunkler
  Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 % der Fläche, ein Glanzlicht.
  Muss klein (48 px) im Inventar lesbar sein.
interaction: "none"
variants: []
animations: []
collision: none
refs: []
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_RationPack.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_RationPack.png
package: P46
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_RationPack.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Ration pack" im Stil der freigegebenen Stilprobe UI_Icon_Item_NovaCola (Martin, 07.11.): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: graues, kompaktes Folienpaket mit Aufdruck-Streifen (Militärration), leicht zerknittert.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_ProteinBar.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_ProteinBar.png
package: P46
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_ProteinBar.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Protein bar" im Stil der freigegebenen Stilprobe UI_Icon_Item_NovaCola (Martin, 07.11.): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: Riegel in schlichter Folie, ein Ende aufgerissen, heller Riegel schaut heraus.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_CannedStew.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_CannedStew.png
package: P46
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_CannedStew.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Canned stew" im Stil der freigegebenen Stilprobe UI_Icon_Item_NovaCola (Martin, 07.11.): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: Konservendose mit Papieretikett (dampfende Schüssel als Bild), Deckel mit Ring.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_FreshFruit.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_FreshFruit.png
package: P46
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_FreshFruit.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Fresh fruit" im Stil der freigegebenen Stilprobe UI_Icon_Item_NovaCola (Martin, 07.11.): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: zwei bis drei fremdartige, leuchtend frische Früchte (Hydro-Deck), grünes Blatt daran.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_BatteryPack.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_BatteryPack.png
package: P46
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_BatteryPack.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Battery pack" im Stil der freigegebenen Stilprobe UI_Icon_Item_NovaCola (Martin, 07.11.): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: klobiger Akkublock mit Tragegriff und Ladeanzeige (drei grüne Balken).
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_FusionCell.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_FusionCell.png
package: P46
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_FusionCell.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Fusion cell" im Stil der freigegebenen Stilprobe UI_Icon_Item_NovaCola (Martin, 07.11.): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: zylindrische Energiezelle mit Glasfenster, kühles Leuchten innen, Metallkappen oben und unten.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_SpareFuse.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_SpareFuse.png
package: P46
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_SpareFuse.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Spare fuse" im Stil der freigegebenen Stilprobe UI_Icon_Item_NovaCola (Martin, 07.11.): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: Stations-Sicherung: Glasröhre mit Metallkappen und Glühdraht, Warn-Aufkleber.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_CoughSyrup.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_CoughSyrup.png
package: P46
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_CoughSyrup.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Cough syrup" im Stil der freigegebenen Stilprobe UI_Icon_Item_NovaCola (Martin, 07.11.): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: kleine braune Flasche mit Schraubdeckel und Messbecher, klebriger Tropfen am Rand.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_Painkillers.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_Painkillers.png
package: P46
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_Painkillers.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Painkillers" im Stil der freigegebenen Stilprobe UI_Icon_Item_NovaCola (Martin, 07.11.): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: Blisterpackung mit weißen Tabletten, ein Fach leer.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_Antibiotics.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_Antibiotics.png
package: P46
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_Antibiotics.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Antibiotics" im Stil der freigegebenen Stilprobe UI_Icon_Item_NovaCola (Martin, 07.11.): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: Militär-Medizinschachtel mit Kreuz-Symbol und Schablonen-Nummer, Ampullen darin.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_Spice.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_Spice.png
package: P46
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_Spice.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Spice" im Stil der freigegebenen Stilprobe UI_Icon_Item_NovaCola (Martin, 07.11.): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: kleines verschnürtes Tütchen mit orange-rotem Pulver, ein paar Krümel daneben (verboten, aber verlockend).
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### SM_Hangar_TicketTerminal.fbx
*(P72: steht im Hangar westlich der Shuttle-Rampe bei X -4,5, Z -21, Vorderseite zur Rampe. Bis zur Lieferung setzt `Build Departure into Main` eine graue Box.)*
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_TicketTerminal.fbx
package: P72
scene: Hangar
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.8, 0.6, 1.8]
pivot: bottom_center
tris_max: 600
materials: [Terminal]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_TicketTerminal_Albedo.png   # 128x256, Point
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_TicketTerminal_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Ticketautomat für den Flug nach Hause: schmale, stehende Säule im Stil der Station (beige Kunststoff, Metallkanten, abgestoßene Ecken),
  oben ein kleiner CRT-Bildschirm mit grüner Schrift "TICKETS - COLONIES" (Emission), darunter Kartenschlitz und Ticket-Ausgabe,
  ein verblasster Aufkleber mit Shuttle-Silhouette. Vorderseite (Bildschirm) zeigt nach +Z. Gleiche Machart wie SM_Hall_VendingMachine.
interaction: none
variants: []
animations: []
collision: prefab
refs: [docs/style/refs/ai_lounge_terminal_01.webp, docs/style/refs/ai_counter_terminal_01.webp]
placeholder: Graue Box 0.8 x 0.6 x 1.8 m vom DepartureBuilder, Pivot bottom_center, Material-Slots [Terminal]
feedback: []
```

### SFX_Shuttle_Engine_Loop.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Shuttle_Engine_Loop.ogg
package: P72
scene: Hangar / Flug nach Hause
status: final
priority: P3
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Triebwerk eines kleinen, alten Shuttles im Flug: tiefes, gleichmäßiges Brummen mit leichtem Rauschen, kein Startknall.
  Nahtlos loopend, 5-15 s. Die Tonhöhe wird im Spiel mit der Geschwindigkeit verändert. Freier Sound (CC0 bevorzugt,
  sonst CC-BY mit Eintrag in CREDITS.md), umgewandelt nach OGG, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, der Flug ist bisher stumm. Der Builder hängt den Ton ein, sobald die Datei da ist.
feedback: []
```

### UI_Icon_Item_Pistol.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_Pistol.png
package: P82
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_Pistol.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Pistol" im Stil der freigegebenen Waren-Icons (UI_Icon_Item_NovaCola und die elf aus P46): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: kleine, alte Pistole, abgegriffenes Griffstück, leicht schräg.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_DataChip.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_DataChip.png
package: P82
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_DataChip.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Data chip" im Stil der freigegebenen Waren-Icons (UI_Icon_Item_NovaCola und die elf aus P46): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: Datenchip wie SM_Item_DataChip: flaches Modul mit Kontakten und kleiner Status-LED.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_PocketWatch.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_PocketWatch.png
package: P82
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_PocketWatch.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Pocket watch" im Stil der freigegebenen Waren-Icons (UI_Icon_Item_NovaCola und die elf aus P46): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: alte Taschenuhr mit Deckel offen und Kette, Messing.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_SilverRing.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_SilverRing.png
package: P82
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_SilverRing.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Silver ring" im Stil der freigegebenen Waren-Icons (UI_Icon_Item_NovaCola und die elf aus P46): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: schlichter Silberring, Gravur innen angedeutet, ein Glanzpunkt.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_AlienTrinket.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_AlienTrinket.png
package: P82
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_AlienTrinket.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Alien trinket" im Stil der freigegebenen Waren-Icons (UI_Icon_Item_NovaCola und die elf aus P46): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: kleines fremdartiges Schmuckstück, leicht leuchtend, organische Form.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_StimulantVials.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_StimulantVials.png
package: P82
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_StimulantVials.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Stimulant vials" im Stil der freigegebenen Waren-Icons (UI_Icon_Item_NovaCola und die elf aus P46): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: drei kleine Glasampullen mit grellgrüner Flüssigkeit in einem Halter (wie Fracht „CHEM STIMULANT VIALS“).
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_DocumentScanner.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_DocumentScanner.png
package: P82
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_DocumentScanner.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Document scanner" im Stil der freigegebenen Waren-Icons (UI_Icon_Item_NovaCola und die elf aus P46): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: Handscanner wie SM_Item_DocScanner: Griff, kleines Display, Scanfenster.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_Liquor.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_Liquor.png
package: P82
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_Liquor.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Liquor" im Stil der freigegebenen Waren-Icons (UI_Icon_Item_NovaCola und die elf aus P46): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: Flasche wie SM_Item_Liquor: selbstgebrannter Schnaps, handgeschriebenes Etikett.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_WeaponParts.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_WeaponParts.png
package: P82
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_WeaponParts.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Weapon parts" im Stil der freigegebenen Waren-Icons (UI_Icon_Item_NovaCola und die elf aus P46): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: Waffenteile wie SM_Item_WeaponParts: Lauf, Feder, Abzugsgruppe, lose zusammengelegt.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_CashBundle.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_CashBundle.png
package: P82
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_CashBundle.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Cash bundle" im Stil der freigegebenen Waren-Icons (UI_Icon_Item_NovaCola und die elf aus P46): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: Geldbündel wie SM_Item_CashBundle: alte Scheine mit Gummiband.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_AlienSpecimen.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_AlienSpecimen.png
package: P82
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_AlienSpecimen.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Alien specimen" im Stil der freigegebenen Waren-Icons (UI_Icon_Item_NovaCola und die elf aus P46): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: Glas mit etwas Fremdem darin wie SM_Item_AlienSpecimen, trübe Flüssigkeit.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### UI_Icon_Item_UVLamp.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_UVLamp.png
package: P82
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_UVLamp.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "UV lamp" im Stil der freigegebenen Waren-Icons (UI_Icon_Item_NovaCola und die elf aus P46): Pixel-Art, leichte
  Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 %
  der Fläche, ein Glanzlicht, klein (48 px) im Inventar lesbar. Motiv: UV-Handlampe wie SM_Item_UVLamp, violetter Lichtkegel angedeutet.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### SM_Kiosk_Shutter.fbx
```yaml
path: Assets/_Project/Art/Models/Kiosk/SM_Kiosk_Shutter.fbx
package: P90
scene: Kiosk
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.08, 2.6, 1.3]
pivot: top_center
tris_max: 200
materials: [Shutter]
textures:
  - Assets/_Project/Art/Textures/Kiosk/T_Kiosk_Shutter_Albedo.png   # 128x128, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Rollgitter für die Kiosk-Durchreiche als eigenes Teil: füllt die ganze Öffnung über der Theke von Blende bis Tresen, Metalllamellen mit Rost wie am bisherigen Gitter von SM_Kiosk_Counter. Muss zu SM_Kiosk_Counter passen.
interaction: "wird per Skript ganz geöffnet (nach oben eingefahren) oder ganz geschlossen"
variants: []
animations: []
collision: prefab
refs: [Assets/_Project/Art/Models/Kiosk/SM_Kiosk_Counter.fbx]
placeholder: Box 0.08 x 2.6 x 1.3 m, Pivot top_center, Material-Slot [Shutter]
feedback: []
```

### favicon.png
```yaml
path: Assets/WebGLTemplates/Kestrel9/favicon.png
package: P49
scene: Browser-Tab (Web-Vorlage)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/WebGLTemplates/Kestrel9/favicon.png   # 64x64, transparent
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Favicon für den Browser-Tab von Kestrel-9: Pixel-Art, Farben aus der Palette, transparenter Hintergrund. Motiv: stilisierte Raumstation oder Kestrel-Silhouette
  mit einer "9", klare Form mit kräftiger Kontur. Muss bei 16x16 Pixeln noch erkennbar sein, also wenige große Formen und keine feine Schrift.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: PNG 64x64, Magenta/Grau-Raster
feedback: []
```

### SM_Counter_SecurityShutter.fbx
```yaml
path: Assets/_Project/Art/Models/Counter/SM_Counter_SecurityShutter.fbx
package: P91
scene: Schalter
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0.08, 4.4, 2.2]
pivot: top_center
tris_max: 300
materials: [Shutter]
textures:
  - Assets/_Project/Art/Textures/Counter/T_Counter_SecurityShutter_Albedo.png   # 256x128, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Schweres Stahl-Sichtgitter vor der Schalterscheibe, breite Lamellen ohne Durchblick, abgenutzt, gelb-schwarzer Warnstreifen an der Unterkante. Fällt bei Verhaftungen und bei der Schleuse herab und verdeckt die Prüfkammer ganz.
interaction: "fällt per Skript von oben herab und fährt wieder hoch"
variants: []
animations: []
collision: none
refs: []
placeholder: 12 graue Lamellen aus dem Builder, 4.4 x 2.2 m
feedback: []
```

### SFX_Arrest_Scuffle.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Arrest_Scuffle.ogg
package: P91
scene: Counter
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Hinter einem Stahlgitter gedämpft: schwere Stiefel, kurzes Gerangel, Handschellen klicken, jemand wird weggezerrt, ca. 3 s. Keine verständlichen Wörter. Freier Sound (CC0 bevorzugt, sonst CC-BY mit Eintrag in CREDITS.md) oder synthetisch.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Bis zur Lieferung ein vorhandener Ton.
feedback: []
```

### SFX_Shutter_Drop.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Shutter_Drop.ogg
package: P91
scene: Counter
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Schweres Stahlgitter fällt und schlägt hart auf, ca. 0,5 s.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Bis zur Lieferung ein vorhandener Ton.
feedback: []
```

### SFX_Shutter_Rise.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Shutter_Rise.ogg
package: P91
scene: Counter
status: final
priority: P2
needed_by: 2026-11-16
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Schweres Stahlgitter fährt ratternd nach oben, ca. 1 s.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Bis zur Lieferung ein vorhandener Ton.
feedback: []
```

### SK_NPC_Guard.fbx
```yaml
path: Assets/_Project/Art/Models/Characters/SK_NPC_Guard.fbx
package: P104
scene: Prüfkammer
status: final
priority: P1
needed_by: 2026-11-20
size_m: [0.6, 0.4, 1.85]
pivot: bottom_center
tris_max: 3000
materials: [Body]
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Guard_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Wachmann der Stationssicherheit, gleiche Bauart und gleiches Skelett wie die SK_NPC_*-Reisenden (Humanoid-Rig): dunkle Uniform, Helm mit Visier, Schlagstock am Gürtel, Handschellen. Wirkt massig und gesichtslos.
interaction: "kommt in die Prüfkammer und packt den Reisenden"
variants: []
animations: [Idle, Walk, Grab]
collision: prefab
refs: [Assets/_Project/Art/Models/Characters/SK_NPC_Human.fbx]
placeholder: Grauer Humanoid wie SK_NPC_Human
feedback: []
```

### SM_Ext_Shuttle_Docked.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_Shuttle_Docked.fbx
package: P106
scene: Außen, Hangar
status: final
priority: P1
needed_by: 2026-11-20
size_m: [6.0, 11.0, 4.0]
pivot: center
tris_max: 4000
materials: [Hull, Detail]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_Shuttle_Docked_Albedo.png   # 512x512, Point
  - Assets/_Project/Art/Textures/Exterior/T_Ext_Shuttle_Docked_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Das Personal-Shuttle von außen, an der Station angedockt, gleiche Bauart wie SM_Hangar_Shuttle: klobig, weißer Lack mit Rost, Seriennummer, Positionslichter, Fensterreihe. An einer Längsseite eine Luke für den Andockring.
interaction: "hängt außen am Hangar, von einem Fenster aus sichtbar"
variants: []
animations: []
collision: prefab
refs: [Assets/_Project/Art/Models/Hangar/SM_Hangar_Shuttle.fbx]
placeholder: Box 6 x 11 x 4 m
feedback: []
```

### SM_Ext_ShuttleDockingCollar.fbx
```yaml
path: Assets/_Project/Art/Models/Exterior/SM_Ext_ShuttleDockingCollar.fbx
package: P106
scene: Außen, Hangar
status: final
priority: P2
needed_by: 2026-11-20
size_m: [3.0, 3.0, 2.5]
pivot: center
tris_max: 800
materials: [Collar]
textures:
  - Assets/_Project/Art/Textures/Exterior/T_Ext_ShuttleDockingCollar_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Andockring und kurze Röhre zwischen Hangarwand und Shuttle-Luke: Metallsegmente, Warnstreifen, Klammern, Kabel.
interaction: "verbindet Station und Shuttle"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Röhre 3 x 3 x 2.5 m
feedback: []
```

### SM_Shuttle_Cabin_Interior.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Shuttle_Cabin_Interior.fbx
package: P106
scene: Shuttle innen
status: final
priority: P1
needed_by: 2026-11-20
size_m: [3.2, 8.0, 2.5]
pivot: bottom_center
tris_max: 3000
materials: [Cabin, Detail]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Shuttle_Cabin_Albedo.png   # 512x512, Point
  - Assets/_Project/Art/Textures/Hangar/T_Shuttle_Cabin_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Begehbare Kabine des Personal-Shuttles: schmaler Mittelgang, Sitzreihen links und rechts, kleine Fenster, Gepäckfächer, Warnschilder, vorne ein Sitz fürs Abheben und ein Panel. Innenansicht, Wände von innen geschlossen, abgenutzt.
interaction: "begehbar, Sitz vorne startet den Abflug"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box innen 3.2 x 8 x 2.5 m
feedback: []
```

### SM_Hangar_Wall_Viewport.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_Wall_Viewport.fbx
package: P106
scene: Hangar
status: final
priority: P2
needed_by: 2026-11-20
size_m: [4.0, 0.3, 4.0]
pivot: bottom_center
tris_max: 800
materials: [Wall, Glass]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_Wall_Viewport_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Hangarwand-Modul 4 x 4 m wie SM_Hangar_Wall_4x4, mit großem Sichtfenster in einer kleinen Nische und Haltestange. Daneben kann das Schott zum Shuttle sitzen.
interaction: "Blick von innen auf das angedockte Shuttle"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Wandmodul mit Glas
feedback: []
```

### SM_Hangar_Ship_Courier.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_Ship_Courier.fbx
package: P107
scene: Hangar
status: final
priority: P2
needed_by: 2026-11-20
size_m: [5.0, 8.0, 3.0]
pivot: bottom_center
tris_max: 2000
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_Ship_Courier_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Kleines schnelles Kurierschiff auf Landekufen, schlank, grell lackiert, zerkratzt. Passt in die Reihe von SM_Hangar_Shuttle.
interaction: "steht auf einem Landeplatz, landet morgens"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 5 x 8 x 3 m
feedback: []
```

### SM_Hangar_Ship_Hauler.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_Ship_Hauler.fbx
package: P107
scene: Hangar
status: final
priority: P2
needed_by: 2026-11-20
size_m: [5.0, 8.0, 3.0]
pivot: bottom_center
tris_max: 2000
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_Ship_Hauler_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Kleiner kastiger Frachter auf Landebeinen, Ladeluke hinten, rostig, Frachtbehälter außen verzurrt. Passt in die Reihe von SM_Hangar_Shuttle.
interaction: "steht auf einem Landeplatz, landet morgens"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 5 x 8 x 3 m
feedback: []
```

### SM_Hangar_Ship_Skiff.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_Ship_Skiff.fbx
package: P107
scene: Hangar
status: final
priority: P2
needed_by: 2026-11-20
size_m: [5.0, 8.0, 3.0]
pivot: bottom_center
tris_max: 2000
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_Ship_Skiff_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Einfaches offenes Beiboot, zwei Sitze, aufgeflickte Platten, passt zu Namen wie SKIFF PELL. Passt in die Reihe von SM_Hangar_Shuttle.
interaction: "steht auf einem Landeplatz, landet morgens"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 5 x 8 x 3 m
feedback: []
```

### SM_Hangar_Ship_Family.fbx
```yaml
path: Assets/_Project/Art/Models/Hangar/SM_Hangar_Ship_Family.fbx
package: P107
scene: Hangar
status: final
priority: P2
needed_by: 2026-11-20
size_m: [5.0, 8.0, 3.0]
pivot: bottom_center
tris_max: 2000
materials: [Hull]
textures:
  - Assets/_Project/Art/Textures/Hangar/T_Hangar_Ship_Family_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Altes rundliches Familienschiff, Wohnwagen-Charakter, Vorhänge an den Fenstern, Quobb- oder Xirrik-Beschriftung. Passt in die Reihe von SM_Hangar_Shuttle.
interaction: "steht auf einem Landeplatz, landet morgens"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 5 x 8 x 3 m
feedback: []
```

### SFX_Guard_Boots.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Guard_Boots.ogg
package: P104
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Schwere Stiefel auf Metall, ein paar entschlossene Schritte, ca. 1,5 s.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, bis die Lieferung da ist.
feedback: []
```

### SFX_Airlock_Alarm.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Airlock_Alarm.ogg
package: P105
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Schriller, kurzer Alarm vor dem Öffnen des Außenschotts, ca. 1 s.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, bis die Lieferung da ist.
feedback: []
```

### SFX_Airlock_Decompress.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Airlock_Decompress.ogg
package: P105
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Gewaltiger Luftsog ins All, dumpfes Ausrauschen, dann Stille, ca. 3 s.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, bis die Lieferung da ist.
feedback: []
```

### SFX_Ship_Land.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Ship_Land.ogg
package: P107
scene: Hangar
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Kleines Schiff setzt mit Triebwerksfauchen und Fahrwerks-Schlag auf, gedämpft aus der Ferne, ca. 3 s.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, bis die Lieferung da ist.
feedback: []
```

### SFX_Telescope_Coin.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Telescope_Coin.ogg
package: P29
scene: Plattform
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Münze fällt klackernd in ein altes Münzfernrohr, Mechanik klickt auf, ca. 0,8 s.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, bis die Lieferung da ist.
feedback: []
```

### T_NPC_Human_Albedo_V1.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Human_Albedo_V1.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Human_Albedo_V1.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 1 von T_NPC_Human_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Human passt: dunkle Haut, graues Haar, Mechaniker-Overall in Oliv. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Human_Albedo.png]
placeholder: Kopie von T_NPC_Human_Albedo
feedback: []
```

### T_NPC_Human_Albedo_V2.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Human_Albedo_V2.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Human_Albedo_V2.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 2 von T_NPC_Human_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Human passt: helle Haut, rotes Haar, Reisemantel in Dunkelblau. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Human_Albedo.png]
placeholder: Kopie von T_NPC_Human_Albedo
feedback: []
```

### T_NPC_Human_Albedo_V3.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Human_Albedo_V3.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Human_Albedo_V3.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 3 von T_NPC_Human_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Human passt: mittlere Haut, schwarzes Haar, Arbeitsjacke in Senf. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Human_Albedo.png]
placeholder: Kopie von T_NPC_Human_Albedo
feedback: []
```

### T_NPC_Human_Albedo_V4.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Human_Albedo_V4.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Human_Albedo_V4.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 4 von T_NPC_Human_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Human passt: gebräunte Haut, blondes Haar, abgetragener Anzug in Braun. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Human_Albedo.png]
placeholder: Kopie von T_NPC_Human_Albedo
feedback: []
```

### T_NPC_Human_Albedo_V5.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Human_Albedo_V5.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Human_Albedo_V5.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 5 von T_NPC_Human_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Human passt: blasse Haut, weißes Haar, Pullover in Dunkelrot. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Human_Albedo.png]
placeholder: Kopie von T_NPC_Human_Albedo
feedback: []
```

### T_NPC_Vellani_Albedo_V1.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Vellani_Albedo_V1.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Vellani_Albedo_V1.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 1 von T_NPC_Vellani_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Vellani passt: blaugrau mit dunklem Kamm, Kleidung Ocker. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Vellani_Albedo.png]
placeholder: Kopie von T_NPC_Vellani_Albedo
feedback: []
```

### T_NPC_Vellani_Albedo_V2.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Vellani_Albedo_V2.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Vellani_Albedo_V2.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 2 von T_NPC_Vellani_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Vellani passt: violett, heller Kamm, Kleidung Dunkelgrün. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Vellani_Albedo.png]
placeholder: Kopie von T_NPC_Vellani_Albedo
feedback: []
```

### T_NPC_Vellani_Albedo_V3.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Vellani_Albedo_V3.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Vellani_Albedo_V3.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 3 von T_NPC_Vellani_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Vellani passt: türkis, Kleidung Rost. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Vellani_Albedo.png]
placeholder: Kopie von T_NPC_Vellani_Albedo
feedback: []
```

### T_NPC_Vellani_Albedo_V4.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Vellani_Albedo_V4.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Vellani_Albedo_V4.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 4 von T_NPC_Vellani_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Vellani passt: grau mit rötlichen Flecken, Kleidung Schwarz. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Vellani_Albedo.png]
placeholder: Kopie von T_NPC_Vellani_Albedo
feedback: []
```

### T_NPC_Vellani_Albedo_V5.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Vellani_Albedo_V5.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Vellani_Albedo_V5.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 5 von T_NPC_Vellani_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Vellani passt: hellblau, Kleidung Senf. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Vellani_Albedo.png]
placeholder: Kopie von T_NPC_Vellani_Albedo
feedback: []
```

### T_NPC_Quobb_Albedo_V1.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Quobb_Albedo_V1.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Quobb_Albedo_V1.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 1 von T_NPC_Quobb_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Quobb passt: moosgrün, Kleidung Violett. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Quobb_Albedo.png]
placeholder: Kopie von T_NPC_Quobb_Albedo
feedback: []
```

### T_NPC_Quobb_Albedo_V2.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Quobb_Albedo_V2.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Quobb_Albedo_V2.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 2 von T_NPC_Quobb_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Quobb passt: orange, Kleidung Teal. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Quobb_Albedo.png]
placeholder: Kopie von T_NPC_Quobb_Albedo
feedback: []
```

### T_NPC_Quobb_Albedo_V3.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Quobb_Albedo_V3.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Quobb_Albedo_V3.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 3 von T_NPC_Quobb_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Quobb passt: lila gefleckt, Kleidung Ocker. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Quobb_Albedo.png]
placeholder: Kopie von T_NPC_Quobb_Albedo
feedback: []
```

### T_NPC_Quobb_Albedo_V4.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Quobb_Albedo_V4.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Quobb_Albedo_V4.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 4 von T_NPC_Quobb_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Quobb passt: blass-rosa, Kleidung Dunkelblau. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Quobb_Albedo.png]
placeholder: Kopie von T_NPC_Quobb_Albedo
feedback: []
```

### T_NPC_Quobb_Albedo_V5.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Quobb_Albedo_V5.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Quobb_Albedo_V5.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 5 von T_NPC_Quobb_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Quobb passt: dunkelbraun, Kleidung Rot. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Quobb_Albedo.png]
placeholder: Kopie von T_NPC_Quobb_Albedo
feedback: []
```

### T_NPC_Sorvak_Albedo_V1.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Sorvak_Albedo_V1.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Sorvak_Albedo_V1.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 1 von T_NPC_Sorvak_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Sorvak passt: dunkelgrün, Rüstung Rost. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Sorvak_Albedo.png]
placeholder: Kopie von T_NPC_Sorvak_Albedo
feedback: []
```

### T_NPC_Sorvak_Albedo_V2.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Sorvak_Albedo_V2.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Sorvak_Albedo_V2.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 2 von T_NPC_Sorvak_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Sorvak passt: sandfarben, Rüstung Dunkelgrau. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Sorvak_Albedo.png]
placeholder: Kopie von T_NPC_Sorvak_Albedo
feedback: []
```

### T_NPC_Sorvak_Albedo_V3.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Sorvak_Albedo_V3.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Sorvak_Albedo_V3.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 3 von T_NPC_Sorvak_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Sorvak passt: rotbraun, Rüstung Schwarz. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Sorvak_Albedo.png]
placeholder: Kopie von T_NPC_Sorvak_Albedo
feedback: []
```

### T_NPC_Sorvak_Albedo_V4.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Sorvak_Albedo_V4.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Sorvak_Albedo_V4.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 4 von T_NPC_Sorvak_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Sorvak passt: graublau, Rüstung Bronze. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Sorvak_Albedo.png]
placeholder: Kopie von T_NPC_Sorvak_Albedo
feedback: []
```

### T_NPC_Sorvak_Albedo_V5.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Sorvak_Albedo_V5.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Sorvak_Albedo_V5.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 5 von T_NPC_Sorvak_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Sorvak passt: fast schwarz, Rüstung Olive. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Sorvak_Albedo.png]
placeholder: Kopie von T_NPC_Sorvak_Albedo
feedback: []
```

### T_NPC_Xirrik_Albedo_V1.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Xirrik_Albedo_V1.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Xirrik_Albedo_V1.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 1 von T_NPC_Xirrik_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Xirrik passt: goldgelb, Panzer dunkel. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Xirrik_Albedo.png]
placeholder: Kopie von T_NPC_Xirrik_Albedo
feedback: []
```

### T_NPC_Xirrik_Albedo_V2.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Xirrik_Albedo_V2.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Xirrik_Albedo_V2.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 2 von T_NPC_Xirrik_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Xirrik passt: tiefblau, Panzer Türkis. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Xirrik_Albedo.png]
placeholder: Kopie von T_NPC_Xirrik_Albedo
feedback: []
```

### T_NPC_Xirrik_Albedo_V3.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Xirrik_Albedo_V3.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Xirrik_Albedo_V3.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 3 von T_NPC_Xirrik_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Xirrik passt: grün-schwarz gestreift. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Xirrik_Albedo.png]
placeholder: Kopie von T_NPC_Xirrik_Albedo
feedback: []
```

### T_NPC_Xirrik_Albedo_V4.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Xirrik_Albedo_V4.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Xirrik_Albedo_V4.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 4 von T_NPC_Xirrik_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Xirrik passt: blutrot, Panzer Grau. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Xirrik_Albedo.png]
placeholder: Kopie von T_NPC_Xirrik_Albedo
feedback: []
```

### T_NPC_Xirrik_Albedo_V5.png
```yaml
path: Assets/_Project/Art/Textures/Characters/T_NPC_Xirrik_Albedo_V5.png
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Xirrik_Albedo_V5.png   # 128x128, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Variante 5 von T_NPC_Xirrik_Albedo mit exakt gleichem UV-Layout, damit sie auf SK_NPC_Xirrik passt: beige, Panzer Violett. Nur Farben ändern, gleiches Pixel-Niveau wie das Original.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Textures/Characters/T_NPC_Xirrik_Albedo.png]
placeholder: Kopie von T_NPC_Xirrik_Albedo
feedback: []
```

### SM_NPC_Hair_Short.fbx
```yaml
path: Assets/_Project/Art/Models/Characters/SM_NPC_Hair_Short.fbx
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0.28, 0.3, 0.25]
pivot: bottom_center
tris_max: 300
materials: [Hair]
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Hair_Albedo.png   # 64x64, Point
texel_px_per_m: 128
palette: docs/style/palette.png
look: >
  Kurze Haare für den Kopf von SK_NPC_Human, sitzt aufgesteckt am Kopfknochen und passt auf den vorhandenen Kopf, ohne ihn zu durchstoßen. Textur möglichst neutral, die Haarfarbe wird im Spiel eingefärbt.
interaction: "wird am Kopfknochen befestigt"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Models/Characters/SK_NPC_Human.fbx]
placeholder: Keiner, bis zur Lieferung ohne Frisur.
feedback: []
```

### SM_NPC_Hair_Long.fbx
```yaml
path: Assets/_Project/Art/Models/Characters/SM_NPC_Hair_Long.fbx
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0.28, 0.3, 0.25]
pivot: bottom_center
tris_max: 300
materials: [Hair]
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Hair_Albedo.png   # 64x64, Point
texel_px_per_m: 128
palette: docs/style/palette.png
look: >
  Lange Haare bis zur Schulter für den Kopf von SK_NPC_Human, sitzt aufgesteckt am Kopfknochen und passt auf den vorhandenen Kopf, ohne ihn zu durchstoßen. Textur möglichst neutral, die Haarfarbe wird im Spiel eingefärbt.
interaction: "wird am Kopfknochen befestigt"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Models/Characters/SK_NPC_Human.fbx]
placeholder: Keiner, bis zur Lieferung ohne Frisur.
feedback: []
```

### SM_NPC_Hair_Bun.fbx
```yaml
path: Assets/_Project/Art/Models/Characters/SM_NPC_Hair_Bun.fbx
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0.28, 0.3, 0.25]
pivot: bottom_center
tris_max: 300
materials: [Hair]
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Hair_Albedo.png   # 64x64, Point
texel_px_per_m: 128
palette: docs/style/palette.png
look: >
  Haarknoten für den Kopf von SK_NPC_Human, sitzt aufgesteckt am Kopfknochen und passt auf den vorhandenen Kopf, ohne ihn zu durchstoßen. Textur möglichst neutral, die Haarfarbe wird im Spiel eingefärbt.
interaction: "wird am Kopfknochen befestigt"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Models/Characters/SK_NPC_Human.fbx]
placeholder: Keiner, bis zur Lieferung ohne Frisur.
feedback: []
```

### SM_NPC_Hat_Cap.fbx
```yaml
path: Assets/_Project/Art/Models/Characters/SM_NPC_Hat_Cap.fbx
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0.28, 0.3, 0.25]
pivot: bottom_center
tris_max: 300
materials: [Hair]
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Hair_Albedo.png   # 64x64, Point
texel_px_per_m: 128
palette: docs/style/palette.png
look: >
  Abgewetzte Schirmmütze für den Kopf von SK_NPC_Human, sitzt aufgesteckt am Kopfknochen und passt auf den vorhandenen Kopf, ohne ihn zu durchstoßen. Textur möglichst neutral, die Haarfarbe wird im Spiel eingefärbt.
interaction: "wird am Kopfknochen befestigt"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Models/Characters/SK_NPC_Human.fbx]
placeholder: Keiner, bis zur Lieferung ohne Frisur.
feedback: []
```

### SM_NPC_Hat_Beanie.fbx
```yaml
path: Assets/_Project/Art/Models/Characters/SM_NPC_Hat_Beanie.fbx
package: P108
scene: Prüfkammer
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0.28, 0.3, 0.25]
pivot: bottom_center
tris_max: 300
materials: [Hair]
textures:
  - Assets/_Project/Art/Textures/Characters/T_NPC_Hair_Albedo.png   # 64x64, Point
texel_px_per_m: 128
palette: docs/style/palette.png
look: >
  Wollmütze für den Kopf von SK_NPC_Human, sitzt aufgesteckt am Kopfknochen und passt auf den vorhandenen Kopf, ohne ihn zu durchstoßen. Textur möglichst neutral, die Haarfarbe wird im Spiel eingefärbt.
interaction: "wird am Kopfknochen befestigt"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/Models/Characters/SK_NPC_Human.fbx]
placeholder: Keiner, bis zur Lieferung ohne Frisur.
feedback: []
```

### UI_Terminal_Panel.png
```yaml
path: Assets/_Project/Art/UI/Terminal/UI_Terminal_Panel.png
package: P95
scene: UI (alle Menüs und Fenster)
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Terminal/UI_Terminal_Panel.png   # 96x96, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Rahmen für Fenster als 9-Slice: Metallkante mit Nieten, innen dunkel, die Ecken dürfen beim Strecken nicht verzerren (mindestens 24 px Rand). Terminal-Stil der Station: dunkles Metall, CRT-Grün und Bernstein, Pixel-Art, Farben aus der Palette.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/STYLE.md]
placeholder: Bis zur Lieferung einfache Farbflächen.
feedback: []
```

### UI_Terminal_PanelInset.png
```yaml
path: Assets/_Project/Art/UI/Terminal/UI_Terminal_PanelInset.png
package: P95
scene: UI (alle Menüs und Fenster)
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Terminal/UI_Terminal_PanelInset.png   # 64x64, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Vertieftes Feld für Listen als 9-Slice, dunkler als der Rahmen, feine Innenlinie. Terminal-Stil der Station: dunkles Metall, CRT-Grün und Bernstein, Pixel-Art, Farben aus der Palette.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/STYLE.md]
placeholder: Bis zur Lieferung einfache Farbflächen.
feedback: []
```

### UI_Terminal_Button_Normal.png
```yaml
path: Assets/_Project/Art/UI/Terminal/UI_Terminal_Button_Normal.png
package: P95
scene: UI (alle Menüs und Fenster)
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Terminal/UI_Terminal_Button_Normal.png   # 96x32, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Knopf im Ruhezustand als 9-Slice, Metall mit bernsteinfarbener Kante. Terminal-Stil der Station: dunkles Metall, CRT-Grün und Bernstein, Pixel-Art, Farben aus der Palette.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/STYLE.md]
placeholder: Bis zur Lieferung einfache Farbflächen.
feedback: []
```

### UI_Terminal_Button_Hover.png
```yaml
path: Assets/_Project/Art/UI/Terminal/UI_Terminal_Button_Hover.png
package: P95
scene: UI (alle Menüs und Fenster)
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Terminal/UI_Terminal_Button_Hover.png   # 96x32, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Derselbe Knopf, Maus darüber: Kante leuchtet CRT-Grün. Terminal-Stil der Station: dunkles Metall, CRT-Grün und Bernstein, Pixel-Art, Farben aus der Palette.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/STYLE.md]
placeholder: Bis zur Lieferung einfache Farbflächen.
feedback: []
```

### UI_Terminal_Button_Pressed.png
```yaml
path: Assets/_Project/Art/UI/Terminal/UI_Terminal_Button_Pressed.png
package: P95
scene: UI (alle Menüs und Fenster)
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Terminal/UI_Terminal_Button_Pressed.png   # 96x32, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Derselbe Knopf gedrückt: eingedrückt, dunkler. Terminal-Stil der Station: dunkles Metall, CRT-Grün und Bernstein, Pixel-Art, Farben aus der Palette.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/STYLE.md]
placeholder: Bis zur Lieferung einfache Farbflächen.
feedback: []
```

### UI_Terminal_Divider.png
```yaml
path: Assets/_Project/Art/UI/Terminal/UI_Terminal_Divider.png
package: P95
scene: UI (alle Menüs und Fenster)
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Terminal/UI_Terminal_Divider.png   # 128x8, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Trennlinie, kachelbar in der Breite. Terminal-Stil der Station: dunkles Metall, CRT-Grün und Bernstein, Pixel-Art, Farben aus der Palette.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/STYLE.md]
placeholder: Bis zur Lieferung einfache Farbflächen.
feedback: []
```

### UI_Terminal_WarnStripe.png
```yaml
path: Assets/_Project/Art/UI/Terminal/UI_Terminal_WarnStripe.png
package: P95
scene: UI (alle Menüs und Fenster)
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Terminal/UI_Terminal_WarnStripe.png   # 64x16, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Gelb-schwarzer Warnstreifen, kachelbar in der Breite. Terminal-Stil der Station: dunkles Metall, CRT-Grün und Bernstein, Pixel-Art, Farben aus der Palette.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/STYLE.md]
placeholder: Bis zur Lieferung einfache Farbflächen.
feedback: []
```

### UI_Terminal_Scanlines.png
```yaml
path: Assets/_Project/Art/UI/Terminal/UI_Terminal_Scanlines.png
package: P95
scene: UI (alle Menüs und Fenster)
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Terminal/UI_Terminal_Scanlines.png   # 64x64, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Sehr dezente CRT-Zeilen als transparente Überlagerung, kachelbar. Terminal-Stil der Station: dunkles Metall, CRT-Grün und Bernstein, Pixel-Art, Farben aus der Palette.
interaction: "none"
variants: []
animations: []
collision: none
refs: [docs/STYLE.md]
placeholder: Bis zur Lieferung einfache Farbflächen.
feedback: []
```

### SFX_Paper_Pickup_01.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Paper_Pickup_01.ogg
package: P102
scene: Counter
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Blatt Papier wird leise vom Tisch genommen, Variante 1, sehr leise und weich, nicht raschelig-scharf, ca. 0,2 bis 0,3 s. Darf auch bei vielen Wiederholungen nicht nerven.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Paper_Pickup_02.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Paper_Pickup_02.ogg
package: P102
scene: Counter
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Blatt Papier wird leise vom Tisch genommen, Variante 2, sehr leise und weich, nicht raschelig-scharf, ca. 0,2 bis 0,3 s. Darf auch bei vielen Wiederholungen nicht nerven.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Paper_Pickup_03.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Paper_Pickup_03.ogg
package: P102
scene: Counter
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Blatt Papier wird leise vom Tisch genommen, Variante 3, sehr leise und weich, nicht raschelig-scharf, ca. 0,2 bis 0,3 s. Darf auch bei vielen Wiederholungen nicht nerven.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Paper_Pickup_04.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Paper_Pickup_04.ogg
package: P102
scene: Counter
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Blatt Papier wird leise vom Tisch genommen, Variante 4, sehr leise und weich, nicht raschelig-scharf, ca. 0,2 bis 0,3 s. Darf auch bei vielen Wiederholungen nicht nerven.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Paper_Slide_01.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Paper_Slide_01.ogg
package: P102
scene: Counter
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Blatt Papier wird sanft abgelegt, Variante 1, sehr leise und weich, nicht raschelig-scharf, ca. 0,2 bis 0,3 s. Darf auch bei vielen Wiederholungen nicht nerven.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Paper_Slide_02.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Paper_Slide_02.ogg
package: P102
scene: Counter
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Blatt Papier wird sanft abgelegt, Variante 2, sehr leise und weich, nicht raschelig-scharf, ca. 0,2 bis 0,3 s. Darf auch bei vielen Wiederholungen nicht nerven.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Paper_Slide_03.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Paper_Slide_03.ogg
package: P102
scene: Counter
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Blatt Papier wird sanft abgelegt, Variante 3, sehr leise und weich, nicht raschelig-scharf, ca. 0,2 bis 0,3 s. Darf auch bei vielen Wiederholungen nicht nerven.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Paper_Slide_04.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Paper_Slide_04.ogg
package: P102
scene: Counter
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Blatt Papier wird sanft abgelegt, Variante 4, sehr leise und weich, nicht raschelig-scharf, ca. 0,2 bis 0,3 s. Darf auch bei vielen Wiederholungen nicht nerven.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Human_01.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Human_01.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Menschliches unverständliches Murmeln, wie Gesprächsfetzen hinter einer Wand, Variante 1 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Human_02.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Human_02.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Menschliches unverständliches Murmeln, wie Gesprächsfetzen hinter einer Wand, Variante 2 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Human_03.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Human_03.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Menschliches unverständliches Murmeln, wie Gesprächsfetzen hinter einer Wand, Variante 3 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Human_04.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Human_04.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Menschliches unverständliches Murmeln, wie Gesprächsfetzen hinter einer Wand, Variante 4 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Human_05.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Human_05.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Menschliches unverständliches Murmeln, wie Gesprächsfetzen hinter einer Wand, Variante 5 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Human_06.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Human_06.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Menschliches unverständliches Murmeln, wie Gesprächsfetzen hinter einer Wand, Variante 6 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Human_07.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Human_07.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Menschliches unverständliches Murmeln, wie Gesprächsfetzen hinter einer Wand, Variante 7 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Human_08.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Human_08.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Menschliches unverständliches Murmeln, wie Gesprächsfetzen hinter einer Wand, Variante 8 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Human_09.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Human_09.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Menschliches unverständliches Murmeln, wie Gesprächsfetzen hinter einer Wand, Variante 9 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Human_10.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Human_10.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Menschliches unverständliches Murmeln, wie Gesprächsfetzen hinter einer Wand, Variante 10 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Vellani_01.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Vellani_01.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Weiches, fiepsendes Trällern, Variante 1 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Vellani_02.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Vellani_02.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Weiches, fiepsendes Trällern, Variante 2 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Vellani_03.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Vellani_03.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Weiches, fiepsendes Trällern, Variante 3 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Vellani_04.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Vellani_04.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Weiches, fiepsendes Trällern, Variante 4 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Vellani_05.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Vellani_05.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Weiches, fiepsendes Trällern, Variante 5 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Vellani_06.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Vellani_06.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Weiches, fiepsendes Trällern, Variante 6 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Vellani_07.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Vellani_07.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Weiches, fiepsendes Trällern, Variante 7 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Vellani_08.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Vellani_08.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Weiches, fiepsendes Trällern, Variante 8 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Vellani_09.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Vellani_09.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Weiches, fiepsendes Trällern, Variante 9 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Vellani_10.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Vellani_10.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Weiches, fiepsendes Trällern, Variante 10 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Quobb_01.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Quobb_01.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Gluckernd, blubbernd, freundlich, Variante 1 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Quobb_02.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Quobb_02.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Gluckernd, blubbernd, freundlich, Variante 2 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Quobb_03.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Quobb_03.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Gluckernd, blubbernd, freundlich, Variante 3 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Quobb_04.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Quobb_04.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Gluckernd, blubbernd, freundlich, Variante 4 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Quobb_05.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Quobb_05.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Gluckernd, blubbernd, freundlich, Variante 5 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Quobb_06.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Quobb_06.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Gluckernd, blubbernd, freundlich, Variante 6 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Quobb_07.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Quobb_07.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Gluckernd, blubbernd, freundlich, Variante 7 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Quobb_08.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Quobb_08.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Gluckernd, blubbernd, freundlich, Variante 8 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Quobb_09.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Quobb_09.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Gluckernd, blubbernd, freundlich, Variante 9 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Quobb_10.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Quobb_10.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Gluckernd, blubbernd, freundlich, Variante 10 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Sorvak_01.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Sorvak_01.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Tiefes, kehliges Grollen, Variante 1 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Sorvak_02.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Sorvak_02.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Tiefes, kehliges Grollen, Variante 2 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Sorvak_03.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Sorvak_03.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Tiefes, kehliges Grollen, Variante 3 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Sorvak_04.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Sorvak_04.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Tiefes, kehliges Grollen, Variante 4 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Sorvak_05.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Sorvak_05.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Tiefes, kehliges Grollen, Variante 5 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Sorvak_06.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Sorvak_06.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Tiefes, kehliges Grollen, Variante 6 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Sorvak_07.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Sorvak_07.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Tiefes, kehliges Grollen, Variante 7 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Sorvak_08.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Sorvak_08.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Tiefes, kehliges Grollen, Variante 8 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Sorvak_09.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Sorvak_09.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Tiefes, kehliges Grollen, Variante 9 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Sorvak_10.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Sorvak_10.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Tiefes, kehliges Grollen, Variante 10 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Xirrik_01.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Xirrik_01.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Trockenes Klicken und Zischen, Variante 1 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Xirrik_02.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Xirrik_02.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Trockenes Klicken und Zischen, Variante 2 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Xirrik_03.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Xirrik_03.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Trockenes Klicken und Zischen, Variante 3 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Xirrik_04.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Xirrik_04.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Trockenes Klicken und Zischen, Variante 4 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Xirrik_05.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Xirrik_05.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Trockenes Klicken und Zischen, Variante 5 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Xirrik_06.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Xirrik_06.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Trockenes Klicken und Zischen, Variante 6 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Xirrik_07.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Xirrik_07.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Trockenes Klicken und Zischen, Variante 7 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Xirrik_08.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Xirrik_08.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Trockenes Klicken und Zischen, Variante 8 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Xirrik_09.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Xirrik_09.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Trockenes Klicken und Zischen, Variante 9 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Voice_Xirrik_10.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Voice_Xirrik_10.ogg
package: P94
scene: Counter
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Trockenes Klicken und Zischen, Variante 10 von 10, ca. 0,4 bis 0,9 s. Synthetisch erzeugter Laut ohne verständliche Wörter und ohne echte Stimmaufnahme, wirkt wie Sprache im Vorbeigehen.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### UI_Icon_Item_ClownShoes.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_ClownShoes.png
package: P63
scene: UI (Inventar, Handelsfenster, Abrechnung)
status: final
priority: P3
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_ClownShoes.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Waren-Icon "Clown shoes" im Stil der freigegebenen Waren-Icons (UI_Icon_Item_NovaCola): Pixel-Art, leichte Dreiviertel-Ansicht, ca. 2 px dunkle Kontur, Farben aus der Palette, transparenter Hintergrund, Objekt füllt ca. 80 % der Fläche, klein (48 px) im Inventar lesbar. Motiv: übergroßer roter Clownsschuh wie SM_Item_ClownShoes.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Icons/UI_Icon_Item_NovaCola.png]
placeholder: Selbst gezeichneter roter Schuh (32x32 hochskaliert), bis die Lieferung da ist.
feedback: []
```

### SM_Home_Street.fbx
```yaml
path: Assets/_Project/Art/Models/Home/SM_Home_Street.fbx
package: P111
scene: Fernrohr (Diorama der Heimatstadt, nur durchs Münzfernrohr sichtbar)
status: final
priority: P2
needed_by: 2026-11-16
size_m: [12.0, 6.0, 8.0]
pivot: bottom_center
tris_max: 1500
materials: [Street, Windows]
textures:
  - Assets/_Project/Art/Textures/Home/T_Home_Street_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Kleine Straße in der Heimatstadt auf dem Heimatplaneten, bei Nacht, von schräg oben gesehen (wie durch ein Fernrohr aus großer Höhe):
  eine Zeile aus 3 einfachen, etwas heruntergekommenen Wohnhäusern (Arbeiterviertel, gleicher Retro-Stil wie die Station), davor Straße
  mit einer Laterne. Das Familienhaus steht in der Mitte und ist leicht hervorgehoben (Tür, Briefkasten, Wäscheleine). Es hat genau
  **drei Fensteröffnungen zur Kamera** (Mom, Lena, Pip), als dunkle, flache Rechtecke im Material-Slot "Windows" (ca. 0,8 x 1,0 m),
  gut voneinander getrennt; das Leuchten setzt der Code. Übrige Fenster der Nachbarhäuser im Slot "Street", teils schwach gelb gemalt.
interaction: "Ansehen durchs Münzfernrohr (P111): die drei Fenster leuchten je nach Zustand des Familienmitglieds"
variants: []
animations: []
collision: none
refs: []
placeholder: Drei graue Boxen 3 x 5 x 6 m nebeneinander auf einer flachen Platte, mittlere Box mit drei Quads als Fenster; Pivot bottom_center, Material-Slots [Street, Windows].
feedback: []
```

### SM_Station_Wall_Porthole_Large_2x3.fbx
```yaml
path: Assets/_Project/Art/Models/Station/SM_Station_Wall_Porthole_Large_2x3.fbx
package: P51
scene: Motel
status: final
priority: P3
needed_by: 2026-11-23
size_m: [2.0, 0.2, 3.0]
pivot: bottom_center
tris_max: 400
materials: [Wall, Glass]
textures:
  - Assets/_Project/Art/Textures/Station/T_Station_Wall_Porthole_2x3_Albedo.png   # 256x256, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Wie SM_Station_Wall_Porthole_2x3, gleiche Wandstärke und gleiche Textur, aber mit rundem Bullauge Ø 1,2 m (heute 0,6 m), Mitte auf 1,5 m Höhe. Für Zimmer 103. Glas als eigenes Material "Glass".
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Box 2.0 x 0.2 x 3.0 m, Pivot bottom_center, Material-Slots [Wall, Glass]
feedback: []
```

### SFX_PA_Murmur_01.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_PA_Murmur_01.ogg
package: P115
scene: Counter
status: final
priority: P3
needed_by: 2026-11-23
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Stationslautsprecher: kurzes, verrauschtes synthetisches Murmeln ohne verständliche Wörter, 1,5 bis 2,5 s, gedämpfter Bandpass, Variante 1 von 6. Keine Stimme, keine Wörter.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_PA_Murmur_02.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_PA_Murmur_02.ogg
package: P115
scene: Counter
status: final
priority: P3
needed_by: 2026-11-23
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Stationslautsprecher: kurzes, verrauschtes synthetisches Murmeln ohne verständliche Wörter, 1,5 bis 2,5 s, gedämpfter Bandpass, Variante 2 von 6. Keine Stimme, keine Wörter.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_PA_Murmur_03.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_PA_Murmur_03.ogg
package: P115
scene: Counter
status: final
priority: P3
needed_by: 2026-11-23
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Stationslautsprecher: kurzes, verrauschtes synthetisches Murmeln ohne verständliche Wörter, 1,5 bis 2,5 s, gedämpfter Bandpass, Variante 3 von 6. Keine Stimme, keine Wörter.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_PA_Murmur_04.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_PA_Murmur_04.ogg
package: P115
scene: Counter
status: final
priority: P3
needed_by: 2026-11-23
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Stationslautsprecher: kurzes, verrauschtes synthetisches Murmeln ohne verständliche Wörter, 1,5 bis 2,5 s, gedämpfter Bandpass, Variante 4 von 6. Keine Stimme, keine Wörter.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_PA_Murmur_05.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_PA_Murmur_05.ogg
package: P115
scene: Counter
status: final
priority: P3
needed_by: 2026-11-23
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Stationslautsprecher: kurzes, verrauschtes synthetisches Murmeln ohne verständliche Wörter, 1,5 bis 2,5 s, gedämpfter Bandpass, Variante 5 von 6. Keine Stimme, keine Wörter.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_PA_Murmur_06.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_PA_Murmur_06.ogg
package: P115
scene: Counter
status: final
priority: P3
needed_by: 2026-11-23
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Stationslautsprecher: kurzes, verrauschtes synthetisches Murmeln ohne verständliche Wörter, 1,5 bis 2,5 s, gedämpfter Bandpass, Variante 6 von 6. Keine Stimme, keine Wörter.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Vorhandener Ton bleibt, bis die Lieferung da ist.
feedback: []
```

### SM_Motel_FloppyPlayer.fbx
```yaml
path: Assets/_Project/Art/Models/Motel/SM_Motel_FloppyPlayer.fbx
package: P117
scene: Motel
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0.45, 0.3, 0.25]
pivot: bottom_center
tris_max: 400
materials: [Player]
textures:
  - Assets/_Project/Art/Textures/Motel/T_Motel_FloppyPlayer_Albedo.png   # 64x64, Point
  - Assets/_Project/Art/Textures/Motel/T_Motel_FloppyPlayer_Emission.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Klobiger Diskettenspieler aus den 80ern im Stil der Station: beiges Metallgehäuse, breiter Floppy-Schlitz vorne, zwei Drehregler, kleines grünes Display, Lautsprechergitter. Steht auf dem Tisch in Zimmer 103 anstelle des Fernsehers.
interaction: "E legt die Diskette ein oder nimmt sie heraus"
variants: []
animations: []
collision: prefab
refs: []
placeholder: SM_Motel_TV bleibt stehen; Box 0.45 x 0.3 x 0.25 m, Pivot bottom_center, Material-Slots [Player]
feedback: []
```

### SM_Item_FloppyDisk.fbx
```yaml
path: Assets/_Project/Art/Models/Items/SM_Item_FloppyDisk.fbx
package: P117
scene: Motel
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0.09, 0.09, 0.01]
pivot: bottom_center
tris_max: 100
materials: [Disk]
textures:
  - Assets/_Project/Art/Textures/Motel/T_Item_FloppyDisk_Albedo.png   # 64x64, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Eine 3,5-Zoll-Diskette, abgegriffen, Etikett von Hand beschriftet („FOR THE NIGHT SHIFT“), liegt flach auf dem Tisch.
interaction: "liegt auf dem Tisch, solange sie nicht im Spieler steckt"
variants: []
animations: []
collision: prefab
refs: []
placeholder: keine, bis es geliefert ist; Box 0.09 x 0.09 x 0.01 m, Pivot bottom_center, Material-Slots [Disk]
feedback: []
```

### MUS_Floppy_01.ogg
```yaml
path: Assets/_Project/Audio/Music/MUS_Floppy_01.ogg
package: P117
scene: Motel
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Erster Song für den Diskettenspieler in Zimmer 103. Melancholischer, ruhiger Synth-Track wie von einem alten Heimcomputer, im Stil der heruntergekommenen Station: langsam, warm, leicht verstimmt, kein Gesang, keine Stimmen. 90 bis 150 Sekunden, nahtlos loopbar.
interaction: "spielt, solange die Diskette im Spieler steckt"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Kein Ton, der Spieler meldet nur „nothing on this disk yet“.
feedback: []
```

### SM_Counter_LockerInterior.fbx
```yaml
path: Assets/_Project/Art/Models/Counter/SM_Counter_LockerInterior.fbx
package: P118
scene: Schalter
status: final
priority: P3
needed_by: 2026-11-23
size_m: [1.0, 0.48, 1.8]
pivot: bottom_center
tris_max: 400
materials: [Interior]
textures:
  - Assets/_Project/Art/Textures/Counter/T_Counter_LockerInterior_Albedo.png   # 128x128, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Innenleben des Asservatenschranks: Rückwand, Mitteltrennwand und zwei Metallböden, abgenutztes Stahlblech mit Nummern-Etiketten an den Fächern, passend zum Schrank SM_Counter_Locker. Sechs Fächer.
interaction: "zeigt beschlagnahmte Waren in den Fächern"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Graue Grundformen aus dem Builder bleiben, bis es geliefert ist.
feedback: []
```

### SM_Item_EvidenceBag.fbx
```yaml
path: Assets/_Project/Art/Models/Counter/SM_Item_EvidenceBag.fbx
package: P118
scene: Schalter
status: final
priority: P3
needed_by: 2026-11-23
size_m: [0.16, 0.05, 0.22]
pivot: bottom_center
tris_max: 150
materials: [Bag]
textures:
  - Assets/_Project/Art/Textures/Counter/T_Item_EvidenceBag_Albedo.png   # 128x128, Point
texel_px_per_m: 64
palette: docs/style/palette.png
look: >
  Durchsichtiger Beweisbeutel aus Plastik mit rotem Siegelstreifen und beschriftetem Etikett, darin ein unscharfer Gegenstand. Für Waren ohne eigenes Modell.
interaction: "liegt im Asservatenschrank"
variants: []
animations: []
collision: prefab
refs: []
placeholder: Graue Grundformen aus dem Builder bleiben, bis es geliefert ist.
feedback: []
```

### SFX_Floppy_Insert.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Floppy_Insert.ogg
package: P127
scene: Zimmer 103
status: Vorschau
priority: P3
needed_by: 2026-11-23
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Diskette wird in ein altes 3,5-Zoll-Laufwerk geschoben: mechanisches Einrasten mit kurzem Motoranlauf, 0,4 bis 0,8 s, trocken, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: UI-Klick bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_Floppy_Eject.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Floppy_Eject.ogg
package: P127
scene: Zimmer 103
status: Vorschau
priority: P3
needed_by: 2026-11-23
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Auswurf einer 3,5-Zoll-Diskette: Knopfdruck und federndes Herausschnappen, 0,3 bis 0,6 s, trocken, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: UI-Klick bleibt, bis die Lieferung da ist.
feedback: []
```

### SM_Item_Cassette.fbx
*(P132: Kassetten ersetzen den Floppy-Player. Kaufbar im Kiosk, liegen auf der Station, Geschenke von Figuren.)*
```yaml
path: Assets/_Project/Art/Models/Items/SM_Item_Cassette.fbx
package: P132
scene: Schalterraum, Zimmer 103, Kiosk
status: final
priority: P3
needed_by: 2026-11-23
size_m: [0.10, 0.064, 0.012]
pivot: bottom_center
tris_max: 150
materials: [Cassette]
textures:
  - Assets/_Project/Art/Textures/Items/T_Item_Cassette_Albedo.png   # 128x128, Point
texel_px_per_m: 128
palette: docs/style/palette.png
look: >
  Eine abgegriffene Audiokassette, Gehäuse halb durchsichtig mit sichtbaren Spulen, Papieretikett mit handgeschriebenem Titel. Liegt flach oder steckt aufrecht in einem Slot des Kassettenspielers (die obere Kante mit dem Etikett schaut heraus).
interaction: "liegt auf Tisch/Regal oder steckt im Kassettenspieler; wird wie eine Ware ins Inventar genommen"
variants: [T_Item_Cassette_Label_01.png bis _06.png: sechs Etikettfarben/-titel auf demselben Modell, je 64x32, Point]
animations: []
collision: prefab
refs: []
placeholder: keine, bis es geliefert ist; Box 0.10 x 0.064 x 0.012 m, Pivot bottom_center, Material-Slots [Cassette]
feedback: []
```

### UI_Icon_Item_Cassette.png
```yaml
path: Assets/_Project/Art/UI/Icons/UI_Icon_Item_Cassette.png
package: P132
scene: UI
status: final
priority: P3
needed_by: 2026-11-23
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Icons/UI_Icon_Item_Cassette.png   # 128x128, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Inventar-Icon einer Audiokassette im selben Stil wie die übrigen Waren-Icons (UI_Icon_Item_NovaCola).
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: PNG 128x128, Magenta/Grau-Raster
feedback: []
```

### SM_Counter_CassettePlayer.fbx
```yaml
path: Assets/_Project/Art/Models/Counter/SM_Counter_CassettePlayer.fbx
package: P132
scene: Schalterraum, Zimmer 103
status: final
priority: P3
needed_by: 2026-11-23
size_m: [0.45, 0.20, 0.16]
pivot: bottom_center
tris_max: 900
materials: [Player]
textures:
  - Assets/_Project/Art/Textures/Counter/T_Counter_CassettePlayer_Albedo.png   # 256x256, Point
  - Assets/_Project/Art/Textures/Counter/T_Counter_CassettePlayer_Emission.png   # 64x64, Point
texel_px_per_m: 128
palette: docs/style/palette.png
look: >
  Robustes Tape-Deck aus Stationsbeständen mit drei nebeneinanderliegenden, oben offenen Slots, in denen Kassetten aufrecht stecken (sichtbar bis zum Etikett). Kleine Lämpchen je Slot (Emission: grün = spielt gerade, gelb = wartet), ein großer Play/Stop-Kipphebel, Lautsprechergitter vorne. Steht auf einem Regal oder Tisch. Derselbe Spieler ersetzt den Floppy-Player in Zimmer 103.
interaction: "[E] legt eine Kassette in den nächsten freien Slot oder nimmt eine heraus; spielt die eingelegten nacheinander"
variants: []
animations: []
collision: prefab
refs: []
placeholder: keine, bis es geliefert ist; Box 0.45 x 0.20 x 0.16 m, Pivot bottom_center, Material-Slots [Player]; Slot-Positionen als leere Objekte Slot_1..Slot_3 im FBX
feedback: []
```

### SFX_Cassette_Insert.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Cassette_Insert.ogg
package: P132
scene: Schalterraum, Zimmer 103
status: final
priority: P3
needed_by: 2026-11-23
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Kassette rastet in ein Tape-Deck ein: kurzes Plastikklappern, dann ein satter Klick, 0,3 bis 0,6 s, trocken, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: SFX_Floppy_Insert, bis die Lieferung da ist.
feedback: []
```

### SFX_Cassette_Eject.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Cassette_Eject.ogg
package: P132
scene: Schalterraum, Zimmer 103
status: final
priority: P3
needed_by: 2026-11-23
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Kassette wird aus einem Tape-Deck gezogen: Federklick und Plastikschaben, 0,3 bis 0,6 s, trocken, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: SFX_Floppy_Eject, bis die Lieferung da ist.
feedback: []
```

### SFX_Cassette_Play.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Cassette_Play.ogg
package: P132
scene: Schalterraum, Zimmer 103
status: final
priority: P3
needed_by: 2026-11-23
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Mechanischer Play-Kipphebel eines alten Tape-Decks rastet ein, dazu kurzes Motoranlaufen, 0,4 bis 0,8 s, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: UI-Klick bleibt, bis die Lieferung da ist.
feedback: []
```

### SFX_CleaningBot_Loop.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_CleaningBot_Loop.ogg
package: P137
scene: Halle
status: final
priority: P3
needed_by: 2026-11-23
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Putzroboter bei der Arbeit, nahtlos loopbar 3 bis 5 s: rotierende Bürsten, leises Saugen, ab und zu ein fröhliches Quietschen oder Trällern, damit es lustig wirkt. Mono, wird als 3D-Klang mit kurzer Reichweite abgespielt.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: kein Klang, bis die Lieferung da ist.
feedback: []
```

### SFX_CleaningBot_Turn.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_CleaningBot_Turn.ogg
package: P137
scene: Halle
status: final
priority: P3
needed_by: 2026-11-23
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Putzroboter wendet: zwei kurze, hohe Piepser wie ein kleiner Lieferroboter, 0,3 bis 0,5 s, mono.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: []
placeholder: kein Klang, bis die Lieferung da ist.
feedback: []
```

### UI_Cursor_Default.png
```yaml
path: Assets/_Project/Art/UI/UI_Cursor_Default.png
package: P138
scene: UI
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/UI_Cursor_Default.png   # 32x32, transparent, Point, Hotspot 0,0 (Spitze oben links)
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Mauszeiger im Terminal-Stil des Spiels (wie UI_Terminal_*): pixeliger Pfeil in Bernstein (#ffd46b) mit dunkler Kontur, gut lesbar auf hellen Papieren und dunklen Wänden.
interaction: "Zeiger in Menüs, am Schalter und in Fenstern"
variants: []
animations: []
collision: prefab
refs: []
placeholder: PNG 32x32, Magenta/Grau-Raster
feedback: []
```

### UI_Cursor_Hover.png
```yaml
path: Assets/_Project/Art/UI/UI_Cursor_Hover.png
package: P138
scene: UI
status: final
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/UI_Cursor_Hover.png   # 32x32, transparent, Point, Hotspot gleich wie UI_Cursor_Default
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Zeiger über etwas Klickbarem: derselbe Stil wie UI_Cursor_Default, aber als zeigende Hand oder Pfeil mit kleinem Leuchtrahmen, damit man sofort sieht, dass man klicken kann.
interaction: "Zeiger über Knöpfen, Papieren, Stempeln"
variants: []
animations: []
collision: prefab
refs: []
placeholder: PNG 32x32, Magenta/Grau-Raster
feedback: []
```

### MUS_Cassette_01.ogg
```yaml
path: Assets/_Project/Audio/Music/MUS_Cassette_01.ogg
package: P132
scene: Schalterraum, Zimmer 103
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Song 1 von 5 für die Kassetten (P132), gleicher Stil wie MUS_Floppy_01 (kein Gesang, keine Stimmen, 90 bis 150 Sekunden, nahtlos loopbar, OGG). Stimmung: Nachtschicht: ruhiger, warmer Synth-Track, langsam, wie ein alter Heimcomputer um 3 Uhr nachts. Selbst erzeugt oder frei (CC0/CC-BY, dann mit Quelle und Lizenz für CREDITS.md). Die fünf sollen sich deutlich unterscheiden, damit eine Playlist aus drei Kassetten abwechslungsreich klingt.
interaction: "läuft, wenn die Kassette im Kassettendeck steckt; drei Kassetten spielen nacheinander als Playlist"
variants: []
animations: []
collision: prefab
refs: [Assets/_Project/Audio/Music/MUS_Floppy_01.ogg]
placeholder: Alle Kassetten spielen MUS_Floppy_01.
feedback: []
```

### MUS_Cassette_02.ogg
```yaml
path: Assets/_Project/Audio/Music/MUS_Cassette_02.ogg
package: P132
scene: Schalterraum, Zimmer 103
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Song 2 von 5 für die Kassetten (P132), gleicher Stil wie MUS_Floppy_01 (kein Gesang, keine Stimmen, 90 bis 150 Sekunden, nahtlos loopbar, OGG). Stimmung: Leicht beschwingt: lockerer Groove mit weichem Bass und Arpeggio, wie Radiomusik aus einer Bordkantine. Selbst erzeugt oder frei (CC0/CC-BY, dann mit Quelle und Lizenz für CREDITS.md). Die fünf sollen sich deutlich unterscheiden, damit eine Playlist aus drei Kassetten abwechslungsreich klingt.
interaction: "läuft, wenn die Kassette im Kassettendeck steckt; drei Kassetten spielen nacheinander als Playlist"
variants: []
animations: []
collision: prefab
refs: [Assets/_Project/Audio/Music/MUS_Floppy_01.ogg]
placeholder: Alle Kassetten spielen MUS_Floppy_01.
feedback: []
```

### MUS_Cassette_03.ogg
```yaml
path: Assets/_Project/Audio/Music/MUS_Cassette_03.ogg
package: P132
scene: Schalterraum, Zimmer 103
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Song 3 von 5 für die Kassetten (P132), gleicher Stil wie MUS_Floppy_01 (kein Gesang, keine Stimmen, 90 bis 150 Sekunden, nahtlos loopbar, OGG). Stimmung: Sehnsucht nach zu Hause: melancholisch, wenige Töne, Pad-Flächen, etwas verstimmt. Selbst erzeugt oder frei (CC0/CC-BY, dann mit Quelle und Lizenz für CREDITS.md). Die fünf sollen sich deutlich unterscheiden, damit eine Playlist aus drei Kassetten abwechslungsreich klingt.
interaction: "läuft, wenn die Kassette im Kassettendeck steckt; drei Kassetten spielen nacheinander als Playlist"
variants: []
animations: []
collision: prefab
refs: [Assets/_Project/Audio/Music/MUS_Floppy_01.ogg]
placeholder: Alle Kassetten spielen MUS_Floppy_01.
feedback: []
```

### MUS_Cassette_04.ogg
```yaml
path: Assets/_Project/Audio/Music/MUS_Cassette_04.ogg
package: P132
scene: Schalterraum, Zimmer 103
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Song 4 von 5 für die Kassetten (P132), gleicher Stil wie MUS_Floppy_01 (kein Gesang, keine Stimmen, 90 bis 150 Sekunden, nahtlos loopbar, OGG). Stimmung: Bar-Lounge: gemütlich, jazzig angehauchter Synth mit Swing, gedämpftes Schlagzeug. Selbst erzeugt oder frei (CC0/CC-BY, dann mit Quelle und Lizenz für CREDITS.md). Die fünf sollen sich deutlich unterscheiden, damit eine Playlist aus drei Kassetten abwechslungsreich klingt.
interaction: "läuft, wenn die Kassette im Kassettendeck steckt; drei Kassetten spielen nacheinander als Playlist"
variants: []
animations: []
collision: prefab
refs: [Assets/_Project/Audio/Music/MUS_Floppy_01.ogg]
placeholder: Alle Kassetten spielen MUS_Floppy_01.
feedback: []
```

### MUS_Cassette_05.ogg
```yaml
path: Assets/_Project/Audio/Music/MUS_Cassette_05.ogg
package: P132
scene: Schalterraum, Zimmer 103
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Song 5 von 5 für die Kassetten (P132), gleicher Stil wie MUS_Floppy_01 (kein Gesang, keine Stimmen, 90 bis 150 Sekunden, nahtlos loopbar, OGG). Stimmung: Station im Leerlauf: ambientartig, pulsierend, gleichmäßiger Rhythmus, leicht hypnotisch. Selbst erzeugt oder frei (CC0/CC-BY, dann mit Quelle und Lizenz für CREDITS.md). Die fünf sollen sich deutlich unterscheiden, damit eine Playlist aus drei Kassetten abwechslungsreich klingt.
interaction: "läuft, wenn die Kassette im Kassettendeck steckt; drei Kassetten spielen nacheinander als Playlist"
variants: []
animations: []
collision: prefab
refs: [Assets/_Project/Audio/Music/MUS_Floppy_01.ogg]
placeholder: Alle Kassetten spielen MUS_Floppy_01.
feedback: []
```

### A_NPC_Sit.fbx
```yaml
path: Assets/_Project/Art/Animations/Characters/A_NPC_Sit.fbx
package: P134
scene: Halle, Bar
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Sitz-Idle für die Reisenden: eine einzelne Animation (Take "Sit") auf demselben Humanoid-Skelett wie SK_NPC_Human (alle SK_NPC_* sind in Unity als Humanoid importiert, die Animation wird retargetet). Figur sitzt auf einer Bank oder einem Hocker mit Sitzhöhe 0,45 bis 0,75 m, Hände auf den Oberschenkeln oder verschränkt, leichtes Atmen und ab und zu ein Blick zur Seite. 4 bis 6 Sekunden, nahtlos loopbar. Das Becken liegt im Ursprung auf Sitzhöhe 0,45 m, die Füße stehen auf dem Boden.
interaction: "Abendgäste sitzen auf den Hallenbänken und an der Bar"
variants: []
animations: [Sit]
collision: prefab
refs: [Assets/_Project/Art/Models/Characters/SK_NPC_Human.fbx]
placeholder: Gäste stehen an Stehplätzen mit Idle.
feedback: []
```

### UI_SpeechBubble.png
```yaml
path: Assets/_Project/Art/UI/Terminal/UI_SpeechBubble.png
package: P143
scene: Schalter (Sprechblasen der Reisenden)
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Terminal/UI_SpeechBubble.png   # 96x96, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Rahmen der Sprechblase als 9-Slice, passend zu UI_Terminal_Panel, aber leichter: schmale Metallkante (mindestens 16 px Rand, Ecken verzerren beim Strecken nicht), abgerundete Pixel-Ecken, innen dunkles, leicht durchscheinendes Glas (Alpha etwa 85 %), eine dünne CRT-grüne Innenlinie. Oben links ein schmales Feld für den Namen des Sprechers. Pixel-Art, Farben aus der Palette.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Terminal/UI_Terminal_Panel.png, docs/STYLE.md]
placeholder: UI_Terminal_Panel, bis die Lieferung da ist.
feedback: []
```

### UI_SpeechBubble_Tail.png
```yaml
path: Assets/_Project/Art/UI/Terminal/UI_SpeechBubble_Tail.png
package: P143
scene: Schalter (Sprechblasen der Reisenden)
status: offen
priority: P2
needed_by: 2026-11-20
size_m: [0, 0, 0]
pivot: none
tris_max: 0
materials: []
textures:
  - Assets/_Project/Art/UI/Terminal/UI_SpeechBubble_Tail.png   # 32x24, transparent, Point
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Zipfel der Sprechblase, der zum Kopf des Sprechers zeigt: nach links unten zeigend, dieselbe Kante und Füllung wie UI_SpeechBubble, oben offen, damit er nahtlos an die Unterkante der Blase anschließt. Das Spiel spiegelt ihn, wenn der Sprecher auf der anderen Seite steht.
interaction: "none"
variants: []
animations: []
collision: none
refs: [Assets/_Project/Art/UI/Terminal/UI_SpeechBubble.png]
placeholder: Kein Zipfel, bis die Lieferung da ist.
feedback: []
```

### SFX_Cassette_Skip.ogg
```yaml
path: Assets/_Project/Audio/SFX/SFX_Cassette_Skip.ogg
package: P141
scene: Schalterraum, Zimmer 103
status: offen
priority: P3
needed_by: 2026-11-23
size_m: [0, 0, 0]
pivot: bottom_center
tris_max: 0
materials: []
textures: []
texel_px_per_m: 0
palette: docs/style/palette.png
look: >
  Tape-Deck springt zum nächsten Song: kurzes hohes Vorspul-Surren, dann ein mechanischer Klack der Taste, 0,4 bis 0,8 s, trocken, mono, passend zu SFX_Cassette_Insert.
interaction: "none"
variants: []
animations: []
collision: prefab
refs: [Assets/_Project/Audio/SFX/SFX_Cassette_Insert.ogg]
placeholder: SFX_Cassette_Insert, bis die Lieferung da ist.
feedback: []
```
