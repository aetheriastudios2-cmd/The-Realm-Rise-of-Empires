using System.Collections.Generic;
using UnityEngine;

namespace SettlersClone.Economy
{
    [CreateAssetMenu(menuName = "Settlers/Production Recipe", fileName = "NewRecipe")]
    public class ProductionRecipe : ScriptableObject
    {
        [Header("Inputs (consumed per cycle)")]
        public List<ResourceAmount> inputs = new();

        [Header("Output (produced per cycle)")]
        public ResourceAmount output;

        [Header("Timing")]
        [Tooltip("Seconds per production cycle")]
        public float cycleTime = 10f;

        [Tooltip("Max output units stored at building before production pauses")]
        public int maxOutputStock = 8;

        public bool HasInputs(IReadOnlyDictionary<ResourceType, int> stock)
        {
            foreach (var input in inputs)
            {
                if (!stock.TryGetValue(input.type, out int have) || have < input.amount)
                    return false;
            }
            return true;
        }
    }
}
