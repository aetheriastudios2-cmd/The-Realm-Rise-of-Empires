using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SettlersClone.Economy;
using SettlersClone.Buildings;

namespace SettlersClone.Editor
{
    /// <summary>
    /// Run via: Settlers ▶ Create All Building Data
    /// Auto-generates every BuildingData and ProductionRecipe ScriptableObject
    /// so you don't have to fill them in by hand.
    /// </summary>
    public static class BuildingDataWizard
    {
        private const string RecipePath   = "Assets/Resources/BuildingCatalogue/Recipes";
        private const string DataPath     = "Assets/Resources/BuildingCatalogue/Buildings";

        [MenuItem("Settlers/Create All Building Data", priority = 3)]
        public static void CreateAll()
        {
            if (!EditorUtility.DisplayDialog("Create Building Data",
                "Auto-create all BuildingData and ProductionRecipe assets?", "Yes", "Cancel"))
                return;

            System.IO.Directory.CreateDirectory(Application.dataPath +
                "/../" + RecipePath.Replace("Assets/", ""));
            System.IO.Directory.CreateDirectory(Application.dataPath +
                "/../" + DataPath.Replace("Assets/", ""));

            var recipes = CreateRecipes();
            CreateBuildingDatas(recipes);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Done",
                "All BuildingData and ProductionRecipe assets created in\n" +
                $"{DataPath}\n{RecipePath}", "OK");
        }

        // ------------------------------------------------------------------ Recipes

        private static Dictionary<string, ProductionRecipe> CreateRecipes()
        {
            var map = new Dictionary<string, ProductionRecipe>();

            map["Log"]     = Recipe("LogRecipe",    ResourceType.Log,     1, 20f);
            map["Plank"]   = Recipe("PlankRecipe",  ResourceType.Plank,   1, 15f,
                                    (ResourceType.Log,     1));
            map["Stone"]   = Recipe("StoneRecipe",  ResourceType.Stone,   1, 25f);
            map["Coal"]    = Recipe("CoalRecipe",   ResourceType.Coal,    1, 20f);
            map["IronOre"] = Recipe("IronOreRecipe",ResourceType.IronOre, 1, 20f);
            map["GoldOre"] = Recipe("GoldOreRecipe",ResourceType.GoldOre, 1, 30f);
            map["Iron"]    = Recipe("IronRecipe",   ResourceType.Iron,    1, 20f,
                                    (ResourceType.IronOre, 1),
                                    (ResourceType.Coal,    1));
            map["Gold"]    = Recipe("GoldRecipe",   ResourceType.Gold,    1, 25f,
                                    (ResourceType.GoldOre, 1),
                                    (ResourceType.Coal,    1));
            map["Grain"]   = Recipe("GrainRecipe",  ResourceType.Grain,   1, 30f);
            map["Flour"]   = Recipe("FlourRecipe",  ResourceType.Flour,   1, 15f,
                                    (ResourceType.Grain,   1));
            map["Bread"]   = Recipe("BreadRecipe",  ResourceType.Bread,   1, 10f,
                                    (ResourceType.Flour,   1));
            map["Fish"]    = Recipe("FishRecipe",   ResourceType.Fish,    1, 18f);
            map["Meat"]    = Recipe("MeatRecipe",   ResourceType.Meat,    1, 22f);
            map["Tools"]   = Recipe("ToolsRecipe",  ResourceType.Tools,   1, 25f,
                                    (ResourceType.Iron,    1),
                                    (ResourceType.Plank,   1));
            map["Sword"]   = Recipe("SwordRecipe",  ResourceType.Sword,   1, 30f,
                                    (ResourceType.Iron,    1),
                                    (ResourceType.Coal,    1));
            map["Bow"]     = Recipe("BowRecipe",    ResourceType.Bow,     1, 28f,
                                    (ResourceType.Plank,   2));
            return map;
        }

        private static ProductionRecipe Recipe(string assetName, ResourceType output,
            int outputAmt, float cycleTime,
            params (ResourceType type, int amount)[] inputs)
        {
            string path = $"{RecipePath}/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ProductionRecipe>(path);
            if (existing != null) return existing;

            var r = ScriptableObject.CreateInstance<ProductionRecipe>();
            r.output    = new ResourceAmount(output, outputAmt);
            r.cycleTime = cycleTime;
            foreach (var (t, a) in inputs)
                r.inputs.Add(new ResourceAmount(t, a));
            AssetDatabase.CreateAsset(r, path);
            return r;
        }

        // ------------------------------------------------------------------ Building Data

        private static void CreateBuildingDatas(Dictionary<string, ProductionRecipe> recipes)
        {
            // Storage
            BD("Headquarters", BuildingType.Headquarters, null,
               isStorage: true, storeCap: 400, buildTime: 0,
               cost: new[] { (ResourceType.None, 0) });

            BD("Warehouse", BuildingType.Warehouse, null,
               isStorage: true, storeCap: 100,
               cost: new[] { (ResourceType.Plank, 4), (ResourceType.Stone, 2) });

            // Woodcutting
            BD("Woodcutter", BuildingType.Woodcutter, recipes["Log"],
               cost: new[] { (ResourceType.Plank, 2) });
            BD("Sawmill", BuildingType.Sawmill, recipes["Plank"],
               cost: new[] { (ResourceType.Plank, 4), (ResourceType.Stone, 2) }, workers: 1);

            // Stone
            BD("Quarry", BuildingType.Quarry, recipes["Stone"],
               cost: new[] { (ResourceType.Plank, 3) }, workers: 2);

            // Mining
            BD("Coal Mine",  BuildingType.CoalMine,  recipes["Coal"],
               cost: new[] { (ResourceType.Plank, 4), (ResourceType.Tools, 1) }, workers: 2);
            BD("Iron Mine",  BuildingType.IronMine,  recipes["IronOre"],
               cost: new[] { (ResourceType.Plank, 4), (ResourceType.Tools, 1) }, workers: 2);
            BD("Gold Mine",  BuildingType.GoldMine,  recipes["GoldOre"],
               cost: new[] { (ResourceType.Plank, 5), (ResourceType.Tools, 2) }, workers: 2);
            BD("Smelter",    BuildingType.Smelter,   recipes["Iron"],
               cost: new[] { (ResourceType.Stone, 5), (ResourceType.Plank, 2) });
            BD("Gold Smelter",BuildingType.GoldSmelter, recipes["Gold"],
               cost: new[] { (ResourceType.Stone, 6), (ResourceType.Plank, 2) });

            // Food
            BD("Farm",         BuildingType.Farm,        recipes["Grain"],
               cost: new[] { (ResourceType.Plank, 3) }, workers: 2);
            BD("Mill",         BuildingType.Mill,        recipes["Flour"],
               cost: new[] { (ResourceType.Plank, 2), (ResourceType.Stone, 1) });
            BD("Bakery",       BuildingType.Bakery,      recipes["Bread"],
               cost: new[] { (ResourceType.Plank, 2), (ResourceType.Stone, 2) });
            BD("Fishing Hut",  BuildingType.FishingHut,  recipes["Fish"],
               cost: new[] { (ResourceType.Plank, 2) });
            BD("Hunter",       BuildingType.Hunter,      recipes["Meat"],
               cost: new[] { (ResourceType.Plank, 2) });

            // Crafting
            BD("Toolsmith", BuildingType.Toolsmith, recipes["Tools"],
               cost: new[] { (ResourceType.Plank, 3), (ResourceType.Stone, 2) });
            BD("Armory",    BuildingType.Armory,    recipes["Sword"],
               cost: new[] { (ResourceType.Plank, 3), (ResourceType.Stone, 3) });
            BD("Bowyer",    BuildingType.Bowyer,    recipes["Bow"],
               cost: new[] { (ResourceType.Plank, 3) });

            // Military
            BDMilitary("Barracks",   BuildingType.Barracks,   garrison: 8,  soldiers: 8,  territory: 0,
               cost: new[] { (ResourceType.Plank, 4), (ResourceType.Stone, 2) });
            BDMilitary("Watchtower", BuildingType.Watchtower, garrison: 2,  soldiers: 2,  territory: 5,
               cost: new[] { (ResourceType.Plank, 3), (ResourceType.Stone, 5) });
            BDMilitary("Fortress",   BuildingType.Fortress,   garrison: 16, soldiers: 16, territory: 10,
               cost: new[] { (ResourceType.Stone, 20), (ResourceType.Plank, 10), (ResourceType.Tools, 5) });
        }

        private static void BD(string displayName, BuildingType type,
            ProductionRecipe recipe,
            (ResourceType, int)[] cost = null,
            bool isStorage = false, int storeCap = 50,
            float buildTime = 15f, int workers = 1)
        {
            string path = $"{DataPath}/{type}.asset";
            if (AssetDatabase.LoadAssetAtPath<BuildingData>(path) != null) return;

            var d = ScriptableObject.CreateInstance<BuildingData>();
            d.type         = type;
            d.displayName  = displayName;
            d.recipe       = recipe;
            d.isStorage    = isStorage;
            d.storageCapacity = storeCap;
            d.buildTime    = buildTime;
            d.maxWorkers   = workers;
            if (cost != null)
                foreach (var (rt, amt) in cost)
                    if (rt != ResourceType.None)
                        d.constructionCost.Add(new ResourceAmount(rt, amt));
            AssetDatabase.CreateAsset(d, path);
        }

        private static void BDMilitary(string displayName, BuildingType type,
            int garrison, int soldiers, float territory,
            (ResourceType, int)[] cost)
        {
            string path = $"{DataPath}/{type}.asset";
            if (AssetDatabase.LoadAssetAtPath<BuildingData>(path) != null) return;

            var d = ScriptableObject.CreateInstance<BuildingData>();
            d.type            = type;
            d.displayName     = displayName;
            d.isMilitary      = true;
            d.maxGarrison     = garrison;
            d.maxSoldiers     = soldiers;
            d.territoryRadius = territory;
            d.buildTime       = 20f;
            foreach (var (rt, amt) in cost)
                d.constructionCost.Add(new ResourceAmount(rt, amt));
            AssetDatabase.CreateAsset(d, path);
        }
    }
}
