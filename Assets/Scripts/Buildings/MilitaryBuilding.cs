using System;
using System.Collections.Generic;
using UnityEngine;
using SettlersClone.Map;

namespace SettlersClone.Buildings
{
    // Handles territory claim and garrison for military structures.
    [RequireComponent(typeof(Building))]
    public class MilitaryBuilding : MonoBehaviour
    {
        private Building building;
        private HexGrid  grid;

        private readonly List<HexCell> claimedCells   = new();
        private readonly List<object>  garrisonedUnits = new();

        public int GarrisonCount => garrisonedUnits.Count;
        public IReadOnlyList<HexCell> ClaimedCells => claimedCells;

        public event Action<MilitaryBuilding> OnTerritoryChanged;

        private void Awake()
        {
            building = GetComponent<Building>();
            grid     = FindObjectOfType<HexGrid>();
        }

        private void OnEnable()
        {
            building.OnConstructed += HandleConstructed;
            building.OnDestroyed   += HandleDestroyed;
        }

        private void OnDisable()
        {
            building.OnConstructed -= HandleConstructed;
            building.OnDestroyed   -= HandleDestroyed;
        }

        private void HandleConstructed(Building b)
        {
            ClaimTerritory();
        }

        private void HandleDestroyed(Building b)
        {
            ReleaseClaims();
        }

        private void ClaimTerritory()
        {
            if (building.Data == null || grid == null) return;
            int radius = Mathf.RoundToInt(building.Data.territoryRadius);
            var cells  = grid.GetCellsInRadius(building.Location.Coordinates, radius);
            foreach (var cell in cells)
            {
                cell.SetOwner(building.OwnerId);
                claimedCells.Add(cell);
            }
            OnTerritoryChanged?.Invoke(this);
        }

        private void ReleaseClaims()
        {
            foreach (var cell in claimedCells)
                cell.SetOwner(-1);
            claimedCells.Clear();
            OnTerritoryChanged?.Invoke(this);
        }

        public bool AddSoldier(object soldier)
        {
            if (garrisonedUnits.Count >= building.Data.maxGarrison) return false;
            garrisonedUnits.Add(soldier);
            return true;
        }

        public bool RemoveSoldier(object soldier) => garrisonedUnits.Remove(soldier);
    }
}
