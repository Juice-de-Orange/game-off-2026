# Asset-Regeln

Ziel: Martin baut mit Platzhaltern, Max ersetzt sie durch finale Assets, **ohne dass eine Szene oder ein Prefab
angefasst werden muss**. Dafür gelten feste Namen, Pfade und technische Eckdaten.

## Grundprinzip: Platzhalter → final durch Datei-Ersatz
1. Wer einen Platzhalter braucht, legt ihn **unter dem finalen Namen am finalen Pfad** an und trägt ihn in
   `docs/ASSET-REQUESTS.md` ein (Status `offen`).
2. Max ersetzt später **nur den Dateiinhalt** (gleicher Dateiname, Datei im Explorer überschreiben).
   Die `.meta`-Datei bleibt unverändert → die GUID bleibt gleich → alle Referenzen in Szenen/Prefabs bleiben heil.
3. Ab dem Eintrag in der Liste fasst nur noch Max die Datei an. Status → `final`.

**Nie** die `.meta`-Datei löschen oder ein Asset in Unity löschen und neu anlegen — dann brechen die Referenzen.

## Ordner
```
Assets/_Project/
  Art/
    Models/<Bereich>/      FBX-Modelle            (Bereiche: Station, Props, Characters, Gadgets)
    Textures/<Bereich>/    PNG-Texturen
    Materials/<Bereich>/   Unity-Materialien
    Animations/            Animationen, Animator Controller
    UI/                    UI-Grafiken (Icons, Dokumente, Stempel)
  Audio/
    Music/                 Musik
    SFX/                   Soundeffekte
    Voice/<Charakter>/     Voicelines
  Fonts/                   Schriftarten (TTF/OTF) + TextMeshPro-Font-Assets
  Prefabs/<Feature>/       Prefabs (Station, Props, Characters, UI ...)
ArtSource/                 Arbeitsdateien (.blend, .psd, Rohaufnahmen) — außerhalb von Assets/, über Git LFS
```
`.blend`-Dateien gehören **nicht** in `Assets/` (Unity würde Blender zum Import brauchen). Export als FBX nach `Art/Models/`.

## Namensschema
`<Präfix>_<Bereich>_<Name>[_<Variante>].<ext>` — PascalCase, keine Leerzeichen, Englisch.

| Präfix | Typ | Beispiel |
|---|---|---|
| `SM_` | statisches Modell (FBX) | `SM_Bar_Counter.fbx` |
| `SK_` | Modell mit Skelett (FBX) | `SK_Traveler_Alien01.fbx` |
| `T_` | Textur (PNG), Suffix = Typ | `T_Bar_Counter_Albedo.png`, `T_Bar_Counter_Emission.png` |
| `M_` | Material | `M_Bar_Counter.mat` |
| `A_` | Animation | `A_Traveler_Walk.anim` |
| `UI_` | UI-Grafik | `UI_Doc_Passport.png`, `UI_Icon_Scanner.png` |
| `MUS_` | Musik | `MUS_Shift_Loop.ogg` |
| `SFX_` | Soundeffekt | `SFX_Stamp_Approve.ogg` |
| `VO_` | Voiceline | `VO_Barkeeper_Greeting01.ogg` |
| `FNT_` | Schrift | `FNT_Title.ttf` |
| `PF_` | Prefab | `PF_Bar_Counter.prefab` |

## Technische Eckdaten
**3D (Blender → Unity)**
- 1 Unity-Einheit = 1 Meter. Spielerhöhe ca. 1,8 m, Türen 2,2 m.
- Pivot: unten mittig (Props), bei Türen an der Angel.
- FBX-Export: "Apply Transforms" an, Forward −Z, Up Y, nur ausgewählte Objekte, keine Kamera/Lichter.
- Low-Poly. Richtwert: Prop < 1.000 Tris, Charakter < 3.000 Tris.

**Texturen**
- PNG, Potenzen von 2, klein: Props 64–256 px, Charaktere/Wände bis 512 px.
- Bei Retro-Look: Filter Mode **Point**, Mipmaps aus. (Wird festgelegt, wenn der Art Style steht.)
- UI-Grafiken und Dokumente: voll aufgelöst, Filter Bilinear (müssen lesbar sein).

**Audio**
- Im Projekt: OGG (Musik, SFX, Voice). WAV-Rohdateien nach `ArtSource/Audio/`.
- Voice: Mono, 44,1 kHz. Musik: Stereo, Import "Streaming". SFX: "Decompress On Load".
- Jede freie Datei sofort in `CREDITS.md` eintragen (Quelle + Lizenz). Nur CC0, CC-BY oder ähnlich offen.

**Schriften**
- Nur frei lizenziert (OFL o. ä.), in `CREDITS.md` eintragen. Für TextMeshPro ein Font Asset daneben ablegen.

## Platzhalter erkennen
- Platzhalter-Materialien in Magenta/Grau-Raster (`M_Placeholder.mat`), damit sofort sichtbar ist, was noch fehlt.
- Platzhalter-Modelle: Unity-Primitive oder ProBuilder-Blöcke in ungefährer Endgröße.

## Asset-Liste
Offene Anforderungen stehen in `docs/ASSET-REQUESTS.md`.
