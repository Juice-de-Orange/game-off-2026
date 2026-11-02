# Entscheidungen

Nur anhängen, nie löschen. Wird eine Entscheidung revidiert, neuen Eintrag schreiben, der auf den alten verweist.

| Datum | Wer | Entscheidung | Warum |
|---|---|---|---|
| 29.10. | max | Unity 6000.3.23f1, WebGL als Hauptziel | Browser-Spiele bekommen im Jam deutlich mehr Bewertungen |
| 29.10. | max | Alle arbeiten auf `main`, Abstimmung über `docs/team/` | Zwei Leute, ein Monat — Branches wären mehr Aufwand als Nutzen |
| 29.10. | max | Szenen/Prefabs nur mit Claim bearbeiten, Features in eigenen Sandbox-Szenen bauen | Unity-Szenen lassen sich nicht zuverlässig mergen |
| 29.10. | max | Abstimmung der Claudes über Dateien: eine Datei pro Übergabe/Frage, Claims im Board | Keine gleichzeitigen Änderungen an derselben Datei → keine Merge-Konflikte |
| 29.10. | max | Unity-Projekt erst am 01.11. anlegen | 2D/3D und Vorlage hängen vom Theme ab |
| 31.10. | max + martin | Timeline: 3 Wochen Entwicklung (bis Feature-Freeze 22.11.), 1 Woche Polish (bis Content-Freeze 28.11.), 29.–30.11. Puffer, Abgabe 30.11. | Polish-Woche schützen; früher WebGL-Build und Checkpoint 09.11. lassen Zeit zum Umsteuern |
| 31.10. | max + martin | Max macht alle finalen Assets; Martin nutzt nur Platzhalter und baut den Startbildschirm (Spielstart bis Menü) | Klare Zuständigkeit, keine Überschneidung bei Art-Dateien |
| 31.10. | max + martin | Konzept steht vor dem Theme: Zollstation im All (Prüf-Gameplay + frei begehbare Station); das Theme wird am 02.11. eingewoben | Schneller Start; Theme Interpretation wird bewertet, deshalb bewusst einweben statt dranhängen |
| 31.10. | max + martin | 3D, Spiellänge 3 Tage (~10–15 min) | Sicher fertig werden; Jam-Spieler spielen selten länger |
| 31.10. | max + martin | Prüfung am Schalter: feste Schalter-Ansicht + Dokumente als scharfe 2D-Karten | Lesbar trotz Low-Res-Look, schnell zu bauen |
| 31.10. | max + martin | Spielsprache Englisch | Internationaler Jam, mehr Bewertungen |
| 31.10. | max + martin | Tech-Stack: C#, Universal 3D (URP), uGUI + TextMeshPro, Cinemachine 3, ProBuilder, AI Navigation, ScriptableObjects, DOTween, eigener FPS-Controller; WebGL 2, Brotli + Decompression Fallback, Wasm 2023, IL2CPP | URP ist für WebGL empfohlen; einsteigerfreundlich; Tuning über ScriptableObjects |
| 31.10. | max + martin | Luftschleuse: hoher Abzug bei Unschuldigen + versteckter Moral-Wert, der Briefe und Ende färbt | Wenig Aufwand, starke Wirkung |
| 31.10. | max + martin | Schriften: Orbitron (Titel) + Share Tech Mono (Dokumente/UI), OFL | Sci-Fi-Look ohne Lizenzrisiko |
| 31.10. | max + martin | Musik/SFX aus freien Assets; Voicelines selbst aufnehmen + ElevenLabs, an einem Tag nach dem Feature-Freeze | Spart Zeit; Texte stehen dann fest |
| 31.10. | max + martin | **Zentrales Entwicklungskonzept: Pakete** mit festem Umfang (`docs/team/PAKETE.md`); wer Zeit hat, nimmt ein offenes Paket; Überarbeitungen als Folgepakete | Klare Abgrenzung statt fester Zuständigkeiten, flexibel bei unbekannter Zeit pro Woche |
| 31.10. | max + martin | Benötigte Assets in `docs/ASSET-REQUESTS.md` (Dateiname, Platzhalter, finales Aussehen); Platzhalter unter finalem Pfad, Max ersetzt nur den Dateiinhalt | Max' Asset-Pipeline arbeitet die Liste ab; keine Szenen-/Prefab-Änderungen beim Austausch |
| 31.10. | max | Asset-Pipeline-Format: `ASSET-REQUESTS.md` mit YAML-Block pro Asset, `STYLE.md` + Palette, Ablieferung per PR von `assets/<Paket>`-Branches, Texel-Dichte 64/128 px/m, max. 2 Material-Slots pro Prop | Abgestimmt mit der Pipeline; PRs halten `main` sauber, wenige Materialien schonen Draw Calls im Browser |
| 31.10. | max + martin | Art-Style-Richtung: Grafik wie Mouthwashing, Asset-Design wie Alien: Isolation; final nach der Theme-Bekanntgabe | Gefällt beiden; Low-Poly/Low-Res ist solo schaffbar und browserfreundlich |
| 31.10. | max | Asset-PRs gibt immer Max frei und merged; beide dürfen Anpassungen per `feedback` + `status: rework` anfordern; Max integriert nach dem Merge in Unity | Einheitlicher Look; nur einer erzeugt neue `.meta`-Dateien → keine GUID-Konflikte |
| 02.11. | max | Stations-Grundriss freigegeben (`docs/STATION.md`): zentrale Halle, Schalter mit Prüfkammer als Luftschleuse, Spieler wohnt im Motelzimmer 103, Gadgets am Kiosk, Diner als eigener Raum (Soll), kein sichtbarer Vorgesetzter (Durchsagen/Memos), 3 gezielte Fenster + Bullauge, 2-m-Modulraster | Kurze Laufwege, Luftschleusen-Knopf hat einen sichtbaren Ort, kleiner Modul-Bausatz deckt die ganze Station ab |
| 02.11. | max | Retro-Look (640×360) vorerst aus, volle Auflösung; wie der Retro-Look final umgesetzt wird, entscheiden wir, wenn die Assets drin sind | Low-Res-Variante gefiel noch nicht; Look besser mit finalen Assets beurteilen |
| 02.11. | max | Station 2.0: Hangar 36 × 28 × 16 m mit sichtbarer Energiebarriere, halbrunde Aussichtsplattform statt Panoramafenster, Barfenster Nord, Außenwelt (Station von außen, Sonne, Planeten), vorbeifliegende Schiffe, Shuttle-Ankunft durch die Barriere, Sonne steht jeden Tag anders (Theme "Shift"); Pakete P22–P26 | Pipeline liefert schnell → Atmosphäre und Look als Stärke ausbauen; Ankunft und wandernde Sonne verbinden Optik mit Gameplay und Theme |
| 02.11. | max | Asset-Funktionen festgelegt: Türen automatisch mit Zugangsstufen; Bänke hinsetzen + Zeit vorspulen; Getränkeautomat kaufen mit Wirkung; Anzeigesäule Live-Ankunftsplan; Asservatenschrank mit echten Gegenständen; Pinnwand Tagesbulletin + Fahndungen; Lautsprecher = Vorgesetzter, Warnlampe = Alarm; Durchreiche animiert; Terminal-Datenbank; Münzfernrohr mit Familien-Story; Bullauge Abendblick; Barhocker → Barkeeper; Shuttle mit Rampe; durchsuchbare Kisten; Flackern/Stromausfälle an Tag 3; Familienschicksal sichtbar (Stadtlichter, Foto, Brief); Hangartor entfällt; Schleusentor klappt nach außen (Pakete P27–P36) | Jedes Asset bekommt einen Zweck; die Familie und der Zerfall der Station werden in der Welt sichtbar statt nur in Texten |
