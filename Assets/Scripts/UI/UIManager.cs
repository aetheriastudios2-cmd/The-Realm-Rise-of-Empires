using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SettlersClone.Core;
using SettlersClone.Buildings;

namespace SettlersClone.UI
{
    // Top-level UI orchestrator: routes game state to the correct panels
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject buildMenuPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private GameObject buildingInfoPanel;

        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI gameTimeLabel;
        [SerializeField] private TextMeshProUGUI gameSpeedLabel;
        [SerializeField] private Button          pauseButton;
        [SerializeField] private Button          buildMenuToggle;

        [Header("Building Info")]
        [SerializeField] private TextMeshProUGUI buildingNameLabel;
        [SerializeField] private TextMeshProUGUI buildingStateLabel;
        [SerializeField] private Button          demolishButton;

        private Building selectedBuilding;
        private bool     buildMenuOpen;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void Start()
        {
            pauseButton?.onClick.AddListener(OnPauseClicked);
            buildMenuToggle?.onClick.AddListener(ToggleBuildMenu);
            demolishButton?.onClick.AddListener(OnDemolishClicked);
            SetBuildMenuOpen(false);
            SetBuildingInfoVisible(false);
        }

        private void Update()
        {
            if (GameManager.Instance == null) return;
            if (gameTimeLabel != null)
            {
                float t = GameManager.Instance.GameTime;
                gameTimeLabel.text = $"{(int)(t / 60):00}:{(int)(t % 60):00}";
            }

            if (Input.GetKeyDown(KeyCode.B)) ToggleBuildMenu();
        }

        // --- Game state ---

        private void HandleGameStateChanged(GameState state)
        {
            hudPanel?.SetActive(state == GameState.Playing || state == GameState.Paused);
            pauseMenuPanel?.SetActive(state == GameState.Paused);
            victoryPanel?.SetActive(state == GameState.Victory);
            defeatPanel?.SetActive(state == GameState.Defeat);
        }

        // --- Pause ---

        private void OnPauseClicked() => GameManager.Instance?.TogglePause();

        // --- Build menu ---

        private void ToggleBuildMenu() => SetBuildMenuOpen(!buildMenuOpen);

        private void SetBuildMenuOpen(bool open)
        {
            buildMenuOpen = open;
            buildMenuPanel?.SetActive(open);
            if (!open) BuildingManager.Instance?.CancelPlacement();
        }

        // --- Building info panel ---

        public void SelectBuilding(Building building)
        {
            selectedBuilding = building;
            SetBuildingInfoVisible(building != null);
            if (building == null) return;
            if (buildingNameLabel)  buildingNameLabel.text  = building.Data.displayName;
            if (buildingStateLabel) buildingStateLabel.text = building.State.ToString();
        }

        private void SetBuildingInfoVisible(bool visible)
        {
            buildingInfoPanel?.SetActive(visible);
        }

        private void OnDemolishClicked()
        {
            selectedBuilding?.Demolish();
            SelectBuilding(null);
        }

        // --- Speed controls ---

        public void SetSpeed(float speed)
        {
            GameManager.Instance?.SetGameSpeed(speed);
            if (gameSpeedLabel) gameSpeedLabel.text = $"x{speed:0.0}";
        }
    }
}
