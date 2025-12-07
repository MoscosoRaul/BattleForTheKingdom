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

