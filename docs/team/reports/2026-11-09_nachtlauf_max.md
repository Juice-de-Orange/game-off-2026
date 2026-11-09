# Nachtlauf 08./09.11. – Bericht für Max

Stand am Ende des Laufs: siehe Abschnitt „Live und Prüfung“. Alles ist auf `main`, die Docs sind aktuell.

> **Nachtrag 09.11.:** Martin hat nach dem Nachtlauf P85-Rest, P87, P28, P29-Rest, P32, P33-Rest, P34, P109, P110 und P111 gebaut. Was unten als offen steht, ist damit teilweise erledigt; `PAKETE.md` und `BOARD.md` zeigen den aktuellen Stand. Seit dem 09.11. früh ist `f7a90ca` live, inklusive seiner Pakete.

## Das Wichtigste in Kürze
- **Fertig:** Muss-Pakete P69, P12, P16, P11, P99. Dazu P62, P63, P80, P77, P85 (zum Teil), P97, P98, P100, P101, P102, P103, P104, P105, P95, P107, P108, P29 (zum Teil), P33 (zum Teil), P26 (zum Teil), P50, P83, P19-Vorbereitung, Kiosk-Gitter (#21).
- **Nicht gebaut:** P106, P94 und P28, P32 (sichtbare Gegenstände), P48, P51. Warum, steht unten.
- **Elf Lieferungen** selbst freigegeben und eingebaut, eine (P106) nur angesehen.
- Geprüft mit Unity-Tests, Economy Report und im Browser auf Tag 1. **Was nur du beurteilen kannst, steht direkt unten.**

## Bitte anspielen oder anhören
| Was | Wie | Worauf achten |
|---|---|---|
| **Schleuse** (P105) | Am Schalter: Klappe auf, dann Knopf | Fliegt die Figur gut hinaus? Stärke: `DialogueTuning.airlockSuction` |
| **Verhaftung** (P104) | Knopf ARREST | Wache packt (Animation Grab), dann fällt das Gitter |
| **Tod in der Familie** (P69) | Mehrere Abende nichts schicken | Tonfall der Briefe, Texte in `Household.asset` |
| **Familie weg** (P12) | Lena sterben lassen | FAMILY LOST: passt das Ende? |
| **Moral-Schlusssatz** (P12) | Ticket heimfliegen | Drei Sätze in `DepartureTuning` |
| **UV-Lampe, Scanner** (P62, P16) | Tag 4 / Tag 6 am Kiosk kaufen | Liegen links hinten auf dem Tisch; Klick schaltet. Lesbarkeit von SEAL FORGED |
| **Abendgäste** (P101) | Jemanden durchlassen, nach der Schicht in Halle und Bar | Stehen sie gut? E redet |
| **Fernrohr** (P29) | Plattform, 1 Credit | Zoom, Schwenk, Stadtlichter |
| **Papier-Geräusche** (P102) | Viele Papiere bewegen | **Nach Gehör**; Lautstärke `PaperVolume` im `SoundSetBuilder` |
| **Gitter-Geräusche** (P91) | Verhaftung | **Nach Gehör** |
| **Menüs** (P95) | Alle Fenster durchklicken | Lesbarkeit, Rahmen |
| **Gesichter und Passfotos** (P108) | Ein paar Reisende vergleichen | Passt das Foto? Frisuren-Höhen: `HairHeights` im `TravelerPhotoBuilder` |
| **Schiffe im Hangar** (P107) | Morgens in den Hangar | Landung, Plätze |
| **Neue Geräusche** | Verhaftung, Schleuse, Hangar | **Nach Gehör:** Stiefel, Alarm, Dekompression, Landung |
| **Clownsschuhe** (P63) | Pim Oddo an Tag 8 verhaften, dann Inventar | Quietscht es lustig genug? |

## Fertig im Einzelnen
Genaueres steht jeweils unter „Ergebnis“ in `PAKETE.md`.

**Familie und Enden**
- P69: Nach zwei weiteren Nächten auf kritisch stirbt die Person. Am Morgen davor warnt der Brief. Tote brauchen nichts mehr, die Kosten sinken, Lena schreibt andere Briefe. Sterben alle oder Lena: FAMILY LOST.
- P12: FAMILY LOST neu, DISCHARGED gab es schon, Ticket-Enden mit Toten, Schlusssatz nach Moral.

**Schalter**
- P62/P16: Lampe ab Tag 4, Scanner ab Tag 6 im Kiosk, gefälschte Siegel an Tag 5, 7, 9, 11. Der Validator prüft, dass davor keine auftauchen.
- P100: Reisende sagen nach dem Stempel etwas. P98: Figuren antworten an Tag 1–4 persönlich.
- P104/P105: Wache und Schleuse. P33 (zum Teil): Warnlampe gelb/rot, Lautsprecher-Lämpchen.
- P102: leiser, je 4 Varianten. P97: alle Wanduhren gleich.

**Figuren**
- P85 (zum Teil): Mira bringt Essen, Senn Medizin, Korr Bonus oder Rüge per Durchsage, Shifter-Paket im Brief.
- P77: Senn, Quill, Nell und Oddo lassen etwas da. Noch da beim nächsten Auftritt: Pfand. Verkauft: zählt als verraten.

**Station**
- P101: Abendgäste. P99: Dealer sitzt. P80: Umschlag. P63: Clownsschuhe. P90: Kiosk-Gitter. P50: Türschilder.
- P108: Jede Person sieht anders aus, mit passendem Passfoto, auch in den endlosen Tagen. P107: Schiffe im Hangar.
- P26 (zum Teil): Bar-Ventilator dreht, Putzroboter fährt. P95: Terminal-Stil überall.
- P29 (zum Teil): Fernrohr, Stadtlichter folgen der Familie. P83: Asservatenschrank aufbrechen.
- P103: Lena fragt nach der Drohne, Korr gibt einen Tipp.

## Offen und warum
| Paket | Stand | Grund |
|---|---|---|
| **P106** Shuttle angedockt, begehbar | Lieferung liegt auf `assets/P106-au-en-hangar`, **nicht gemergt** | Groß: Hangarwand, Innenraum, Abflug umbauen, muss man begehen und anspielen. Lieber mit dir |
| **P94** Laute je Spezies | wartet | 50 Laute bestellt, noch nicht geliefert |
| P29 Rest | offen | Familienfoto im Spind, Brief auf dem Tisch |
| P33 Rest | offen | Nahansicht der Pinnwand im Stehen |
| P85 Rest | offen | Nells Sohn, Zusammenfassung Tag 11, Verhör-Durchsage |
| P26 Rest | offen | siehe Stand im Paket |
| P32 | offen | Gegenstände sichtbar im Schrank: braucht Modellplätze |
| P28 Sitzplätze, P48 Menü in Main | offen | Nicht mehr geschafft; P48 laut Paket erst, wenn alle Muss-Pakete fertig sind |
| P51 Zimmer 103 | offen | Gemälde von Elo braucht dein Foto, Bullauge ein neues Modell |
| P49 | bei dir | Unity-Lizenz und Secrets |
| P19 | vorbereitet | Upload braucht dein itch-Login |

## Lieferungen, die ich freigegeben habe
Alle Vorschauen angesehen, Dateien gegen die Bestellung geprüft.
| Lieferung | Inhalt | Eingebaut |
|---|---|---|
| `assets/P90-kiosk` | Kiosk-Theke neu, Rollgitter | ja |
| `assets/P91-schalter` | Sicherheitsgitter | ja |
| `assets/P91-counter-sfx` | Gitter und Handgemenge | ja |
| `assets/P95-ui-…` | 8 Terminal-Grafiken | ja, Scanlines und Warnstreifen noch ungenutzt |
| `assets/P104-pr-fkammer` | Wachmann | ja |
| `assets/P24-planets-sky-2` | Heimatplanet neu | ja |
| `assets/P102-paper-sfx` | 8 Papier-Geräusche, Münze fürs Fernrohr | ja |
| `assets/P63-ui-…` | Icon der Clownsschuhe (ersetzt meinen Platzhalter) | ja |
| `assets/P104-P107-sfx` | Stiefel, Alarm, Dekompression, Landung | ja |
| `assets/P107-hangar` | 4 kleine Schiffe | ja |
| `assets/P108-pr-fkammer` | 25 Texturen, 5 Frisuren/Hüte | ja |
| `assets/P106-au-en-hangar` | Shuttle, Kabine, Fenster | **nein**, siehe oben |


## Annahmen, die du leicht ändern kannst
- **P77:** Rückgabe läuft automatisch beim nächsten Auftritt am Schalter. Es sind vorhandene Waren, keine eigenen Gegenstände mit Label.
- **P101:** Niemand schläft im Wohnmodul, alle stehen. **P107:** Schiffe haben keine Kollision (stehen abseits).
- **P105:** Statt der Scheibe wackelt die Sicht.
- **Moral:** Befohlene Schleusen zählen weiter negativ (das war schon so). Fehlerfreies Spiel endet deshalb mit dem mittleren Satz („when the orders said to“), der saubere Satz bleibt für Spieler, die niemanden durch die Schleuse schicken. Grenze in `DepartureTuning.moraleBloodyBelow`.
- **Zeth, Tag 5:** Aus „Erlaubnis abgelaufen“ wurde ein gefälschtes Siegel, damit Tag 5 das erste Beispiel hat. Varn, Nimi und Sirr sind jetzt ebenfalls Verhaftungen. An Tag 9 gibt es dadurch keinen echten Durchlass mehr, das passt zur Inspektionswoche.

## Live und Prüfung
- **Live:** `4f65648` auf kestrel9.maxlworks.com (Version und Dateien nach dem Deploy geprüft). Alles auf `main` gepusht.
- **Automatisch geprüft:** 186 EditMode-Tests alle grün, Validate Travelers ohne Fehler, Economy Report alle Ziele ✓, WebGL startet.
- **Im Browser gesehen:** Menü, Tag 1, Inventar, Pause, Verhaftung mit Wache, Schleuse mit einer Figur (Sorvak), Terminal-Stil.
- **Im Editor-Bild gesehen:** Dealer, Kiosk-Gitter, Gadgets auf dem Tisch, Wanduhr.
- **Nicht von Hand geprüft:** Spätere Story-Tage, Tod und Briefe im Spiel, UV/Scanner auf echten Papieren, Abendgäste, Aufbrechen, Reaktionen nach dem Stempel, Schiffe (nur Plätze im Editor geprüft), Hangar morgens. Die Regeln dahinter decken Tests ab, das Zusammenspiel nicht. Das Fernrohr im Browser nur mit Tastatur: echtes Schwenken mit der Maus konnte die Automatisierung nicht testen.
- **Occlusion:** Ohne Sonne bleibt 1 Blick (bei (6; 1,6; −5), rund 30 von 36864 Pixeln) mit kleiner Abweichung, nicht wie geplant 0; mit Sonne 2 Blicke, der bekannte (rund 100 Pixel) und dieser. Sichtbar war nichts Falsches.
- **Geldänderungen:** Wurden gemessen, Report grün. Pfand und Bonus aus P77/P85 rechnet der Simulator nicht mit (wenige Credits).

## GitHub-Issues
Geschlossen: #19, #20, #21, #23. Offen: #16 (wartet auf die Laute), #17 (dein Urteil zum Hangar-Flackern), #25 (vorerst nicht).

## Herunterfahren
**Nicht heruntergefahren.** Die Main-session hat auf meine Frage mit „Ja, der PC muss anbleiben“ geantwortet: Der Asset-Nachtlauf `pc_night` läuft noch (3 Blender-Prozesse, 193 offene Zeilen) und fährt den PC nach eigener Aussage selbst herunter. Ob das geklappt hat, kann ich nicht mehr prüfen.
