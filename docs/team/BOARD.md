# Team-Board

> Pull vor jeder Änderung. Kleine Änderungen, sofort pushen — besonders bei Claims.

## Meilensteine
Rhythmus: 3 Wochen Entwicklung, 1 Woche Polish, 2 Tage Puffer.

| Datum | Ziel | Status |
|---|---|---|
| 01.11. | Theme kommt ca. **22:37 MEZ** → jeder für sich Gedanken, Repo öffentlich | ✅ Theme "Shift" · Repo noch privat (Call) |
| 02.11. | Abend-Call: Theme ins Konzept einweben, `DESIGN.md` finalisieren, Unity-Projekt anlegen | 🔶 Projekt + Station 2.0 stehen; Call offen |
| 03.11. | Prototyp: Kern-Mechanik spielbar (graue Klötze reichen) | ✅ 04.11.: Schicht am Schalter spielbar (Dokumente, Stempel, Regeln, Verwarnzettel); Geld/Tage fehlen noch (P08–P10) |
| 05.11. | Erster WebGL-Testbuild auf itch.io (Draft) — danach jede Woche ein Build | 🔶 Build läuft lokal (18 MB); 05.11. martin: Build mit Menü (20 MB) in Chrome getestet ✔; Upload P19 offen |
| 09.11. | Checkpoint: Trägt der Kern-Loop? Wenn nicht, jetzt umsteuern | ⏳ |
| 16.11. | Vertical Slice: ein Level komplett mit Ziel, Sieg/Niederlage, Menü, Neustart | ⏳ |
| 22.11. | **Feature-Freeze**: keine neuen Features mehr, nur Inhalte & Polish | ⏳ |
| 28.11. | **Content-Freeze**: nur noch Bugfixes, Audio, Juice | ⏳ |
| 29.–30.11. | Puffer: itch-Seite, finaler Build, Browser-Tests | ⏳ |
| 30.11. | **Abgabe** auf itch.io (offizielle Deadline 01.12., 22:37 MEZ) | ⏳ |

### Stand 02.11. abends (max)
- Fertig: P03, P04, P05, P17 (aus), P20, P21, P22, P23, P24, P27 · P01 wartet auf Martin · 9 Asset-PRs gemergt und eingebaut
- Neu angelegt: P22–P36 (Station 2.0 + Mechaniken aus Max' Asset-Durchgang) — **im Call verteilen**
- Pipeline: ~90 Aufträge offen (M003, M006, M007); Builder setzt alles automatisch ein, sobald es geliefert ist
- Nächste Gameplay-Schritte für den Kern-Loop: P06 Dokumente → P07 Regeln → P08 Entscheidungen → P09 Tagesablauf → P10 Geld/Familie

### Stand 04.11. abends (max)
- Fertig heute: P37 Station-Fixes innen, P38 Schalter (Sessel, Aufstehen, Stempel), P39 Außenstation 2.0, **P06 Dokumente**, **P07 Reisende & Regeln**
- Spielregeln festgelegt (DESIGN.md "Regeln am Schalter", DECISIONS 04.11.), inkl. Pflicht-Luftschleuse und **The Shifter**
- 41 EditMode-Tests + `GameOff/Validate Travelers` sichern die Regeln und alle Inhalte ab
- Pipeline: 22 neue Aufträge (Außenhülle M015, Dokument-Hintergründe, Verwarnzettel, Drucker, Stempel-Abdrücke) → P42
- Als Nächstes: P41 → P40 → P08 → P09 → P10 → P15 (Reihenfolge in PAKETE.md)

## Aufgabenteilung
- **max**: alle finalen Assets (Art, Sound, Musik).
- **martin**: Startbildschirm — alles vom Spielstart bis zum Menü. Baut mit Platzhalter-Assets, Max ersetzt sie.
- Abstimmung: immer wieder unter der Woche, am Wochenende intensiver. Verfügbare Zeit pro Woche ist offen und ergibt sich unterwegs.

## In Arbeit
Wer etwas bearbeitet, trägt es hier ein (`/claim`). Geclaimte Szenen/Prefabs/Assets fasst der andere nicht an.
Welche **Pakete** gerade wer baut, steht in `PAKETE.md`.

| Wer | Aufgabe | Geclaimte Dateien | Seit |
|---|---|---|---|
| martin | P43 Optionen in Reitern (danach P18 Audio) | `Menu.unity`, `Main.unity` (nur Pausemenü), `PF_PauseMenu.prefab`, `FirstPersonController.cs` (Y-Achse umkehren) | 05.11. |

## Backlog
Die Arbeit läuft über **Pakete** in `PAKETE.md` (Entwurf, wird am 02.11. freigegeben). Diese Liste nur für Ideen,
die noch kein Paket sind. Bei Zeitnot wird von unten gestrichen.

### Muss (ohne das kein Spiel)
- [ ] 

### Soll (macht es gut)
- [ ] 

### Kann (wenn Zeit bleibt)
- [ ] 

## Erledigt
- [x] P02 Startbildschirm & Menü: Boot-Splash, Hauptmenü mit Station im All als Hintergrund, Optionen, Credits, Pausemenü; WebGL getestet (martin, 05.11.)
- [x] P01 abgeschlossen: Martin hat das Projekt geöffnet und einen WebGL-Build getestet (martin, 05.11.)
- [x] P07 Reisende & Regeln: Regel-Engine, Bewertung, Validator + 41 Tests, Schicht-Ablauf, Verwarnzettel, Test-Tag (max, 04.11.)
- [x] P06 Dokumente als 3D-Papiere: Schublade, ziehen, lesen, stempeln, zurückgeben; scharfe Ebene für den Retro-Look (max, 04.11.)
- [x] P39 Außenstation 2.0: verbundenes Fachwerk, Habs, Solar, Antenne, Außenhülle (Platzhalter bis zur Lieferung) (max, 04.11.)
- [x] P38 Schalter: Sessel anwählbar, Aufstehen mit Q/Esc/Rechtsklick, Stempel hochheben und stempeln (max, 04.11.)
- [x] P37 Station-Fixes innen: Türen, Wand-Props, Kiosk-Front, Hangar-Fassade, alle Platzhalter ersetzt (max, 04.11.)
- [x] Lieferungen M013/M014 gemergt (PR #10–#13) und eingebaut; 32 alte Asset-Branches gelöscht, Auto-Delete an (max, 04.11.)
- [x] Stations-Grundriss freigegeben (`docs/STATION.md`), 48 Asset-Anforderungen für die Pipeline (max, 02.11.)
- [x] P01 Unity-Projekt angelegt (WebGL-Testbuild steht noch aus) (max, 02.11.)
- [x] P04 Station als Graybox in `Main.unity` (max, 02.11.)
- [x] P03 Spieler-Controller + Interaktion, Spieler in `Main.unity` (max, 02.11.)
- [x] P05 Schalteransicht mit Stempeln und Knöpfen (max, 02.11.)
- [x] P17 Retro-Look 640×360, UI scharf — vorerst AUS, Entscheidung nach Asset-Integration (max, 02.11.)
- [x] P27 Automatische Türen mit Zugangsstufen (max, 02.11.)
- [x] Asset-Durchgang: Funktionen festgelegt → Pakete P27–P36 (max, 02.11.)
- [x] Lieferungen Bar/Halle/Hangar/Bausatz-b/Schalter-b gemergt (PR #4–#8), integriert und eingebaut (max, 02.11.)
- [x] P24 Himmel & Außenwelt: Sonne, Planeten, Ring, Mond, Außenmodule, SetDay (max, 02.11.)
- [x] P23 Energiebarriere am Hangartor (Shader, Puls) (max, 02.11.)
- [x] P22 Grundriss 2.0: Hangar 36×28×16, Aussichtsplattform, Barfenster (max, 02.11.)
- [x] P21 Station aus Bausatz-Modulen (Wände, Böden, Decken, Schalter-Möbel, Bar) (max, 02.11.)
- [x] P20 Licht & Stimmung: Raumfarben, Leuchtstreifen, Nebel, Sternenhimmel, gebackenes Licht (max, 02.11.)
- [x] Repo, Team-Workflow und Setup-Anleitung (max, 29.10.)
- [x] Unity 6000.3.23f1 + WebGL bei Max installiert, Lizenz aktiv (max, 29.10.)
- [x] Martin (iMaterix) ins Repo eingeladen, Setup-Anleitung als Gist verschickt (max, 29.10.)
- [x] Timeline + Aufgabenteilung beschlossen (max + martin, 31.10.)
- [x] Konzept-Entwurf `docs/DESIGN.md`: Zollstation im All, 3 Tage, Englisch (max + martin, 31.10.)
- [x] Tech-Stack, Schriften, Luftschleuse/Moral, Art-Style-Richtung beschlossen (max + martin, 31.10.)
- [x] Paket-Konzept + erste 19 Pakete `docs/team/PAKETE.md` (max, 31.10.)
- [x] Asset-Pipeline angebunden: `ASSETS.md`, `ASSET-REQUESTS.md`, `STYLE.md` + Palette, Deploy-Key, PR-Ablauf (max, 31.10.)
- [x] Session-Briefing zeigt Asset-PRs und Asset-Status (max, 31.10.)

## Vor dem 01.11. noch offen
- [ ] martin: Setup nach Anleitung durchlaufen, klonen (Einladung angenommen ✔ 31.10.)
- [x] martin: itch.io-Benutzernamen an Max schicken → `iMaterix`
- [x] beide: gemeinsamen Chat einrichten (Discord)
- [x] beide: Ablauf zum Start: Theme anschauen, Gedanken machen, Abend-Call, gemeinsam festlegen
- [x] max: itch.io-Projektseite als Entwurf angelegt: https://juice-de-orange.itch.io/game-off-2026 (HTML, 960×540, Vollbild-Button, keine Zahlungen, KI-Kennzeichnung an)
- [x] max: Martin (`iMaterix`) als Admin + Mitautor auf der itch-Seite eingetragen
- [ ] martin: itch-Admin-Einladung annehmen: https://itch.io/game/accept-admin/5272072/19899599
- [ ] martin: Unity-Grundlagen anschauen: uGUI-Menü (Canvas, Buttons), Szenenwechsel (`SceneManager.LoadScene`)
- [ ] max: Tools installieren/testen: Blender (FBX-Export nach `docs/ASSETS.md`), Texturtool, Audacity
- [x] max: Deploy-Key mit Schreibrecht für die Asset-Pipeline eintragen (31.10., Titel `asset-pipeline`)
- [ ] max: Ab 01.11. (Repo öffentlich): `main` per Ruleset schützen — nur max/martin pushen direkt, Pipeline nur per PR
- [ ] **beide, Call 02.11.: Repo öffentlich machen.** Vorher die urheberrechtlich geschützten Referenzbilder (`docs/style/refs/ai_*`, `mw_*`)
  aus der Git-Historie entfernen (einmaliger Force-Push nach Absprache, Martin klont danach neu), Bilder lokal/privat weiter nutzen.
  Max hat zugestimmt (02.11.), Martin muss noch zustimmen. Repo ist bis dahin privat (Pflicht: öffentlich bis 01.12.).
- [ ] beide, Call 02.11.: **Audio höher priorisieren** — Audio ist eine der 6 Bewertungskategorien (P18 + Atmo-Sounds als Muss?).
- [x] Lizenz: Code MIT (`LICENSE`), eigene Assets vorbehalten, KI-Offenlegung im README (max, 02.11.)
- [x] max: 35 Referenzbilder in `docs/style/refs/` (31.10.)
- [ ] max: `docs/STYLE.md` + Palette prüfen, nach dem Theme freigeben → Pipeline baut 3 Muster-Assets
- [ ] max: ElevenLabs-Lizenz klären (Free-Plan erlaubt keine kommerzielle Nutzung, verlangt Namensnennung)
- [x] beide: Art-Style-Richtung: Mouthwashing-Grafik + Alien-Isolation-Assets (final am 02.11. nach dem Theme)
- [x] beide: Tech-Stack bestätigt (31.10., in `DECISIONS.md`)
- [ ] beide: `docs/DESIGN.md` (Entwurf) und `docs/ASSETS.md` lesen
