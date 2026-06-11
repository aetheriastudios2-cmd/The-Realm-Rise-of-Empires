using UnityEngine;
using SettlersClone.Economy;

namespace SettlersClone.Buildings
{
    // Drives the production timer for buildings that have a recipe.
    // Workers must be assigned before production starts.
    [RequireComponent(typeof(Building))]
    public class ProductionBuilding : MonoBehaviour
    {
        private Building         building;
        private ProductionRecipe recipe;
        private float            timer;
        private int              assignedWorkers;

        public bool IsProducing     { get; private set; }
        public int  AssignedWorkers => assignedWorkers;

        private void Awake()
        {
            building = GetComponent<Building>();
        }

        private void Start()
        {
            recipe = building.Data?.recipe;
        }

        private void Update()
        {
            if (recipe == null) return;
            if (building.State != BuildingState.Active) return;
            if (assignedWorkers <= 0) { IsProducing = false; return; }
            if (building.IsOutputFull(recipe.output.type)) { IsProducing = false; return; }
            if (!recipe.HasInputs(building.LocalStock)) { IsProducing = false; return; }

            IsProducing = true;
            timer += Time.deltaTime;

            if (timer >= recipe.cycleTime)
            {
                timer = 0f;
                RunProductionCycle();
            }
        }

        private void RunProductionCycle()
        {
            foreach (var input in recipe.inputs)
                building.ConsumeInput(input.type, input.amount);

            building.ProduceOutput(recipe.output.type, recipe.output.amount);
        }

        public void AssignWorker()   => assignedWorkers = Mathf.Min(assignedWorkers + 1, building.Data.maxWorkers);
        public void UnassignWorker() => assignedWorkers = Mathf.Max(0, assignedWorkers - 1);
    }
}
