using UnityEngine;
using UnityEditor;
using SettlersClone.Buildings;
using SettlersClone.UI;

namespace SettlersClone.Editor
{
    /// <summary>
    /// Settlers ▶ Create Placeholder Prefabs
    /// Generates coloured-cube placeholder prefabs for every building type
    /// and the three settler types, so the game is immediately playable
    /// without custom art.
    /// </summary>
    public static class PrefabFactory
    {
        private const string PrefabPath = "Assets/Prefabs";

        [MenuItem("Settlers/Create Placeholder Prefabs", priority = 4)]
        public static void CreateAll()
        {
            if (!EditorUtility.DisplayDialog("Create Prefabs",
                "Generate placeholder prefabs for all buildings and settlers?",
                "Yes", "Cancel")) return;

            System.IO.Directory.CreateDirectory(
                Application.dataPath + "/../" + PrefabPath.Replace("Assets/", "") + "/Buildings");
            System.IO.Directory.CreateDirectory(
                Application.dataPath + "/../" + PrefabPath.Replace("Assets/", "") + "/Settlers");

            CreateBuildingPrefabs();
            CreateSettlerPrefabs();
            CreateHexCellPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Done",
                $"Placeholder prefabs created in {PrefabPath}", "OK");
        }

        // ------------------------------------------------------------------ Buildings

        private static void CreateBuildingPrefabs()
        {
            foreach (BuildingType type in System.Enum.GetValues(typeof(BuildingType)))
            {
                string path = $"{PrefabPath}/Buildings/{type}.prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) continue;

                var go = CreateBuildingRoot(type);
                PrefabUtility.SaveAsPrefabAsset(go, path);
                Object.DestroyImmediate(go);
            }
        }

        private static GameObject CreateBuildingRoot(BuildingType type)
        {
            // Root with game logic components
            var root = new GameObject(type.ToString());
            root.AddComponent<Building>();
            root.AddComponent<BuildingClickHandler>();
            root.AddComponent<BoxCollider>();

            if (IsProduction(type))   root.AddComponent<ProductionBuilding>();
            if (IsStorage(type))      root.AddComponent<StorageBuilding>();
            if (IsMilitary(type))     root.AddComponent<MilitaryBuilding>();

            // Visual child — coloured cube
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            Object.DestroyImmediate(visual.GetComponent<BoxCollider>());
            visual.transform.SetParent(root.transform, false);

            float scale = IsLarge(type) ? 2f : IsMedium(type) ? 1.5f : 1f;
            visual.transform.localScale  = new Vector3(scale, scale * 0.8f, scale);
            visual.transform.localPosition = new Vector3(0f, scale * 0.4f, 0f);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat.shader.name == "Hidden/InternalErrorShader")
                mat = new Material(Shader.Find("Standard"));
            mat.color = BuildingColour(type);
            visual.GetComponent<Renderer>().material = mat;

            // Small roof triangle for visual interest
            if (!IsStorage(type))
            {
                var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
                roof.name = "Roof";
                Object.DestroyImmediate(roof.GetComponent<BoxCollider>());
                roof.transform.SetParent(root.transform, false);
                roof.transform.localScale    = new Vector3(scale * 0.9f, 0.3f, scale * 0.9f);
                roof.transform.localPosition = new Vector3(0f, scale * 0.85f, 0f);
                var roofMat = new Material(mat) { color = BuildingColour(type) * 0.7f };
                roof.GetComponent<Renderer>().material = roofMat;
            }

            return root;
        }

        // ------------------------------------------------------------------ Settlers

        private static void CreateSettlerPrefabs()
        {
            CreateSettlerPrefab<Settlers.SettlerCarrier>("Carrier",
                new Color(0.9f, 0.85f, 0.6f), 0.4f);
            CreateSettlerPrefab<Settlers.SettlerWorker>("Worker",
                new Color(0.5f, 0.75f, 0.4f), 0.4f);
            CreateSettlerPrefab<Settlers.SettlerSoldier>("Soldier",
                new Color(0.3f, 0.3f, 0.85f), 0.45f);
        }

        private static void CreateSettlerPrefab<T>(string name, Color colour, float radius)
            where T : Component
        {
            string path = $"{PrefabPath}/Settlers/{name}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            // Body
            var root = new GameObject(name);
            root.AddComponent<T>();
            root.AddComponent<UnityEngine.AI.NavMeshAgent>();
            root.AddComponent<CapsuleCollider>();

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            Object.DestroyImmediate(body.GetComponent<CapsuleCollider>());
            body.transform.SetParent(root.transform, false);
            body.transform.localScale    = new Vector3(radius, 0.5f, radius);
            body.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat.shader.name == "Hidden/InternalErrorShader")
                mat = new Material(Shader.Find("Standard"));
            mat.color = colour;
            body.GetComponent<Renderer>().material = mat;

            // Head
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            Object.DestroyImmediate(head.GetComponent<SphereCollider>());
            head.transform.SetParent(root.transform, false);
            head.transform.localScale    = new Vector3(0.3f, 0.3f, 0.3f);
            head.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            head.GetComponent<Renderer>().material = mat;

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        // ------------------------------------------------------------------ HexCell

        private static void CreateHexCellPrefab()
        {
            string path = $"{PrefabPath}/HexCell.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            var go = new GameObject("HexCell");
            go.AddComponent<Map.HexCell>();
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        // ------------------------------------------------------------------ Helpers

        private static bool IsProduction(BuildingType t) =>
            t != BuildingType.Headquarters && t != BuildingType.Warehouse &&
            t != BuildingType.Barracks && t != BuildingType.Watchtower &&
            t != BuildingType.Fortress;

        private static bool IsStorage(BuildingType t) =>
            t == BuildingType.Headquarters || t == BuildingType.Warehouse;

        private static bool IsMilitary(BuildingType t) =>
            t == BuildingType.Barracks || t == BuildingType.Watchtower ||
            t == BuildingType.Fortress || t == BuildingType.Harbor;

        private static bool IsLarge(BuildingType t) =>
            t == BuildingType.Headquarters || t == BuildingType.Fortress;

        private static bool IsMedium(BuildingType t) =>
            t == BuildingType.Warehouse || t == BuildingType.Barracks ||
            t == BuildingType.Smelter   || t == BuildingType.GoldSmelter ||
            t == BuildingType.Armory    || t == BuildingType.Sawmill;

        private static Color BuildingColour(BuildingType t) => t switch
        {
            BuildingType.Headquarters  => new Color(0.9f, 0.8f, 0.1f),
            BuildingType.Warehouse     => new Color(0.8f, 0.7f, 0.3f),
            BuildingType.Woodcutter    => new Color(0.5f, 0.3f, 0.1f),
            BuildingType.Sawmill       => new Color(0.6f, 0.4f, 0.2f),
            BuildingType.Quarry        => new Color(0.6f, 0.6f, 0.6f),
            BuildingType.CoalMine      => new Color(0.2f, 0.2f, 0.2f),
            BuildingType.IronMine      => new Color(0.4f, 0.3f, 0.5f),
            BuildingType.GoldMine      => new Color(0.9f, 0.75f, 0.1f),
            BuildingType.Smelter       => new Color(0.8f, 0.4f, 0.1f),
            BuildingType.Farm          => new Color(0.7f, 0.85f, 0.2f),
            BuildingType.Mill          => new Color(0.9f, 0.9f, 0.6f),
            BuildingType.Bakery        => new Color(0.95f, 0.8f, 0.5f),
            BuildingType.FishingHut    => new Color(0.2f, 0.5f, 0.8f),
            BuildingType.Toolsmith     => new Color(0.5f, 0.5f, 0.5f),
            BuildingType.Armory        => new Color(0.3f, 0.3f, 0.6f),
            BuildingType.Barracks      => new Color(0.4f, 0.1f, 0.1f),
            BuildingType.Watchtower    => new Color(0.6f, 0.5f, 0.3f),
            BuildingType.Fortress      => new Color(0.3f, 0.3f, 0.35f),
            _                          => new Color(0.5f, 0.5f, 0.5f)
        };
    }
}
