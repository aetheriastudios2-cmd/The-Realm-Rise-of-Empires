using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SettlersClone.Economy;
using SettlersClone.Buildings;

namespace SettlersClone.Core
{
    // Global resource tracking singleton. Aggregates across all StorageBuildings.
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        private readonly Dictionary<ResourceType, int> globalStock = new();

        public event Action<ResourceType, int> OnResourceChanged;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        // Seed the HQ with starting resources
        public void InitialiseStartingResources()
        {
            AddResource(ResourceType.Log,   30);
            AddResource(ResourceType.Plank, 20);
            AddResource(ResourceType.Stone, 20);
            AddResource(ResourceType.Tools, 10);
            AddResource(ResourceType.Bread, 10);
        }

        public int GetAmount(ResourceType type) =>
            globalStock.TryGetValue(type, out int v) ? v : 0;

        public void AddResource(ResourceType type, int amount)
        {
            if (amount <= 0) return;
            if (!globalStock.ContainsKey(type)) globalStock[type] = 0;
            globalStock[type] += amount;
            OnResourceChanged?.Invoke(type, globalStock[type]);
        }

        public bool TrySpend(ResourceType type, int amount)
        {
            if (GetAmount(type) < amount) return false;
            globalStock[type] -= amount;
            OnResourceChanged?.Invoke(type, globalStock[type]);
            return true;
        }

        public bool CanAfford(List<ResourceAmount> costs)
        {
            var totals = new Dictionary<ResourceType, int>();
            foreach (var c in costs)
            {
                if (!totals.ContainsKey(c.type)) totals[c.type] = 0;
                totals[c.type] += c.amount;
            }
            return totals.All(kvp => GetAmount(kvp.Key) >= kvp.Value);
        }

        public void Spend(List<ResourceAmount> costs)
        {
            foreach (var c in costs)
                TrySpend(c.type, c.amount);
        }

        public IReadOnlyDictionary<ResourceType, int> GlobalStock => globalStock;
    }
}
