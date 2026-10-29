# Setup-Anleitung für Game Off 2026 — für Claude Code

> **An den Menschen:** Öffne Claude Code in einem beliebigen Ordner und schreib:
> „Lies diese Datei und führ mich durch das Setup: <Pfad zu dieser Datei>"
> Claude stellt dir Fragen und erledigt den Rest. Du brauchst ca. 1–2 Stunden (Unity-Download ist groß).

---

## Anweisungen an Claude

Du richtest den Rechner eines **absoluten Anfängers** für den Game Jam *GitHub Game Off 2026* ein.
Er hat Claude Code seit heute, noch **kein Unity, evtl. keinen GitHub-Account**, und arbeitet unter **Windows**.
Sein Teampartner ist **Max** (GitHub & itch.io: `Juice-de-Orange`), der das Projekt schon vorbereitet hat.

### Wie du vorgehst
- Sprich **Deutsch**, einfach und ohne Fachjargon. Erkläre bei jedem Schritt in einem Satz, *wozu* er gut ist.
- **Ein Schritt nach dem anderen.** Prüfe nach jedem Schritt, ob er wirklich geklappt hat (Befehl ausführen, Ausgabe ansehen), bevor du weitergehst.
- **Rückfragen immer als Multiple-Choice** (AskUserQuestion-Tool), nie als offene Frage im Text. Empfohlene Option zuerst und markiert.
- Was du selbst per Befehl erledigen kannst, erledigst du selbst. Was er im Browser oder in einem Fenster tun muss (Accounts anlegen, Logins), erklärst du Klick für Klick und wartest auf seine Bestätigung.
- Interaktive Logins: Sag ihm, er soll den Befehl mit `!` davor in Claude Code eintippen (z. B. `! gh auth login --web`). Klappt das nicht, soll er ihn in einem separaten PowerShell-Fenster ausführen.
- Prüfe zu Beginn, was schon installiert ist, und überspring Erledigtes.
- Führe am Ende eine Checkliste und sag ihm genau, was er an Max schicken muss.
- Bei Fehlern: Ursache erklären, Lösung versuchen; nach 2–3 Fehlversuchen anhalten und ihm sagen, er soll Max fragen.

### Feste Fakten (nicht ändern, nicht nachfragen)
| Was | Wert |
|---|---|
| Jam-Seite | https://itch.io/jam/game-off-2026 |
| Start / Theme | 1. November 2026 |
| Abgabe | 1. Dezember 2026, 13:37 PST = **22:37 Uhr MEZ** (Team plant 30.11.) |
| Engine | **Unity 6000.3.23f1** — exakt diese Version, keine andere |
| Unity-Modul | **Web Build Support (WebGL)** — Pflicht |
| Repo | https://github.com/Juice-de-Orange/game-off-2026 (privat bis 1.11., dann öffentlich) |
| Lokaler Ordner | `C:\Game-Off-2026` (empfohlen) |

---

## Schritte

### 1. Bestandsaufnahme
Prüfe: `winget --version`, `git --version`, `git lfs version`, `gh --version`, ob Unity Hub installiert ist
(`C:\Program Files\Unity Hub`), ob die Unity CLI da ist (`unity --version`), und ob `C:\Game-Off-2026` existiert.
Zeig ihm kurz, was fehlt.

### 2. Werkzeuge installieren
Fehlende Tools per winget (jeweils mit `--accept-source-agreements --accept-package-agreements`):
- Git: `winget install --id Git.Git -e` (enthält Git LFS)
- GitHub CLI: `winget install --id GitHub.cli -e`
- Unity Hub: `winget install --id Unity.UnityHub -e`

Danach ggf. neues Terminal nötig, damit die Befehle gefunden werden. Dann `git lfs install` ausführen.

### 3. GitHub-Account
- Frage, ob er schon einen GitHub-Account hat.
- Falls nein: Er legt ihn auf https://github.com/signup an (Benutzername gut überlegen — steht später in den Credits).
- Login: `! gh auth login --web --hostname github.com --git-protocol https` → Code im Browser eingeben.
- Prüfen: `gh auth status`.
- Git-Identität setzen (mit der anonymen GitHub-Noreply-Adresse, damit seine echte Mail nicht öffentlich im Repo steht):
  - Login: `gh api user --jq .login`, ID: `gh api user --jq .id`
  - `git config --global user.name "<login>"`
  - `git config --global user.email "<id>+<login>@users.noreply.github.com"`

### 4. itch.io-Account und Jam beitreten
- Account auf https://itch.io/register anlegen (oder „Log in with GitHub").
- Auf https://itch.io/jam/game-off-2026 auf **„Join jam"** klicken. Damit kann er später auch bewerten.

### 5. Unity installieren
1. Unity-Account anlegen: https://id.unity.com (oder im Unity Hub „Sign in" → „Create account").
2. Unity Hub starten, anmelden. Der Hub bietet eine kostenlose **Personal-Lizenz** an → annehmen
   (Preferences → Licenses → Add → „Get a free personal license", falls nicht automatisch).
3. Unity **6000.3.23f1** mit **Web Build Support** installieren. Zwei Wege — empfiehl Weg A:
   - **A) Per Unity CLI** (du machst es):
     PowerShell: `$env:UNITY_CLI_CHANNEL='beta'; irm https://public-cdn.cloud.unity3d.com/hub/prod/cli/install.ps1 | iex`
     dann neues Terminal und `unity install 6000.3.23f1 --module webgl --yes --accept-eula`
     (Modulliste prüfen mit `unity install-modules -e 6000.3.23f1 --list`; falls dort Visual Studio Community angeboten wird, mitinstallieren — das ist der Code-Editor für C#.)
   - **B) Per Unity Hub** (er klickt): Installs → Install Editor → Archive → „download archive" → 6000.3.23f1 suchen → Module **Web Build Support** und **Microsoft Visual Studio Community** anhaken.
     Achtung: Hub bietet standardmäßig eine *andere* Version an — es muss genau 6000.3.23f1 sein.
4. Prüfen: Im Hub unter „Installs" steht 6000.3.23f1 mit WebGL-Symbol, bzw. `unity editors --installed`.

Der Download ist mehrere GB groß — währenddessen mit Schritt 6/7 weitermachen.

### 6. Max Bescheid geben
Er schickt Max: **GitHub-Benutzername** und **itch.io-Benutzername**.
Max lädt ihn dann ins GitHub-Repo ein (und später auf itch.io als Mitautor der Spielseite).

### 7. Repo-Einladung annehmen und Projekt klonen
Sobald Max ihn eingeladen hat:
- Einladungen ansehen: `gh api /user/repository_invitations --jq '.[] | {id, repo: .repository.full_name}'`
- Annehmen: `gh api -X PATCH /user/repository_invitations/<id>`
  (alternativ: Link aus der Einladungs-Mail oder https://github.com/Juice-de-Orange/game-off-2026/invitations)
- Klonen: `gh repo clone Juice-de-Orange/game-off-2026 C:\Game-Off-2026`
- Prüfen: In `C:\Game-Off-2026` liegen `README.md`, `CLAUDE.md`, `CREDITS.md`.

Hinweis: Das Unity-Projekt selbst legt Max erst am 1.11. nach der Theme-Bekanntgabe an.
Bis dahin ist das Repo bewusst fast leer — das ist richtig so.

### 8. Mini-Git-Kurs (5 Minuten)
Erkläre ihm kurz und praktisch:
- Vor dem Arbeiten: `git pull` — holt Max' Änderungen.
- Nach dem Arbeiten: speichern → committen → `git push`. Klein und oft.
- **Szenen und Prefabs nie gleichzeitig mit Max bearbeiten** — die lassen sich nicht sauber zusammenführen. Vorher kurz im Chat absprechen; jeder arbeitet am besten in eigenen Szenen/Prefabs.
- Er kann Claude im Projektordner jederzeit sagen: „Hol die neuesten Änderungen" oder „Committe und pushe meine Arbeit" — Claude kennt die Teamregeln aus der `CLAUDE.md` im Repo.
- Externe Grafiken/Sounds immer mit Quelle und Lizenz in `CREDITS.md` eintragen.

### 9. Ab dem 1. November (nur erklären, nicht jetzt ausführen)
1. Max legt das Unity-Projekt an und pusht es.
2. Er macht `git pull` in `C:\Game-Off-2026`.
3. Unity Hub → Projects → **Add → Add project from disk** → `C:\Game-Off-2026` wählen → mit 6000.3.23f1 öffnen.
   Wenn Unity anbietet, die Version zu wechseln oder zu upgraden: **Nein**.
4. Erster Start dauert ein paar Minuten (Unity baut den `Library`-Ordner auf).

### 10. Abschluss-Checkliste
Zeig ihm am Ende diese Liste mit ✅/❌ und sag, was noch offen ist:
- [ ] Git, Git LFS, GitHub CLI installiert
- [ ] GitHub-Account, `gh auth status` ok, Git-Name und Noreply-Mail gesetzt
- [ ] itch.io-Account, Game Off 2026 beigetreten
- [ ] Unity-Account, Unity Hub angemeldet, Personal-Lizenz aktiv
- [ ] Unity 6000.3.23f1 + Web Build Support (+ Visual Studio) installiert
- [ ] GitHub- und itch.io-Benutzername an Max geschickt
- [ ] Repo-Einladung angenommen, Repo nach `C:\Game-Off-2026` geklont
- [ ] Mini-Git-Kurs verstanden
- [ ] Gemeinsamer Chat (z. B. Discord) mit Max eingerichtet
