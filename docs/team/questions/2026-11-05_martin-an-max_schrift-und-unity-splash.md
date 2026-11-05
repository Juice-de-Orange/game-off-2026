---
von: martin
an: max
status: erledigt
erstellt: 2026-11-05 09:58
blockiert: nein
---

# Drei Fragen zum Startbildschirm: Orbitron-Schrift, „Made with Unity“-Splash, Menü-Hintergrund in Main

## Kontext
Ich baue P02 (Startbildschirm & Menü: Boot-Splash, Hauptmenü, Optionen, Credits, Pausemenü).
Erzeugt wird das von `Scripts/Editor/MenuSceneBuilder.cs`, Menü **GameOff → Build Menu Scenes**.

## Frage 1: Orbitron
Laut `docs/DECISIONS.md` (31.10.) und `docs/DESIGN.md` soll für Titel und Menü **Orbitron** verwendet werden
und für UI-Texte **Share Tech Mono**. Im Projekt liegt aber nur Share Tech Mono (`Assets/_Project/Fonts/`).
Orbitron gibt es weder im Projekt noch in der Git-Historie, noch in einem Asset-Branch. In `docs/ASSET-REQUESTS.md`
steht sie nur als Stilvorgabe für die Schilder-Texturen.

Hast du Orbitron absichtlich weggelassen (z. B. weil die Schrift über die Pipeline kommen soll oder weil du dir
den Look noch überlegst), oder fehlt sie einfach nur?
- A) Fehlt nur: Martin holt Orbitron (OFL, Google Fonts) nach `Assets/_Project/Fonts/`, trägt sie in `CREDITS.md` ein
  und stellt Titel und Menü-Überschriften im Builder darauf um.
- B) Absichtlich: Die Schrift kommt von dir bzw. der Pipeline, Martin wartet darauf.
- C) Wir bleiben bei Share Tech Mono für alles (dann tragen wir das in `DECISIONS.md` ein).

## Frage 2: Unity-Splash („Made with Unity“)
Seit Unity 6 ist der Unity-Splash auch mit Unity Personal **optional**. Im Projekt ist er gerade an
(`ProjectSettings.asset`: `m_ShowUnitySplashScreen: 1`). Er erscheint nur im Build (auch im WebGL-Build auf itch.io),
und zwar vor unserem eigenen Boot-Splash. Im Browser kommen also 2–3 Sekunden zur Ladezeit dazu, dann folgen zwei Splashs
hintereinander. Unity wird ohnehin in unseren Credits genannt („Made with Unity for GitHub Game Off 2026“).

Sollen wir ihn abschalten (Project Settings → Player → Splash Image → „Show Splash Screen“ aus)?
- A) Abschalten: Martin stellt es um (gemeinsame Projekteinstellung, deshalb vorher die Frage).
- B) Anlassen.
- C) Hast du ihn absichtlich angelassen? Dann bitte kurz den Grund.

## Frage 3: Menü-Hintergrund später direkt in Main (Variante B)?
Im Hauptmenü sieht man jetzt die Station von außen (**Variante A**): `Menu.unity` enthält Instanzen von `PF_SkyRig`,
`PF_Exterior` und `PF_Station_Graybox`, und eine Kamera fährt langsam um die Station. Deine Prefabs werden nur benutzt,
nicht verändert; nach einem Neubau durch dich (z. B. Hülle M015) muss ich nur **GameOff → Build Menu Scenes** laufen lassen.
Einschränkung: Das gebackene Licht gehört zu Main. Im Menü leuchten deshalb nur die Echtzeit-Sonne und selbstleuchtende Teile.

**Variante B** wäre: Das Spiel startet direkt in Main mit einer Außenkamera, das Menü liegt darüber, und bei „Start Shift“
fliegt die Kamera in Zimmer 103 zum Spieler. Das sieht besser aus (echtes Licht, nahtloser Übergang), greift aber tief in
`Main.unity` und den Spielstart ein, also in deinen Bereich.
- A) Ja, später als eigenes Folgepaket (`Folgt auf: P02`), z. B. in der Polish-Woche.
- B) Nein, Variante A reicht.
- C) Ja, aber erst wenn P09 (Tagesablauf) steht, weil sich der Spielstart dadurch ändert.

## Annahme bis zur Antwort
Ich habe Frage 1 und 2 bewusst nicht angefasst, weil ich mir nicht sicher war. Für Frage 3 bleibt es bei Variante A:
- Das Menü läuft mit Share Tech Mono. Eine spätere Umstellung ist nur eine Konstante im `MenuSceneBuilder` plus einmal neu bauen.
- Der Unity-Splash bleibt an, bis du antwortest.

## Antwort

**Max, 05.11.:**
- **Frage 1 – A:** Orbitron fehlt nur. Bitte hol sie nach `Assets/_Project/Fonts/`, trag sie in `CREDITS.md` ein und stell Titel und
  Menü-Überschriften im Builder um.
- **Frage 2 – A:** Unity-Splash abschalten. Bitte stell du die Projekteinstellung um.
- **Frage 3 – A:** Ja, später als eigenes Folgepaket. Es steht als **P48** (Kann, für die Polish-Woche) in `PAKETE.md`.
  Bis dahin bleibt Variante A.

**Umgesetzt (martin, 05.11.):** Orbitron (`Fonts/Orbitron-Regular.ttf` + `Orbitron SDF.asset`, `CREDITS.md`) für Spieltitel, OPTIONS, CREDITS, PAUSED im `MenuSceneBuilder`; Unity-Splash in den Projekteinstellungen aus. Variante B bleibt P48.
