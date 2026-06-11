using System.Collections.Generic;
using UnityEngine;
using SettlersClone.Buildings;

namespace SettlersClone.Map
{
    public enum TerrainType
    {
        Grassland = 0,
        Forest    = 1,
        Mountain  = 2,
        Water     = 3,
        Desert    = 4,
        Snow      = 5
    }

    public enum BuildingSlotSize { Small, Medium, Large }

    public class HexCell : MonoBehaviour
    {
        public HexCoordinates Coordinates { get; private set; }
        public TerrainType    Terrain     { get; private set; }
        public int            Elevation   { get; private set; }
        public bool           HasRoad     { get; private set; }
        public Building       OccupiedBy  { get; private set; }
        public int            OwnerId     { get; private set; } = -1; // -1 = unclaimed

        [SerializeField] private Renderer terrainRenderer;
        [SerializeField] private GameObject roadOverlay;
        [SerializeField] private GameObject highlightOverlay;

        private readonly HexCell[] neighbours = new HexCell[6];

        public void Initialise(HexCoordinates coords, TerrainType terrain, int elevation)
        {
            Coordinates = coords;
            Terrain     = terrain;
            Elevation   = elevation;
            ApplyTerrainVisuals();
        }

        public void SetNeighbour(int direction, HexCell cell)
        {
            neighbours[direction] = cell;
            if (cell != null) cell.neighbours[(direction + 3) % 6] = this;
        }

        public HexCell GetNeighbour(int direction) => neighbours[direction];

        public IEnumerable<HexCell> GetAllNeighbours()
        {
            foreach (var n in neighbours)
                if (n != null) yield return n;
        }

        public bool IsPassable()   => Terrain != TerrainType.Water && OccupiedBy == null;
        public bool IsBuildable()  => Terrain != TerrainType.Water && Terrain != TerrainType.Mountain && OccupiedBy == null;
        public bool IsRoadable()   => Terrain != TerrainType.Water;

        public bool TryPlaceBuilding(Building building)
        {
            if (!IsBuildable()) return false;
            OccupiedBy = building;
            return true;
        }

        public void RemoveBuilding()
        {
            OccupiedBy = null;
        }

        public void SetRoad(bool hasRoad)
        {
            HasRoad = hasRoad;
            if (roadOverlay != null) roadOverlay.SetActive(hasRoad);
        }

        public void SetOwner(int playerId)
        {
            OwnerId = playerId;
        }

        public void SetHighlight(bool active)
        {
            if (highlightOverlay != null) highlightOverlay.SetActive(active);
        }

        public float MovementCost()
        {
            if (!IsPassable()) return float.MaxValue;
            float cost = Terrain switch
            {
                TerrainType.Grassland => 1f,
                TerrainType.Forest    => 1.5f,
                TerrainType.Desert    => 1.3f,
                TerrainType.Snow      => 1.8f,
                _                     => 1f
            };
            if (HasRoad) cost *= 0.5f;
            return cost;
        }

        private void ApplyTerrainVisuals()
        {
            if (terrainRenderer == null) return;
            terrainRenderer.material.color = Terrain switch
            {
                TerrainType.Grassland => new Color(0.33f, 0.60f, 0.22f),
                TerrainType.Forest    => new Color(0.13f, 0.38f, 0.14f),
                TerrainType.Mountain  => new Color(0.55f, 0.55f, 0.55f),
                TerrainType.Water     => new Color(0.20f, 0.47f, 0.80f),
                TerrainType.Desert    => new Color(0.88f, 0.78f, 0.48f),
                TerrainType.Snow      => new Color(0.93f, 0.96f, 0.98f),
                _                     => Color.white
            };
        }
    }
}
