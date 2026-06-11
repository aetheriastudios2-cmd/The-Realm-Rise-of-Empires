using System.Collections;
using UnityEngine;
using SettlersClone.Buildings;
using SettlersClone.Economy;

namespace SettlersClone.Settlers
{
    // Workers walk to their assigned building and enable its production.
    public class SettlerWorker : Settler
    {
        private Building           workplace;
        private ProductionBuilding production;

        protected override void Awake()
        {
            base.Awake();
            settlerType = SettlerType.Worker;
        }

        public void AssignToBuilding(Building building)
        {
            workplace  = building;
            production = building.GetComponent<ProductionBuilding>();
            StopAllCoroutines();
            StartCoroutine(WorkLoop());
        }

        private IEnumerator WorkLoop()
        {
            // Walk to the building
            State = SettlerState.MovingTo;
            MoveTo(workplace.transform.position);
            yield return new WaitUntil(HasArrived);

            // Register as worker
            production?.AssignWorker();
            State = SettlerState.Working;

            // Stay until building is destroyed
            yield return new WaitUntil(() => workplace == null || workplace.State == BuildingState.Destroyed);

            production?.UnassignWorker();
            SettlerManager.Instance?.OnWorkerFinished(this);
            State = SettlerState.Idle;
        }

        public void Unassign()
        {
            production?.UnassignWorker();
            StopAllCoroutines();
            State = SettlerState.Idle;
        }
    }
}
