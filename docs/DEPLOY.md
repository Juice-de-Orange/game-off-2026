# Live-Version: kestrel9.maxlworks.com

Der aktuelle Stand von `main` läuft öffentlich auf Geoffrey (Contabo, 144.91.89.2) als Docker-Container. Paket: P49.
itch.io (P19) ist davon getrennt und bleibt der Weg für die Abgabe.

## Ablauf
```
GitHub Actions (.github/workflows/deploy-web.yml), nachts 03:00 Uhr MEZ oder per Knopf
  → prüft version.txt der Live-Seite: nur bauen, wenn sich Assets/, Packages/, ProjectSettings/ oder deploy/ geändert haben
  → Unity-WebGL-Build (WebBuild.Build) → Docker-Image kestrel9:<sha>
  → docker save | ssh geoffrey → /opt/kestrel9/deploy.sh
       prüft den Image-Namen, startet das Image, prüft die Antwort, bei Fehler zurück auf die vorige Version
```
**Stand 05.11.:** Der Workflow baut noch nicht, weil die Secrets fehlen. Bis dahin spielt max von Hand ein (unten).

Wer sofort eine neue Version will: GitHub → Actions → **Deploy web version** → **Run workflow**.
Schlägt ein Lauf fehl, bleibt die letzte Version online, und GitHub schickt eine Mail.

## Dateien
| Wo | Was |
|---|---|
| `Scripts/Editor/WebBuild.cs` | Build-Einstieg; setzt Vorlage und Statistik nur für diesen Build |
| `Assets/WebGLTemplates/Kestrel9/` | Webseite um das Spiel (Vollfenster, Ladebalken, Impressum und Datenschutz) |
| `deploy/Dockerfile`, `deploy/nginx.conf` | Container: nginx ohne Root-Rechte, ohne Access-Log, mit `version.txt` |
| `deploy/server/` | Vorlagen für Geoffrey: `deploy.sh`, `compose.yml`, `nginx-site.conf` |

Auf Geoffrey liegen diese Kopien: `/opt/kestrel9/{deploy.sh,compose.yml,.env}`, `/etc/nginx/sites-available/kestrel9`, eine Zeile in
`/etc/nginx/modules-enabled/60-stream-pete.conf`. **Änderungen an `deploy/server/` spielt der Workflow nicht ein, die müssen von Hand auf den Server.**

## Lokal bauen
Unity muss dafür geschlossen sein:
```
Unity.exe -batchmode -nographics -buildTarget WebGL -projectPath . -executeMethod GameOff.EditorTools.WebBuild.Build -customBuildPath <Ordner außerhalb des Repos> -quit -logFile -
```

Danach Unitys eigene Änderungen zurücksetzen: `ShareTechMono SDF.asset`, `Orbitron SDF.asset`, `ProjectSettings.asset`, `Assets/Resources/`.

## Von Hand einspielen
1. Commit pushen und dafür lokal bauen (oben). Der Commit bestimmt die Version: `S=$(git rev-parse <commit>)`.
2. Build als `site/` zusammen mit `deploy/Dockerfile` und `deploy/nginx.conf` nach Geoffrey in einen leeren Ordner kopieren, dort `docker build --build-arg GIT_SHA=$S -t kestrel9:$S .`
3. `docker save kestrel9:$S | SSH_ORIGINAL_COMMAND="deploy $S" /opt/kestrel9/deploy.sh` (startet, prüft, rollt bei Fehler zurück).
4. Prüfen: `curl https://kestrel9.maxlworks.com/version.txt`. Besucher sind nur Sekunden unterbrochen.

**Messen und durchspielen:** `node tools/measure-webgl.mjs <Build> <Ausgabe> [full|quick]`, mit `K9_CPU=4` für ein schwächeres Gerät; `node tools/drive-webgl.mjs <Build> <Ausgabe>` spielt per Skript mit echten Eingaben. Beide starten ein eigenes Chrome ohne Fenster und liefern nichts mit dem Spiel aus.

**Lokal im Browser testen:** Build-Ordner mit `python -m http.server` ausliefern. Den Stuhl kann die Automatisierung nicht erreichen; dafür in einer Testkopie der `index.html` die Instanz merken (`.then(function (i) { window.k9 = i; ...`), dann `k9.SendMessage('PF_CounterStation','Sit')`. Echte Tasten und den Weg zum Stuhl prüft ein Mensch.

## Secrets (GitHub → Settings → Secrets and variables → Actions)
| Name | Inhalt |
|---|---|
| `UNITY_LICENSE` | Inhalt von `C:\ProgramData\Unity\Unity_lic.ulf` (Unity Hub → Preferences → Licenses → Add → Get a free personal license) |
| `UNITY_EMAIL`, `UNITY_PASSWORD` | Unity-Login |
| `DEPLOY_KEY` | Privater SSH-Schlüssel, in `authorized_keys` von `max@geoffrey` auf `deploy.sh` festgelegt |

Fehlen Secrets, baut der Workflow nicht und zeigt nur eine Warnung. Setzen in einem eigenen Terminal, damit nichts im Verlauf steht: `python deploy/set-secrets.py` (liest den privaten Schlüssel aus `~/.ssh/kestrel9_deploy_key`, fragt den Unity-Login ab).

## Dev-Werkzeuge
Im Build standardmäßig aus, Besucher schalten sie unter Options → DEV ein. **Vor der Abgabe:** Reiter im `MenuSceneBuilder` entfernen und
`showDevTools` am `DayManager` ausschalten (Board-Backlog).

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
des SNI-Routers erfasst die Subdomain (14 Rotationen). Das Spiel speichert Einstellungen lokal im Browser (IndexedDB) und sendet nichts an uns.
