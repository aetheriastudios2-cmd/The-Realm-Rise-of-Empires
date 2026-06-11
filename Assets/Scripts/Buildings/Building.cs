using System;
using System.Collections.Generic;
using UnityEngine;
using SettlersClone.Economy;
using SettlersClone.Map;

namespace SettlersClone.Buildings
{
    public enum BuildingState { UnderConstruction, Active, Paused, Destroyed }

    public class Building : MonoBehaviour
    {
        [SerializeField] private BuildingData data;

        public BuildingData Data      => data;
        public BuildingType Type      => data.type;
        public BuildingState State    { get; private set; } = BuildingState.UnderConstruction;
        public HexCell       Location { get; private set; }
        public int           OwnerId  { get; private set; }

        private float constructionProgress = 0f;

        // Local resource stock (input + output buffers at this building)
        private readonly Dictionary<ResourceType, int> localStock = new();

        // Output stock waiting for a carrier to collect
        private readonly Dictionary<ResourceType, int> outputStock = new();

        public event Action<Building>               OnConstructed;
        public event Action<Building, ResourceType> OnOutputReady;
        public event Action<Building>               OnDestroyed;

        public void Initialise(BuildingData buildingData, HexCell cell, int ownerId)
        {
            data     = buildingData;
            Location = cell;
            OwnerId  = ownerId;
            cell.TryPlaceBuilding(this);
        }

        private void Update()
        {
            if (State == BuildingState.UnderConstruction)
            {
                constructionProgress += Time.deltaTime;
                if (constructionProgress >= data.buildTime)
                    CompleteConstruction();
            }
        }

        private void CompleteConstruction()
        {
            State = BuildingState.Active;
            OnConstructed?.Invoke(this);
        }

        // --- Resource management ---

        public bool HasOutput(ResourceType type) =>
            outputStock.TryGetValue(type, out int qty) && qty > 0;

        public int GetOutputQty(ResourceType type) =>
            outputStock.TryGetValue(type, out int qty) ? qty : 0;

        public void AddInput(ResourceType type, int amount)
        {
            if (!localStock.ContainsKey(type)) localStock[type] = 0;
            localStock[type] += amount;
        }

        public bool ConsumeInput(ResourceType type, int amount)
        {
            if (!localStock.TryGetValue(type, out int have) || have < amount) return false;
            localStock[type] -= amount;
            return true;
        }

        public int GetInputQty(ResourceType type) =>
            localStock.TryGetValue(type, out int qty) ? qty : 0;

        public void ProduceOutput(ResourceType type, int amount)
        {
            if (!outputStock.ContainsKey(type)) outputStock[type] = 0;
            outputStock[type] += amount;
            OnOutputReady?.Invoke(this, type);
        }

        public int CollectOutput(ResourceType type, int maxAmount)
        {
            if (!outputStock.TryGetValue(type, out int qty)) return 0;
            int collected = Mathf.Min(qty, maxAmount);
            outputStock[type] -= collected;
            return collected;
        }

        public bool IsOutputFull(ResourceType type)
        {
            int max = data.recipe != null ? data.recipe.maxOutputStock : 0;
            return GetOutputQty(type) >= max;
        }

        public void SetPaused(bool paused)
        {
            if (State == BuildingState.Active && paused)    State = BuildingState.Paused;
            else if (State == BuildingState.Paused && !paused) State = BuildingState.Active;
        }

        public void Demolish()
        {
            State = BuildingState.Destroyed;
            Location?.RemoveBuilding();
            OnDestroyed?.Invoke(this);
            Destroy(gameObject, 0.5f);
        }

        public IReadOnlyDictionary<ResourceType, int> LocalStock => localStock;
    }
}
