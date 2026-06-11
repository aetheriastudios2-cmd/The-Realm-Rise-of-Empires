using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SettlersClone.Core
{
    public enum GameState { Loading, Playing, Paused, Victory, Defeat }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private ResourceManager resourceManager;

        [Header("Session Settings")]
        [SerializeField] private int startingSeed = 42;

        public GameState State    { get; private set; } = GameState.Loading;
        public float     GameTime { get; private set; }
        public int       LocalPlayerId => 0;

        public event Action<GameState> OnGameStateChanged;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            resourceManager.InitialiseStartingResources();
            SetState(GameState.Playing);
        }

        private void Update()
        {
            if (State == GameState.Playing)
                GameTime += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Escape))
                TogglePause();
        }

        public void TogglePause()
        {
            if (State == GameState.Playing)
            {
                Time.timeScale = 0f;
                SetState(GameState.Paused);
            }
            else if (State == GameState.Paused)
            {
                Time.timeScale = 1f;
                SetState(GameState.Playing);
            }
        }

        public void SetGameSpeed(float speed) =>
            Time.timeScale = State == GameState.Playing ? Mathf.Clamp(speed, 0.25f, 4f) : 0f;

        public void TriggerVictory()
        {
            Time.timeScale = 0f;
            SetState(GameState.Victory);
        }

        public void TriggerDefeat()
        {
            Time.timeScale = 0f;
            SetState(GameState.Defeat);
        }

        public void RestartGame() =>
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        private void SetState(GameState newState)
        {
            State = newState;
            OnGameStateChanged?.Invoke(State);
        }
    }
}
