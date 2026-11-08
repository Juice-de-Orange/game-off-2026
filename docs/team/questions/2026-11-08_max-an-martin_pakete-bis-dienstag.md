---
von: max
an: martin
status: offen
erstellt: 2026-11-08 18:30
blockiert: nein
---

# Übernimmst du bis Dienstag P97, P98, P85, P77 und P94?

## Kontext
Max hat am 08.11. deine Hilfe im Urlaub gern angenommen: Dein Claude baut und testet, du selbst kannst nichts anschauen. Deshalb diese Pakete, die sich weitgehend automatisch prüfen lassen:

| Paket | Inhalt | Prüfung, die offen bleibt |
|---|---|---|
| **P98** Figuren-Antworten (neu) | Eigene Feld-Antworten für die Figuren an Tag 1–4 | keine, nur Text |
| **P97** Spieluhr (#19) | Eine Uhrzeit nach Phase, alle Uhren laufen mit | Blick auf die Uhren: Max |
| **P85**, **P77** | Deine Entwürfe (Figuren-Folgen, Verwahrung) | Abnahme im Spiel: Max |
| **P94** Stimmen (#16) | 10 Laute je Spezies bestellen, zufällig abspielen | Nach Gehör: Max; Lieferungen gibt nur Max frei |

Die Reihenfolge entscheidest du; P98 und P97 sind am kleinsten.

**Bitte beachten**
- Max hält gerade `Main.unity`, `Menu.unity`, `Boot.unity`, alle generierten Prefabs, `StationGrayboxBuilder` und `EconomyBuilder` (P92/P93/P96, BOARD „In Arbeit“). Vor Rebuild All, Build Station oder Bake bitte BOARD prüfen und `/claim`.
- **Dein P65 hat sich geändert (P93):** Das Schild vor der Bar ist weg, der Barkeeper nennt seinen Wunsch nur noch im Gespräch. `BarWishSign.cs` ist gelöscht.
- **P95 (Menüs im Terminal-Stil, Max)** berührt später deine Fenster für Inventar, Handel und Abrechnung.
- **Neu an Tag 1–4 (P15):** `Scripts/Editor/StoryDayContent.cs` füllt sie, Reisende in `Data/Travelers/Normal/`. Figuren an festen Plätzen stehen jetzt exakt dort (`CharacterPlanner.Queue`, P86).
- **Rebuild-Rauschen:** `Build Story Characters` ändert in `Day05–12` nur interne `rid`-Werte; bitte nicht mitcommitten, wenn du dort nichts änderst.
- Geld- oder Preisänderungen brauchen wie immer `GameOff/Economy Report` mit allen ✓.
- Live-Deploys macht Max. Offene GitHub-Issues schließt, wer sie behebt und deployt ist, mit einem kurzen Kommentar.

## Frage
Passt das so, oder möchtest du etwas davon nicht übernehmen?
- A) Passt, ich nehme sie wie oben.
- B) Nur einen Teil (bitte nennen).

## Annahme bis zur Antwort
A. Die Pakete stehen in `PAKETE.md` bei dir; nimm sie wie gewohnt mit „Take“, bevor du anfängst.

## Antwort

