using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SettlersClone.Buildings;
using SettlersClone.Core;
using SettlersClone.Economy;

namespace SettlersClone.UI
{
    // Side panel for selecting which building to place
    public class BuildMenuController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform  buttonContainer;
        [SerializeField] private GameObject buildButtonPrefab;
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI tooltipName;
        [SerializeField] private TextMeshProUGUI tooltipDesc;
        [SerializeField] private TextMeshProUGUI tooltipCost;

        [Header("Catalogue")]
        [SerializeField] private List<BuildingData> catalogue = new();

        [Header("Categories")]
        [SerializeField] private List<BuildingCategory> categories = new();

        [System.Serializable]
        private struct BuildingCategory
        {
            public string           categoryName;
            public List<BuildingData> buildings;
        }

        private void Start()
        {
            PopulateButtons();
            if (tooltipPanel) tooltipPanel.SetActive(false);
        }

        private void PopulateButtons()
        {
            foreach (var data in catalogue)
            {
                var go  = Instantiate(buildButtonPrefab, buttonContainer);
                var btn = go.GetComponent<Button>();
                var img = go.GetComponentInChildren<Image>();
                var lbl = go.GetComponentInChildren<TextMeshProUGUI>();

                if (img != null && data.icon != null) img.sprite = data.icon;
                if (lbl != null) lbl.text = data.displayName;

                var capturedData = data;
                btn.onClick.AddListener(() => OnBuildingSelected(capturedData));

                var trigger = go.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                AddPointerEnterEvent(trigger, () => ShowTooltip(capturedData));
                AddPointerExitEvent(trigger, HideTooltip);
            }
        }

        private void OnBuildingSelected(BuildingData data)
        {
            if (BuildingManager.Instance == null) return;
            if (!ResourceManager.Instance.CanAfford(data.constructionCost))
            {
                Debug.Log($"Cannot afford {data.displayName}");
                return;
            }
            BuildingManager.Instance.BeginPlacement(data);
        }

        private void ShowTooltip(BuildingData data)
        {
            if (tooltipPanel == null) return;
            tooltipPanel.SetActive(true);
            if (tooltipName) tooltipName.text = data.displayName;
            if (tooltipDesc) tooltipDesc.text = data.description;
            if (tooltipCost) tooltipCost.text = FormatCost(data.constructionCost);
        }

        private void HideTooltip()
        {
            if (tooltipPanel) tooltipPanel.SetActive(false);
        }

        private string FormatCost(List<ResourceAmount> costs)
        {
            if (costs == null || costs.Count == 0) return "Free";
            var sb = new System.Text.StringBuilder("Cost: ");
            for (int i = 0; i < costs.Count; i++)
            {
                sb.Append($"{costs[i].amount} {costs[i].type}");
                if (i < costs.Count - 1) sb.Append(", ");
            }
            return sb.ToString();
        }

        private static void AddPointerEnterEvent(
            UnityEngine.EventSystems.EventTrigger trigger, UnityEngine.Events.UnityAction action)
        {
            var entry = new UnityEngine.EventSystems.EventTrigger.Entry
                { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
            entry.callback.AddListener(_ => action());
            trigger.triggers.Add(entry);
        }

        private static void AddPointerExitEvent(
            UnityEngine.EventSystems.EventTrigger trigger, UnityEngine.Events.UnityAction action)
        {
            var entry = new UnityEngine.EventSystems.EventTrigger.Entry
                { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
            entry.callback.AddListener(_ => action());
            trigger.triggers.Add(entry);
        }
    }
}
