using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SettlersClone.Economy;
using SettlersClone.Buildings;
using SettlersClone.Map;

namespace SettlersClone.Core
{
    /// <summary>
    /// Serialises/deserialises the full game state to a JSON save file.
    /// Press F5 to quick-save, F9 to quick-load.
    /// </summary>
    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveLoadManager Instance { get; private set; }

        [SerializeField] private HexGrid        grid;
        [SerializeField] private BuildingManager buildingManager;
        [SerializeField] private ResourceManager resourceManager;

        private const string SaveFolder   = "Saves";
        private const string QuickSaveFile = "quicksave.json";

        public event Action OnSaved;
        public event Action OnLoaded;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5)) QuickSave();
            if (Input.GetKeyDown(KeyCode.F9)) QuickLoad();
        }

        // ------------------------------------------------------------------ Public API

        public void QuickSave()  => SaveGame(QuickSaveFile);
        public void QuickLoad()  => LoadGame(QuickSaveFile);

        public void SaveGame(string filename)
        {
            var state = CaptureState();
            string json = JsonUtility.ToJson(state, prettyPrint: true);
            string path = GetSavePath(filename);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, json);
            Debug.Log($"[SaveLoad] Saved to {path}");
            OnSaved?.Invoke();
        }

        public bool LoadGame(string filename)
        {
            string path = GetSavePath(filename);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[SaveLoad] No save file at {path}");
                return false;
            }
            string json  = File.ReadAllText(path);
            var    state = JsonUtility.FromJson<GameSaveState>(json);
            ApplyState(state);
            Debug.Log($"[SaveLoad] Loaded from {path}");
            OnLoaded?.Invoke();
            return true;
        }

        public bool HasSave(string filename) => File.Exists(GetSavePath(filename));

        private static string GetSavePath(string filename) =>
            Path.Combine(Application.persistentDataPath, SaveFolder, filename);

        // ------------------------------------------------------------------ Capture

        private GameSaveState CaptureState()
        {
            var state = new GameSaveState
            {
                gameTime   = GameManager.Instance?.GameTime ?? 0f,
                savedAt    = DateTime.UtcNow.ToString("O"),
                resources  = new List<ResourceSaveEntry>(),
                buildings  = new List<BuildingSaveEntry>()
            };

            // Resources
            if (resourceManager != null)
                foreach (var kvp in resourceManager.GlobalStock)
                    state.resources.Add(new ResourceSaveEntry
                        { type = (int)kvp.Key, amount = kvp.Value });

            // Buildings
            foreach (var b in buildingManager.GetAllBuildings())
            {
                if (b.Location == null) continue;
                state.buildings.Add(new BuildingSaveEntry
                {
                    buildingType = (int)b.Type,
                    hexX         = b.Location.Coordinates.X,
                    hexZ         = b.Location.Coordinates.Z,
                    ownerId      = b.OwnerId,
                    state        = (int)b.State
                });
            }

            return state;
        }

        // ------------------------------------------------------------------ Apply

        private void ApplyState(GameSaveState state)
        {
            // Demolish all current buildings
            foreach (var b in new List<Building>(buildingManager.GetAllBuildings()))
                b.Demolish();

            // Restore resources
            foreach (var entry in state.resources)
                resourceManager.AddResource((ResourceType)entry.type, entry.amount);

            // Restore buildings
            foreach (var entry in state.buildings)
            {
                var coords = new HexCoordinates(entry.hexX, entry.hexZ);
                var cell   = grid?.GetCell(coords);
                if (cell == null) continue;

                var data = buildingManager.GetData((BuildingType)entry.buildingType);
                if (data == null) continue;

                buildingManager.SpawnBuilding(data, cell, entry.ownerId);
            }
        }
    }

    // ------------------------------------------------------------------ Data classes

    [Serializable]
    public class GameSaveState
    {
        public float                  gameTime;
        public string                 savedAt;
        public List<ResourceSaveEntry> resources;
        public List<BuildingSaveEntry> buildings;
    }

    [Serializable]
    public class ResourceSaveEntry
    {
        public int type;
        public int amount;
    }

    [Serializable]
    public class BuildingSaveEntry
    {
        public int buildingType;
        public int hexX, hexZ;
        public int ownerId;
        public int state;
    }
}
