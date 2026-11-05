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
- FBX binär 7.4, nur Mesh (keine eingebetteten Texturen, keine Kamera/Lichter).
- 1 Unity-Einheit = 1 Meter. Spielerhöhe ca. 1,8 m, Türen 2,2 m.
- Pivot: unten mittig (Props), bei Türen an der Angel.
- FBX-Export: "Apply Transforms" an, Forward −Z, Up Y, nur ausgewählte Objekte, keine Kamera/Lichter.
- Low-Poly. Richtwert: Prop < 1.000 Tris, Charakter < 3.000 Tris.
- **Material-Slots:** benannte Slots im FBX, pro Prop 1, höchstens 2 (Draw Calls im Browser). Die `M_`-Materialien werden in Unity angelegt.
- Kollision: Standard = Collider im Prefab. Bei Bedarf liefert die Pipeline ein vereinfachtes zweites Mesh `<Name>_Col`.

**Texturen**
- PNG, Potenzen von 2, 64–512 px. Albedo ohne eingebackenes Licht, Emission als eigene Textur, keine Normal-/ORM-Maps.
- **Texel-Dichte:** 64 px/m für Welt-Props, 128 px/m für Nahsicht am Schalter.
- Filter, Palette und Stil: siehe `docs/STYLE.md`.
- UI-Grafiken und Dokumente: voll aufgelöst, Filter Bilinear (müssen lesbar sein).

**Audio**
- Im Projekt: OGG (Musik, SFX, Voice). WAV-Rohdateien nach `ArtSource/Audio/`.
- Voice: Mono, 44,1 kHz. Musik: Stereo, Import "Streaming". SFX: "Decompress On Load".
- Jede freie Datei sofort in `CREDITS.md` eintragen (Quelle + Lizenz). Nur CC0, CC-BY oder ähnlich offen.

**Schriften**
- Nur frei lizenziert (OFL o. ä.), in `CREDITS.md` eintragen. Für TextMeshPro ein Font Asset daneben ablegen.

## Regeln für Platzhalter (damit der Austausch sauber klappt)
- Der Platzhalter ist eine **echte Datei desselben Typs** am finalen Pfad mit finalem Namen: eine FBX-Box bzw. ein PNG in der finalen Größe.
- Er hat schon die **Material-Slot-Namen** aus dem Eintrag, ungefähr die Größe aus `size_m` und denselben Pivot.
- **Importeinstellungen** werden einmal gesetzt und danach nicht mehr geändert.
- Skripte greifen **nicht** auf Unterobjekt-Namen im FBX zu.
- **Collider, Skripte, Licht** hängen am Prefab (`PF_`), nie im FBX.
- Texturen behalten den Namen und die gleiche oder eine größere Zweierpotenz.

## Ablieferung durch die Asset-Pipeline
- Die Pipeline liest `docs/ASSET-REQUESTS.md` und `docs/STYLE.md` von `main`: **etwa stündlich** von selbst,
  sofort, wenn Max ihr schreibt (z. B. „Game Off: neue Einträge“). Aus `offen`-Einträgen werden Pakete,
  aus `rework` mit neuer `feedback`-Zeile Nacharbeits-Pakete.
- **Ein PR pro Paket** mit 3–6 zusammengehörigen Assets eines Bereichs (z. B. „Bar: Tresen, Hocker, Regal“).
  Einzelne Nacharbeiten bekommen einen kleinen eigenen PR.
- **Draft-PR = in Arbeit**, **„Ready for review“ = Vorschau**. Im PR-Text pro Asset: Vorschaubilder (schräg vorne + Nahansicht),
  Tris, Material-Slots, Texturen mit Größe, Maße in m, Pivot. In `ASSET-REQUESTS.md` ändert die Pipeline nur die
  `status`-Zeile ihrer eigenen Einträge → auf `main` steht der neue Status erst nach dem Merge.
- Ist ein PR „Ready for review“, bekommt **Max eine Push-Nachricht** aufs Handy (plus Tagesbericht). Martin sieht PRs auf GitHub
  und im Session-Briefing.
- Der Start ist an die Freigabe von `docs/STYLE.md` durch Max gebunden. Danach kommt zuerst ein Muster aus 3 Assets (`assets/P00-sample`).
- Sie liefert auf einem eigenen Branch `assets/<Paket>`: jede Datei am finalen Pfad (**nie** die `.meta`), Arbeitsdatei unter `ArtSource/`,
  Vorschaubild unter `docs/asset-previews/`. Auf `main` schreibt sie nie.
- **Die Pipeline kann selbst keine Pull Requests anlegen (M002):** Sie pusht den Branch, der Lead schickt Max den PR-Link in den Chat. Eine Lieferung ohne PR ist also ein normaler Zustand. **Das Session-Briefing und `/sync` zeigen Branches, die noch nicht auf `main` sind**; so bleibt nichts liegen wie bei P42.
- **Jede Lieferung, auch jede Nacharbeit, bekommt einen neuen Branch von aktuellem `main` und einen eigenen PR.** Gemergte
  Branches löscht GitHub automatisch (seit 04.11.); auf alte Branches nicht weiterpushen, sonst entsteht kein PR.
- **Freigabe und Merge macht immer Max** (Art-Verantwortung). Martin kann Anpassungen anfordern, gibt aber nicht frei.
  Status im Eintrag: `Vorschau` → `final`.
- Commit-Format der Pipeline: `assets(Pxx): SM_Bar_Counter.fbx final - 820 tris, 1 material`.
- Material, Prefab, Importeinstellungen und Szenen macht das Team im Unity-Editor, nicht die Pipeline.

## Asset bestellen (Max und Martin)
**Ein Eintrag in `docs/ASSET-REQUESTS.md` auf `main` ist die Bestellung.** Beide dürfen bestellen, ohne vorher zu fragen. Claude erledigt das auf Zuruf („bestell X“):
1. Am Ende von `docs/ASSET-REQUESTS.md` einen Eintrag anhängen: Überschrift `### <Dateiname>`, darunter der YAML-Block aus der Vorlage dort, mit `status: offen`.
   `package` (Pxx), Maße, Pivot und `look` sind am wichtigsten: Die Pipeline baut, was dort steht.
2. Prüfen: `python tools/check_asset_requests.py` (muss `0 broken` melden).
3. Commit + **Push auf `main`**. Erst dann sieht es die Pipeline; sie liest die Datei etwa stündlich.
4. Weiterbauen: Platzhalter am finalen Pfad anlegen (Regeln unten) oder, wie bei den Händlern, den Builder eine graue Box setzen lassen.

Danach passiert der Rest ohne dich: Die Pipeline liefert per Pull Request, **Max** gibt frei und baut es ein. Den Stand zeigt `status` im Eintrag.

**Erst mit Max absprechen:** nur große Aufträge (viele Assets, neue Räume wie P53) und alles, was den Stil ändert (`docs/STYLE.md`).
Eine Notiz wie „Bestellung nach Absprache“ im Paket reicht nicht: Solange kein Eintrag auf `main` liegt, baut niemand etwas.
Nicht bestellbar: Voicelines (`VO_`), siehe „Was die Pipeline (nicht) kann“.

## Anpassungen anfordern (Max und Martin)
In `docs/ASSET-REQUESTS.md` beim Eintrag eine datierte Zeile in `feedback` ergänzen und `status: rework` setzen,
auf `main` pushen. Beispiel: `- "2026-11-10 martin: Tresen 20 cm niedriger, Spieler sieht sonst nicht drüber"`.
Die Pipeline arbeitet den Eintrag erneut ab und liefert wieder per Pull Request.

## Integration nach dem Merge (Max)
0. **Der schnelle Weg:** `git pull`, dann **GameOff → Rebuild All After Deliveries** (alle Builder in der richtigen Reihenfolge), dann **GameOff → Setup Main Lighting + Bake**. Danach Tests und ein Blick ins Spiel. `Main.unity`, `Menu.unity`, `Boot.unity` und die generierten Prefabs vorher claimen.
1. `git pull`, Unity öffnen und den Import abwarten.
2. Prüfen: Ersetzte Dateien haben **keine neue** `.meta` bekommen. Für neue Dateien ohne Platzhalter erzeugt Unity
   eine `.meta` → **sofort committen und pushen**, damit Martin dieselbe GUID bekommt.
3. Neue Texturen: Importeinstellungen nach `docs/STYLE.md` setzen (Point, keine Mipmaps), `M_`-Material anlegen bzw. anpassen.
4. In der Szene kurz ansehen (Größe, Pivot, Material), dann Status im Eintrag auf `final`.
5. Passt etwas nicht: `feedback` + `status: rework` statt selbst nachzubessern.

## Was die Pipeline (nicht) kann
- Gut: statische Low-Poly-Props, Raummodule (Wände, Böden, Hangar-Teile), Texturen und Decals, UI-Grafiken/Dokumente, einfache Animationen (Türen, Schalter, Lüfter).
- Eingeschränkt: Figuren mit Rig (`SK_`) nur als einfacher Low-Poly-Körper. Gesichter und Charakter-Animationen sind die schwächste Stelle → bei der Gestaltung der Reisenden einplanen.
- Nicht: Voicelines (`VO_`), Musik komponieren. Freie Musik/SFX sucht sie heraus (CC0/CC-BY, Eintrag in `CREDITS.md`, Umwandlung nach OGG), die Auswahl nach Gehör trifft das Team.

## Platzhalter erkennen
- Platzhalter-Materialien in Magenta/Grau-Raster (`M_Placeholder.mat`), damit sofort sichtbar ist, was noch fehlt.
- Platzhalter-Modelle: Unity-Primitive oder ProBuilder-Blöcke in ungefährer Endgröße.

## Asset-Liste
Offene Anforderungen stehen in `docs/ASSET-REQUESTS.md`.
