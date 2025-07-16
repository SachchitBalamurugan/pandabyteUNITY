using System.Collections;
using UnityEngine;
using TurnBasedCore.Core.Players;

namespace TurnBasedCore.Core.TurnSystem
{
    public class TurnTimerManager : MonoBehaviour
    {
        public static TurnTimerManager Instance { get; private set; }

        private Coroutine currentTimer;
        private float turnDuration;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void StartTurnTimer(IPlayerController player, float duration)
        {
            StopTurnTimer(); // Ensure previous timer is stopped
            turnDuration = duration;
            currentTimer = StartCoroutine(TimerRoutine(player));
        }

        public void StopTurnTimer()
        {
            if (currentTimer != null)
            {
                StopCoroutine(currentTimer);
                currentTimer = null;
            }
        }

        private IEnumerator TimerRoutine(IPlayerController player)
        {
            float remaining = turnDuration;

            while (remaining > 0f)
            {
                yield return new WaitForSeconds(1f);
                remaining--;

                // TODO: Update UI here if needed
                Debug.Log($"[Timer] {player.Info.Nickname} has {remaining} seconds left.");
            }

            Debug.Log($"[Timer] {player.Info.Nickname}'s time is up!");

            if (player.Info.AllowAutoAction)
                player.PerformAutoAction();
            else
                Debug.LogWarning($"[Timer] AutoAction is disabled for {player.Info.Nickname}");
        }
    }
}
