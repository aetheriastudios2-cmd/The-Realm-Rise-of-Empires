using UnityEngine;
using SettlersClone.Buildings;
using SettlersClone.Map;

namespace SettlersClone.Core
{
    // Placed on a scene root object — wires up HQ spawn and initial game state
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private HexGrid        grid;
        [SerializeField] private BuildingManager buildingManager;
        [SerializeField] private BuildingData    headquartersData;
        [SerializeField] private int             mapSeed = 42;

        private void Start()
        {
            SpawnHeadquarters();
        }

        private void SpawnHeadquarters()
        {
            if (headquartersData == null || grid == null) return;

            // Place HQ near the centre of the map
            var center = HexCoordinates.FromOffsetCoordinates(grid.Width / 2, grid.Height / 2);
            HexCell cell = grid.GetCell(center);

            // Find a buildable cell near centre if the exact centre isn't suitable
            if (cell == null || !cell.IsBuildable())
            {
                for (int r = 1; r <= 5; r++)
                {
                    var candidates = grid.GetCellsInRadius(center, r);
                    foreach (var c in candidates)
                    {
                        if (c.IsBuildable()) { cell = c; break; }
                    }
                    if (cell != null && cell.IsBuildable()) break;
                }
            }

            if (cell == null || !cell.IsBuildable())
            {
                Debug.LogWarning("GameBootstrap: Could not find suitable HQ placement.");
                return;
            }

            buildingManager.SpawnBuilding(headquartersData, cell, 0);
            Debug.Log($"Headquarters placed at {cell.Coordinates}");
        }
    }
}
