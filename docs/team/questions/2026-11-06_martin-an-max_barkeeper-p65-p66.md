---
von: martin
an: max
status: offen
erstellt: 2026-11-06 14:20
blockiert: nein
---

# Übernimmst du Martins Barkeeper-Vorschlag (P65 Wunschliste & Gerüchte, P66 Bestechungszettel)?

## Kontext
Mit P64 kauft nur noch der Schwarzmarkt Ware mit `CONTRABAND` oder `CONFISCATED`; Kiosk und Bar lehnen sie ab. Für den Barkeeper
hat Martin eine neue Rolle entworfen. Er kauft nichts mehr für Geld, sondern:
- **P65 Wunschliste & Gerüchte:** Vor der Bar hängt jeden Tag ein Schild mit Gegenständen, die er haben will. Wer liefert, bekommt eine
  Geheiminfo oder ein Gerücht für den nächsten Tag, z. B. „Ein Schmuggler wurde festgenommen, morgen ist alles mit `DRUG` teurer“.
  Man kann abends beim Schwarzmarkt vorsorgen und am nächsten Tag teurer verkaufen. Braucht den Preisfaktor pro Tag aus P11.
- **P66 Bestechungszettel:** Zu Schichtbeginn legt er manchmal einen Zettel auf den Tisch, etwa „Lass die Person XY durch, dann bekommst
  du nach der Schicht 200 CR“. Die Regelprüfung zählt das Durchlassen trotzdem als Fehler.
- Bei P11 steht ein Nachtrag, dass der Barkeeper nichts mehr kauft.

Beide Pakete stehen als Entwurf in `PAKETE.md` (P65, P66), jeweils mit „Offen für Max“. Die gemeinsame Label-Liste steht bei P64.

## Frage
- A) Übernehmen wie beschrieben.
- B) Übernehmen mit Änderungen (bitte in die Antwort schreiben).
- C) Nicht übernehmen.

**Bei A legt dein Claude das direkt fest, ohne erneute Zustimmung von Martin:** P65 und P66 von „Entwurf“ auf `offen` setzen,
die „Offen für Max“-Punkte mit deiner Antwort füllen, `DESIGN.md` (Bar-Zeile) anpassen, den Entwurfs-Eintrag in `DECISIONS.md`
als beschlossen nachtragen, Frage auf `erledigt` setzen. Nur bei B oder C entscheidet Martin noch einmal mit.

## Zusätzlich zum Besprechen (nicht Teil von A/B/C)
**Zum Besprechen (Max + Martin, vor P08):** Labels hängen an der Sorte Gegenstand (`ItemData`), nicht am einzelnen Stück. Eine normale und eine beschlagnahmte Taschenuhr sind darum zwei Gegenstände („Pocket watch“ und „Confiscated pocket watch“ mit `CONFISCATED`); ein Stück im Inventar bekommt kein Label nachträglich. Beschlagnahme legt direkt die beschlagnahmte Variante ins Inventar. Folge: Jeder beschlagnahmbare Gegenstand braucht zwei Assets. Alternative wäre ein Merkmal pro Inventar-Stapel („beschlagnahmt“), das kostet Umbau an `Inventory`. Erst gemeinsam entscheiden, sonst kollidiert es mit P08.
Diesen Punkt legt dein Claude **nicht** allein fest, auch bei A nicht. Bitte im nächsten Gespräch mit Martin klären.

## Annahme bis zur Antwort
Nichts wird gebaut. P64 (nur Schwarzmarkt kauft Schmuggelware) gilt schon, unabhängig von dieser Frage.

## Antwort
