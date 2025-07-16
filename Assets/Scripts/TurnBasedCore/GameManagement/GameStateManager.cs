using UnityEngine;

namespace TurnBasedCore.Core.GameManagement
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public GamePhase CurrentPhase { get; private set; } = GamePhase.None;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void SetPhase(GamePhase newPhase)
        {
            if (CurrentPhase == newPhase)
                return;

            CurrentPhase = newPhase;
            Debug.Log($"[GameStateManager] Phase changed to: {newPhase}");

            switch (newPhase)
            {
                case GamePhase.Setup:
                    HandleSetup();
                    break;

                case GamePhase.InProgress:
                    HandleInProgress();
                    break;

                case GamePhase.End:
                    HandleEnd();
                    break;
            }
        }

        private void HandleSetup()
        {
            Debug.Log("[GameStateManager] Handling setup phase...");
            // Initialize players, shuffle, assign data etc.
        }

        private void HandleInProgress()
        {
            Debug.Log("[GameStateManager] Game started...");
            // Start turn manager etc.
        }

        private void HandleEnd()
        {
            Debug.Log("[GameStateManager] Game ended.");
            // Show result UI, disconnect, cleanup
        }
    }
}
