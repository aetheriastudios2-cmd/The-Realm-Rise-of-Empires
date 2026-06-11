using System;
using System.Collections.Generic;
using UnityEngine;
using SettlersClone.Map;
using SettlersClone.Buildings;

namespace SettlersClone.Economy
{
    // Tracks which player owns which cells and handles border rendering
    public class TerritoryManager : MonoBehaviour
    {
        public static TerritoryManager Instance { get; private set; }

        [SerializeField] private HexGrid grid;

        // Player ID -> set of claimed cells
        private readonly Dictionary<int, HashSet<HexCell>> playerTerritories = new();

        public event Action<int, HexCell> OnCellClaimed;
        public event Action<int, HexCell> OnCellLost;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            BuildingManager.Instance.OnBuildingPlaced += HandleBuildingPlaced;
        }

        private void OnDisable()
        {
            BuildingManager.Instance.OnBuildingPlaced -= HandleBuildingPlaced;
        }

        private void HandleBuildingPlaced(Building building)
        {
            var mil = building.GetComponent<MilitaryBuilding>();
            if (mil != null) mil.OnTerritoryChanged += RefreshTerritoryFromMilitary;
        }

        private void RefreshTerritoryFromMilitary(MilitaryBuilding mil)
        {
            int pid = mil.GetComponent<Building>().OwnerId;
            if (!playerTerritories.ContainsKey(pid))
                playerTerritories[pid] = new HashSet<HexCell>();

            foreach (var cell in mil.ClaimedCells)
            {
                if (playerTerritories[pid].Add(cell))
                    OnCellClaimed?.Invoke(pid, cell);
            }
        }

        public bool IsInTerritory(HexCell cell, int playerId) =>
            playerTerritories.TryGetValue(playerId, out var set) && set.Contains(cell);

        public int GetOwner(HexCell cell)
        {
            foreach (var kvp in playerTerritories)
                if (kvp.Value.Contains(cell)) return kvp.Key;
            return -1;
        }

        public int GetTerritorySize(int playerId) =>
            playerTerritories.TryGetValue(playerId, out var set) ? set.Count : 0;
    }
}
