using System.Collections.Generic;
using UnityEngine;
using SettlersClone.Economy;

namespace SettlersClone.Buildings
{
    // Warehouses and Headquarters act as central storage hubs.
    // The ResourceManager delegates to these for global stock tracking.
    [RequireComponent(typeof(Building))]
    public class StorageBuilding : MonoBehaviour
    {
        private Building building;
        private readonly Dictionary<ResourceType, int> vault = new();
        private int totalStored;

        public int TotalCapacity => building.Data != null ? building.Data.storageCapacity : 50;
        public int TotalStored   => totalStored;
        public bool IsFull       => totalStored >= TotalCapacity;

        private void Awake() => building = GetComponent<Building>();

        public bool Deposit(ResourceType type, int amount)
        {
            int canStore = Mathf.Min(amount, TotalCapacity - totalStored);
            if (canStore <= 0) return false;
            if (!vault.ContainsKey(type)) vault[type] = 0;
            vault[type] += canStore;
            totalStored += canStore;
            return true;
        }

        public int Withdraw(ResourceType type, int amount)
        {
            if (!vault.TryGetValue(type, out int have)) return 0;
            int taken = Mathf.Min(have, amount);
            vault[type] -= taken;
            totalStored -= taken;
            return taken;
        }

        public int Query(ResourceType type) =>
            vault.TryGetValue(type, out int qty) ? qty : 0;

        public IReadOnlyDictionary<ResourceType, int> Vault => vault;
    }
}
