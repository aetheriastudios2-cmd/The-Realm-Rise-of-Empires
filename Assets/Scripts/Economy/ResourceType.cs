using System;
using UnityEngine;

namespace SettlersClone.Economy
{
    [Serializable]
    public enum ResourceType
    {
        None      = 0,
        // Raw natural resources
        Log       = 1,
        Stone     = 2,
        Coal      = 3,
        IronOre   = 4,
        GoldOre   = 5,
        Grain     = 6,
        Fish      = 7,
        // Processed goods
        Plank     = 10,
        Iron      = 11,
        Gold      = 12,
        Flour     = 13,
        Bread     = 14,
        Beer      = 15,
        Meat      = 16,
        // Tools & weapons
        Tools     = 20,
        Sword     = 21,
        Shield    = 22,
        Bow       = 23,
        Arrow     = 24,
        // Economy
        Coin      = 30
    }

    [Serializable]
    public struct ResourceAmount
    {
        public ResourceType type;
        public int amount;

        public ResourceAmount(ResourceType type, int amount)
        {
            this.type   = type;
            this.amount = amount;
        }
    }
}
