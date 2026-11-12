---
von: martin
an: max
status: offen
erstellt: 2026-11-09 03:00
blockiert: nein
---

# Kannst du Welle 1 anspielen? (P34 Terminal, P28 Sitzplätze + P29-Rest, P32 Schrank + P33-Rest)

## Kontext
Martin hat die drei Pakete parallel von drei Agents bauen lassen (je eigener Worktree, nur lokal), danach zusammengeführt:
Rebuild All, Licht + Occlusion gebacken, Validate Travelers und EditMode-Tests grün, Konsole ohne Fehler. **Angespielt hat es noch niemand.**
Details: `PAKETE.md` (Ergebnis-Zeilen), `MECHANICS.md` (neue Zeilen unter „Gebaut“).

## Frage
Bitte einmal durchspielen und hier kurz notieren, was nicht stimmt:
- **Terminal (Schalter):** Klick auf den Röhrenbildschirm am Tisch → Fenster lesbar? Klickfläche sitzt auf dem Bildschirm? Ein Gesuchter wird erkannt? Öffnet Esc zusätzlich das Pausenmenü?
- **Asservatenschrank:** „As ordered“ wählen → Ware liegt sichtbar in den Fächern? Größe der Modelle ok? Aufbrechen abends bei offenen Türen?
- **Pinnwand im Stehen:** [E] → Vollbild mit Aushang, Fahndungsfotos, Memo; A/D blättert ältere Tage; ältere Zettel an der Wand sichtbar?
- **Bänke in der Halle:** sitzen, Blick zur Plattform, Durchsagen; abends [F] (wirkt erst mit P109 Sperrstunde).
- **Barhocker 1–3:** öffnen das Barkeeper-Gespräch, danach steht man wieder auf?
- **Bullauge in 103:** Kamerafahrt nach draußen zum Heimatplaneten – sieht das gut aus? Gedanke passt zum Familienzustand?
- **Foto am Spind / Brief auf dem Tisch in 103:** Stufen bei kranker/kritischer Familie, zittrige Schrift lesbar?
- Texte (Durchsagen, Gedanken) stehen in `Data/Rest/RestSpotTuning.asset` – Martin hat sie freigegeben, Änderungen gern direkt dort.

## Annahme bis zur Antwort
Alles gilt als fertig; Fehler, die du findest, werden Folge-Pakete oder Fixes.

## Antwort

**Max, 11.11., Zwischenstand:** Noch nicht angespielt, die Frage bleibt offen. Der Punkt „A/D blättert ältere Tage“ an der Pinnwand ist mit P130 hinfällig.

**Automatisch geprüft von Max' Claude am 12.11. im Browser-Build b8b9cc1 (P147); Punkte mit ‚braucht Max‘ ergänzt Max.**
Lauf: lokaler Headless-Chrome mit `tools/drive-webgl.mjs`, Tag 1–3 gespielt, Dev-Tools an (rote Dev-Knöpfe und Hitboxen im Bild). Der Mausfang lässt sich headless nur simulieren. Screenshot-Namen beziehen sich auf den P147-Ordner bei Max.

- **Terminal:**
  - ✓ Klick auf den Röhrenbildschirm öffnet das Terminal, der Text ist gut lesbar (`w1_terminal_open`, `w1_terminal_korr`).
  - ✓ Die Klickfläche sitzt auf dem Bildschirm (Klick auf den CRT links neben DOCUMENTS).
  - ✓ Ein Gesuchter wird erkannt: Tag 3, KAEL DROSS → „WANTED LIST … MATCH · ORDER: ARREST“ (`d3_t2_terminal`).
  - ✓ Esc schließt nur das Terminal, es öffnet sich kein Pausenmenü (`w1_terminal_esc2`).
- **Asservatenschrank:**
  - ✓ Nach „As ordered“ liegt die Ware sichtbar in den Fächern: Tag 1 eine Dose und ein versiegelter Beweisbeutel (Spice, Taschenuhr; `w1_locker_contents_close`), Tag 3 drei Antibiotika-Dosen nach einer echten Verhaftung (`w1_locker_d3_antibiotics`).
  - braucht Max: Größe der Modelle. Im Bild wirken die Dosen etwa handgroß und plausibel.
  - ✓ Aufbrechen am Abend: [E] öffnet erst die Türen, dann kommt „Break the seals…“. BREAK IT → „Nobody saw. Taken: Spice x1, Pocket watch x1.“, die Fächer sind danach leer (`w1_locker_open`, `w1_locker_broken`).
- **Pinnwand im Stehen:**
  - ✓ [E] → Vollbild „PINBOARD · TODAY“ mit Aushang, Fahndung (Tag 3 mit Fotos), Markt-Zettel und Memo (`w1_pin_closeup`, `w1_pin_closeup_d3`).
  - entfällt: A/D blättern und ältere Zettel an der Wand (P130). A/D ändert nichts, an der Wand hängen nur die Zettel von heute.
- **Bänke in der Halle:**
  - ✓ Sitzen auf A, B und C geht; A und B blicken aufs Aussichtsdeck (`w1_bench_a`, `w2_benchB`, `w2_benchC`).
  - ✓ Durchsagen kommen als Untertitel, ein Murmel-Laut startet (per Audio-Hook gemessen).
  - ✓ [F] am Abend: Die Sperrstunde kam etwa 5 s später (Tag 2, `w1_bench_F_0_5s` … `_9_5s`).
  - ✗ Untertitel überlagern sich: Der Text der Bank (Hallendurchsage bzw. „It's late. The hall is empty…“) und die Sperrstunden-Zeile stehen übereinander und sind dann unlesbar (`w1_bench_a_4s`, `w1_bench_F_5_5s`).
- **Barhocker 1–3:** ✓ Alle drei öffnen das Barkeeper-Gespräch (Tag 1 nach der Sperrstunde „Closed for the night…“, Tag 3 abends „Nothing I need today…“). Nach BACK oder Esc steht man wieder (`w1_stool1_curfew`, `w1_stool1_back`, `w1_stool3_esc`). Nicht geprüft: Hocker 1 ist von einem Abendgast besetzt (P145).
- **Bullauge in 103:**
  - ✓ Die Kamera fährt hinaus zum Heimatplaneten (`w1_porthole_t02` … `_t32`). braucht Max: ob die Fahrt gut aussieht.
  - ✓ Der Gedanke passt zum Zustand der Familie: Tag 1 alle gesund („Pip asked if I can see them…“), Tag 2 Pip krank („Pip needs medicine…“), Tag 3 Pip kritisch („If Pip doesn't get medicine soon…“).
  - ✗ Ab Tag 3 verdeckt die Innenwand die rechte Bildhälfte, der Planet steht am linken Rand (Tag 1 frei, Tag 2 etwa ein Fünftel verdeckt): `w1_porthole_d1_run2_send`, `w1_porthole_d2_sick`, `w1_porthole_d3_critical`. Vermutlich dreht der Himmel pro Tag weiter (SkyRig 0/18/36°) und die Kamera zielt schräg durchs Fenster auf den Planeten.
  - ✗ Gedanke und Durchsage am ersten Morgen liegen übereinander, wenn man direkt nach dem Brief ans Bullauge geht (`w1_porthole_d1_run2_send`). Das ist derselbe Fehler wie bei den Bänken.
- **Foto am Spind / Brief auf dem Tisch:**
  - ✓ Foto: Tag 1 intakt, Tag 2 (krank) verblasst mit Knick, Tag 3 (kritisch) zusätzlich dunkler (`w1_photo_d1`, `w1_photo_d2_sick`, `w1_photo_d3_critical`). Nicht geprüft: die zerrissene Stufe nach einem Todesfall (nicht erreicht).
  - ✓ Brief: Tag 1 gerade, Tag 2 leicht zittrig, Tag 3 deutlich zittrig. Im Screenshot ist er noch lesbar (`w1_letter_table_d1`, `_d2_sick`, `_d3_critical`). braucht Max: ob er sich angenehm liest.
- Texte in `RestSpotTuning`: nichts geändert.
