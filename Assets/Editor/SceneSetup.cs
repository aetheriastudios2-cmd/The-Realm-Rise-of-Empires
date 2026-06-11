using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace SettlersClone.Editor
{
    /// <summary>
    /// Run via: top menu  Settlers ▶ Setup Scene
    /// Builds the entire MainGame scene hierarchy from scratch —
    /// managers, camera rig, lighting, UI canvas, and NavMesh surface.
    /// </summary>
    public static class SceneSetup
    {
        [MenuItem("Settlers/Setup Scene", priority = 1)]
        public static void SetupScene()
        {
            if (!EditorUtility.DisplayDialog("Setup Scene",
                "This will populate the current scene with all Settlers manager GameObjects, " +
                "camera rig, lighting, and UI canvas.\n\nProceed?", "Yes", "Cancel"))
                return;

            Undo.SetCurrentGroupName("Settlers Scene Setup");
            int group = Undo.GetCurrentGroup();

            CreateLighting();
            var hexGrid      = CreateMap();
            var managers     = CreateManagers(hexGrid);
            CreateCameraRig();
            CreateUI();
            CreateNavMeshSurface(hexGrid);

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("Done",
                "Scene setup complete!\n\n" +
                "Next steps:\n" +
                "1. Create BuildingData ScriptableObjects (see README.txt)\n" +
                "2. Create Prefabs for buildings + settlers\n" +
                "3. Assign them in BuildingManager & SettlerManager inspectors\n" +
                "4. Bake NavMesh (Window → AI → Navigation → Bake)\n" +
                "5. Press Play!", "OK");
        }

        // ------------------------------------------------------------------ Lighting

        private static void CreateLighting()
        {
            var existing = GameObject.Find("Directional Light");
            if (existing != null) return;

            var go    = new GameObject("Directional Light");
            var light = go.AddComponent<Light>();
            light.type      = LightType.Directional;
            light.color     = new Color(1f, 0.956f, 0.839f);
            light.intensity = 1.2f;
            light.shadows   = LightShadows.Soft;
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Undo.RegisterCreatedObjectUndo(go, "Create Lighting");
        }

        // ------------------------------------------------------------------ Map

        private static GameObject CreateMap()
        {
            var root = new GameObject("_Map");
            Undo.RegisterCreatedObjectUndo(root, "Create Map");

            // HexGrid holder — attach HexGrid component manually after scene setup
            // because we don't have prefab refs here
            var gridGO = new GameObject("HexGrid");
            gridGO.transform.SetParent(root.transform);
            // User adds HexGrid + HexMesh components and assigns prefabs in inspector

            // Terrain placeholder
            var terrain = GameObject.CreatePrimitive(PrimitiveType.Plane);
            terrain.name = "TerrainPlaceholder";
            terrain.transform.SetParent(root.transform);
            terrain.transform.localScale = new Vector3(40f, 1f, 30f);
            terrain.transform.position   = new Vector3(100f, -0.1f, 75f);
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat.shader.name == "Hidden/InternalErrorShader")
                mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.33f, 0.60f, 0.22f);
            terrain.GetComponent<Renderer>().material = mat;
            terrain.isStatic = true;

            return gridGO;
        }

        // ------------------------------------------------------------------ Managers

        private static Dictionary<string, GameObject> CreateManagers(GameObject hexGrid)
        {
            var root = new GameObject("_Managers");
            Undo.RegisterCreatedObjectUndo(root, "Create Managers");

            var result = new Dictionary<string, GameObject>();

            result["GameManager"]      = CreateManager<Core.GameManager>(root,     "GameManager");
            result["ResourceManager"]  = CreateManager<Core.ResourceManager>(root, "ResourceManager");
            result["BuildingManager"]  = CreateManager<Buildings.BuildingManager>(root, "BuildingManager");
            result["SettlerManager"]   = CreateManager<Settlers.SettlerManager>(root,   "SettlerManager");
            result["TerritoryManager"] = CreateManager<Economy.TerritoryManager>(root,  "TerritoryManager");
            result["RoadManager"]      = CreateManager<Map.RoadManager>(root,           "RoadManager");

            // GameBootstrap
            var bootstrap = CreateManager<Core.GameBootstrap>(root, "GameBootstrap");
            result["GameBootstrap"] = bootstrap;

            // SpawnPoint marker
            var spawn = new GameObject("SpawnPoint");
            spawn.transform.SetParent(root.transform);
            spawn.transform.position = new Vector3(100f, 0f, 75f);
            result["SpawnPoint"] = spawn;

            return result;
        }

        private static GameObject CreateManager<T>(GameObject parent, string name)
            where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.AddComponent<T>();
            return go;
        }

        // ------------------------------------------------------------------ Camera

        private static void CreateCameraRig()
        {
            if (GameObject.Find("RTSCamera") != null) return;

            // Rig root (carries RTSCameraController)
            var rig = new GameObject("RTSCamera");
            rig.AddComponent<Camera.RTSCameraController>();
            rig.transform.position = new Vector3(100f, 0f, 75f);
            Undo.RegisterCreatedObjectUndo(rig, "Create Camera Rig");

            // Camera child (angled down)
            var camGO = new GameObject("Camera");
            camGO.transform.SetParent(rig.transform);
            camGO.transform.localPosition = new Vector3(0f, 40f, -20f);
            camGO.transform.localRotation = Quaternion.Euler(55f, 0f, 0f);
            var cam = camGO.AddComponent<UnityEngine.Camera>();
            cam.clearFlags      = CameraClearFlags.Skybox;
            cam.fieldOfView     = 60f;
            cam.nearClipPlane   = 0.3f;
            cam.farClipPlane    = 500f;
            cam.tag = "MainCamera";

            // Audio listener on camera
            camGO.AddComponent<AudioListener>();
        }

        // ------------------------------------------------------------------ UI Canvas

        private static void CreateUI()
        {
            if (GameObject.Find("UI") != null) return;

            var uiRoot = new GameObject("UI");
            Undo.RegisterCreatedObjectUndo(uiRoot, "Create UI");

            // Canvas
            var canvasGO = new GameObject("Canvas");
            canvasGO.transform.SetParent(uiRoot.transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();

            // EventSystem
            var evSys = new GameObject("EventSystem");
            evSys.transform.SetParent(uiRoot.transform);
            evSys.AddComponent<EventSystem>();
            evSys.AddComponent<StandaloneInputModule>();

            // HUD Panel
            var hud = CreatePanel(canvasGO, "HUDPanel",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -50f), new Vector2(0f, 0f));
            hud.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
            hud.AddComponent<UI.ResourceBarController>();

            // Build Menu Panel (right side, toggleable)
            var buildMenu = CreatePanel(canvasGO, "BuildMenuPanel",
                new Vector2(1f, 0f), new Vector2(1f, 1f),
                new Vector2(-240f, 50f), new Vector2(0f, -50f));
            buildMenu.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
            buildMenu.AddComponent<UI.BuildMenuController>();
            buildMenu.SetActive(false); // hidden until B pressed

            // Building Info Panel (bottom centre)
            var infoPanel = CreatePanel(canvasGO, "BuildingInfoPanel",
                new Vector2(0.3f, 0f), new Vector2(0.7f, 0f),
                new Vector2(0f, 0f), new Vector2(0f, 180f));
            infoPanel.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.15f, 0.9f);
            AddLabel(infoPanel, "BuildingName", 22, FontStyles.Bold,
                     new Vector2(0f, 1f), new Vector2(1f, 1f),
                     new Vector2(10f, -45f), new Vector2(-10f, -5f));
            AddLabel(infoPanel, "BuildingState", 16, FontStyles.Normal,
                     new Vector2(0f, 1f), new Vector2(1f, 1f),
                     new Vector2(10f, -80f), new Vector2(-10f, -50f));
            CreateButton(infoPanel, "DemolishButton", "Demolish",
                         new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                         new Vector2(-60f, 10f), new Vector2(60f, 50f),
                         new Color(0.8f, 0.2f, 0.2f));
            infoPanel.SetActive(false);

            // Pause Menu Panel
            var pause = CreatePanel(canvasGO, "PauseMenuPanel",
                new Vector2(0.3f, 0.2f), new Vector2(0.7f, 0.8f),
                Vector2.zero, Vector2.zero);
            pause.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
            AddLabel(pause, "PauseTitle", 36, FontStyles.Bold,
                     new Vector2(0f, 1f), new Vector2(1f, 1f),
                     new Vector2(0f, -60f), new Vector2(0f, -10f));
            pause.SetActive(false);

            // UIManager on canvas root
            canvasGO.AddComponent<UI.UIManager>();
        }

        private static GameObject CreatePanel(GameObject parent, string name,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.5f);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin  = anchorMin;
            rt.anchorMax  = anchorMax;
            rt.offsetMin  = offsetMin;
            rt.offsetMax  = offsetMax;
            return go;
        }

        private static void AddLabel(GameObject parent, string name, int fontSize,
            FontStyles style, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = name;
            tmp.fontSize  = fontSize;
            tmp.fontStyle = style;
            tmp.color     = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        private static void CreateButton(GameObject parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            go.AddComponent<Button>();
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            var txtGO = new GameObject("Label");
            txtGO.transform.SetParent(go.transform, false);
            var tmp = txtGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = 18;
            tmp.color     = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            var trt = txtGO.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
        }

        // ------------------------------------------------------------------ NavMesh Surface

        private static void CreateNavMeshSurface(GameObject mapRoot)
        {
            // Attach NavMeshSurface to the terrain placeholder so baking works
            var terrain = GameObject.Find("TerrainPlaceholder");
            if (terrain == null) return;

#if UNITY_AI_NAVIGATION
            if (terrain.GetComponent<Unity.AI.Navigation.NavMeshSurface>() == null)
                terrain.AddComponent<Unity.AI.Navigation.NavMeshSurface>();
#endif
        }

        // ------------------------------------------------------------------ Validate

        [MenuItem("Settlers/Validate Setup", priority = 2)]
        public static void ValidateSetup()
        {
            var issues = new List<string>();
            Check<Core.GameManager>(issues,    "GameManager");
            Check<Core.ResourceManager>(issues, "ResourceManager");
            Check<Buildings.BuildingManager>(issues, "BuildingManager");
            Check<Settlers.SettlerManager>(issues, "SettlerManager");

            var cam = GameObject.FindObjectOfType<UnityEngine.Camera>();
            if (cam == null) issues.Add("No Camera in scene");

            var ui = GameObject.Find("Canvas");
            if (ui == null) issues.Add("No UI Canvas in scene");

            if (issues.Count == 0)
                EditorUtility.DisplayDialog("Validation", "All systems present!", "OK");
            else
                EditorUtility.DisplayDialog("Validation Issues",
                    "Missing:\n• " + string.Join("\n• ", issues), "OK");
        }

        private static void Check<T>(List<string> issues, string label) where T : Component
        {
            if (GameObject.FindObjectOfType<T>() == null)
                issues.Add(label);
        }
    }
}
