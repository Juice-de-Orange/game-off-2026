---
von: martin
an: max
status: beantwortet
erstellt: 2026-11-06 15:15
blockiert: nein
---

# Können wir auf dem Live-Server Bugreports aus dem Spiel sammeln?

## Kontext
Die Live-Version läuft auf Geoffrey (`kestrel9.maxlworks.com`, P49, `docs/DEPLOY.md`) als nginx-Container ohne eigenes Backend.
Martins Idee: Testspieler sollen Bugs direkt im Spiel melden, damit wir für die Bugfix-Phase alles an einem Ort haben.

## Martins Vorschlag
- **Kleines Fenster im Spiel** (z. B. Taste oder Knopf im Pausenmenü): Dort listet jeder Spieler die Bugs, die ihm in der Session auffallen.
  Einträge lassen sich während der Session sammeln.
- **SUBMIT-Knopf** am Ende: schickt die Liste an den Server, der sie erst einmal nur speichert.
- **Bugfix-Phase:** Wir holen uns alle Meldungen vom Server und arbeiten sie ab.
- **Versionsstempel:** Jede Meldung braucht die Version, auf der der Bug auftrat. Wie genau, weiß Martin nicht.

## Hinweise von Martins Claude (zum Abwägen, keine Entscheidung)
- **Version:** Die Live-Seite hat schon `version.txt` mit dem Commit (z. B. `acd5e58`). Das Spiel kann sie beim Start lesen oder der
  Build bekommt den Commit fest eingebaut (`WebBuild.Build`). So steht an jeder Meldung, welcher Stand es war, plus Szene, Tag,
  Uhrzeit und Browser.
- **Server:** nginx allein kann nichts speichern. Nötig wäre ein kleiner Dienst neben dem Spiel, z. B. ein Mini-Container, der
  POST `/api/bugreport` annimmt und je Meldung eine JSON-Datei oder eine Zeile in eine Datei schreibt. nginx leitet nur diesen Pfad weiter.
- **Schutz:** Größenlimit pro Meldung und ein einfaches Rate-Limit in nginx, damit niemand den Server vollschreibt. Nur Textfelder.
- **Datenschutz:** Die Seite hat Impressum und Datenschutzerklärung. Freitext von Spielern ist personenbezogen, sobald jemand
  seinen Namen hineinschreibt. Darum: keine IP speichern (das Access-Log ist schon aus), Hinweis im Fenster, Absatz in der Datenschutzerklärung.
- **Abholen:** per `scp` von Geoffrey oder ein Skript in `tools/`, das die Meldungen in eine Datei (z. B. `docs/team/bugs/`) schreibt.
- **itch.io:** Dort gibt es den Server nicht. Entweder schickt auch die itch-Version an Geoffrey (dann braucht der Dienst CORS)
  oder das Fenster gibt es nur in der Live-Version.

## Frage
- A) Ja, machen wir. Max baut den Server-Teil (Dienst, nginx, Datenschutz), Martin das Fenster im Spiel. Wir legen dafür ein Paket an.
- B) Ja, aber einfacher (z. B. nur ein Link auf ein Formular oder GitHub-Issues). Bitte beschreiben.
- C) Nein, nicht nötig.

## Annahme bis zur Antwort
Nichts wird gebaut.

## Antwort
Zwischenstand (Max, 06.11.): Entscheiden wir später. Die Frage bleibt offen, es wird nichts gebaut.

**B** (Max, 07.11.): Einfacher, ohne eigenen Server. Ein Knopf im Pausenmenü („REPORT BUG“) öffnet ein vorausgefülltes GitHub-Issue (`/issues/new?title=…&body=…`) mit Version (Commit aus `version.txt` bzw. im Build), Tag, Phase, Szene und Browser. Kein Backend, kein neuer Datenschutz-Aufwand, funktioniert auch auf itch.io. Nachteil: Spieler brauchen ein GitHub-Konto – für Testspieler in Ordnung. Bitte als eigenes Paket anlegen (Kann/Soll); bauen kann es, wer zuerst Zeit hat (das Pausenmenü baut `Build Menu Scenes`).
