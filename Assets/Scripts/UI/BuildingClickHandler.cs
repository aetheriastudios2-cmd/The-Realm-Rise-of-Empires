using UnityEngine;
using SettlersClone.Buildings;

namespace SettlersClone.UI
{
    // Attach to the building's root GameObject so clicks open the info panel
    public class BuildingClickHandler : MonoBehaviour
    {
        private Building building;

        private void Awake() => building = GetComponent<Building>();

        private void OnMouseUpAsButton()
        {
            if (UIManager.Instance == null) return;
            UIManager.Instance.SelectBuilding(building);
        }
    }
}
