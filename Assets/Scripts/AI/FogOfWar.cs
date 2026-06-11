using System.Collections.Generic;
using UnityEngine;
using SettlersClone.Map;

namespace SettlersClone.AI
{
    /// <summary>
    /// Fog-of-war: hides hex cells outside the player's visibility range.
    /// Cells within a building/settler's sight radius are revealed;
    /// cells already explored but currently out of range are shown as "shroud"
    /// (greyed out); unvisited cells are hidden entirely.
    /// </summary>
    public class FogOfWar : MonoBehaviour
    {
        public static FogOfWar Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private HexGrid grid;
        [SerializeField] private int     buildingSightRadius = 4;
        [SerializeField] private int     settlerSightRadius  = 2;
        [SerializeField] private Color   hiddenColour        = new(0.05f, 0.05f, 0.07f);
        [SerializeField] private Color   shroudColour        = new(0.25f, 0.25f, 0.30f, 0.7f);
        [SerializeField] private float   updateInterval      = 0.25f;

        private readonly HashSet<HexCell> visible  = new();
        private readonly HashSet<HexCell> explored = new();
        private float timer;

        public bool IsVisible(HexCell cell)  => visible.Contains(cell);
        public bool IsExplored(HexCell cell) => explored.Contains(cell);

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            // Hide all cells initially
            if (grid == null) return;
            foreach (var cell in grid.AllCells)
                ApplyFogState(cell, FogState.Hidden);
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= updateInterval) { timer = 0f; Recalculate(); }
        }

        private void Recalculate()
        {
            if (grid == null) return;
            visible.Clear();

            // Reveal around player buildings
            var buildings = Buildings.BuildingManager.Instance?.GetAllBuildings();
            if (buildings != null)
                foreach (var b in buildings)
                    if (b.OwnerId == 0 && b.State == Buildings.BuildingState.Active)
                        RevealAround(b.Location, buildingSightRadius);

            // Reveal around player settlers
            var settlers = FindObjectsOfType<Settlers.Settler>();
            foreach (var s in settlers)
                if (s.CurrentCell != null)
                    RevealAround(s.CurrentCell, settlerSightRadius);

            // Apply states
            foreach (var cell in grid.AllCells)
            {
                if      (visible.Contains(cell))  ApplyFogState(cell, FogState.Visible);
                else if (explored.Contains(cell)) ApplyFogState(cell, FogState.Shroud);
                else                              ApplyFogState(cell, FogState.Hidden);
            }
        }

        private void RevealAround(HexCell centre, int radius)
        {
            if (centre == null) return;
            foreach (var cell in grid.GetCellsInRadius(centre.Coordinates, radius))
            {
                visible.Add(cell);
                explored.Add(cell);
            }
        }

        private enum FogState { Hidden, Shroud, Visible }

        private void ApplyFogState(HexCell cell, FogState state)
        {
            var rend = cell.GetComponentInChildren<Renderer>();
            if (rend == null) return;

            // Show/hide 3D objects on the cell
            foreach (var child in cell.GetComponentsInChildren<Renderer>())
            {
                child.enabled = state != FogState.Hidden;
            }

            // Tint terrain
            if (state == FogState.Hidden)
            {
                rend.material.color = hiddenColour;
            }
            else if (state == FogState.Shroud)
            {
                var c = rend.material.color;
                rend.material.color = Color.Lerp(c, shroudColour, 0.5f);
            }
            // Visible — leave terrain colour as-is (TerrainGenerator set it)

            // Hide building GameObjects in fog
            if (cell.OccupiedBy != null)
                cell.OccupiedBy.gameObject.SetActive(state == FogState.Visible);
        }
    }
}
