using System;
using System.Collections.Generic;
using UnityEngine;
using SettlersClone.Economy;
using SettlersClone.Map;

namespace SettlersClone.Buildings
{
    public class BuildingManager : MonoBehaviour
    {
        public static BuildingManager Instance { get; private set; }

        [SerializeField] private HexGrid grid;
        [SerializeField] private List<BuildingData> buildingCatalogue = new();

        private readonly List<Building> allBuildings = new();
        private Building ghostBuilding;
        private BuildingData selectedData;
        private HexCell      hoveredCell;

        public event Action<Building> OnBuildingPlaced;
        public event Action<Building> OnBuildingRemoved;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        // --- Placement preview ---

        public void BeginPlacement(BuildingData data)
        {
            CancelPlacement();
            selectedData = data;
            if (data.prefab != null)
            {
                ghostBuilding = Instantiate(data.prefab).GetComponent<Building>();
                SetGhostAlpha(0.5f);
            }
        }

        public void CancelPlacement()
        {
            if (ghostBuilding != null) Destroy(ghostBuilding.gameObject);
            ghostBuilding = null;
            selectedData  = null;
        }

        private void Update()
        {
            if (selectedData == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                HexCell cell = grid.GetCellAt(hit.point);
                if (cell != hoveredCell)
                {
                    hoveredCell?.SetHighlight(false);
                    hoveredCell = cell;
                    cell?.SetHighlight(true);
                }
                if (ghostBuilding != null && cell != null)
                    ghostBuilding.transform.position = HexMetrics.Center(cell.Coordinates);
            }

            if (Input.GetMouseButtonDown(0) && hoveredCell != null)
                TryPlace(hoveredCell);

            if (Input.GetMouseButtonDown(1))
                CancelPlacement();
        }

        private void TryPlace(HexCell cell)
        {
            if (!cell.IsBuildable())
            {
                Debug.Log("Cannot build here.");
                return;
            }
            if (!Core.ResourceManager.Instance.CanAfford(selectedData.constructionCost))
            {
                Debug.Log("Not enough resources.");
                return;
            }
            Core.ResourceManager.Instance.Spend(selectedData.constructionCost);
            SpawnBuilding(selectedData, cell, 0);
            CancelPlacement();
        }

        public Building SpawnBuilding(BuildingData data, HexCell cell, int ownerId)
        {
            if (data.prefab == null) return null;
            var go = Instantiate(data.prefab, HexMetrics.Center(cell.Coordinates), Quaternion.identity);
            var building = go.GetComponent<Building>();
            building.Initialise(data, cell, ownerId);
            allBuildings.Add(building);
            building.OnDestroyed += b => { allBuildings.Remove(b); OnBuildingRemoved?.Invoke(b); };
            OnBuildingPlaced?.Invoke(building);
            return building;
        }

        public List<Building> GetBuildingsOfType(BuildingType type) =>
            allBuildings.FindAll(b => b.Type == type);

        public List<Building> GetAllBuildings() => allBuildings;

        public BuildingData GetData(BuildingType type) =>
            buildingCatalogue.Find(d => d.type == type);

        private void SetGhostAlpha(float alpha)
        {
            if (ghostBuilding == null) return;
            foreach (var r in ghostBuilding.GetComponentsInChildren<Renderer>())
            {
                var mat = r.material;
                var c = mat.color; c.a = alpha;
                mat.color = c;
            }
        }
    }
}
