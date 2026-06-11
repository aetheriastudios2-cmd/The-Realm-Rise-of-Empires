using System.Collections;
using UnityEngine;
using SettlersClone.Economy;
using SettlersClone.Buildings;
using SettlersClone.Map;

namespace SettlersClone.Settlers
{
    // A carrier walks between a source building and a destination storage/building
    // carrying one resource at a time — the core loop of The Settlers.
    public class SettlerCarrier : Settler
    {
        private Building      sourceBuilding;
        private Building      destinationBuilding;
        private ResourceType  carryType  = ResourceType.None;
        private int           carryQty;
        private HexGrid       grid;

        [SerializeField] private GameObject carryVisualPrefab;
        private GameObject carryVisualInstance;

        public bool IsCarrying => carryType != ResourceType.None && carryQty > 0;

        protected override void Awake()
        {
            base.Awake();
            settlerType = SettlerType.Carrier;
            grid = FindObjectOfType<HexGrid>();
        }

        // Called by SettlerManager to assign a delivery task
        public void AssignDelivery(Building from, Building to, ResourceType type)
        {
            sourceBuilding      = from;
            destinationBuilding = to;
            carryType           = type;
            carryQty            = 0;
            StopAllCoroutines();
            StartCoroutine(DeliveryLoop());
        }

        private IEnumerator DeliveryLoop()
        {
            // 1. Walk to source
            State = SettlerState.MovingTo;
            MoveTo(sourceBuilding.transform.position);
            yield return new WaitUntil(HasArrived);

            // 2. Collect resource
            carryQty = sourceBuilding.CollectOutput(carryType, 1);
            if (carryQty <= 0) { BeIdle(); yield break; }
            ShowCarryVisual(true);

            // 3. Walk to destination
            MoveTo(destinationBuilding.transform.position);
            yield return new WaitUntil(HasArrived);

            // 4. Deposit
            if (destinationBuilding.Data.isStorage)
                Core.ResourceManager.Instance.AddResource(carryType, carryQty);
            else
                destinationBuilding.AddInput(carryType, carryQty);

            carryType = ResourceType.None;
            carryQty  = 0;
            ShowCarryVisual(false);

            // 5. Return to idle / notify manager
            SettlerManager.Instance?.OnCarrierFinished(this);
            BeIdle();
        }

        private void BeIdle()
        {
            State = SettlerState.Idle;
            sourceBuilding = destinationBuilding = null;
        }

        private void ShowCarryVisual(bool show)
        {
            if (carryVisualPrefab == null) return;
            if (show && carryVisualInstance == null)
            {
                carryVisualInstance = Instantiate(carryVisualPrefab,
                    transform.position + Vector3.up * 1.5f, Quaternion.identity, transform);
            }
            else if (!show && carryVisualInstance != null)
            {
                Destroy(carryVisualInstance);
                carryVisualInstance = null;
            }
        }
    }
}
