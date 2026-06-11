using System;
using System.Collections.Generic;
using UnityEngine;
using SettlersClone.Economy;

namespace SettlersClone.Economy
{
    /// <summary>
    /// Simple trade system: player can exchange resources at defined exchange rates.
    /// A Harbor building unlocks trading; the player can set up trade routes
    /// that auto-execute every trade interval.
    /// </summary>
    public class TradeSystem : MonoBehaviour
    {
        public static TradeSystem Instance { get; private set; }

        [Header("Trade rates (units of X to get 1 unit of Y)")]
        [SerializeField] private List<TradeRate> rates = new();

        [Header("Timing")]
        [SerializeField] private float tradeInterval = 60f;

        private readonly List<TradeRoute> activeRoutes = new();
        private float timer;
        private bool  harborAvailable;

        public event Action<TradeRoute> OnTradeExecuted;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            PopulateDefaultRates();
        }

        private void Update()
        {
            harborAvailable = Core.ResourceManager.Instance != null &&
                              Buildings.BuildingManager.Instance
                                  .GetBuildingsOfType(Buildings.BuildingType.Harbor)
                                  .Exists(b => b.OwnerId == 0 &&
                                               b.State == Buildings.BuildingState.Active);

            if (!harborAvailable) return;

            timer += Time.deltaTime;
            if (timer >= tradeInterval) { timer = 0f; ExecuteRoutes(); }
        }

        // ------------------------------------------------------------------ Route management

        public bool AddRoute(TradeRoute route)
        {
            if (!harborAvailable) return false;
            activeRoutes.Add(route);
            return true;
        }

        public void RemoveRoute(TradeRoute route) => activeRoutes.Remove(route);

        // ------------------------------------------------------------------ Manual trade

        public bool ExecuteTrade(ResourceType give, int giveAmt, ResourceType receive)
        {
            if (!harborAvailable) return false;
            var rm = Core.ResourceManager.Instance;
            if (rm == null) return false;

            int receiveAmt = CalculateReceive(give, giveAmt, receive);
            if (receiveAmt <= 0) return false;
            if (!rm.TrySpend(give, giveAmt)) return false;

            rm.AddResource(receive, receiveAmt);
            UI.NotificationSystem.Instance?.Notify(
                $"Traded {giveAmt} {give} → {receiveAmt} {receive}", UI.NotifyType.Success);
            return true;
        }

        public int CalculateReceive(ResourceType give, int giveAmt, ResourceType receive)
        {
            float giveValue    = GetValue(give);
            float receiveValue = GetValue(receive);
            if (receiveValue <= 0) return 0;
            return Mathf.FloorToInt(giveAmt * giveValue / receiveValue);
        }

        // ------------------------------------------------------------------ Auto routes

        private void ExecuteRoutes()
        {
            foreach (var route in activeRoutes)
            {
                if (ExecuteTrade(route.give, route.giveAmount, route.receive))
                    OnTradeExecuted?.Invoke(route);
            }
        }

        // ------------------------------------------------------------------ Rates

        private float GetValue(ResourceType type)
        {
            foreach (var r in rates) if (r.type == type) return r.value;
            return 1f;
        }

        private void PopulateDefaultRates()
        {
            if (rates.Count > 0) return;
            rates.Add(new TradeRate { type = ResourceType.Log,    value = 1f  });
            rates.Add(new TradeRate { type = ResourceType.Plank,  value = 2f  });
            rates.Add(new TradeRate { type = ResourceType.Stone,  value = 1.5f});
            rates.Add(new TradeRate { type = ResourceType.Coal,   value = 2f  });
            rates.Add(new TradeRate { type = ResourceType.IronOre,value = 2f  });
            rates.Add(new TradeRate { type = ResourceType.Iron,   value = 4f  });
            rates.Add(new TradeRate { type = ResourceType.Gold,   value = 8f  });
            rates.Add(new TradeRate { type = ResourceType.Bread,  value = 3f  });
            rates.Add(new TradeRate { type = ResourceType.Tools,  value = 5f  });
            rates.Add(new TradeRate { type = ResourceType.Sword,  value = 6f  });
            rates.Add(new TradeRate { type = ResourceType.Coin,   value = 10f });
        }
    }

    [Serializable] public class TradeRate  { public ResourceType type; public float value; }
    [Serializable] public class TradeRoute { public ResourceType give; public int giveAmount; public ResourceType receive; }
}
