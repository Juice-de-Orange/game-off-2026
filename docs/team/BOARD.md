# Team-Board

> Pull vor jeder Änderung. Kleine Änderungen, sofort pushen — besonders bei Claims.

## Meilensteine
Rhythmus: 3 Wochen Entwicklung, 1 Woche Polish, 2 Tage Puffer.

| Datum | Ziel | Status |
|---|---|---|
| 01.11. | Theme kommt ca. **22:37 MEZ** → jeder für sich Gedanken, Repo öffentlich | ⏳ |
| 02.11. | Abend-Call: Theme ins Konzept einweben, `DESIGN.md` finalisieren, Unity-Projekt anlegen | ⏳ |
| 03.11. | Prototyp: Kern-Mechanik spielbar (graue Klötze reichen) | ⏳ |
| 05.11. | Erster WebGL-Testbuild auf itch.io (Draft) — danach jede Woche ein Build | ⏳ |
| 09.11. | Checkpoint: Trägt der Kern-Loop? Wenn nicht, jetzt umsteuern | ⏳ |
| 16.11. | Vertical Slice: ein Level komplett mit Ziel, Sieg/Niederlage, Menü, Neustart | ⏳ |
| 22.11. | **Feature-Freeze**: keine neuen Features mehr, nur Inhalte & Polish | ⏳ |
| 28.11. | **Content-Freeze**: nur noch Bugfixes, Audio, Juice | ⏳ |
| 29.–30.11. | Puffer: itch-Seite, finaler Build, Browser-Tests | ⏳ |
| 30.11. | **Abgabe** auf itch.io (offizielle Deadline 01.12., 22:37 MEZ) | ⏳ |

## Aufgabenteilung
- **max**: alle finalen Assets (Art, Sound, Musik).
- **martin**: Startbildschirm — alles vom Spielstart bis zum Menü. Baut mit Platzhalter-Assets, Max ersetzt sie.
- Abstimmung: immer wieder unter der Woche, am Wochenende intensiver. Verfügbare Zeit pro Woche ist offen und ergibt sich unterwegs.

## In Arbeit
Wer etwas bearbeitet, trägt es hier ein (`/claim`). Geclaimte Szenen/Prefabs/Assets fasst der andere nicht an.
Welche **Pakete** gerade wer baut, steht in `PAKETE.md`.

| Wer | Aufgabe | Geclaimte Dateien | Seit |
|---|---|---|---|
| | | | |

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
- [x] max: 35 Referenzbilder in `docs/style/refs/` (31.10.)
- [ ] max: `docs/STYLE.md` + Palette prüfen, nach dem Theme freigeben → Pipeline baut 3 Muster-Assets
- [ ] max: ElevenLabs-Lizenz klären (Free-Plan erlaubt keine kommerzielle Nutzung, verlangt Namensnennung)
- [x] beide: Art-Style-Richtung: Mouthwashing-Grafik + Alien-Isolation-Assets (final am 02.11. nach dem Theme)
- [x] beide: Tech-Stack bestätigt (31.10., in `DECISIONS.md`)
- [ ] beide: `docs/DESIGN.md` (Entwurf) und `docs/ASSETS.md` lesen
