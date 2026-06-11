using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SettlersClone.Buildings;
using SettlersClone.Economy;

namespace SettlersClone.Settlers
{
    // Orchestrates all settler units: spawning, task assignment, recycling.
    public class SettlerManager : MonoBehaviour
    {
        public static SettlerManager Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private SettlerCarrier carrierPrefab;
        [SerializeField] private SettlerWorker  workerPrefab;
        [SerializeField] private SettlerSoldier soldierPrefab;

        [Header("Spawn")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private int       initialCarriers = 8;
        [SerializeField] private int       initialWorkers  = 4;

        private readonly List<SettlerCarrier> idleCarriers = new();
        private readonly List<SettlerWorker>  idleWorkers  = new();
        private readonly List<SettlerSoldier> soldiers     = new();

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1f); // let map generate first
            SpawnInitialSettlers();
            BuildingManager.Instance.OnBuildingPlaced  += HandleBuildingPlaced;
            BuildingManager.Instance.OnBuildingRemoved += HandleBuildingRemoved;
            StartCoroutine(TaskAssignmentLoop());
        }

        private void SpawnInitialSettlers()
        {
            Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            for (int i = 0; i < initialCarriers; i++) SpawnCarrier(pos);
            for (int i = 0; i < initialWorkers;  i++) SpawnWorker(pos);
        }

        // --- Spawn methods ---

        public SettlerCarrier SpawnCarrier(Vector3 position)
        {
            var c = Instantiate(carrierPrefab, position, Quaternion.identity);
            idleCarriers.Add(c);
            return c;
        }

        public SettlerWorker SpawnWorker(Vector3 position)
        {
            var w = Instantiate(workerPrefab, position, Quaternion.identity);
            idleWorkers.Add(w);
            return w;
        }

        public SettlerSoldier SpawnSoldier(Vector3 position)
        {
            var s = Instantiate(soldierPrefab, position, Quaternion.identity);
            soldiers.Add(s);
            return s;
        }

        // --- Task assignment loop (runs every 2 seconds) ---

        private IEnumerator TaskAssignmentLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(2f);
                AssignWorkerTasks();
                AssignCarrierTasks();
            }
        }

        private void AssignWorkerTasks()
        {
            if (idleWorkers.Count == 0) return;
            var allBuildings = BuildingManager.Instance.GetAllBuildings();
            foreach (var building in allBuildings)
            {
                if (building.State != BuildingState.Active) continue;
                var prod = building.GetComponent<ProductionBuilding>();
                if (prod == null || prod.AssignedWorkers >= building.Data.maxWorkers) continue;
                if (idleWorkers.Count == 0) break;
                var worker = idleWorkers[0];
                idleWorkers.RemoveAt(0);
                worker.AssignToBuilding(building);
            }
        }

        private void AssignCarrierTasks()
        {
            if (idleCarriers.Count == 0) return;

            // Find storage buildings to act as destinations
            var storages = BuildingManager.Instance.GetAllBuildings()
                .FindAll(b => b.Data.isStorage && b.State == BuildingState.Active);
            if (storages.Count == 0) return;

            var allBuildings = BuildingManager.Instance.GetAllBuildings();
            foreach (var source in allBuildings)
            {
                if (idleCarriers.Count == 0) break;
                if (source.Data.isStorage) continue;
                if (source.State != BuildingState.Active) continue;
                if (source.Data.recipe == null) continue;

                var outType = source.Data.recipe.output.type;
                if (!source.HasOutput(outType)) continue;

                var destination = NearestStorage(source, storages);
                if (destination == null) continue;

                var carrier = idleCarriers[0];
                idleCarriers.RemoveAt(0);
                carrier.AssignDelivery(source, destination, outType);
            }
        }

        private Building NearestStorage(Building from, List<Building> storages)
        {
            Building best = null; float bestDist = float.MaxValue;
            foreach (var s in storages)
            {
                float d = Vector3.Distance(from.transform.position, s.transform.position);
                if (d < bestDist) { bestDist = d; best = s; }
            }
            return best;
        }

        // --- Callbacks ---

        public void OnCarrierFinished(SettlerCarrier carrier) => idleCarriers.Add(carrier);
        public void OnWorkerFinished(SettlerWorker worker)    => idleWorkers.Add(worker);
        public void OnSoldierKilled(SettlerSoldier soldier)   => soldiers.Remove(soldier);

        private void HandleBuildingPlaced(Building b)
        {
            // New building needs workers — will be picked up on next loop tick
        }

        private void HandleBuildingRemoved(Building b)
        {
            // Workers assigned to destroyed building self-release via their coroutine
        }
    }
}
