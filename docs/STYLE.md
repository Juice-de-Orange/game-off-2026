# Style Guide

> **Status: ENTWURF (31.10.)** — Richtung beschlossen: **Grafik wie Mouthwashing, Asset-Design wie Alien: Isolation.**
> Endgültig festgelegt wird der Stil nach der Theme-Bekanntgabe (02.11.). Max (Art-Verantwortung) gibt die Datei dann frei. Die Asset-Pipeline baut zuerst
> **ein Muster aus 3 Assets** zum Abnicken, bevor sie die Liste abarbeitet.

## Stilrichtung
- **Rendering & Grafik → Mouthwashing:** Low-Poly, kleine Point-gefilterte Texturen, niedrige Render-Auflösung,
  harte Pixel, flaches bzw. gebackenes Licht.
- **Asset-Design → Alien: Isolation:** analoger Retro-Futurismus der späten 70er/80er. Klobige Industrie-Technik,
  CRT-Monitore mit grüner oder bernsteinfarbener Schrift, beige/cremefarbene Kunststoffgehäuse, Kippschalter,
  Drehregler, Kabelbündel, Rohre, Gitterroste, Warnstreifen, gepolsterte Wandpaneele, schwere Schotttüren.
  Technik wirkt gebraucht, gewartet, nie elegant. Keine Hologramme, keine Touchscreens.
- **Keine fremden Marken:** keine Logos, Namen oder Schriftzüge aus Alien oder Mouthwashing, alles selbst erfunden.

## Stimmung
Kleine, heruntergekommene Zollstation am Rand des Alls. Funktional, eng, abgenutzt. Kaltes Neonlicht in den
Gängen, warmes Licht in Bar und Diner. Bürokratie trifft Weltraum-Arbeiterklasse. Unterschwellig bedrückend,
aber nicht Horror.

## Palette
`docs/style/palette.png` — 24 Farben (Entwurf). Alle Texturen bleiben in dieser Palette.

| Gruppe | Farben |
|---|---|
| Neutral (Metall, Wände, Kunststoff) | `#0D0F14` `#1B1F27` `#2C323C` `#444B55` `#646B73` `#8B9096` `#B7B9B5` `#E2DDCF` |
| Warm (Kunstholz, Rost, Bar, Diner) | `#3A2418` `#5E3A24` `#8A5A34` `#B9824A` |
| Kalt (Station, Bodenbeläge, Glas) | `#1E3A46` `#2F5B66` `#4F8A8B` `#8FBFB6` |
| Akzente / Leuchtfarben (CRT-Grün = `#9BE05A`, CRT-Bernstein = `#FFD36B`) | `#FF9A3C` warmes Orange · `#FFD36B` Bernstein · `#E2463F` Alarmrot · `#7A1F2B` Dunkelrot · `#9BE05A` Giftgrün · `#3F7A3A` Dunkelgrün · `#5EC8FF` Bildschirm-Cyan · `#C27BD6` Neonviolett |

## Rendering
- Interne Render-Auflösung: **640 × 360**, hochskaliert mit Point-Filter (Paket P17). UI und Dokumente in voller Auflösung darüber.
- Texturen: Filter **Point**, keine Mipmaps, keine Normal-/ORM-Maps. Albedo ohne eingebackenes Licht, Emission getrennt.
- Licht: hauptsächlich gebacken, wenige Echtzeit-Lichter, leichter Nebel in Gängen und im Hangar.

## Texel-Dichte
- Welt-Props und Räume: **64 px/m**
- Nahsicht am Schalter (Tisch, Knöpfe, Gadgets): **128 px/m**
- UI-Grafiken und Dokumente: volle Auflösung, Filter Bilinear

## Formen & Oberflächen
- Low-Poly mit **harten Kanten**, flache Flächen, wenig Rundungen (Rundungen mit 6–8 Segmenten).
- Abnutzung **mittel**: Kratzer, Dellen, abgeplatzter Lack an Kanten, Schmutz in Ecken. Nichts ist neu.
- Proportionen leicht übertrieben (dicke Rahmen, klobige Knöpfe), damit sie bei 640 × 360 lesbar bleiben.
- Leuchtelemente (Streifen, Displays, Schilder) über Emission in den Akzentfarben. Pro Raum eine dominante Leuchtfarbe:
  Schalter = Cyan, Bar = Orange/Violett, Diner = Bernstein, Hangar = Alarmrot/Weiß, Motel = Giftgrün.

## Schrift
- Titel, Menü, Stationsschilder: **Orbitron** (OFL).
- Dokumente, UI-Texte, Displays: **Share Tech Mono** (OFL).
- Schilder und Decals: erfundene Firmennamen und Texte, Englisch, Großbuchstaben, gern mit Seriennummern und Warnhinweisen.

## Referenzen
35 Bilder unter `docs/style/refs/` (`ai_` = Alien: Isolation, `mw_` = Mouthwashing, `real_` = freie Fotos, Name `<präfix>_<thema>_<nr>.webp`).
Was jedes Bild zeigt und wofür es steht: `docs/style/refs/README.md`.
Gesucht: Mouthwashing-Screenshots (Rendering, Licht, Auflösung) und Alien-Isolation-Screenshots
(Terminals, Gänge, Türen, Möbel, Kantine/Bar). Referenzbilder dienen nur zur Orientierung und werden nicht ins Spiel übernommen.
