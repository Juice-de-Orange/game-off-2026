---
von: martin
an: max
status: offen
erstellt: 2026-11-09 04:30
blockiert: nein
---

# Kannst du Welle 2 anspielen? (P109 Sperrstunde, P110 Zoll bei der Drohne, P111 Fernrohr-Diorama)

## Kontext
Ergebnis der Mechanik-Prüfung nach Welle 1 (siehe `MECHANICS.md`, Lücken): Der Abend hatte kein Ende, Schmuggel per Drohne war risikolos,
und das Fernrohr zeigte dasselbe wie das neue Bullauge. Martin hat das entschieden, drei Agents haben es gebaut, zusammengeführt mit
Rebuild All, Bake, Validate Travelers und allen EditMode-Tests grün. **Angespielt hat es noch niemand.** Details in `PAKETE.md` (Ergebnis-Zeilen).

## Frage
Bitte einmal durchspielen und hier notieren, was nicht stimmt:
- **Sperrstunde (P109):** Abend bis 23 Uhr laufen lassen (oder Bank [F]): Gong + Durchsage, Kiosk-Gitter fällt sichtbar, Automat/Drohne/Ticket/GIVE
  zu, Barkeeper redet weiter, Anzeigesäule „NIGHT · CURFEW“, Bett geht. **Reichen 5 echte Minuten Abend?**
- **Zoll (P110):** Schmuggelware (z. B. Spice) per Drohne schicken → roter Hinweis beim Beladen; in 1 von 4 Nächten: rote Zeile in der Abrechnung,
  Ware fehlt, Strafpunkt, Briefzeile am Morgen. Beim dritten Strafpunkt DISCHARGED nach dem Schließen der Abrechnung.
- **Fernrohr (P111):** Auf der Plattform ganz hineinzoomen auf den Planeten → Blende zur Heimatstraße (noch Platzhalter-Boxen, `SM_Home_Street.fbx`
  ist bestellt); drei Fenster passend zur Familie (krank = gedimmt, kritisch = flackert, tot = dunkel); herauszoomen → zurück.
  Das Bullauge in 103 zeigt weiter nur den Planeten.
- Die Asset-Bestellung `SM_Home_Street.fbx` wartet auf die Pipeline (P2, bis 16.11.).

## Annahme bis zur Antwort
Alles gilt als fertig; Fehler werden Fixes oder Folge-Pakete.

## Antwort

**Max, 09.11., vorläufig:** Abend bleibt bei 5 echten Minuten. Angespielt ist noch nicht, die Frage bleibt offen.

**Automatisch geprüft von Max' Claude am 12.11. im Browser-Build b8b9cc1 (P147); Punkte mit ‚braucht Max‘ ergänzt Max.**
Lauf: lokaler Headless-Chrome mit `tools/drive-webgl.mjs`, Tag 1–3 gespielt, Dev-Tools an. Screenshot-Namen beziehen sich auf den P147-Ordner bei Max.

- **Sperrstunde (P109):**
  - ✓ Der Abend läuft bis 23 Uhr: An Tag 1 kam „[Day] Curfew on day 1“ nach rund 5 Minuten Abend, an Tag 2 mit [F] auf der Bank sofort.
  - ✓ Untertitel „Curfew. All shops are closed. Return to your quarters.“, danach „It's 23:00. Go back to room 103…“. Der Laut startet (per Audio-Hook gemessen). braucht Max: wie Gong und Murmeln klingen.
  - ✓ Das Kiosk-Gitter ist unten (`w2_kiosk_grille`). Das Herunterfallen selbst habe ich nicht gesehen, weil die Sperrstunde kam, während ich auf der Bank saß.
  - ✗ (klein) Das Neonschild „OPEN“ neben dem Kiosk leuchtet nach der Sperrstunde weiter, obwohl das Gitter unten ist (`w2_kiosk_grille`).
  - ✓ Automat, Drohne, Ticketautomat und Kiosk melden „Closed for the night. Curfew.“ (`w2_curfew_vending`, `w2_curfew_drone`, `w2_curfew_ticket`, `w2_curfew_kiosk`). Beim Barkeeper ist GIVE weg, er redet aber weiter („Today I wanted: 2x LUXURY. Too late now.“, `w1_stool1_curfew`).
  - ✓ Die Anzeigesäule zeigt „DAY 1 · NIGHT · CURFEW · NO MORE ARRIVALS“ (`crop_board`, Ausschnitt aus `w2_benchC`).
  - ✓ Das Bett geht („End day 1?“ mit SLEEP, `w2_bed_curfew`).
  - braucht Max: Reichen 5 echte Minuten Abend? Der automatische Lauf mit Pausen für Screenshots taugt dafür nicht als Maßstab.
- **Zoll (P110):**
  - ✓ Beim Beladen erscheint bei Spice der rote Hinweis „Customs may inspect this.“ (`w2_drone_spice_selected`).
  - ✓ Nacht 1 (Spice) wurde nicht kontrolliert: Die Abrechnung zeigt „AT HOME Spice x1“.
  - ✓ Nacht 2 (beschlagnahmtes Spice aus dem Schrank) wurde kontrolliert:
    - rote Zeile „Customs opened your parcel and kept the confiscated spice. DISCIPLINARY PENALTY 1 OF 3.“ und „AT HOME nothing“ (`w2_settle_d2`)
    - am Morgen die Briefzeile „The drone came with a customs note. The confiscated spice never arrived.“ (`w2_letter_d3`)

    Die 25-%-Quote lässt sich mit 2 Nächten nicht prüfen.
  - nicht geprüft: dritter Strafpunkt → DISCHARGED. Das hängt am Zufall, erreicht wurde nur 1 Strafpunkt.
- **Fernrohr (P111):**
  - ✓ Ganz hineingezoomt blendet es auf die Heimatstraße um (`w2_scope_street`), herausgezoomt geht es zurück zum Planeten (`w2_scope_back`, `w2_scope_back2`).
  - entfällt: „noch Platzhalter-Boxen“. `SM_Home_Street.fbx` ist geliefert (status: final) und im Build zu sehen.
  - ✓ Die Fenster folgen der Familie:
    - Tag 1: alle drei hell.
    - Tag 2 (Pip krank): ein Fenster gedimmt (`w2_scope_street_d2_sick`).
    - Tag 3 (Mom und Lena krank, Pip kritisch): zwei gedimmt, eins flackert (`w2_scope_d3_crit_a` … `_e`). Über 5 Bilder schwankte seine Helligkeit zwischen 69 und 124, die anderen beiden blieben gleich.

    Nicht geprüft: dunkles Fenster nach einem Todesfall (nicht erreicht).
  - ✓ Das Bullauge in 103 zeigt weiter nur den Planeten.
- Asset-Bestellung `SM_Home_Street.fbx`: entfällt, sie ist geliefert.
