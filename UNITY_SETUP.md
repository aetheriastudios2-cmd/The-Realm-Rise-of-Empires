# The Realm – Settlers 3D Clone: Unity Setup Guide

## Requirements
- Unity **2022.3 LTS** (or newer 2022.3.x patch)
- Universal Render Pipeline (URP) recommended
- AI Navigation package (included in manifest.json)
- TextMeshPro (included in manifest.json)

---

## Opening the Project
1. Open **Unity Hub**
2. Click **Add project from disk** and select this repository root
3. Unity will import all packages from `Packages/manifest.json` automatically

---

## Scene Setup (first time only)

### 1. Create the Main Scene hierarchy

```
Scene root
├── _Managers
│   ├── GameManager          [GameManager, ResourceManager components]
│   ├── BuildingManager      [BuildingManager component]
│   ├── SettlerManager       [SettlerManager component]
│   ├── TerritoryManager     [TerritoryManager component]
│   ├── RoadManager          [RoadManager component]
│   └── GameBootstrap        [GameBootstrap component]
├── _Map
│   └── HexGrid              [HexGrid component]  ← also add HexMesh child
├── RTSCamera                [RTSCameraController on parent; Camera child at y=40, looking down ~60°]
├── Directional Light        [pre-created in scene file]
└── UI
    ├── Canvas
    │   ├── HUDPanel
    │   │   ├── ResourceBar  [ResourceBarController]
    │   │   └── TimeDisplay
    │   ├── BuildMenuPanel   [BuildMenuController]
    │   ├── BuildingInfoPanel
    │   └── PauseMenuPanel
    └── EventSystem
```

### 2. Create Building Prefabs

For each building type create a prefab with:
- A mesh (primitive or model) for the building body
- `Building` component
- `ProductionBuilding` component (if it produces)
- `StorageBuilding` component (if isStorage=true)
- `MilitaryBuilding` component (if isMilitary=true)
- `BuildingClickHandler` component
- Collider (Box/Mesh) so mouse clicks register

**Minimal placeholder prefab:**
```
BuildingRoot (Building + ProductionBuilding + BuildingClickHandler + BoxCollider)
  └── Visual (Cube mesh, scaled to taste)
```

### 3. Create ScriptableObject Assets

Follow `Assets/Resources/BuildingCatalogue/README.txt` to create:
- One **BuildingData** asset per building type
- One **ProductionRecipe** asset per recipe

Assign prefabs and recipes in the BuildingData inspector.

### 4. Wire up the NavMesh
1. Add a **NavMesh Surface** component to the `_Map` object
2. Set Agent Type → Humanoid
3. Click **Bake** after generating the terrain

### 5. Assign references in Inspector
- `HexGrid` → assign `cellPrefab` and `hexMeshPrefab`
- `BuildingManager` → assign `HexGrid` ref and populate `buildingCatalogue`
- `SettlerManager` → assign carrier/worker/soldier prefabs
- `GameBootstrap` → assign `HexGrid`, `BuildingManager`, `headquartersData`
- `UIManager` → assign all panel references

---

## Controls

| Action           | Input                   |
|------------------|-------------------------|
| Pan camera       | W / A / S / D or edge scroll |
| Rotate camera    | Q / E                   |
| Zoom             | Mouse wheel             |
| Drag pan         | Middle mouse button     |
| Open build menu  | **B**                   |
| Place building   | Left click on hex       |
| Cancel placement | Right click / Escape    |
| Pause            | Escape                  |

---

## Production Chains

```
Forest tile → Woodcutter → Log
Log         → Sawmill    → Plank
Mountain    → Quarry     → Stone
Mountain    → Coal Mine  → Coal
Mountain    → Iron Mine  → IronOre
IronOre + Coal → Smelter → Iron
Iron + Plank   → Toolsmith → Tools
Iron + Coal    → Armory    → Sword

Grassland   → Farm       → Grain
Grain       → Mill       → Flour
Flour       → Bakery     → Bread

Water tile  → Fishing Hut → Fish
```

---

## Architecture Overview

| System             | File(s)                                                |
|--------------------|--------------------------------------------------------|
| Hex grid map       | `Map/HexGrid`, `HexCell`, `HexCoordinates`, `HexMesh` |
| Terrain generation | `Map/TerrainGenerator`                                 |
| Road/flag system   | `Map/RoadManager`                                      |
| Building placement | `Buildings/BuildingManager`, `Building`                |
| Production loop    | `Buildings/ProductionBuilding`, `Economy/ProductionRecipe` |
| Storage            | `Buildings/StorageBuilding`, `Core/ResourceManager`    |
| Territory          | `Buildings/MilitaryBuilding`, `Economy/TerritoryManager` |
| Settler AI         | `Settlers/SettlerCarrier`, `SettlerWorker`, `SettlerSoldier`, `SettlerManager` |
| Pathfinding        | `Pathfinding/AStarPathfinder`                          |
| Camera             | `Camera/RTSCameraController`                           |
| UI                 | `UI/UIManager`, `BuildMenuController`, `ResourceBarController` |
| Game state         | `Core/GameManager`                                     |
