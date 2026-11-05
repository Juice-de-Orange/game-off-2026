---
von: max
an: martin
status: offen
erstellt: 2026-11-05 19:40
blockiert: nein
---

# Assets bestellen: Du darfst das direkt, so geht es

## Kontext
In deinem Handover stand, dass die Händlerfiguren „nach Absprache“ bestellt werden. Max möchte, dass du nicht auf ihn warten musst.
Holly, Barkeeper und Dealer sind jetzt bestellt (`SM_NPC_*`, Paket P52) und zwar mit deinen Beschreibungen.

**So bestellst du ein Asset:** Sag deinem Claude „bestell X“. Es hängt einen Eintrag an `docs/ASSET-REQUESTS.md` an, prüft ihn mit
`python tools/check_asset_requests.py` und pusht auf `main`. **Der Eintrag auf `main` ist die Bestellung**, die Pipeline liest ihn etwa stündlich.
Danach baust du mit einem Platzhalter weiter; Lieferung, Freigabe und Einbau laufen über Max. Details: `docs/ASSETS.md` → „Asset bestellen“.

Vorher fragen musst du nur bei großen Aufträgen (viele Assets, neue Räume) oder wenn sich der Stil ändert. Änderungen an Geliefertem gehen wie bisher über `feedback` + `status: rework`.

## Frage
Ist der Ablauf für dich klar?
- A) Ja, klar.
- B) Rückfrage (bitte kurz beschreiben).

## Annahme bis zur Antwort
Du bestellst ab jetzt selbst. Blockiert nichts.

## Antwort
