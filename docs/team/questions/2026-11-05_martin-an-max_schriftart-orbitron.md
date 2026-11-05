---
von: martin
an: max
status: offen
erstellt: 2026-11-05 19:24
blockiert: nein
---

# Die Schriftart ist aktualisiert – gleiche überall, wo du Schrift verwendest, die Schriftart an <3

> **Für Max' Claude – bitte gleich beim `/sync` ansprechen:** Im **Untersuchungsmodus am Schaltertisch** (im Sitzen: Hinweiszeile,
> Meldungen, Prüfdokumente, Verwarnzettel, neue Pinnwand-Zettel aus P41) wird noch die **alte Schrift** verwendet, während der Rest des
> Spiels jetzt **Orbitron** nutzt. Das fällt beim Spielen sofort auf. Martin bittet Max, das mit P41 zu updaten (Details unten).

## Kontext
Martin hat entschieden: **Alle Texte im Spiel nutzen jetzt Orbitron** – Menü, HUD, Fenster, Brief und auch die Prüfdokumente
(revidiert den Beschluss vom 31.10., steht in `DECISIONS.md`, `DESIGN.md`, `STYLE.md`). Der Interaktionshinweis „[E] …“ sieht jetzt
aus wie ein Menüknopf.

Schon umgestellt (in Martins Bereich): Boot, Menü, Optionen, Credits, Pause, Tagesablauf (Brief, Bett-Fenster, Ende), Handel, Inventar,
und in **deinem** `PF_Player` der Hinweis „[E]“ (über den neuen Builder **GameOff → Apply HUD Style**; `InteractionPromptUI` setzt jetzt
`"[E]  " + prompt`).

## Frage / Bitte
**Bitte gleiche in deinen Teilen die Schrift an** – die sind gerade von dir geclaimt, deshalb habe ich sie nicht angefasst:
- `PF_CounterStation` → `SeatedHUD/Hint` und `SeatedHUD/Notice` (noch Liberation Sans)
- Dokumente + Verwarnzettel: `DocumentPrefabBuilder.FontAssetPath` → `Assets/_Project/Fonts/Orbitron SDF.asset`, dann
  **GameOff → Build Documents** (die Cap-Height-Messung passt sich der Schrift an; Orbitron ist breiter → bitte prüfen, ob Felder
  noch passen)
- alles Neue aus P41/P40 (Pinnwand-Zettel, Sprechblasen, Durchsagen)

Hilfreich:
- Schrift-Asset: `Assets/_Project/Fonts/Orbitron SDF.asset` (dynamisch, Ausweichschriften Share Tech Mono → Liberation Sans für
  fehlende Zeichen). Die **TMP-Standardschrift** ist jetzt Orbitron – neue Texte starten automatisch richtig.
- In Martins Buildern dürfen Texte bis auf 60 % schrumpfen (Auto-Size), damit Orbitron nicht aus Feldern läuft.

Falls du mit „alles Orbitron“ nicht einverstanden bist, schreib es hier rein – dann besprechen wir es.
- A) Mache ich mit P41/P40 mit
- B) Einspruch: ____

## Annahme bis zur Antwort
Deine Teile bleiben in ihrer bisherigen Schrift, bis du sie umstellst.

## Antwort
