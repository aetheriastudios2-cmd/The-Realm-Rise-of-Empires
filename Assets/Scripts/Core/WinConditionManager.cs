using System.Collections.Generic;
using UnityEngine;
using SettlersClone.Buildings;
using SettlersClone.AI;

namespace SettlersClone.Core
{
    /// <summary>
    /// Checks win/loss conditions each tick.
    ///   Victory  — all EnemyAI factions are defeated
    ///   Defeat   — player HQ is destroyed
    /// </summary>
    public class WinConditionManager : MonoBehaviour
    {
        [SerializeField] private float checkInterval = 5f;

        private float timer;
        private bool  gameOver;

        private void Update()
        {
            if (gameOver) return;
            timer += Time.deltaTime;
            if (timer >= checkInterval) { timer = 0f; Check(); }
        }

        private void Check()
        {
            // Defeat: player has no HQ
            var playerHQs = BuildingManager.Instance.GetBuildingsOfType(BuildingType.Headquarters)
                .FindAll(b => b.OwnerId == 0 && b.State == BuildingState.Active);
            if (playerHQs.Count == 0)
            {
                gameOver = true;
                GameManager.Instance?.TriggerDefeat();
                return;
            }

            // Victory: no enemies left
            var enemies = FindObjectsOfType<EnemyAI>();
            if (enemies.Length == 0)
            {
                gameOver = true;
                GameManager.Instance?.TriggerVictory();
            }
        }
    }
}
