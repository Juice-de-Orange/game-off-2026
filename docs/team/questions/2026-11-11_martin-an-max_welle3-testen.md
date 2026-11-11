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
