using System.Collections.Generic;
using UnityEngine;
using TurnBasedCore.Core.Players;

namespace TurnBasedCore.Core.TurnSystem
{
    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance { get; private set; }

        private List<IPlayerController> turnOrder = new();
        private int currentTurnIndex = -1;

        private TurnSettingsSO settings;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void Initialize(TurnSettingsSO turnSettings, List<IPlayerController> players)
        {
            settings = turnSettings;
            turnOrder = new List<IPlayerController>(players);
            currentTurnIndex = -1;

            if (settings.EnableDebugLogs)
                Debug.Log($"[TurnManager] Initialized with {players.Count} players.");

            StartNextTurn();
        }

        public void StartNextTurn()
        {
            currentTurnIndex = (currentTurnIndex + 1) % turnOrder.Count;
            IPlayerController currentPlayer = turnOrder[currentTurnIndex];

            if (settings.EnableDebugLogs)
                Debug.Log($"[TurnManager] Starting turn for: {currentPlayer.Info.Nickname}");

            // Start player turn
            currentPlayer.StartTurn();
        }

        public void EndTurn(IPlayerController player)
        {
            if (turnOrder.Count == 0 || currentTurnIndex < 0 || currentTurnIndex >= turnOrder.Count)
            {
                Debug.LogWarning("[TurnManager] Invalid turn index or empty turn order.");
                return;
            }

            IPlayerController currentPlayer = turnOrder[currentTurnIndex];

            if (currentPlayer.Info.ActorNumber != player.Info.ActorNumber)
            {
                Debug.LogWarning($"[TurnManager] Turn ended by wrong player: {player.Info.ActorNumber}");
                return;
            }

            if (settings.EnableDebugLogs)
                Debug.Log($"[TurnManager] Turn ended: {currentPlayer.Info.Nickname}");

           // TurnTimerManager.Instance.StopTurnTimer();
            StartNextTurn();
        }


        public IPlayerController GetCurrentPlayer() => 
            turnOrder.Count > 0 ? turnOrder[currentTurnIndex] : null;

        public void ForceSetTurnOrder(List<IPlayerController> newOrder)
        {
            turnOrder = new List<IPlayerController>(newOrder);
            currentTurnIndex = -1;
        }
    }
}
