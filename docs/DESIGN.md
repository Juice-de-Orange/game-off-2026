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

## Regeln am Schalter (festgelegt 04.11., Max)
**Fehlerarten:** Papiere widersprechen sich (Name/ID) · Papier abgelaufen oder fehlt · Tagesregeln aus dem Bulletin (wechseln täglich:
gesperrte Herkunft, Pflichtpapiere für bestimmte Spezies, verbotene Waren, Quarantäne) · Fälschung (Siegel, nur mit Gadget) und Schmuggel.

| Fall | Richtige Entscheidung |
|---|---|
| Alles in Ordnung | Durchlassen |
| Papierfehler, gesperrte Herkunft, fehlendes Pflichtpapier | Abweisen |
| Schmuggel, Fälschung, Fahndung „verhaften“ | Verhaften (Luftschleuse zählt auch als richtig, kostet aber Moral) |
| Seuchen-Quarantäne, Fahndung „auf Sicht terminieren“ | **Luftschleuse ist Pflicht** (Verhaften = Befehl verweigert) |

Priorität bei mehreren Fehlern: Pflicht-Schleuse > Verhaften > Abweisen > Durchlassen. Die richtige Entscheidung wird aus den Daten
berechnet; jeder Reisende hat zusätzlich eine beabsichtigte Lösung, Prüfwerkzeug + Tests vergleichen beides (keine Logikfehler im Spiel).

**Feedback:** Fehler → sofort Verwarnzettel aus dem Drucker am Tisch (mit Grund); die ersten zwei pro Tag sind Verwarnungen, danach Abzug.
**Regeln lesen:** Bulletin + Fahndungsliste an der Pinnwand, Lautsprecher-Durchsagen; im Sitzen kann man sich umschauen.
**Reisende:** stehen in der Prüfkammer am Fenster, begrüßen dich; Nachfragen per Klick auf ein Feld eines Dokuments. Alien-Sprache
erscheint erst als Glyphen und wird kurz „entschlüsselt“.

**The Shifter:** wiederkehrender Reisender, jeden Tag mit neuer Identität (Name, Herkunft, Detail), erkennbar an einer Kleinigkeit.
Tag 1–2 harmloser, charmanter Grenzgänger. Tag 3 steht er auf der Fahndungsliste: „auf Sicht terminieren“, ohne Begründung. Hast du ihm
geholfen, steckt er dir etwas für die Familie zu. Gehorchen (Schleuse), verhaften oder heimlich durchlassen färbt Briefe und Ende.

## Station (kompakt, kurze Laufwege — Grundriss Stand 2.0: `docs/STATION.md`, Bild: `docs/station/bauplan.html`)
| Ort | Funktion im Spiel |
|---|---|
| **Zollschalter + Prüfkammer** | Arbeitsplatz; Reisende stehen in der Kammer, die zugleich Luftschleuse ist (Außenschott klappt ins All). Kein sichtbarer Vorgesetzter: Lautsprecher-Durchsagen, Memos, Pinnwand |
| **Hangar** (36 × 28 × 16 m) | Shuttle fliegt morgens durch die Energiebarriere ein, Reisende steigen über die Rampe aus und stellen sich an; Frachtdrohne für Pakete nach Hause; durchsuchbare Kisten |
| **Transithalle** | Knotenpunkt mit Anzeigesäule (Live-Ankunftsplan), Bänken, Getränkeautomat |
| **Aussichtsplattform** | Halbrunde Glasfront: Blick auf den Heimatplaneten (Stadtlichter = Zustand der Familie), Sonne, Gasriese; Münzfernrohr |
| **Bar "The Last Orbit"** | Barkeeper (Hocker = Gespräch), Schwarzmarkt unter der Theke, Fensterband |
| **Diner** | Essen, Gerüchte (Soll) |
| **Kiosk** | Gadgets kaufen (Dokumentenscanner) |
| **Motel** | Dein Zimmer 103 (Bett, Brief, Spind mit Familienfoto, Bullauge = Abendblick nach Hause); 101/102 verriegelt |

Theme-Bezug in der Welt (Vorschlag für den Call): die Sonne steht jeden Tag anders (die Station dreht sich), und an Tag 3
flackern Lichter und Schilder häufiger — die Station "verschiebt" sich mit dir.

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
