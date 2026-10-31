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
*(Beispiel-Eintrag der Pipeline, wird übernommen, sobald das Bar-Paket freigegeben ist.)*
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
