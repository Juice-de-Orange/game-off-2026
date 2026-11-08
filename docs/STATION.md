# Station: Bauplan (Ebene 0)

> **Status: FREIGEGEBEN, Stand 2.0 (max, 02.11.2026)** — Grundlage für P04/P21/P22 und die Asset-Liste.
> 2.0: Hangar 36 × 28 × 16 m, Aussichtsplattform statt Panoramafenster, Barfenster Nord.
> Grafische Fassung: `docs/station/bauplan.html` (im Browser öffnen).
> Änderungen am Grundriss nur nach Absprache, dann hier anpassen und in `DECISIONS.md` loggen.

## Koordinaten
- Unity-Welt: **X = Osten, Z = Norden, Y = oben**. Ursprung (0,0,0) = Mitte der Halle auf Bodenhöhe.
- Alles auf einem **2-m-Raster** (Modul-Bausatz: Boden 2×2 m, Wand 2 m breit × 3 m hoch).
- Wandstärke in der Graybox: 0,2 m, mittig auf der Raumkante. Böden auf Y = 0.
- Arbeitsname der Station: **KESTREL-9** (nicht final).

## Räume
| Raum | X von..bis | Z von..bis | Höhe | Leuchtfarbe | Prio |
|---|---|---|---|---|---|
| Halle | −8..8 | −6..6 | 6 | neutral / kaltes Neon | Muss |
| Servicegang ("STAFF ONLY") | −8..−6 | −12..−6 | 3 | neutral | Muss |
| Schalter (Arbeitsplatz) | −6..0 | −12..−6 | 3 | Cyan | Muss |
| Prüfkammer (= Luftschleuse) | 0..4 | −12..−6 | 3 | Alarmrot | Muss |
| Hangar | −18..18 | −40..−12 | 16 | Weiß / Alarmrot am Tor | Muss |
| Aussichtsplattform | Halbkreis, Mitte (0, 6), r = 5 | 6..11 | 6 (Boden −0,36) | neutral / Planetenschein | Soll |
| Motelgang | −20..−8 | 0..2 | 3 | Giftgrün | Muss |
| Motelzimmer 101 (verschlossen) | −12..−8 | 2..6 | 3 | Giftgrün | Muss |
| Motelzimmer 102 (verschlossen) | −16..−12 | 2..6 | 3 | Giftgrün | Muss |
| Motelzimmer 103 = Spielerzimmer | −20..−16 | 2..6 | 3 | Giftgrün | Muss |
| Diner (außer Betrieb, P92) | −16..−8 | −6..0 | 3 | dunkel | Soll |
| Bar | 8..18 | 0..8 | 4 | Orange / Violett | Muss |
| Kiosk (Gadget-Shop) | 8..12 | −6..−2 | 3 | Violett | Muss |

## Öffnungen
| Öffnung | Wand | Position (Mitte) | Breite × Höhe |
|---|---|---|---|
| Halle ↔ Motelgang | X = −8 | Z = 1 | 2,0 × 2,6 (offen) |
| Zimmertüren 101 / 102 / 103 | Z = 2 | X = −10 / −14 / −18 | 1,2 × 2,2 |
| Halle ↔ Diner | X = −8 | Z = −3 | 1,6 × 2,2 |
| Halle ↔ Bar | X = 8 | Z = 3 | 1,6 × 2,2 |
| Kiosk-Durchreiche (Theke) | X = 8 | Z = −4 | 2,8 breit, 1,0–2,2 hoch, Sturz darüber |
| Halle ↔ Servicegang | Z = −6 | X = −7 | 1,2 × 2,2 |
| Servicegang ↔ Hangar | Z = −12 | X = −7 | 1,2 × 2,2 |
| Servicegang ↔ Schalter | X = −6 | Z = −9 | 1,2 × 2,2 |
| Prüfkammer → Halle (Durchlassen) | Z = −6 | X = 2 | 1,6 × 2,2 |
| Hangar → Prüfkammer (Ankunft/Abweisen) | Z = −12 | X = 2 | 1,6 × 2,2 |
| **Außenschott** Prüfkammer → All | X = 4 | Z = −9 | 1,6 × 2,2 |
| **Hangartor** → All (Energiebarriere, P23) | Z = −40 | X = 0 | 28 × 12 |
| Halle ↔ Aussichtsplattform | Z = 6 | X = 0 | 10 × 6 (volle Höhe), 1 Stufe 0,18 m + Plattform −0,36 m |

## Fenster
| Fenster | Wand | Bereich | Brüstung–Sturz |
|---|---|---|---|
| Glasfront Aussichtsplattform | Halbkreis r = 5 | 8 Segmente à 1,95 m | 0,14–6 m (Brüstung 0,5 m über Plattformboden) |
| Barfenster | Z = 8 | X 10..18 | 0,9–2,4 m (Modul `SM_Station_Wall_Window_2x3`) |
| Scheibe Schalter → Prüfkammer (mit Dokument-Durchreiche, `SM_Station_Window_Small_Slot`) | X = 0 | Z −11..−7 | 0,9–2,2 m |
| Fenster Schalter → Hangar (2× `SM_Station_Wall_Window_2x3`) | Z = −12 | X −6..−2 | 0,9–2,4 m |
| Bullauge Zimmer 103 (`SM_Station_Wall_Porthole_2x3`) | X = −20 | Z = 5 | Ø 0,6 m, Mitte 1,5 m |

## Am Schalter im Sitzen (Stand P40/P41)
- **Blick:** Kamera 1,55 m hoch, 20° geneigt, mit A/D ±70° drehbar (`SeatedLookTuning`), die Neigung geht dabei auf waagerecht.
- **Pinnwand:** `SM_Counter_Board` an der Nordwand des Schalterraums, Mitte (−1,5 / 1,75 / −6,23), etwa 80° links vom Tisch; zwei Zettel, Position in `DocumentPrefabBuilder.WireBoard` (Kopie, bei Verschieben mit anpassen).
- **Reisender:** auf der Standmarkierung (2, −9), Blick zum Schalter. Kommt durch die Hangartür (Süd). Abgang durchlassen: Nordtür, abweisen: Hangartür; Verhaftung und Schleuse hinter dem Gitter vor der Scheibe (P91). Die beiden Kammertüren haben eine unsichtbare Sperre gegen den Spieler.

## Wichtige Positionen (für Spawnpunkte, NavMesh, Interaktion)
| Punkt | Position (X, Z) |
|---|---|
| Spieler-Start (Bett in 103) | (−18, 4) |
| Schalterstuhl / Kamera-Anker Schalteransicht | (−2, −9), Blick nach +X |
| Standmarkierung Reisender in der Kammer | (2, −9) |
| Shuttle-Landefeld (Mitte) | (0, −28) |
| Shuttle-Rampe | (0, −23) |
| Shuttle-Einflug von außen (P25) | (0, 6, −90) |
| Warteschlange (Absperrgitter X = 1 und 3) | Z −23..−12 |
| Frachtdrohne (Pad 3×3) | (−13, −16,5) |
| Bar-Theke | X 14..15, Z 1..7 |
| Kiosk-Theke | X 8,2..9, Z −5,4..−2,6 |
| Anzeigesäule Halle | (0, 0), 2×2 |

## Abläufe
- **Reisende:** Shuttle → Warteschlange → Prüfkammer (Standmarkierung) → Durchlassen: Nordtür → Halle → Motel/Bar.
  Abweisen: zurück in den Hangar. Verhaften: Wache kommt über die Halle durch die Nordtür.
  Luftschleuse: hinter dem heruntergefallenen Gitter, nur zu hören (P91).
- **Spieler:** Zimmer 103 → Motelgang → Halle → Servicegang → Schalter. Hangar (Frachtdrohne) über den Servicegang.
  Der Spieler betritt die Prüfkammer nie.
- Laufwege bei 4 m/s: Zimmer → Schalter 6 s, Halle → Plattform 3 s, Schalter → Hangar-Südende 11 s (gemessen, Stand 2.0).

## Graybox in Unity (P04)
- Prefab: `Assets/_Project/Prefabs/Station/PF_Station_Graybox.prefab`, eingebaut in `Main.unity` (Instanz `Station_Graybox`).
- Erzeugt vom Editor-Skript `Assets/_Project/Scripts/Editor/StationGrayboxBuilder.cs`, Menü **GameOff → Build Station Graybox**.
  Grundriss geändert? Zahlen im Skript anpassen (gleiche Tabellen wie oben), Menü ausführen, NavMesh neu backen.
  Das Skript ersetzt die Instanz in der offenen Szene und überschreibt das Prefab (vorher claimen!).
- Struktur: `Floors`, `Ceilings`, `Walls/<Raum>`, `Glass`, `Lights` (ein Punktlicht pro Raum in der Raumfarbe),
  `Props_Placeholder` (Blöcke mit den Namen aus `ASSET-REQUESTS.md`), `Markers` (PlayerSpawn, CounterCameraAnchor,
  TravelerStandPoint, ShuttleRamp).
- Materialien: `Assets/_Project/Art/Materials/Graybox/M_Graybox_*` (nur Graybox, werden durch den Bausatz ersetzt).
- NavMesh: `NavMeshSurface` am Prefab-Root, Daten in `Assets/_Project/Scenes/Main/`. Agent-Radius **0,3 m**
  (Personaltüren sind 1,2 m breit). Decken und Glas sind ausgeschlossen.
- Gemessene Wege (NavMesh, 4 m/s): Zimmer 103 → Schalter 24 m / 6 s · Schalter → Frachtdrohne 13 m / 3 s ·
  Schalter → Bar 28 m / 7 s · Bar → Zimmer 103 33 m / 8 s · Shuttle → Kammer 6 m / 1,5 s.
- Noch offen (andere Pakete): Türen sind nur Öffnungen (Schiebetüren mit P05/P08), Spieler-Controller (P03).

## Licht & Stimmung (P20)
- Lichter kommen aus dem Graybox-Builder: pro Raum ein Raster gebackener Punktlichter (alle 4–5 m) mit Leuchtstreifen darunter
  (`LightFixtures`, Platzhalter für `SM_Station_Ceiling_2x2_Light`). Streifen in voller Raumfarbe, Raumlicht je Bereich zu Weiß hin
  aufgehellt (`LightTint` im Skript). Hangar: weißes Arbeitslicht + zwei rote Lichter am Tor. Prüfkammer: rote Warnlampe.
  Halle: bläulicher Schein am Panoramafenster. Zwei flackernde Echtzeit-Neons (Servicegang, Motelgang, `LightFlicker`).
- Szenen-Licht per Menü **GameOff → Setup Main Lighting + Bake** (`Scripts/Editor/MainLightingSetup.cs`): Skybox `M_Sky_Space`
  (Platzhalter-Textur `T_Sky_Space.png`), Ambient flach dunkelblau, Nebel Exp² 0,025, `PlanetLight` (gerichtet, kalt, von Norden),
  Lightmaps 4 Texel/m, max. 1024 px, 1 Bounce, Light Probes im 2,5-m-Raster (für Knöpfe, Stempel, später Reisende), NavMesh neu.
- **Nach jedem Neubau der Graybox:** erst *Build Station Graybox*, dann *Setup Main Lighting + Bake*, warten bis der Bake fertig ist,
  Szene speichern. Bake dauert ca. 1 Minute; Lightmaps liegen in `Scenes/Main/` (~1,6 MB).

## Bausatz-Module (P21)
- Der Builder legt unter `Visual/<Raum>` die Module aus `Art/Models/Station/` im 2-m-Raster an: Boden (y = −0,1), Decke (y = Raumhöhe),
  Wand 2×3 m, Türwand (nur für 1,6 × 2,2-m-Türen in der Zellenmitte). Hohe Räume werden in 3-m-Reihen gestapelt.
- Modul-Vorderseite = lokale −Z; der Builder dreht jede Wand zur Raummitte.
- Wo kein Modul passt, bleibt eine sichtbare Graybox-Füllung (ohne Collider). Kollision kommt immer aus den unsichtbaren Graybox-Blöcken.
- Möbel mit Modell werden per `KitProp` gesetzt (Collider aus den Modell-Grenzen), sonst Grau-Block.

## Himmel & Außenwelt (P24)
- Menü **GameOff → Build Sky & Exterior** (`Scripts/Editor/SkyExteriorBuilder.cs`) baut `Prefabs/Environment/PF_SkyRig` und `PF_Exterior`
  und setzt beide in die offene Szene. Neue Lieferungen (Außen-/Schiffsmodelle, Himmelstexturen) werden beim nächsten Lauf genutzt.
- **SkyRig** (`Scripts/Core/SkyRig.cs`): Sonne (Billboard-Glühen + Echtzeit-Richtungslicht mit Schatten), Heimatplanet (Norden, knapp unter
  Augenhöhe, Stadtlichter auf der Nachtseite), Gasriese mit Ring (Nordost, hoch), Mond (Nordwest). `SetDay(1..3)` dreht den Himmel um 0/18/36°
  und senkt die Sonne von 16° auf 3° → das Licht durch die Fenster ändert sich jeden Tag (Theme "Shift"). Im Editor: Feld `previewDay`.
- Shader: `GameOff/SkyPlanet` (Tag/Nacht nach `_GameOffSunDir`, Atmosphärenrand), `GameOff/SkyGlow` (Billboard im Vertex-Shader),
  `GameOff/PlanetRing`. Kein Nebel auf Himmelsobjekten.
- **Außenwelt** (Hülle und Verbindungsteile geliefert, P42): Wohnmodule West/Ost (West begehbar seit P53, siehe „Wohnmodul“; Ost nur Außenmodell), Träger, Solarflügel Nord/Süd, Antenne, Andockarm mit geparktem Schiff,
  Einfassung der Hangaröffnung, blinkende Positionslichter (`BlinkLight`). Sichtbar von Plattform, Bar und Hangar.
- Nebeldichte auf 0,012 gesenkt, damit die Außenwelt durch die Fenster sichtbar bleibt.
- Platzhalter-Texturen (prozedural) liegen an den finalen Pfaden unter `Art/Textures/Sky/` — die Pipeline ersetzt nur die Dateien.

## Wohnmodul (P53)
Plan und Teile: `docs/station/wohnmodul.md`. Gebaut von `StationHabBuilder` (Menü `GameOff/Build Hab Module into Main`, Teil von Rebuild All) aus der Tabelle `HabLayout`, Prefab `PF_Station_Hab`, nur in Main.
- **Weg:** `Door_HabTube` (x −20, z 1) → Podest → Treppe bis y 7,42 (für den Spieler eine Rampe mit 31,7°) → Bogen nach Norden → Schott bei z 14 → Flur x −37,6..−35,4 bis z 38.
- **Zimmer:** 201–212 rechts und links vom Flur, 201–208 und 212 verriegelt, 209–211 offen. Keine Bewohner, nur Grundausstattung.
- **Kollision:** einfache Boxen wie in der Station, die Modelle haben keine.
- **Navigation:** eigene Surface, `Link_MotelCorridor` verbindet sie durch die Tür mit der Station.
- **Sonne:** `SunBlockers` sind Quader nur für den Schatten und werden nicht gecullt, sonst fällt Sonnenlicht durch die Hülle. Das Modul hat keine Fenster.
- **Licht:** gebacken in der Lichtfarbe von Zimmer 103, je Zimmer eine Lampe, Lichtsonden über `ProbePositions`.
- **Außen:** `Hab_West` nutzt `SM_Ext_HabModule_Entry`, die Röhre steht in `HabLayout.Outside` und kommt von `SkyExteriorBuilder`; sie teilt sich das Material `M_Ext_HabModule`.

## Abwechslung & Rundungen (Builder, vorbereitet 02.11.)
- Der Builder setzt automatisch, sobald die Pipeline liefert (Aufträge M007): Wandvarianten pro Raumtyp (gepolstert in Motel/Schalter,
  Holz in Bar/Diner, ~35 % Detailwände mit Rohren/Lüftung/Schäden/Bildschirm/Spinden, feste Verteilung), Füllstück 2 × 1 m (Bar),
  Gitterboden (Servicegang), Deckenleuchten-Platten, Tonnengewölbe in 2 m breiten Gängen, gerundete Raumecken, runde Säulen an der
  Plattform-Öffnung, runde Türrahmen, Rundbogen Halle ↔ Motelgang, Plattform-Portal, Hallenkuppel, Plattform-Geländer, Hangar-Rippen.
- **NavMesh-Wege und Türen:** Personaltüren und Zimmer 103/101/102 haben ein NavMesh-Hindernis. Weg-Messungen *durch* diese Türen
  melden deshalb "teilweise" — das ist gewollt (Reisende bleiben draußen). Der Spieler ist kein NavMesh-Agent und kommt durch.
