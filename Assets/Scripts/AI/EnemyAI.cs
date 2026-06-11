using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SettlersClone.Map;
using SettlersClone.Buildings;
using SettlersClone.Settlers;

namespace SettlersClone.AI
{
    public enum EnemyState { Idle, Expanding, Raiding, Defending }

    /// <summary>
    /// Drives a CPU-controlled enemy faction.
    /// Spawns its own buildings, trains soldiers, and periodically raids
    /// the player's territory.
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private int    playerId   = 1;
        [SerializeField] private string factionName = "Dark Empire";

        [Header("Spawn")]
        [SerializeField] private HexCell startCell;
        [SerializeField] private BuildingData hqData;
        [SerializeField] private BuildingData watchtowerData;
        [SerializeField] private BuildingData barracksData;
        [SerializeField] private BuildingData woodcutterData;

        [Header("Difficulty")]
        [SerializeField] private float expandInterval  = 60f;
        [SerializeField] private float raidInterval    = 120f;
        [SerializeField] private int   maxSoldiers     = 20;
        [SerializeField] private float raidStrength    = 0.5f; // fraction of soldiers sent

        [Header("Soldier Prefab")]
        [SerializeField] private SettlerSoldier soldierPrefab;

        private HexGrid           grid;
        private EnemyState        state       = EnemyState.Idle;
        private Building          hq;
        private List<Building>    ownBuildings = new();
        private List<SettlerSoldier> ownSoldiers = new();

        public event Action<EnemyAI> OnDefeated;

        private void Start()
        {
            grid = FindObjectOfType<HexGrid>();
            StartCoroutine(Bootstrap());
        }

        private IEnumerator Bootstrap()
        {
            yield return new WaitForSeconds(3f);
            PlaceInitialBuildings();
            StartCoroutine(ExpandLoop());
            StartCoroutine(RaidLoop());
            StartCoroutine(TrainLoop());
        }

        // ------------------------------------------------------------------ Building placement

        private void PlaceInitialBuildings()
        {
            if (startCell == null || hqData == null) return;

            hq = BuildingManager.Instance.SpawnBuilding(hqData, startCell, playerId);
            if (hq != null) ownBuildings.Add(hq);

            // Place a couple of woodcutters nearby
            foreach (var cell in grid.GetCellsInRadius(startCell.Coordinates, 2))
            {
                if (cell.IsBuildable() && woodcutterData != null)
                {
                    var b = BuildingManager.Instance.SpawnBuilding(woodcutterData, cell, playerId);
                    if (b != null) { ownBuildings.Add(b); break; }
                }
            }
        }

        private IEnumerator ExpandLoop()
        {
            while (hq != null)
            {
                yield return new WaitForSeconds(expandInterval);
                state = EnemyState.Expanding;
                TryExpand();
                state = EnemyState.Idle;
            }
        }

        private void TryExpand()
        {
            if (hq == null) return;

            // Place a watchtower to claim more territory
            var candidates = grid.GetCellsInRadius(hq.Location.Coordinates, 8);
            foreach (var cell in candidates)
            {
                if (cell.IsBuildable() && cell.OwnerId == -1 && watchtowerData != null)
                {
                    var t = BuildingManager.Instance.SpawnBuilding(watchtowerData, cell, playerId);
                    if (t != null) { ownBuildings.Add(t); break; }
                }
            }

            // Place a barracks if we don't have one
            bool hasBarracks = ownBuildings.Exists(b => b.Type == BuildingType.Barracks);
            if (!hasBarracks && barracksData != null)
            {
                foreach (var cell in grid.GetCellsInRadius(hq.Location.Coordinates, 3))
                {
                    if (cell.IsBuildable())
                    {
                        var b = BuildingManager.Instance.SpawnBuilding(barracksData, cell, playerId);
                        if (b != null) { ownBuildings.Add(b); break; }
                    }
                }
            }
        }

        // ------------------------------------------------------------------ Training

        private IEnumerator TrainLoop()
        {
            while (hq != null)
            {
                yield return new WaitForSeconds(30f);
                if (ownSoldiers.Count < maxSoldiers && soldierPrefab != null)
                {
                    var s = Instantiate(soldierPrefab,
                        hq.transform.position + UnityEngine.Random.insideUnitSphere * 3f,
                        Quaternion.identity);
                    s.Garrison(hq);
                    ownSoldiers.Add(s);
                }
            }
        }

        // ------------------------------------------------------------------ Raiding

        private IEnumerator RaidLoop()
        {
            yield return new WaitForSeconds(raidInterval * 0.5f); // stagger first raid
            while (hq != null)
            {
                yield return new WaitForSeconds(raidInterval);
                if (ownSoldiers.Count > 3)
                {
                    state = EnemyState.Raiding;
                    StartCoroutine(ExecuteRaid());
                }
            }
        }

        private IEnumerator ExecuteRaid()
        {
            int sendCount = Mathf.Max(1, Mathf.RoundToInt(ownSoldiers.Count * raidStrength));
            var raiders   = new List<SettlerSoldier>(ownSoldiers.GetRange(0, sendCount));

            // Find a player building to attack
            var targets = BuildingManager.Instance.GetAllBuildings()
                .FindAll(b => b.OwnerId == 0 && b.State == BuildingState.Active);

            if (targets.Count == 0) { state = EnemyState.Idle; yield break; }

            var target = targets[UnityEngine.Random.Range(0, targets.Count)];
            foreach (var raider in raiders)
                raider.transform.position =
                    Vector3.MoveTowards(raider.transform.position,
                                        target.transform.position,
                                        0.1f);

            // Simple: march toward target for 30 s then return
            float elapsed = 0f;
            while (elapsed < 30f && target != null && target.State != BuildingState.Destroyed)
            {
                foreach (var r in raiders)
                    if (r != null)
                        r.transform.position = Vector3.MoveTowards(
                            r.transform.position, target.transform.position,
                            Time.deltaTime * 4f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Return survivors
            foreach (var r in raiders)
                if (r != null && hq != null)
                    r.Garrison(hq);

            state = EnemyState.Idle;
        }

        // ------------------------------------------------------------------ Defeat

        private void Update()
        {
            if (hq != null && hq.State == BuildingState.Destroyed)
                HandleDefeated();
        }

        private void HandleDefeated()
        {
            foreach (var s in ownSoldiers)
                if (s != null) Destroy(s.gameObject);
            ownSoldiers.Clear();
            OnDefeated?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
