---
von: martin
an: max
status: offen
erstellt: 2026-11-11 14:45
blockiert: nein
---

# Kannst du Welle 3 anspielen? (P128 Spielstand, P130 Pinnwand nur heute, P133 Durchsage, P140 Hilfe auf [H])

## Kontext
Drei Agents haben die Pakete parallel gebaut, zusammengeführt mit Rebuild All, Bake, Check Occlusion (2 kleine Abweichungen wie bisher),
Validate Travelers 0 und 308/308 EditMode-Tests. **Angespielt hat es noch niemand.** Details in `PAKETE.md` (Ergebnis-Zeilen).

## Frage
Bitte einmal durchspielen (gern im Live-Build) und hier notieren, was nicht stimmt:
- **Spielstand (P128):** Bis Tag 3–4 spielen, Seite neu laden → Menü zeigt „CONTINUE · DAY n“ → gleicher Kontostand, Inventar, Familie,
  Strafpunkte, Figuren erinnern sich? START bei vorhandenem Stand fragt „NEW RUN?“. Nach einem Ende ist CONTINUE weg.
  Läuft das im Browser (itch/kestrel9) – bleibt der Stand nach dem Schließen des Tabs?
- **Pinnwand (P130):** Nur die Zettel von heute, keine Blätter-Knöpfe, keine alten Zettel an der Wand.
- **Durchsage (P133):** An Tag 1 kurz nach dem Brief: Murmeln + „Officer, report to the customs counter…“. Klingt das gut, kommt es zur richtigen Zeit?
- **Hilfe (P140):** Unten nur „[H] Help“. [H] am Schalter, beim Lesen, an der Pinnwand, auf der Bank, im Fernrohr, im Flug → passende Tasten?
  [H]/Esc schließt, ohne dass Esc zusätzlich aufsteht oder das Pausenmenü öffnet? Im Browser: Fängt ein Klick die Maus wieder ein?
- `tools/measure-webgl.mjs` klickt nach START jetzt „NEW RUN“ (bei (0.375, 0.667)) – falls du misst und es hängt, liegt es daran.

## Annahme bis zur Antwort
Alles gilt als fertig; Fehler werden Fixes oder Folge-Pakete.

## Antwort

**Max, 11.11., Zwischenstand:** Noch nicht angespielt. Max spielt Welle 3 erst live ein (live ist noch `37b758d`) und testet dann dort, auch ob der Spielstand das Schließen des Tabs übersteht. #37 und #42 werden erst danach geschlossen.

**Automatisch geprüft von Max' Claude am 12.11. im Browser-Build b8b9cc1 (P147); Punkte mit ‚braucht Max‘ ergänzt Max.**
Lauf: lokaler Headless-Chrome mit `tools/drive-webgl.mjs`, Tag 1–3 gespielt, Dev-Tools an. Der Mausfang lässt sich headless nur simulieren. Screenshot-Namen beziehen sich auf den P147-Ordner bei Max.

- **Spielstand (P128):**
  - ✓ Bis Tag 3 gespielt, dann Chrome komplett beendet und mit demselben Profil neu gestartet → Menü „CONTINUE · DAY 3“ (`w3_menu_after_restart`).
  - ✓ Nach CONTINUE ist der Stand wieder da (`w3_continue_letter`, `w3_continue_inventory`):
    - Kontostand 48 CR, wie in der Abrechnung von Tag 2
    - Inventar: Taschenuhr und beschlagnahmte Taschenuhr
    - Familie: derselbe Brief (Mom und Lena krank, Pip kritisch)
    - Strafpunkte: 1, laut Log nach dem Laden
    - Figuren: Konsole „[Save] continued on day 3: 48 credits, 2 item stacks, 5 character records“, dieselben Auftritte wie vor dem Schließen, und das Ende nennt Mira Talsk von Tag 2
  - ✓ START über einem Stand fragt „NEW RUN? Your saved run (day 3) will be lost.“ (`w3_newrun_prompt`), BACK führt zurück.
  - ✓ Nach einem Ende (Heimflug, „TOO LATE?“) und Neuladen ist CONTINUE weg (`w3_menu_after_ending_reload`).
  - braucht Max: auf itch.io (iframe) und kestrel9 einmal selbst den Tab schließen und wieder öffnen. Lokal in Chrome bleibt der Stand.
- **Pinnwand (P130):** ✓ Nur die Zettel von heute, Überschrift „PINBOARD · TODAY · <Datum>“, nur ein CLOSE-Knopf, A/D ändern nichts, an der Wand keine alten Zettel (`w1_pin_closeup`, `w1_pin_closeup_d3`, `w3_pin_d3_keyD`, `d3_room_left`).
- **Durchsage (P133):**
  - ✓ An Tag 1, 2,0 s nach dem Schließen des Briefs: Murmel-Laut (2,4 s) und „Officer, report to the customs counter. Your shift starts now.“, danach der Weg-Hinweis (`w3_ann_2_5s`, `w3_ann_9_5s`).
  - braucht Max: wie es klingt.
  - ✗ Geht man sofort ans Bullauge, liegt die Durchsage über dem Gedanken (`w1_porthole_d1_run2_send`). Das ist derselbe Überlagerungsfehler wie in Welle 1 bei den Bänken.
- **Hilfe (P140):**
  - ✓ Unten steht nur „[H] Help“.
  - ✓ [H] zeigt passende Tasten (`w3_help_*`): zu Fuß, am Schalter, beim Lesen, im Terminal, an der Pinnwand, auf der Bank am Abend (mit [F]), am Bullauge, beim Foto, im Fernrohr, im Flug, im Inventar und im Drohnenfenster.
  - ✓ Esc schließt nur die Hilfe:
    - zu Fuß öffnet sich kein Pausenmenü
    - beim Lesen bleiben die Papiere hochgehalten
    - Terminal, Pinnwand und Drohnenfenster bleiben offen
    - beim Foto bleibt man stehen
    - Bullauge, Fernrohr und Flug mit simuliertem Esc des Browsers (Mausfang geht verloren): Die Hilfe geht zu, man bleibt sitzen, im Fernrohr bzw. im Flug, kein Pausenmenü.
  - ✓ (simuliert) Ein Klick fängt die Maus wieder: Bullauge und Fernrohr rufen beim Klick `requestPointerLock` auf. braucht Max: einmal im echten Browser gegenprüfen, Headless-Chrome erlaubt keinen echten Mausfang.
- Hinweis zu `tools/measure-webgl.mjs`: kein Testpunkt, nicht gemessen.
