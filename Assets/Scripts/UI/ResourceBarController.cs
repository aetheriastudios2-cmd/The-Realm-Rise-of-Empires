using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SettlersClone.Core;
using SettlersClone.Economy;

namespace SettlersClone.UI
{
    // Displays resource counts in a top-bar HUD
    public class ResourceBarController : MonoBehaviour
    {
        [System.Serializable]
        private struct ResourceEntry
        {
            public ResourceType type;
            public Image        icon;
            public TextMeshProUGUI label;
        }

        [SerializeField] private List<ResourceEntry> entries = new();

        private void OnEnable()
        {
            if (ResourceManager.Instance != null)
                ResourceManager.Instance.OnResourceChanged += RefreshEntry;
        }

        private void OnDisable()
        {
            if (ResourceManager.Instance != null)
                ResourceManager.Instance.OnResourceChanged -= RefreshEntry;
        }

        private void Start() => RefreshAll();

        private void RefreshAll()
        {
            foreach (var e in entries)
                UpdateLabel(e);
        }

        private void RefreshEntry(ResourceType type, int amount)
        {
            foreach (var e in entries)
            {
                if (e.type == type) UpdateLabel(e);
            }
        }

        private void UpdateLabel(ResourceEntry e)
        {
            if (e.label == null) return;
            int qty = ResourceManager.Instance != null ? ResourceManager.Instance.GetAmount(e.type) : 0;
            e.label.text = qty.ToString();
        }
    }
}
