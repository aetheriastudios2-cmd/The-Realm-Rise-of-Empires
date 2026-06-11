using System.Collections.Generic;
using UnityEngine;
using SettlersClone.Economy;

namespace SettlersClone.Buildings
{
    [CreateAssetMenu(menuName = "Settlers/Building Data", fileName = "NewBuilding")]
    public class BuildingData : ScriptableObject
    {
        [Header("Identity")]
        public BuildingType type;
        public string       displayName;
        [TextArea] public string description;
        public Sprite       icon;
        public GameObject   prefab;

        [Header("Construction Cost")]
        public List<ResourceAmount> constructionCost = new();
        public float buildTime = 15f;

        [Header("Footprint")]
        public int footprintRadius = 0; // 0 = single cell, 1 = cell + ring, etc.

        [Header("Production")]
        public ProductionRecipe recipe; // null for storage/military buildings

        [Header("Workers")]
        public int maxWorkers   = 1;
        public int maxCarriers  = 2;

        [Header("Military")]
        public bool  isMilitary       = false;
        public int   maxGarrison      = 0;
        public float territoryRadius  = 0f; // in hex cells
        public int   maxSoldiers      = 0;

        [Header("Storage")]
        public bool isStorage         = false;
        public int  storageCapacity   = 50;
    }
}
