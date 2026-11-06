---
von: max
an: martin
status: erledigt
erstellt: 2026-11-06 07:50
blockiert: nein
---

# Wie schließen wir die Geldlücke zwischen Kiosk und Schwarzmarkt?

## Kontext
Der Nachtlauf (P57, P58 Punkt 2) hat gefunden: Der Kiosk verkauft zum Grundwert (`Stock_Kiosk_Test`, `sellFactor: 1`), der Schwarzmarkt kauft alles zum 1,2-Fachen (`Stock_BlackMarket_Test`, `buyFactor: 1.2`, `refusedTags` leer). Wer pendelt, verdient ohne Risiko unbegrenzt. Die Werte stammen aus Test-Daten. Max sagt: Shops und Economy sind dein Bereich, deshalb entscheidest du das.

## Frage
- A) Schwarzmarkt kauft nur beschlagnahmte oder verbotene Ware (Regel über Tags). Kleiner Aufwand, passt zur Geschichte. Claudes Empfehlung.
- B) `buyFactor` unter 1. Ein Wert, aber der Schwarzmarkt verliert seinen Reiz.
- C) Erstmal lassen, der Vorrat aus P11 begrenzt die Menge. Die Lücke selbst bleibt.

## Annahme bis zur Antwort
Nichts wird geändert. Fest steht schon (Max, 06.11.): Der Händlervorrat wird begrenzt und morgens aufgefüllt, das gehört in P11.

## Antwort
**A** (Martin, 06.11.): Ware mit dem Label `CONTRABAND` oder `CONFISCATED` kauft nur der Schwarzmarkt. Kiosk und alle anderen
Händler lehnen beide Labels ab, der Schwarzmarkt kauft nur Ware mit mindestens einem der beiden. Gebaut wird das in **P64**
(Martins Bereich, Economy). Das Label `CONFISCATED` gibt es noch nicht, es kommt mit P64.
