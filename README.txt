# Battle for the Kingdom — Final Submission

**Git tag:** `v1.0-leadas`  
**Release:** https://github.com/MoscosoRaul/BattleForTheKingdom/releases/tag/v1.0-leadas

Battle for the Kingdom egy **körökre osztott, kétjátékos stratégiai** játék. A játékosok egységeket mozgatnak, harcolnak, és foglalható mezőket (bánya/templom) szereznek meg. A győzelem fővár elfoglalásával (és a specifikáció szerint templomok birtoklásával) érhető el. :contentReference[oaicite:0]{index=0}

---

## Követelmények (környezet)

- **Unity verzió:** 6000.2.10f1
- **Platform:** Windows 64-bit
- **Build típusa:** Windows x86_64 (standalone)

---

## Futtatás (Windows)

1. Nyisd meg a release-t:  
   https://github.com/MoscosoRaul/BattleForTheKingdom/releases/tag/v1.0-leadas
2. Töltsd le a build `.zip`-et, és csomagold ki egy tetszőleges mappába.
3. Indítás: `BattleForTheKingdom.exe`

> Fontos: a futtatáshoz a `.exe` mellett kell maradnia a `BattleForTheKingdom_Data` mappának és a többi fájlnak (UnityPlayer.dll, stb.), tehát **ne csak az exe-t másold ki**.

---

## Irányítás / játékmenet

- **Egység kijelölése:** kattintás a saját egységre.
- **Mozgás:** kijelölt egységgel kattintás egy szabályos célmezőre (mozgáspont, terep, foglaltság figyelembevételével). :contentReference[oaicite:1]{index=1}
- **Támadás:** kijelölt egységgel kattintás elérhető ellenfélre → a rendszer lefuttatja a harcot. :contentReference[oaicite:2]{index=2}
- **Kör befejezése:** `End Turn` gomb → győzelemellenőrzés, majd a másik játékos következik. :contentReference[oaicite:3]{index=3}
- **Mentés:** `Save` gomb (a játék bármely kör után menthető). :contentReference[oaicite:4]{index=4}

### Terepszabály (implementált extra részlet)
- **Forest (erdő):** az íjász **nem tud távolról támadni** erdőben álló egységre; a lovagot az erdő **lassítja** (a gyakorlatban: erdőn “átmenni” egy körben nem tud, legfeljebb belépni).

---

## Toborzás

- A játékos erőforrást gyűjt (bányák birtoklásával), és ebből egységeket vásárolhat. :contentReference[oaicite:5]{index=5}
- Toborzás a várban (UI panelen a három egységtípus: Katona/Lovag/Íjász). :contentReference[oaicite:6]{index=6}

---

## Mentés / Betöltés állapota

- **Mentés:** implementálva (Save gomb + visszajelzés).
- **Betöltés:** **nem készült el** a leadott verzióban (kimaradt / elfelejtődött a végső integrálás során).

A specifikáció szerint a mentésnek és betöltésnek teljes körűnek kell lennie. :contentReference[oaicite:7]{index=7}

---

## Fordítás / Build (fejlesztői instrukció)

1. Unity Hub → **Open** → a projekt mappa megnyitása.
2. `File → Build Settings…`
3. Platform: **Windows, Mac, Linux → Windows**
4. Target: **x86_64**
5. `Build` → válassz célmappát → Unity elkészíti a futtatható buildet.

---

## Ismert hiányosságok

- **Játék betöltése (Load):** nem került implementálásra a leadott verzióban. :contentReference[oaicite:8]{index=8}
- **Beállítások menü:** nem került implementálásra (specben alacsony prioritású). :contentReference[oaicite:9]{index=9}


//SZKELETON LEADÁSAKOR KÉSZÜLT README//
# Battle for the Kingdom – Szkeleton fázis dokumentáció

Ez a projekt a **Battle for the Kingdom** játék szkeleton fázisának megvalósítását tartalmazza.  
A szkeleton célja, hogy igazolja: a projekt alapjai stabilak, futtathatók és a félév végére a teljes játék felépíthető.

A szkeleton az alábbi követelményeket teljesíti:

- A projekt fordítható
- A projekt futtatható
- Legalább 3 darab teszteset található benne
- A játék története / állapota specifikáció szerint tárolható és visszatölthető
- A projekt struktúrája alkalmas a további fejlesztésre

---

## Unity verzió

A fejlesztéshez használt verzió:

**Unity 6.2.0f1**

A projekt megnyitásához ajánlott ugyan ezt a verziót használni.

---

## A projekt futtatása

1. Nyisd meg a projektet Unity Hub-ból.
2. Nyisd meg a következő jelenetet:

Assets/Scenes/Game.unity
3. A Unity-ben nyomd meg a **Play** gombot.

A projekt Windows build formájában is futtatható (lásd alább).

---

## Tesztek

A projekt tartalmazza a szkeleton fázishoz szükséges legalább 3 darab tesztet.  
Ezek helye:

Assets/Tests/PlayMode/

A projektben megtalálható tesztek:

- `CombatTests.cs`
- `MapGeneratorTests.cs`
- `SaveLoadTests.cs`

A tesztek PlayMode-ban sikeresen futnak.

Megjegyzés: A játék build-elésekor szükség lehet arra, hogy a `Tests` mappát ideiglenesen `Tests~` névre nevezzük át, így a teszt assembly-k nem kerülnek bele a player buildbe.

---

## Játéktörténet / játékállás mentése

A projekt egy JSON alapú mentési rendszert használ.

A mentésért felelős kód:  

Assets/Scripts/Infra/SaveLoadManager.cs

Minta mentési (történet) fájl:  

Assets/StreamingAssets/sample_save.json

A játék futása közben létrejövő mentések helye:

bftk_save.json


Ez a fájl Unity `persistentDataPath` könyvtárában jön létre.

A funkciók megfelelnek a specifikációban előírt mentési követelményeknek.

---

## Build készítése

1. Menj a **File → Build Profiles…** menüpontra.
2. Válaszd ki a **Windows** platformot.
3. Ellenőrizd, hogy a jelenetek a következő sorrendben szerepelnek:

Scenes/Game (0. index)
Scenes/MainMenu (1. index)


4. Nyomd meg a **Build** gombot.
5. Válassz ki egy üres mappát (pl. `Build/`).

A Unity elkészíti az alábbi fájlokat:

- `BattleForTheKingdom.exe`
- `BattleForTheKingdom_Data/` könyvtár

A buildelt játék futtatható és betölti a szükséges rendszereket.

---

## Projektstruktúra

Assets/
├── Scenes/
│ ├── Game.unity
│ └── MainMenu.unity
│
├── Scripts/
│ ├── Domain/ (alap modellek: Map, Tile, Unit, Player stb.)
│ ├── Infra/ (mentés, szolgáltatások)
│ └── Presentation/ (UI, MapView)
│
├── StreamingAssets/
│ └── sample_save.json
│
└── Tests/
└── PlayMode/ (3 szükséges teszt)


A projekt rétegei elkülönülnek (Domain / Infra / Presentation), megfelelve az architekturális elvárásoknak.

---

## Szkeleton fázis összegzés

A projekt minden szükséges elemét tartalmazza:

- A projekt sikeresen buildelhető
- A projekt futtatható `.exe` formájában
- Legalább 3 PlayMode teszt megtalálható és lefut
- A történet / térkép / játékállás JSON formában menthető
- Minta mentési fájl elérhető
- A kódbázis előkészítve a játék további fejlesztésére
- A verzió GitHub-on taggelhető

---

## GitHub tagek

A szkeleton fázis lezárásához a következő tag javasolt:

git tag -a v0.1.0-skeleton -m "Skeleton fázis elkészült"
git push origin v0.1.0-skeleton

---

# A README vége

