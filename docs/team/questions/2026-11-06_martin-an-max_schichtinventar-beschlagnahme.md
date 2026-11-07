---
von: martin
an: max
status: beantwortet
erstellt: 2026-11-06 14:36
blockiert: nein
---

# Wie gehen wir mit Beschlagnahmtem um? Vorschlag: eigenes Schicht-Inventar und Wahl am Schichtende

## Kontext
P64 ist gebaut: Nur der Schwarzmarkt kauft Ware mit `CONTRABAND` oder `CONFISCATED`. Offen ist, wie beschlagnahmte Ware entsteht
und ins Inventar kommt (das gehört zu P08). Dazu gibt es schon einen Gesprächspunkt in `PAKETE.md` bei P64 und P08 sowie in
`2026-11-06_martin-an-max_barkeeper-p65-p66.md`: Labels hängen an der Sorte Gegenstand, nicht am einzelnen Stück. Das bleibt
bewusst offen, bis Max und Martin darüber gesprochen haben. Martins Vorschlag unten hängt damit zusammen.

## Martins Vorschlag
**Zwei Inventare:**
- **Schicht-Inventar** (Beschlagnahme-Inventar): gibt es nur, solange man am Tisch sitzt. Alles, was während der Schicht beschlagnahmt
  wird, landet dort. Im Sitzen lässt sich das normale Inventar nicht öffnen.
- **Freizeit-Inventar** (das heutige aus P46): nach der Schicht. Die vielen Untersuchungsgegenstände müssen dann nicht im normalen Inventar
  verwaltet werden.

**Am Schichtende** fragt das Spiel nach den beschlagnahmten Gegenständen. Drei Möglichkeiten:
1. **„Nothing to report“:** Man nimmt alles Beschlagnahmte heimlich an sich. Folge: 80 % Chance, erwischt zu werden, dann 100 CR Strafe
   und eine Disziplinarstrafe.
2. **„Oh, seems I missed that“:** 60 % Chance, einen Gegenstand nach Wahl aus dem Beschlagnahmten ins Freizeit-Inventar zu übernehmen.
   Er bekommt dann das Label `CONFISCATED`. Wird man erwischt, gibt es eine Geldstrafe, aber keine Disziplinarstrafe.
3. **„As ordered“:** Alles aus dem Schicht-Inventar geht an den Confiscated Locker. Der Locker löscht es erst einmal. Wenn Zeit bleibt,
   kommt eine Mechanik dazu, mit der man den Locker aufbrechen kann; dafür bräuchte er ein eigenes Inventar.

**Zusatzvorschlag: Verwarnungen und Disziplinarstrafen getrennt.**
- **Verwarnungen** bleiben wie heute (P07): Fehler am Schalter → Verwarnzettel aus dem Drucker, pro Tag 2 frei, danach Abzug vom Lohn.
- **Disziplinarstrafen** sind neu: für Vergehen gegen die Station, z. B. erwischt mit beschlagnahmter Ware (Möglichkeit 1). Sie zählen über
  alle Tage, bei 3 Disziplinarstrafen ist Game Over.

**Alle Zahlen sind vorläufig** (80 %, 60 %, 100 CR, 3 Disziplinarstrafen) und ändern sich beim Balancing.

## Hinweise von Martins Claude (zum Abwägen, keine Entscheidung)
- Durch die Trennung bleibt P07 unverändert (`CounterShift.freeWarnings`); Disziplinarstrafen sind ein eigener Zähler.
  Game Over nach 3 Disziplinarstrafen gehört zu den Enden (P12).
- Punkt 2 heißt: Ein Gegenstand bekommt `CONFISCATED` erst beim Übernehmen. Das passt zu beiden Lösungen aus dem offenen
  Gesprächspunkt (eigene Variante pro Gegenstand oder Merkmal pro Inventar-Stapel). Das sollten wir zusammen entscheiden.
- Weil sich die Werte noch ändern, kommen Chancen und Strafen in ein Tuning-Asset in `Data/`, nicht in den Code.
- Der Locker zum Aufbrechen wäre ein eigenes Kann-Paket.

## Vorschlag für die Pakete (Martins Claude)
Nach der Regel „fertige Pakete nicht wieder öffnen“:
- **Neu: Schicht-Inventar** (Folgt auf P46, das ist fertig): zweites Inventar nur am Tisch, normales Inventar im Sitzen gesperrt.
- **P08 anpassen** (noch offen): Beschlagnahme legt Gegenstände ins Schicht-Inventar; Wahl am Schichtende mit den drei Möglichkeiten;
  Zähler für Disziplinarstrafen; Tuning-Asset für Chancen und Strafen.
- **P12 anpassen** (noch offen): Ende „Game Over nach 3 Disziplinarstrafen“.
- **Neu, Kann: Confiscated Locker aufbrechen** (Locker mit eigenem Inventar).
- P07 bleibt unverändert.

## Frage
- A) Übernehmen wie vorgeschlagen, mit dieser Paket-Aufteilung.
- B) Übernehmen mit Änderungen (bitte in die Antwort schreiben).
- C) Nicht übernehmen.

Diesen Vorschlag bitte **mit Martin zusammen** besprechen und festlegen, nicht allein durch Max' Claude.

## Annahme bis zur Antwort
Nichts wird gebaut. P08 wartet auf dieses Gespräch.

## Antwort

**A** (Max, 07.11.): Übernehmen wie vorgeschlagen, mit deiner Paket-Aufteilung (neues Schicht-Inventar als Folgepaket von P46, P08 und P12 anpassen, Locker-Aufbrechen als Kann-Paket, P07 bleibt). **Verwarnungen und Disziplinarstrafen getrennt: ja** – eigener Zähler über alle Tage, 3 Disziplinarstrafen = Game Over (Ende in P12). Alle Zahlen bleiben vorläufig und kommen in ein Tuning-Asset.
Du wolltest das gemeinsam festlegen: Das ist meine Position. Wenn du einverstanden bist, gilt sie, und du legst die Pakete an. Wenn nicht, klären wir es im Call. Offen bleibt der Punkt „CONFISCATED als eigenes Item oder als Merkmal pro Stapel“, den besprechen wir im Call zusammen mit Warenliste 2.

**Im Call bestätigt (max + martin, 07.11.):** A gilt. Beschlagnahmte Ware ist eine **eigene Item-Variante** mit Label CONFISCATED (nicht Merkmal pro Stapel). Pakete legt Martin an (Schicht-Inventar, P08/P12 anpassen, Locker als Kann). P12 hat den Hinweis schon.
