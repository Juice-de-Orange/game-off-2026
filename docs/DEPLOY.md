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

## Secrets (GitHub → Settings → Secrets and variables → Actions)
| Name | Inhalt |
|---|---|
| `UNITY_LICENSE` | Inhalt von `C:\ProgramData\Unity\Unity_lic.ulf` (Unity Hub → Preferences → Licenses → Add → Get a free personal license) |
| `UNITY_EMAIL`, `UNITY_PASSWORD` | Unity-Login |
| `DEPLOY_KEY` | Privater SSH-Schlüssel, in `authorized_keys` von `max@geoffrey` auf `deploy.sh` festgelegt |

Fehlen Secrets, baut der Workflow nicht und zeigt nur eine Warnung.

## Dev-Werkzeuge
Im Build standardmäßig aus, Besucher schalten sie unter Options → DEV ein. **Vor der Abgabe:** Reiter im `MenuSceneBuilder` entfernen und
`showDevTools` am `DayManager` ausschalten (Board-Backlog).

## Fehlersuche und Rollback (auf Geoffrey)
- Stand: `curl -s http://127.0.0.1:18092/version.txt`, `docker ps --filter name=kestrel9`, `docker logs kestrel9-web`
- Von Hand zurück: `docker images kestrel9` zeigt die vorige Version; in `/opt/kestrel9/.env` den Tag eintragen, dann
  `docker compose -f /opt/kestrel9/compose.yml up -d --no-build --pull never`
- Abschalten: `docker compose -f /opt/kestrel9/compose.yml down`. Die Zeile im SNI-Router und `sites-enabled/kestrel9` können bleiben.

## Datenschutz
Die Datenschutzerklärung von maxlworks.com verspricht wenig Logging. Deshalb haben Container und nginx-Site kein Access-Log; nur das Log
des SNI-Routers erfasst die Subdomain (14 Rotationen). Das Spiel speichert Einstellungen lokal im Browser (IndexedDB) und sendet nichts an uns.
