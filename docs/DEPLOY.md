# Live-Version: kestrel9.maxlworks.com

Der aktuelle Stand von `main` läuft öffentlich auf Geoffrey (Contabo, 144.91.89.2) als Docker-Container. Paket: P49.
itch.io (P19) ist davon getrennt und bleibt der Weg für die Abgabe.

> **Stand 09.11.:** Deploy von Hand, der Nacht-Workflow unten läuft nicht und bleibt aus (P49 geschlossen). itch.io: Entwurf `juice-de-orange.itch.io/game-off-2026`, noch nicht öffentlich.

## Ablauf
```
GitHub Actions (.github/workflows/deploy-web.yml), nachts 03:00 Uhr MEZ oder per Knopf
  → prüft version.txt der Live-Seite: nur bauen, wenn sich Assets/, Packages/, ProjectSettings/ oder deploy/ geändert haben
  → Unity-WebGL-Build (WebBuild.Build) → Docker-Image kestrel9:<sha>
  → docker save | ssh geoffrey → /opt/kestrel9/deploy.sh
       prüft den Image-Namen, startet das Image, prüft die Antwort, bei Fehler zurück auf die vorige Version
```
**Stand 09.11.:** Der Workflow baut noch nicht, weil die Secrets fehlen. Bis dahin wird von Hand eingespielt (unten); Max erlaubt Claude das seit dem 08.11. ausdrücklich. Live: `f7c50cc` (13.11.: P146–P150 Release-Schalter, Wachen, Pause im Flug, Komplizin; davor `b8b9cc1` P141–P145). Max' Claude spielt seit 12.11. selbst ein (Skript `~/.claude/scripts/kestrel9-deploy.sh <sha>` mit Freigabe in seiner lokalen `.claude/settings.local.json`); blockt der Auto-Modus doch einmal, startet max es mit `! bash <skript>`.

Wer sofort eine neue Version will: GitHub → Actions → **Deploy web version** → **Run workflow**.
Schlägt ein Lauf fehl, bleibt die letzte Version online, und GitHub schickt eine Mail.

## Dateien
| Wo | Was |
|---|---|
| `Scripts/Editor/WebBuild.cs` | Build-Einstieg; setzt Vorlage, Statistik und Version (`0.1.0+<commit>`, für den Bugreport-Knopf P78) nur für diesen Build. `Build` = Live-Version, `BuildRelease` = Abgabe (P146, unten) |
| `Assets/WebGLTemplates/Kestrel9/` | Webseite um das Spiel (Vollfenster, Ladebalken, Impressum und Datenschutz); für Suchmaschinen freigegeben: Beschreibung, Vorschaubild `og.jpg`, `robots.txt`, `sitemap.xml`, strukturierte Daten (VideoGame). Spielseite mit Text: `maxlworks.com/games/kestrel-9/` (Repo `maxlworks-website`) |
| `deploy/Dockerfile`, `deploy/nginx.conf` | Container: nginx ohne Root-Rechte, ohne Access-Log, mit `version.txt` |
| `deploy/server/` | Vorlagen für Geoffrey: `deploy.sh`, `compose.yml`, `nginx-site.conf` |
| `deploy/reports/` | Bugreport-Backend (P84): `server.py` (Python ohne Pakete), `Dockerfile`, `test_server.py` |
| `deploy/set-report-token.py` | Legt den GitHub-Token des Backends auf Geoffrey ab (im eigenen Terminal ausführen) |

Auf Geoffrey liegen diese Kopien: `/opt/kestrel9/{deploy.sh,compose.yml,.env,reports.env,reports/}`, `/etc/nginx/sites-available/kestrel9`, eine Zeile in
`/etc/nginx/modules-enabled/60-stream-pete.conf`. **Änderungen an `deploy/server/` spielt der Workflow nicht ein, die müssen von Hand auf den Server.**

## Lokal bauen
Unity muss dafür geschlossen sein:
```
Unity.exe -batchmode -nographics -buildTarget WebGL -projectPath . -executeMethod GameOff.EditorTools.WebBuild.Build -customBuildPath <Ordner außerhalb des Repos> -quit -logFile -
```

Danach Unitys eigene Änderungen zurücksetzen: `ShareTechMono SDF.asset`, `Orbitron SDF.asset`, `ProjectSettings.asset`, `Assets/Resources/`, `LiberationSans SDF - Fallback.asset`.

## Von Hand einspielen
1. Commit pushen und dafür lokal bauen (oben). Der Commit bestimmt die Version: `S=$(git rev-parse <commit>)`.
2. Build als `site/` zusammen mit `deploy/Dockerfile` und `deploy/nginx.conf` nach Geoffrey in einen leeren Ordner kopieren, dort `docker build --build-arg GIT_SHA=$S -t kestrel9:$S .`
3. `docker save kestrel9:$S | SSH_ORIGINAL_COMMAND="deploy $S" /opt/kestrel9/deploy.sh` (startet, prüft, rollt bei Fehler zurück).
4. Prüfen: `curl https://kestrel9.maxlworks.com/version.txt`. Besucher sind nur Sekunden unterbrochen.
   Von Windows aus: `C:/Windows/System32/OpenSSH/ssh.exe geoffrey` nehmen (der Schlüssel liegt im Windows-ssh-agent, Git Bashs `ssh` kennt ihn nicht); Docker Desktop muss dafür nicht laufen, gebaut wird auf Geoffrey.

**Messen und durchspielen:** `node tools/measure-webgl.mjs <Build> <Ausgabe> [full|quick]`, mit `K9_CPU=4` für ein schwächeres Gerät; `node tools/drive-webgl.mjs <Build> <Ausgabe>` spielt per Skript mit echten Eingaben. Beide starten ein eigenes Chrome ohne Fenster und liefern nichts mit dem Spiel aus.

**Lokal im Browser testen:** Build-Ordner mit `python -m http.server` ausliefern. Den Stuhl kann die Automatisierung nicht erreichen; dafür in einer Testkopie der `index.html` die Instanz merken (`.then(function (i) { window.k9 = i; ...`), dann `k9.SendMessage('PF_CounterStation','Sit')`. Echte Tasten und den Weg zum Stuhl prüft ein Mensch.

## Secrets (GitHub → Settings → Secrets and variables → Actions)
| Name | Inhalt |
|---|---|
| `UNITY_LICENSE` | Inhalt von `C:\ProgramData\Unity\Unity_lic.ulf` (Unity Hub → Preferences → Licenses → Add → Get a free personal license) |
| `UNITY_EMAIL`, `UNITY_PASSWORD` | Unity-Login |
| `DEPLOY_KEY` | Privater SSH-Schlüssel, in `authorized_keys` von `max@geoffrey` auf `deploy.sh` festgelegt |

Fehlen Secrets, baut der Workflow nicht und zeigt nur eine Warnung. Setzen in einem eigenen Terminal, damit nichts im Verlauf steht: `python deploy/set-secrets.py` (liest den privaten Schlüssel aus `~/.ssh/kestrel9_deploy_key`, fragt den Unity-Login ab).

## Bugreport-Backend (P84)
Das Pausenmenü schickt Berichte an `https://kestrel9.maxlworks.com/api/report`. Der nginx im Spiel-Container leitet nur diese Adresse an den
Container `kestrel9-reports` weiter; der legt mit einem Bot-Token ein GitHub-Issue an (Labels `bug`, `player-report`). Keine Begrenzung der Anzahl
(entfernt am 08.11. auf Max' Wunsch; bei Spam `docker stop kestrel9-reports`). CORS nur für diese Seite und `*.itch.zone`/`*.itch.io`.
- **Reihenfolge:** Das Image `kestrel9-reports:latest` muss auf Geoffrey existieren, bevor `compose.yml` mit dem Dienst `reports` liegt – sonst
  scheitert jeder Spiel-Deploy (`deploy.sh` startet alle Dienste). Ändern: `deploy/reports` nach `/opt/kestrel9/reports` kopieren,
  `docker build -t kestrel9-reports:latest /opt/kestrel9/reports`, `docker compose -f /opt/kestrel9/compose.yml up -d --no-build --pull never reports`.
- **Token:** Fine-grained, nur dieses Repo, nur Issues lesen/schreiben. Setzen oder erneuern: `python deploy/set-report-token.py` (fragt verdeckt).
- **Prüfen:** `docker exec kestrel9-reports wget -qO- http://127.0.0.1:8000/api/health` → `"configured": true`; Tests lokal
  `python -B -m unittest discover -s deploy/reports`.
- **Abschalten:** `docker stop kestrel9-reports`. Das Spiel lädt weiter, nur der Bericht scheitert (502, das Formular sagt „try again later“).
- Host-nginx (`sites-available/kestrel9`) setzt `X-Forwarded-For $remote_addr`, das Backend wertet es seit dem 08.11. nicht mehr aus (Sicherung: `/opt/kestrel9/nginx-site.before-p84`).

## Spielmetriken (P129)
> **Stand 10.11. abends:** Backend mit Metriken live (`/api/health` → `"metrics": true`). Das Spiel läuft noch als Build `37b758d`, aber im Image
> `kestrel9:37b758d` stecken jetzt der neue Spiel-nginx und das visit-Skript samt Fußzeile (nachträglich eingebaut, Original: `kestrel9:pre-p129`).
> Es zählen also Besuche und Herkunft; Spielereignisse kommen erst mit dem nächsten Unity-Build. Sicherungen auf Geoffrey:
> `compose.yml.before-p129`, `reports.before-p129/`. Ein Test-Besuch mit `src=deploytest` steht in `metrics-2026-11-10.jsonl`.

Der WebGL-Build schickt anonyme Spielereignisse an `https://kestrel9.maxlworks.com/api/metrics` (`Scripts/Metrics/`, startet sich selbst, im Editor aus).
Derselbe Container `kestrel9-reports` hängt sie als JSONL an `/opt/kestrel9/metrics/metrics-JJJJ-MM-TT.jsonl` (UTC) an und löscht Tage, die älter als
90 Tage sind (`METRICS_DAYS`). Höchstens 200 MB je Tag, danach 503. Der Spiel-nginx löscht `X-Forwarded-For` auf dieser Route.
- **Einrichten auf Geoffrey (einmal):** `sudo install -d -o 10001 -g 10001 -m 750 /opt/kestrel9/metrics`, dann wie beim Bugreport `deploy/reports` und
  `compose.yml` kopieren, Image neu bauen, `reports` neu starten, danach das Spiel neu deployen (der neue Spiel-nginx kennt `/api/metrics`).
  Prüfen: `/api/health` zeigt `"metrics": true`; der Start-Log sagt `metrics on`.
- **Ohne Ordner** (oder nicht beschreibbar) antwortet `/api/metrics` mit 404, und das Spiel hört für diese Sitzung auf zu senden. Ebenso bei 403
  (lokale Builds). **Abschalten:** Ordner umbenennen und `reports` neu starten.
- **Auswerten:** `scp 'geoffrey:/opt/kestrel9/metrics/*.jsonl' metrics/` und `python tools/metrics_report.py metrics [--since 2026-11-12] [--version <commit>]`.
  Zeigt je Tag (Wiener Zeit) Besuche, geladene Spiele, Spieler und Spielzeit, dieselben Zahlen je Herkunft, dann Sitzungen, wo Sitzungen enden, erreichte Tage, Fehlerquote je Reisendem und häufigste Rügen, Geld/Familie je Tag, Enden, FPS je Browser
  und Phase, Fehlermeldungen.

| Ereignis | Wann | Felder (außer `ts` Serverzeit, `s` Sitzung, `v` Version, `e`, `t` Sekunden seit Spielstart) |
|---|---|---|
| `visit` | Seite geöffnet (sendet `index.html` selbst, vor dem Laden) | `src` (aus `?src=` im Link), `ref` (nur Hostname der verweisenden Seite), `mobile`, `w`/`h` |
| `session` | Spiel geladen | `ua` Browser, `w`/`h` Fenster, `gpu`, `mem`, `cpu`, `quality`, `lang`, `load` Sekunden bis zum Start |
| `day_start` | Morgen | `day`, `story` (Geschichtstag), `money`, `family` (Zustand je Person, 0 gut … 4 tot) |
| `traveler` | jede Entscheidung | `day`, `n`, `id` (Asset-Name), `chosen`, `right`, `ok`, `sec` (ohne Pause), `why` (Rüge bei Fehler), `disobeyed` |
| `shift_end` | Schichtende | `day`, `decided`, `wrong`, `citations`, `sec`, `money` |
| `day_end` | nach der Abrechnung | `day`, `money`, `earned`, `spent`, `ledger` („Grund=Betrag; …“), `family`, `evening` Sekunden, `curfew` (23 Uhr erreicht) |
| `run_end` | Endbildschirm | `day`, `ending` (Titel), `money`, `family` |
| `beat` | alle 30 s | `day`, `phase` (Menu, Morning, Shift, Evening, Night, Finished), `fps`, `slow` (Bilder > 50 ms), `worst_ms`, `paused` |
| `error` | Fehler/Exception im Log | `type`, `msg`, `at` (erste Stack-Zeile), `day`, `phase`; je Meldung einmal, höchstens 25 je Sitzung |

Die Sitzungskennung ist zufällig und lebt nur im Speicher der Seite (`window.K9_SESSION`, das Spiel übernimmt sie über `Browser.jslib`):
nichts im Browser gespeichert, Neuladen = neue Sitzung, keine IP. Spieler sind deshalb Sitzungen, nicht Personen.
**Herkunft messen:** Links mit `?src=<name>` teilen, z. B. `https://kestrel9.maxlworks.com/?src=playtesters`. Ohne `src` zählt der Hostname der
verweisenden Seite (Reddit schickt meist nur `www.reddit.com`, die Reddit-App oft gar nichts).

## Dev-Werkzeuge
Im Live-Build standardmäßig aus, Besucher schalten sie unter Options → DEV ein. Der Abgabe-Build (`WebBuild.BuildRelease`, P146) wird mit
`K9_RELEASE` kompiliert (nur dieser Build, Player Settings bleiben ohne Define): `GameSettings.DevToolsAllowed` ist dort `false`, damit fehlen
der DEV-Reiter (samt TRAFFIC_LINE_DEV und CREDITS ±100), die roten `Dev_…`-Knöpfe, die sichtbaren Hitboxen, Dev-Startgegenstände und Dev-Figuren.
Szene und Prefabs bleiben gleich, nichts muss vor der Abgabe umgebaut werden. Neue Dev-Werkzeuge laufen über `GameSettings.DevTools`,
`DayManager.DevTools` oder `DayManager.DevToolsOn(day)`, sonst landen sie im Abgabe-Build.

## Fehlersuche und Rollback (auf Geoffrey)
- Stand: `curl -s http://127.0.0.1:18092/version.txt`, `docker ps --filter name=kestrel9`, `docker logs kestrel9-web`
- Von Hand zurück: `docker images kestrel9` zeigt die vorige Version; in `/opt/kestrel9/.env` den Tag eintragen, dann
  `docker compose -f /opt/kestrel9/compose.yml up -d --no-build --pull never`
- Abschalten: `docker compose -f /opt/kestrel9/compose.yml down`. Die Zeile im SNI-Router und `sites-enabled/kestrel9` können bleiben.

## Maussperre im Browser
Lehnt der Browser die Maussperre ab (Fenster ohne Fokus, kurz nach Esc), öffnet Unitys Loader ein blockierendes `alert`, und das Spiel steht.
Die Vorlage `Kestrel9` fängt das über `errorHandler` ab. **Für den itch-Build (P19) muss die dort verwendete Vorlage dasselbe tun.**

## Datenschutz
Die Datenschutzerklärung von maxlworks.com verspricht wenig Logging. Deshalb haben Container und nginx-Site kein Access-Log; nur das Log
des SNI-Routers erfasst die Subdomain (14 Rotationen). Das Spiel speichert Einstellungen lokal im Browser (IndexedDB). Gesendet wird nur, was ein Spieler selbst als Bugreport abschickt (P84):
sein Text plus Version, Tag, Phase, Szene und Browserkennung; daraus wird ein GitHub-Issue (GitHub Inc., USA; ab der Abgabe öffentlich).
Die IP wird seit dem 08.11. nicht mehr ausgewertet (vorher nur für die Begrenzung) und weder gespeichert noch geloggt.
Seit P129 (10.11.) schickt der Build außerdem anonyme Spielmetriken (Abschnitt oben), gespeichert 90 Tage auf Geoffrey; die Fußzeile der Live-Seite
weist darauf hin. Vorschlag für die Datenschutzerklärung: „Die Testversion von Kestrel-9 sendet anonyme Spielstatistiken (z. B. Spieltag, Entscheidungen,
Bildrate, Browserkennung, Fehlermeldungen, die Website, von der man kam) an unseren Server, um das Spiel zu verbessern. Es werden keine IP-Adressen, Cookies oder dauerhaften Kennungen
gespeichert; jede Sitzung erhält eine zufällige Kennung, die nur bis zum Schließen der Seite existiert. Die Daten werden nach 90 Tagen gelöscht.“
**Die Datenschutzerklärung auf maxlworks.com muss das erwähnen (Absatz von Claude, 07.11., in Max' Handover).**

## itch.io (P19, Abgabe)
Upload braucht Max' itch-Login (`butler login` einmalig), deshalb nicht automatisch.
1. Build: `Unity.exe -batchmode -nographics -buildTarget WebGL -projectPath . -executeMethod GameOff.EditorTools.WebBuild.BuildRelease -customBuildPath Builds/itch -quit` (oder Menü GameOff → Build Release (itch)). Release = Vorlage `Kestrel9Itch` + `K9_RELEASE` (ohne Dev-Werkzeuge); `WebBuild.Build -itch` baut dieselbe Vorlage mit Dev-Werkzeugen, nur zum Testen. Die Vorlage `Kestrel9Itch` hat keine Impressum-Fußzeile (nur für die Live-Seite) und fängt abgelehnte Maussperren genauso ab.
2. `butler push Builds/itch <user>/<spiel>:html5`, oder den Ordner gezippt auf der itch-Seite hochladen. **Nicht mit PowerShell `Compress-Archive` packen:** Das schreibt `\` in die Pfade, auf itch fehlt dann `Build/` (404, „the download failed“). Python `zipfile` mit `/` funktioniert.
3. itch-Seite: „This file will be played in the browser“, Viewport 1280×720, Fullscreen-Button an. In Chrome und Firefox prüfen.

## Checkliste Abgabetag (P146)
Team reicht am **30.11.** ein; harte Frist **01.12., 13:37 PST = 22:37 MEZ**. Upload und Einreichung selbst: P19.
1. **Stand:** `git pull`, BOARD ohne offene Claims, EditMode-Tests grün, `GameOff/Validate Travelers` und `GameOff/Economy Report` ✓.
2. **Release-Build:** Unity zu, dann
   `Unity.exe -batchmode -nographics -buildTarget WebGL -projectPath . -executeMethod GameOff.EditorTools.WebBuild.BuildRelease -customBuildPath Builds/itch -quit -logFile -`.
   Log endet mit `[WebBuild] Release Succeeded`. Danach Unitys Seiteneffekt-Dateien zurücksetzen (Abschnitt „Lokal bauen“).
3. **Decompression Fallback:** Player Settings → Publishing Settings an (`webGLDecompressionFallback: 1` in `ProjectSettings.asset`), sonst lädt itch den Build nicht.
4. **Lokal prüfen** (`python -m http.server` im Build-Ordner, Chrome **und Firefox**): lädt, Ton nach dem ersten Klick; Optionen ohne DEV-Reiter;
   kein roter `Dev_…`-Knopf (Bett während der Schicht, Kiosk tagsüber); keine Hitbox-Kästen; Konsole beim Start ohne Meldungen aus unserem Code.
5. **ZIP mit Vorwärts-Schrägstrichen** (nie `Compress-Archive`), `index.html` muss oben im ZIP liegen:
   `python -c "import pathlib,sys,zipfile; r=pathlib.Path(sys.argv[1]); z=zipfile.ZipFile(sys.argv[2],'w',zipfile.ZIP_DEFLATED); [z.write(p,p.relative_to(r).as_posix()) for p in sorted(r.rglob('*')) if p.is_file()]; z.close()" Builds/itch Builds/kestrel9-itch.zip`,
   prüfen mit `python -m zipfile -l Builds/kestrel9-itch.zip` (`Build/…`, nicht `Build\…`).
6. **itch-Seite:** ZIP hochladen (oder `butler push`), „played in the browser“, Viewport (hier 1280×720, der Entwurf stand laut BOARD auf 960×540: einmal festlegen),
   Fullscreen-Button an; Titelbild, Screenshots, Beschreibung, Genre und Tags; KI-Kennzeichnung (auch Musik und Laute sind von der KI-Pipeline per Code erzeugt);
   Credits bzw. Link auf `CREDITS.md` (Schriften unter OFL); Hinweis auf Bugreport und anonyme Spielstatistik (P129, laufen auch im itch-Build);
   auf der fertigen Seite noch einmal in Chrome und Firefox spielen. Sichtbarkeit erst auf öffentlich, wenn Max es freigibt.
7. **Repo öffentlich** (heute privat, Pflicht vor 01.12., 22:37 MEZ): vorher prüfen, dass keine Secrets im Verlauf liegen; GitHub → Settings → Change visibility → Public.
   Bugreport-Issues werden damit ebenfalls öffentlich.
8. **Jam-Einreichung** auf der Game-Off-Seite bei itch.io: Projekt auswählen, Repo-Link angeben, abschicken; danach die Einreichung in der Jam-Liste ansehen.
9. Die Live-Version auf kestrel9 bleibt der normale Build (`WebBuild.Build`, Dev-Werkzeuge als Schalter).
