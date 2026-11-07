---
von: martin
an: max
status: offen
erstellt: 2026-11-07 16:30
blockiert: nein
---

# Warenliste 2: gelieferte Item-Modelle als Waren, und jede Ware mit Zweck?

## Kontext
Martin hat alle Waren geprüft (19 Items, 13 Labels, Sortimente von Holly und Dealer, `docs/ECONOMY-REPORT.md`). Grundlage stimmt: jedes Item
hat ein Label, jeder Familienbedarf hat genug Waren (Essen 4, Wärme 2, Medizin 4). Gefunden wurden aber Lücken, die deine Pakete berühren:

1. **Gelieferte Modelle ohne Ware:** `SM_Item_PillBottle`, `_Liquor`, `_WeaponParts`, `_CashBundle`, `_AlienSpecimen` (P32 Asservaten),
   `SM_Item_ClownShoes` (P63), `SM_Item_UVLamp` (P62). Umgekehrt kennen unsere Items ihr Modell nicht (z. B. Data chip, Nova Cola/`SM_Prop_SodaCan`).
2. **Waren ohne Nutzen:** Pistol (140 CR) und Data chip (49 CR) braucht niemand, Rückverkauf beim Dealer ist immer Verlust. Antibiotics (59) und
   Stim injector (35) sind teurer als Medizin am Bett (20). Canned stew und Fresh fruit decken Essen nicht besser als die Protein bar (5 CR).
3. **Wertsachen ohne Quelle:** Pocket watch, Silver ring, Alien trinket – niemand verkauft sie, man findet sie nirgends.
4. **Labels fast leer:** CONFISCATED (kein Item), ALIEN (nur der Trinket), GADGET (nur der Scanner, bis P16 ausgeblendet), PERISHABLE (nur Obst).
5. **Fracht ↔ Waren:** Ein Test-Reisender hat „6x CHEM STIMULANT VIALS“, die Regel sucht „STIMULANT“, unsere Ware heißt „Stim injector“.
   Für die Beschlagnahme (P08) sollten Fracht-Stichworte und Warennamen gleich sein.

## Martins Vorschlag (neues Paket „Warenliste 2“)
- Gelieferte Modelle werden Waren: Liquor → LUXURY · Weapon parts → CONTRABAND+WEAPON · Cash bundle → VALUABLE · Alien specimen → ALIEN+CONTRABAND ·
  Pill bottle → MEDICINE · UV lamp → GADGET (unique, mit P62) · Clown shoes bleiben bei deinem P63. Items bekommen ein Feld für ihr Modell.
- Jede Ware bekommt einen Zweck: teure Waren wirken stärker (z. B. Canned stew lässt Geschwächte gesund werden), Antibiotics heilen „critical“
  (P69), Waffen/Wertsachen sind Ziele für Bar-Wünsche und kommen aus der Beschlagnahme (P08) oder der Bestechung (P66).
- Fracht-Stichworte und Warennamen gleichziehen (z. B. „Stimulant vials“ statt „Stim injector“).
- Jede Änderung an Preisen/Wirkung → neues Rebalancing (Economy Report).

## Frage
- A) Passt so: Martin legt „Warenliste 2“ als Paket an und baut es (Modelle in den Waren, Zweck, Namen); P32/P62/P63 nutzen die Waren dann.
- B) Mit Änderungen (bitte beschreiben, z. B. welche Modelle wohin).
- C) Erst im gemeinsamen Gespräch mit dem Schicht-Inventar/Beschlagnahme klären (hängt an P08).

## Annahme bis zur Antwort
Nichts wird gebaut. Martin und Max sprechen darüber (zusammen mit der Frage `schichtinventar-beschlagnahme`).

## Antwort
