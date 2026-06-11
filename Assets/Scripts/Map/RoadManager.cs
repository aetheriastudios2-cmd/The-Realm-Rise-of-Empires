using System.Collections.Generic;
using UnityEngine;

namespace SettlersClone.Map
{
    // Handles road placement and the flag-based road network used by settlers
    public class RoadManager : MonoBehaviour
    {
        public static RoadManager Instance { get; private set; }

        [SerializeField] private HexGrid grid;

        private readonly HashSet<HexCell> roadCells = new();
        private readonly HashSet<HexCell> flagCells = new();

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public bool PlaceRoad(HexCell cell)
        {
            if (!cell.IsRoadable()) return false;
            cell.SetRoad(true);
            roadCells.Add(cell);
            return true;
        }

        public bool RemoveRoad(HexCell cell)
        {
            if (!roadCells.Contains(cell)) return false;
            cell.SetRoad(false);
            roadCells.Remove(cell);
            flagCells.Remove(cell);
            return true;
        }

        public bool PlaceFlag(HexCell cell)
        {
            if (!roadCells.Contains(cell)) return false;
            flagCells.Add(cell);
            return true;
        }

        public bool IsRoad(HexCell cell) => roadCells.Contains(cell);
        public bool IsFlag(HexCell cell) => flagCells.Contains(cell);

        // Returns the nearest flag on the road network from a start cell
        public HexCell FindNearestFlag(HexCell start)
        {
            if (flagCells.Count == 0) return null;
            HexCell nearest   = null;
            int     bestDist  = int.MaxValue;
            foreach (var flag in flagCells)
            {
                int d = start.Coordinates.DistanceTo(flag.Coordinates);
                if (d < bestDist) { bestDist = d; nearest = flag; }
            }
            return nearest;
        }

        public IEnumerable<HexCell> GetAllRoads()  => roadCells;
        public IEnumerable<HexCell> GetAllFlags()  => flagCells;
    }
}
