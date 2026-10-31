# Game Design

> **Status: ENTWURF (31.10.)** — Konzept von Max & Martin vor der Theme-Bekanntgabe, von Claude ausgearbeitet.
> Punkte mit ❓ sind noch offen. Nach der Theme-Bekanntgabe (01.11., ca. 22:37 MEZ) wird das Theme eingewoben.
> Änderungen am Kern nur nach Absprache (Entscheidung zusätzlich in `DECISIONS.md` loggen).

## Theme
<Theme + unsere Interpretation in 1–2 Sätzen — folgt nach der Bekanntgabe.>
"Theme Interpretation" ist eine eigene Bewertungskategorie im Jam → das Theme muss sichtbar ins Spiel.

## Pitch
Du bist Zollbeamter auf einer kleinen, heruntergekommenen Zollstation im All. Jeden Tag prüfst du Reisende
und entscheidest: durchlassen, abweisen, verhaften — oder die Luftschleuse öffnen. Mit deinem Lohn hältst du
deine Familie auf dem Heimatplaneten am Leben. Nach der Schicht läufst du frei über die Station, kaufst
Gadgets, handelst unter der Theke und schickst Pakete nach Hause.

Arbeitstitel: ❓ (nach dem Theme)
Sprache des Spiels: Englisch.

## Kern-Loop (ein Spieltag)
1. **Morgen — Unterkunft:** Nachricht/Brief der Familie lesen, neue Regeln des Tages (Bulletin), ggf. neues Gadget.
2. **Schicht — Zollschalter:** Reisende kommen nacheinander. Dokumente und Gepäck prüfen, mit den Regeln
   abgleichen, entscheiden. Jede richtige Entscheidung bringt Geld, Fehler kosten Geld (Verwarnung → Abzug).
   Schicht endet nach Zeit oder Anzahl Reisender.
3. **Feierabend — Station erkunden:** Läden besuchen, Geld ausgeben, beschlagnahmte Gegenstände verkaufen,
   behalten oder an die Familie schicken, NPCs zuhören (Hinweise, Story).
4. **Nacht — Schlafen:** In der Unterkunft ins Bett → Tagesabrechnung: Lohn − Miete − Bedarf der Familie
   (Essen, Wärme, Medizin). Was nicht bezahlt wird, hat Folgen (Familienmitglied wird krank, hungert, ...).
5. Nächster Tag: neue Regeln, schwierigere Fälle, neue Gadgets.

**Ziel:** 3 Tage (ca. 10–15 Minuten) überstehen, ohne dass die Familie zerbricht oder du gefeuert wirst.
**Herausforderung:** Zeitdruck am Schalter, wachsende Regelmenge, Geldknappheit, moralische Entscheidungen.
**Belohnung:** Geld, Gadgets, die die Prüfung erleichtern, Story-Momente, mehrere Enden.

## Entscheidungen am Schalter
| Aktion | Wirkung | Risiko |
|---|---|---|
| Durchlassen | Reisender betritt die Station | Falsch durchgelassen → Abzug |
| Abweisen | Reisender fliegt zurück | Falsch abgewiesen → Abzug |
| Verhaften (Knopf) | Wache führt ab, beschlagnahmte Gegenstände landen bei dir | Unschuldig verhaftet → größerer Abzug |
| Luftschleuse (Knopf) | Sofortige Eliminierung, kein Zeitverlust | Unschuldige: hoher Abzug. Jede Nutzung senkt einen versteckten Moral-Wert → färbt Familienbriefe und Ende |

## Station (klein, kompakt — ein Gebäude, kurze Laufwege)
| Ort | Funktion im Spiel |
|---|---|
| **Wachstation / Zollschalter** | Arbeitsplatz. Hier läuft die Schicht. Gadget-Ausgabe durch den Vorgesetzten |
| **Hangar** | Reisende kommen an. Frachtdrohne: Pakete an die Familie schicken |
| **Bar** | Barkeeper verkauft unter der Theke illegale Gegenstände (Schwarzmarkt), kauft Beschlagnahmtes an |
| **Diner** | Essen, NPC-Gespräche, Gerüchte = Hinweise auf kommende Schmuggler (❓ oder nur Atmosphäre) |
| **Weltraum-Motel** | Reisende übernachten. Vorschlag: deine Unterkunft ist ein Motelzimmer → spart einen Raum |

## Gadgets (freischalten über die Tage oder kaufen)
- Dokumentenscanner — prüft Dokumente auf Fälschung (kaufbar)
- UV-Lampe — macht versteckte Siegel sichtbar
- Gepäckscanner — zeigt Schmuggelware
- Bioscanner — prüft Spezies/Identität
❓ Welche werden freigeschaltet, welche gekauft, welche gibt es nur am Schwarzmarkt?

## Familie
- Lebt auf dem Heimatplaneten, braucht täglich Geld (Essen, Wärme, Medizin).
- Du kannst **Gegenstände physisch schicken** (Hangar → Frachtdrohne), auch beschlagnahmte.
  Idee: Ein Medikament direkt zu schicken ist billiger als das Geld dafür — aber Schmuggelware nach Hause
  zu schicken kann auffliegen.
- Briefe der Familie am Morgen erzählen die Story und zeigen die Folgen deiner Entscheidungen.

## Steuerung
- Ego-Perspektive, **WASD + Maus**, **E** = interagieren.
- Am Schalter: E → Kamera fährt fest auf den Tisch, Dokumente sind scharfe 2D-Karten, mit der Maus ziehen und vergleichen.
- Nur Tastatur & Maus (Browser). Kein Gamepad, kein Mobile.

## Umfang
### Muss (ohne das kein Spiel)
- Zollschalter mit Dokumentenprüfung: 2–3 Dokumenttypen, Regeln pro Tag, Durchlassen/Abweisen/Verhaften/Luftschleuse
- Reisende als Daten (ScriptableObjects): Aussehen, Dokumente, Fehler, Gepäck
- Tagesablauf: Morgen → Schicht → Feierabend → Schlafen → Abrechnung, 3 Tage (~15 Reisende)
- Geld + Familie mit Bedarfen und Folgen
- Begehbare Station: Schalter, Unterkunft, Bar (Schwarzmarkt), Hangar (Pakete)
- 1 Gadget (Dokumentenscanner) kaufbar
- Startbildschirm, Menü, Sieg/Niederlage-Ende
### Soll (macht es gut)
- Diner mit Gerüchten/Hinweisen, weitere Gadgets
- Mehrere Enden (abhängig von Familie, Luftschleusen-Nutzung, Geld)
- Voicelines (ElevenLabs) für Reisende und NPCs
- Retro-Look mit Low-Res-Filter
### Kann (wenn Zeit bleibt)
- Story-Reisende, die wiederkommen
- Bestechung am Schalter
- Wachen-NPCs, die durch die Station laufen
### Bewusst NICHT
- Kampf, Physik-Rätsel, offene Welt, Speichern/Laden mitten im Tag, Mehrspieler, Gamepad/Mobile

## Look & Sound
- **Art Style (Richtung, final nach dem Theme):** Grafik wie Mouthwashing (Low-Poly, kleine Texturen, Low-Res-Rendering,
  harte Pixel), Asset-Design wie Alien: Isolation (analoger Retro-Futurismus, CRTs, klobige Industrie-Technik). Details: `docs/STYLE.md`.
- **Dokumente/UI** werden in voller Auflösung über dem Low-Res-Bild gezeichnet → bleiben lesbar.
- **Musik & SFX:** freie Assets (Lizenz in `CREDITS.md`).
- **Voicelines:** selbst aufnehmen, mit ElevenLabs in verschiedene Stimmen umwandeln. Ein Aufnahmetag,
  wenn alle Texte feststehen (nach dem Feature-Freeze).
- **Schrift:** Orbitron (Titel, Menü) + Share Tech Mono (Dokumente, UI), beide OFL / Google Fonts.
- **Sprache:** Englisch (Texte und Voicelines).
- Asset-Regeln: `docs/ASSETS.md`.

## Aufgabenteilung
| Bereich | Hauptverantwortlich |
|---|---|
| Alle finalen Assets (3D, Texturen, UI-Grafik, Sound-/Musik-Auswahl, Voicelines) | max |
| Startbildschirm: alles vom Spielstart bis zum Menü (mit Platzhaltern) | martin |
| Alles andere | offene Pakete nach `docs/team/PAKETE.md`, wer Zeit hat |
