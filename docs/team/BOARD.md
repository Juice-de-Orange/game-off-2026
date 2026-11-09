# Team-Board

> Pull vor jeder Änderung. Kleine Änderungen, sofort pushen — besonders bei Claims.

## Meilensteine
Rhythmus: 3 Wochen Entwicklung, 1 Woche Polish, 2 Tage Puffer.

| Datum | Ziel | Status |
|---|---|---|
| 01.11. | Theme kommt ca. **22:37 MEZ** → jeder für sich Gedanken, Repo öffentlich | ✅ Theme "Shift" · Repo noch privat (Call) |
| 02.11. | Abend-Call: Theme ins Konzept einweben, `DESIGN.md` finalisieren, Unity-Projekt anlegen | 🔶 Projekt + Station 2.0 stehen; Call offen |
| 03.11. | Prototyp: Kern-Mechanik spielbar (graue Klötze reichen) | ✅ 04.11.: Schicht am Schalter spielbar (Dokumente, Stempel, Regeln, Verwarnzettel); Geld/Tage fehlen noch (P08–P10) |
| 05.11. | Erster WebGL-Testbuild auf itch.io (Draft) — danach jede Woche ein Build | 🔶 Build läuft lokal (18 MB); 05.11. martin: Build mit Menü (20 MB) in Chrome getestet ✔; Upload P19 offen · Live seit 05.11. abends: https://kestrel9.maxlworks.com (P49); itch-Entwurf (P19) noch offen |
| 09.11. | Checkpoint: Trägt der Kern-Loop? Wenn nicht, jetzt umsteuern | ⏳ |
| 16.11. | Vertical Slice: ein Level komplett mit Ziel, Sieg/Niederlage, Menü, Neustart | ⏳ |
| 22.11. | **Feature-Freeze**: keine neuen Features mehr, nur Inhalte & Polish | ⏳ |
| 28.11. | **Content-Freeze**: nur noch Bugfixes, Audio, Juice | ⏳ |
| 29.–30.11. | Puffer: itch-Seite, finaler Build, Browser-Tests | ⏳ |
| 30.11. | **Abgabe** auf itch.io (offizielle Deadline 01.12., 22:37 MEZ) | ⏳ |

### Stand 02.11. abends (max)
- Fertig: P03, P04, P05, P17 (aus), P20, P21, P22, P23, P24, P27 · P01 wartet auf Martin · 9 Asset-PRs gemergt und eingebaut
- Neu angelegt: P22–P36 (Station 2.0 + Mechaniken aus Max' Asset-Durchgang) — **im Call verteilen**
- Pipeline: ~90 Aufträge offen (M003, M006, M007); Builder setzt alles automatisch ein, sobald es geliefert ist
- Nächste Gameplay-Schritte für den Kern-Loop: P06 Dokumente → P07 Regeln → P08 Entscheidungen → P09 Tagesablauf → P10 Geld/Familie

### Stand 04.11. abends (max)
- Fertig heute: P37 Station-Fixes innen, P38 Schalter (Sessel, Aufstehen, Stempel), P39 Außenstation 2.0, **P06 Dokumente**, **P07 Reisende & Regeln**
- Spielregeln festgelegt (DESIGN.md "Regeln am Schalter", DECISIONS 04.11.), inkl. Pflicht-Luftschleuse und **The Shifter**
- 41 EditMode-Tests + `GameOff/Validate Travelers` sichern die Regeln und alle Inhalte ab
- Pipeline: 22 neue Aufträge (Außenhülle M015, Dokument-Hintergründe, Verwarnzettel, Drucker, Stempel-Abdrücke) → P42
- Als Nächstes: P41 → P40 → P08 → P09 → P10 → P15 (Reihenfolge in PAKETE.md)

### Stand 05.11. mittags (martin)
- Fertig heute: P01 (abgeschlossen), **P02 Startbildschirm & Menü** (Station im All als Hintergrund, Pause), **P43 Optionen in Reitern** + Vollbild-Knopf, **P18 Audio-Grundgerüst**
- `Main.unity` hat jetzt `PF_PauseMenu` + `PF_AudioManager` (generiert von GameOff → Build Menu Scenes)
- WebGL-Build mit Menü (20 MB) in Chrome getestet, 41/41 Tests grün
- Als Nächstes (martin): P19 itch-Upload mit Web-Vorlage „Minimal“

### Stand 05.11. nachmittags (martin)
- Fertig: **P46 Geld, Inventar & Handelsfenster** (Grundgerüst für alle Items/Geld – Anleitung in PAKETE.md → P46 → Ergebnis), Händler-Platzhalter, Drohne
- Gebaut, Abnahme offen: **P09 Tagesablauf** (wartet auf abfertigbare Reisende)
- Kern-Loop-Abfrage an max für den gemeinsamen Call; P08/P10/P11/P13 erst nach P46; 54/54 Tests grün

### Stand 05.11. abends (max)
- Fertig: **P41** Pinnwand + Durchsagen, **P40** Reisende in der Kammer, **P42** Lieferungen eingebaut (Stationshülle, Papiere, Drucker, Stempel), **P54** Orbitron am Schalter; von martin **P47, P55, P56** (Schiffsverkehr, Dev-Werkzeug)
- **Live-Version:** https://kestrel9.maxlworks.com (`docs/DEPLOY.md`). Automatische Updates warten noch auf Unity-Lizenz + Secrets (P49), bis dahin spielt max von Hand ein
- Bestellt: Holly, Barkeeper, Dealer (P52). Neue Ideen als Pakete: P50 Türschilder, P51 Zimmer 103, P53 Wohnmodul begehbar. Assets bestellen geht jetzt ohne Rückfrage (`docs/ASSETS.md`)
- 71/71 EditMode-Tests grün, WebGL-Build 20 MB
- **Als Nächstes:** Kern-Loop-Call (vor dem Checkpoint 09.11.), martin nimmt P09 ab, dann P08 → P10 → P15 → P12 (`PAKETE.md`)
  - Nachtrag 06.11. (martin): Kern-Loop-Call hat stattgefunden. P19 itch-Upload ist auf später verschoben. P58 Punkt 2 entschieden → P64

### Stand 08.11. abends (max)
- Fertig: **P15** alle 12 Story-Tage, **P86** Korr-Tutorial, **P89** Papiere nebeneinander, **P91** Reisende gehen + Gitter, P88, P92, P93, P96; P90 bis auf #21 und #17
- Live: 4f65648 nach dem Nachtlauf; GitHub-Issues offen: #16 (wartet auf Laute), #17 (Max), #25 (vorerst nicht). Bericht: `docs/team/reports/2026-11-09_nachtlauf_max.md`
- Martin im Urlaub bis Dienstag, sein Claude übernimmt P98, P97, P85, P77, P94 (Frage liegt bei ihm)
- Nach Max' erstem Anspielen neu: P99–P102 (Hocker, Reaktionen, Leute in der Station, Papier-Geräusche), P103–P107 (Essen andeuten, Wachmann, Schleuse mit Ragdoll, Shuttle angedockt, Hangar-Schiffe), P29 geschärft; Assets dafür bestellt (M023)
- Feature-Freeze 22.11. wird eng: Muss-Pakete P12, P16, P69 nicht aus den Augen verlieren
- **Als Nächstes:** Reihenfolge festlegen, Kiosk-Gitter wartet auf die überarbeitete Theke

### Stand 09.11. (max)
- **Nachtlauf (Claude für max, 08./09.11.):** P69, P12, P16, P62, P11, P99, P63, P77, P80, P83, P50, P95, P97, P98, P100–P105, P107, P108 und das Kiosk-Gitter fertig; P19 vorbereitet. Bericht: `docs/team/reports/2026-11-09_nachtlauf_max.md`
- **Martin, Nacht auf 09.11.:** P85 ganz, P87, Welle 1 (P28, P29-Rest, P32, P33-Rest, P34), Welle 2 (P109 Sperrstunde, P110 Zoll, P111 Fernrohr-Diorama). Nur mit Tests geprüft, noch nicht angespielt
- **Live:** `f7a90ca` mit P85-Rest, P87, Welle 1 und 2 und der Fernrohr-Straße (09.11.). Im Browser nur Tag 1 geprüft, Rest wartet aufs Anspielen
- **Fragen an max offen:** Terminal und Moral, Welle 1 anspielen, Welle 2 anspielen (`docs/team/questions/2026-11-09_martin-an-max_*`)
- **Lieferungen noch nicht auf `main`:** `assets/P106-au-en-hangar` (Shuttle, Paket noch offen). Die Straße fürs Fernrohr (P111) ist eingebaut.
- GitHub-Issues offen: #16 (wartet auf die Laute, P94), #17 (Max prüft das Hangar-Flackern), #25 (vorerst nicht)
- **Als Nächstes:** Max spielt an und beantwortet die Fragen; danach Live-Deploy. Noch offen: P94 (wartet auf Laute), P106, P48, P51, P26-Rest, P25, P35

### Stand 09.11. morgens (martin)
- Fertig mit Agents in zwei Wellen: **P34** Terminal, **P28** + P29-Rest, **P32** + P33-Rest, **P109** Sperrstunde, **P110** Zoll bei der Drohne, **P111** Fernrohr-Diorama
- 249/249 EditMode-Tests, Validate Travelers 0 – **noch nicht angespielt** (Testlisten `welle1-testen`, `welle2-testen` bei Max)
- Neu aus Max' Antwort: **P112** Terminal nur Pass, **P113** Moral spürbar (beide offen)
- **Als Nächstes:** Anspielen (Max), dann P112/P113; Hangar-Welle nach Entscheidung zu P106

### Stand 09.11. vormittags (max)
- **P112, P113 und Status-Display-Fix** fertig, **live: `602d9dc`**. Im Browser gesehen: Statusanzeige lesbar, Terminal mit nur zwei Knöpfen, Abfrage, sechs Schleusen, Gedanke am Bullauge. Nicht im Browser gesehen: Bank, Abendgäste, Albtraum-Zeile am Morgen (nur durch Tests und Code-Prüfung gedeckt)
- **P106, P14-Rest, P35, P26-Rest** fertig und **live: `a2735fa`** (09.11. nachmittags). Im Browser gesehen: Schott, Kabine, Abflug, Morgen-Gruppe, Schalter. Nicht im Browser gesehen: Kiste und Zettel (erst ab Tag 3), Bank, Abendgäste, Albtraum-Zeile. Offen bleiben: Anspielen von Welle 1, 2 und der Hangar-Welle (Max), P94 (wartet auf Laute), P51 (Bullauge bestellt), P48 (Polish-Woche), #17 prüfen

## Aufgabenteilung
- **max**: alle finalen Assets (Art, Sound, Musik).
- **martin**: Startbildschirm — alles vom Spielstart bis zum Menü. Baut mit Platzhalter-Assets, Max ersetzt sie.
- Abstimmung: immer wieder unter der Woche, am Wochenende intensiver. Verfügbare Zeit pro Woche ist offen und ergibt sich unterwegs.

## In Arbeit
Wer etwas bearbeitet, trägt es hier ein (`/claim`). Geclaimte Szenen/Prefabs/Assets fasst der andere nicht an.
Welche **Pakete** gerade wer baut, steht in `PAKETE.md`.

| Wer | Aufgabe | Geclaimte Dateien | Seit |
|---|---|---|---|
| martin | P11 Rest (Schwarzmarkt, Beschlagnahme): P08-Gespräch war im Call 07.11., kommt mit P81/P82/P08 | — | 07.11. |

## Backlog
Die Arbeit läuft über **Pakete** in `PAKETE.md` (Entwurf, wird am 02.11. freigegeben). Diese Liste nur für Ideen,
die noch kein Paket sind. Bei Zeitnot wird von unten gestrichen.

### Muss (ohne das kein Spiel)
- [ ] **Vor der Abgabe:** `showDevTools` am DayManager (`PF_DayFlow` in Main) ausschalten und den Reiter DEV im `MenuSceneBuilder` entfernen (P49) – sonst rote Hitboxen und Dev-Knöpfe im Spiel

### Soll (macht es gut)
- [ ] Durchsagen einsprechen (P41 spielt bisher einen Platzhalter-Ton; Clips in `Data/Counter/Announcements.asset`, Texte dort). Die Pipeline macht keine Voicelines, ElevenLabs-Lizenz noch offen
- [ ] 

### Kann (wenn Zeit bleibt)
- [ ] Idee max, jetzt **P63**: **Clownsschuhe** als Beschlagnahme-Gegenstand (Inventar, P08/P46). Zieht man sie an, quietscht jeder Schritt beim Gehen, bis man sie auszieht. Braucht: Anziehen/Ausziehen, Schrittgeräusch `SFX_`
- [ ] 

## Bugs
Gefundene Fehler, die nicht sofort behoben werden. Wer einen behebt: Status `behoben (<name>, <Datum>, <Commit>)`, Zeile bleibt stehen.
Wer einen anfängt, trägt sich unter „Wer“ ein.

| Nr | Was passiert | Wie nachstellen | Gefunden | Wer | Status |
|---|---|---|---|---|---|
| B01 | Spieler fällt beim Laden von Main **sofort durch den Boden** der Station | Unity: Play neu gestartet, über das Menü ins Spiel (Start Shift). Tritt nicht jedes Mal auf: Claude konnte es am 05.11. nicht nachstellen (Menü → Main dreimal, Play aus dem Menü mit Messung pro Frame: Spieler steht stabil bei y = 0,03). Unklar, ob es beim Tageswechsel (Teleport ins Bett-Zimmer, P09) auch passieren kann. Ideen: Spawn genau auf Bodenhöhe (`PlayerSpawn` y = 0, Kapsel-Unterkante = Boden), erster Frame nach dem Laden mit großem `deltaTime`. Möglicher Schutz: etwas über dem Boden spawnen + Auffangen unter dem Boden (zurück zur letzten sicheren Position) | martin, 05.11. | max | abgesichert, **nicht bestätigt** (max, 06.11., P57): nie nachgestellt. Fallschritt gedeckelt, Spawn 5 cm über dem Boden, Rettung auf den letzten sicheren Boden. Tritt es nochmal auf, bitte melden |
| B06 | In der **Abrechnung** überlappt die Zeile „Penalty · 1 citations beyond 2 warnings“ den Betrag -10 CR; außerdem „1 citations“ | Tag mit mehr als 2 Fehlern beenden, dann ins Bett | max, 06.11. (Durchlauf P60) | martin | behoben (martin, 07.11.): Zeile heißt jetzt „Penalty · 1 extra citation“ (Einzahl/Mehrzahl), passt links neben den Betrag |
| B05 | **Verwarnzettel bleiben am nächsten Tag auf dem Tisch liegen** (nach einem Dev-Sprung auch die Papiere des letzten Reisenden) | Falsch entscheiden, dann neuer Tag | max, 05.11. (P41) | max | behoben (max, 06.11., P57): Tisch wird jeden Morgen geleert, im WebGL-Build bestätigt |
| B04 | Ware beim Händler **sinkt beim Kauf nicht** (man kann z. B. beliebig viele Painkillers kaufen) | Händler öffnen, denselben Gegenstand mehrfach kaufen | martin, 05.11. | martin | behoben (martin, 06.11., P11): begrenzter Vorrat pro Tag, „Sold out“, morgens aufgefüllt |
| B03 | Nach dem Schließen von Fenstern (Brief, Handel, BACK) musste man erst klicken, bevor die Kamera sich dreht | WebGL, Fenster mit Klick schließen | martin, 05.11. | martin | behoben (martin, 05.11.): Maus wird beim Zurückgeben der Steuerung sofort gefangen |
| B02 | Beim **steilen Blick nach unten** (z. B. aufs Bett) erscheint kein „[E]“-Hinweis | Vor ein Interactable unterhalb der Augenhöhe stellen und draufschauen. Ursache: der Interaktions-Strahl traf den eigenen Spieler (`PF_Player`) zuerst | martin, 05.11. | martin | behoben (martin, 05.11.): `PlayerInteractor` überspringt Treffer auf den eigenen Spieler |

## Erledigt
- [x] P91 Reisende gehen durch die Türen, Gitter bei Verhaftung und Schleuse (max, 08.11.)
- [x] P15 Inhalte: alle 12 Story-Tage mit normalen Reisenden, Regeln und Briefen (max, 08.11.; Anspielen steht aus)
- [x] P92 Diner außer Betrieb, P93 Barkeeper sagt seinen Wunsch, P96 Händler atmen (max, 08.11.)
- [x] P86 Korr-Tutorial an Tag 1, Korr steht fest vorn (max, 08.11.)
- [x] P89 Schaltertisch: Papiere nebeneinander lesen, Lade und Knöpfe richtig herum, Beschriftung ARREST/AIRLOCK (max, 08.11.)
- [x] P88 Wegweiser eingebaut; Lieferungen Ticketautomat, Triebwerkssound, 12 Waren-Icons gemergt und eingebaut (max, 08.11.)
- [x] P72 Ticket nach Hause: Ticketautomat im Hangar, selbst gesteuerter Abflug, Enden HOME / HOME, BARELY / TOO LATE? (max, 07.11.)
- [x] P84 Bugreport im Spiel: Formular im Pausenmenü, Backend auf Geoffrey legt GitHub-Issues an (max, 07.11.; Token setzt Max)
- [x] P78 Bugreport-Knopf im Pausenmenü: vorausgefülltes GitHub-Issue mit Commit, Tag, Phase, Browser (max, 07.11.)
- [x] P79 DeliveryIntegrator verstellt Waren-Icons und Dokument-Texturen nicht mehr (max, 07.11.)
- [x] P09 Tagesablauf: drei Tage mit Kontroll-Gameplay, von max abgenommen (martin, 07.11.)
- [x] P61 Ton am Schalter und in der Station: nach Gehör abgenommen (max, 07.11.)
- [x] P36 Flackern nach Stationslage: Krisen-Ereignisse → Lampen flackern öfter, Neon setzt aus, ab Lage 0,5 Stromausfälle; Tag 1 ruhig, Tag 3 instabil (martin, 07.11.)
- [x] P31 Anzeigesäule: Tag, Phase, nächstes Shuttle und Marktereignis auf den CRTs der Säule in der Halle (martin, 07.11.)
- [x] P30 Getränkeautomat: [E] kauft Nova Cola, 6 pro Tag, nur abends (martin, 07.11.)
- [x] P66 Bestechungszettel: Zettel an Tag 3 (Quill, 60 CR), Umschlag in der Abrechnung, Ablehnen → Barkeeper zwei Abende ohne Gerücht; Balance-Ziel „korrupt nicht vor Tag 6“; B04 geschlossen (martin, 07.11.)
- [x] P71 Balancing: Simulator + Economy Report + Balance-Test, Ticket 350 CR (~7–8 Tage gute Arbeit), Ereignisse härter, Gerüchte gedämpft, Rundung ,5 auf; P72/P73 als Entwurf, Frage an Max (martin, 07.11.)
- [x] P65 Barkeeper: Schild mit Wunsch nach Label, GIVE gegen ein Gerücht über die Marktereignisse von morgen, Notiz im Inventar; Modell SM_Bar_WishBoard bestellt (martin, 07.11.)
- [x] P68 Markt: Label-Faktoren, feste Ereignisse Tag 2/3 (Preis, Menge, Ankauf, Sortiment), MARKET-Zettel, Durchsage, Verderb über Nacht (martin, 07.11.)
- [x] P53 Wohnmodul begehbar, NavMesh jetzt wirklich gebacken, P26 Deko und P51 Hundebild nachgebessert (max, 06.11.)
- [x] P61 Ton am Schalter und in der Station gebaut, Abnahme nach Gehör steht aus (`Data/Audio/SoundSet.asset`) (max, 06.11.)
- [x] P60 Letzte Platzhalter abgelöst: Wände, Leuchten, Türlampen, Rumpfhaut, Passfotos (max, 06.11.)
- [x] P59 Schleusenklappe schließt nach dem Auslösen (max, 06.11.)
- [x] P58 Entscheidungen aus dem Nachtlauf getroffen (max, 06.11.)
- [x] P57 Nachtlauf: Bugs behoben, Ruckler belegt und reduziert, Messwerkzeuge (max, 06.11.)
- [x] P70 Preisliste: 18 Waren mit Grundpreis und fester Tagesschwankung, Lohn nur für richtige Entscheidungen, Händler und Bett vom Marktpreis, Label LUXURY (martin, 06.11.)
- [x] P13 Drohne deckt Bedarfe der Familie, Geld am Bett = Händlerwert × 1,35, Fusion Cell/POWER, Handel nur nach der Schicht; Abrechnung mit RETURN, nichts vorausgewählt; Game Over bei unbezahlter Miete; `docs/MECHANICS.md`; P69 Entwurf (martin, 06.11.)
- [x] P10 Abrechnung am Abend: Lohn pro Reisendem, Miete, Essen/Heizung/Medizin, Familie Mom/Lena/Pip mit Zustand im Morgenbrief; P11 Händlervorrat (B04); Dev-Knöpfe ±100 CR (martin, 06.11.)
- [x] P67 Stations-Atmo in Main hörbar (`SFX_Amb_Station_Loop`, Freesound/Jovica CC BY), `GameOff → Build Ambience into Main`; weiter in P61 (martin, 06.11.) Später ersetzt durch den synthetischen Loop aus P61 (Max, 06.11.).
- [x] P64 Schwarzmarkt kauft nur Ware mit `CONTRABAND`/`CONFISCATED`, Kiosk und Bar lehnen sie ab; gemeinsame Label-Liste (11 Labels), Menü `GameOff → Update Economy Data`, 73 Tests (martin, 06.11.)
- [x] Alle Texte in Orbitron (Menü, HUD, Fenster, Brief; TMP-Standard), [E]-Hinweis im Menüknopf-Stil (`GameOff → Apply HUD Style`); Schalter-HUD und Dokumente → Notiz an max (martin, 05.11.)
- [x] Orbitron für Spieltitel und Menü-Überschriften (Credits + Lizenz), Unity-Splash abgeschaltet – Antwort von max umgesetzt (martin, 05.11.)
- [x] P47 Menü-Hintergrund: Schiffsverkehr (Frachter, Tanker, Patrouille, Pendler) mit wiederverwendbarem `ShipTraffic` für P25 (martin, 05.11.)
- [x] P46 Geld, Inventar & Handelsfenster: Kontostand, Inventar (I), Handelsfenster, Händler-Platzhalter Holly/Barkeeper/Dealer, Drohne „Send home“, 13 Tests (martin, 05.11.)
- [x] P18 Audio-Grundgerüst: GameMixer (Music/SFX/Voice), AudioManager mit PlayMusic/PlaySfx/PlaySfxAt/PlayVoice, 4 Lautstärkeregler mit Vorschau-Ton (martin, 05.11.)
- [x] P43 Optionen in Reitern (Audio/Controls), Invert Y, Tastenübersicht, Reset; Vollbild-Knopf oben rechts in Haupt- und Pausemenü (martin, 05.11.)
- [x] P02 Startbildschirm & Menü: Boot-Splash, Hauptmenü mit Station im All als Hintergrund, Optionen, Credits, Pausemenü; WebGL getestet (martin, 05.11.)
- [x] P01 abgeschlossen: Martin hat das Projekt geöffnet und einen WebGL-Build getestet (martin, 05.11.)
- [x] P07 Reisende & Regeln: Regel-Engine, Bewertung, Validator + 41 Tests, Schicht-Ablauf, Verwarnzettel, Test-Tag (max, 04.11.)
- [x] P56 Dev-Werkzeug Verkehrslinien: Linien + Panel, SAVE LANE, Paste Traffic Lanes (martin, 05.11.)
- [x] P55 Schiffsverkehr vor Plattform- und Barfenster, nebelfreie Schiffe (martin, 05.11.)
- [x] P06 Dokumente als 3D-Papiere: Schublade, ziehen, lesen, stempeln, zurückgeben; scharfe Ebene für den Retro-Look (max, 04.11.)
- [x] P39 Außenstation 2.0: verbundenes Fachwerk, Habs, Solar, Antenne, Außenhülle (Platzhalter bis zur Lieferung) (max, 04.11.)
- [x] P38 Schalter: Sessel anwählbar, Aufstehen mit Q/Esc/Rechtsklick, Stempel hochheben und stempeln (max, 04.11.)
- [x] P37 Station-Fixes innen: Türen, Wand-Props, Kiosk-Front, Hangar-Fassade, alle Platzhalter ersetzt (max, 04.11.)
- [x] Lieferungen M013/M014 gemergt (PR #10–#13) und eingebaut; 32 alte Asset-Branches gelöscht, Auto-Delete an (max, 04.11.)
- [x] Stations-Grundriss freigegeben (`docs/STATION.md`), 48 Asset-Anforderungen für die Pipeline (max, 02.11.)
- [x] P01 Unity-Projekt angelegt (WebGL-Testbuild steht noch aus) (max, 02.11.)
- [x] P04 Station als Graybox in `Main.unity` (max, 02.11.)
- [x] P03 Spieler-Controller + Interaktion, Spieler in `Main.unity` (max, 02.11.)
- [x] P05 Schalteransicht mit Stempeln und Knöpfen (max, 02.11.)
- [x] P17 Retro-Look 640×360, UI scharf — vorerst AUS, Entscheidung nach Asset-Integration (max, 02.11.)
- [x] P27 Automatische Türen mit Zugangsstufen (max, 02.11.)
- [x] Asset-Durchgang: Funktionen festgelegt → Pakete P27–P36 (max, 02.11.)
- [x] Lieferungen Bar/Halle/Hangar/Bausatz-b/Schalter-b gemergt (PR #4–#8), integriert und eingebaut (max, 02.11.)
- [x] P24 Himmel & Außenwelt: Sonne, Planeten, Ring, Mond, Außenmodule, SetDay (max, 02.11.)
- [x] P23 Energiebarriere am Hangartor (Shader, Puls) (max, 02.11.)
- [x] P22 Grundriss 2.0: Hangar 36×28×16, Aussichtsplattform, Barfenster (max, 02.11.)
- [x] P21 Station aus Bausatz-Modulen (Wände, Böden, Decken, Schalter-Möbel, Bar) (max, 02.11.)
- [x] P20 Licht & Stimmung: Raumfarben, Leuchtstreifen, Nebel, Sternenhimmel, gebackenes Licht (max, 02.11.)
- [x] Repo, Team-Workflow und Setup-Anleitung (max, 29.10.)
- [x] Unity 6000.3.23f1 + WebGL bei Max installiert, Lizenz aktiv (max, 29.10.)
- [x] Martin (iMaterix) ins Repo eingeladen, Setup-Anleitung als Gist verschickt (max, 29.10.)
- [x] Timeline + Aufgabenteilung beschlossen (max + martin, 31.10.)
- [x] Konzept-Entwurf `docs/DESIGN.md`: Zollstation im All, 3 Tage, Englisch (max + martin, 31.10.)
- [x] Tech-Stack, Schriften, Luftschleuse/Moral, Art-Style-Richtung beschlossen (max + martin, 31.10.)
- [x] Paket-Konzept + erste 19 Pakete `docs/team/PAKETE.md` (max, 31.10.)
- [x] Asset-Pipeline angebunden: `ASSETS.md`, `ASSET-REQUESTS.md`, `STYLE.md` + Palette, Deploy-Key, PR-Ablauf (max, 31.10.)
- [x] Session-Briefing zeigt Asset-PRs und Asset-Status (max, 31.10.)

## Vor dem 01.11. noch offen
- [x] martin: Setup nach Anleitung durchlaufen, klonen (Einladung angenommen ✔ 31.10.)
- [x] martin: itch.io-Benutzernamen an Max schicken → `iMaterix`
- [x] beide: gemeinsamen Chat einrichten (Discord)
- [x] beide: Ablauf zum Start: Theme anschauen, Gedanken machen, Abend-Call, gemeinsam festlegen
- [x] max: itch.io-Projektseite als Entwurf angelegt: https://juice-de-orange.itch.io/game-off-2026 (HTML, 960×540, Vollbild-Button, keine Zahlungen, KI-Kennzeichnung an)
- [x] max: Martin (`iMaterix`) als Admin + Mitautor auf der itch-Seite eingetragen
- [x] martin: itch-Admin-Einladung angenommen (Projekt steht im Dashboard, 05.11.)
- [ ] martin: Unity-Grundlagen anschauen: uGUI-Menü (Canvas, Buttons), Szenenwechsel (`SceneManager.LoadScene`)
- [ ] max: Tools installieren/testen: Blender (FBX-Export nach `docs/ASSETS.md`), Texturtool, Audacity
- [x] max: Deploy-Key mit Schreibrecht für die Asset-Pipeline eintragen (31.10., Titel `asset-pipeline`)
- [ ] max: Ab 01.11. (Repo öffentlich): `main` per Ruleset schützen — nur max/martin pushen direkt, Pipeline nur per PR
- [ ] **Repo öffentlich machen (Pflicht bis zur Abgabe). Max und Martin haben das geklärt (max, 05.11.); die Umsetzung steht noch aus, den Termin legen Max und Martin im Kern-Loop-Call fest (max, 06.11.).** Vorher die urheberrechtlich geschützten Referenzbilder (`docs/style/refs/ai_*`, `mw_*`)
  aus der Git-Historie entfernen (einmaliger Force-Push nach Absprache, Martin klont danach neu), Bilder lokal/privat weiter nutzen.
  Auf GitHub ist es am 05.11. noch privat, die Bilder liegen noch in der Historie. Pflicht: öffentlich bis 01.12.
- [ ] beide, im Kern-Loop-Call: **Audio höher priorisieren** — Audio ist eine der 6 Bewertungskategorien (P18 + Atmo-Sounds als Muss?).
- [x] Lizenz: Code MIT (`LICENSE`), eigene Assets vorbehalten, KI-Offenlegung im README (max, 02.11.)
- [x] max: 35 Referenzbilder in `docs/style/refs/` (31.10.)
- [x] max: `docs/STYLE.md` + Palette prüfen, nach dem Theme freigeben → Pipeline baut 3 Muster-Assets
- [ ] max: ElevenLabs-Lizenz klären (Free-Plan erlaubt keine kommerzielle Nutzung, verlangt Namensnennung)
- [x] beide: Art-Style-Richtung: Mouthwashing-Grafik + Alien-Isolation-Assets (final am 02.11. nach dem Theme)
- [x] beide: Tech-Stack bestätigt (31.10., in `DECISIONS.md`)
- [ ] beide: `docs/DESIGN.md` (Entwurf) und `docs/ASSETS.md` lesen
