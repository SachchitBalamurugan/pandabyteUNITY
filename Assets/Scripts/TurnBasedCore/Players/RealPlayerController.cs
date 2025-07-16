using UnityEngine;
using TurnBasedCore.Core.TurnSystem;
using TurnBasedCore.Core.Actions;

namespace TurnBasedCore.Core.Players
{
    public class RealPlayerController : MonoBehaviour, IPlayerController
    {
        public PlayerInfo Info { get; private set; }
        public bool IsFolded => _isFolded;

        private bool _isFolded = false;
        private IActionCommand queuedCommand;

        public void Initialize(PlayerInfo info)
        {
            Info = info;
            _isFolded = false;
        }

        public void StartTurn()
        {
            if (IsFolded)
            {
                Debug.Log($"[Player] Skipping turn (folded): {Info.Nickname}");
                EndTurn();
                return;
            }

            if (Info.IsLocal)
            {
                Debug.Log($"[Player] Turn started: {Info.Nickname}");
                // TODO: Show UI panel here if needed
                // UIManager.Instance.ShowActionPanel(this);
            }
        }

        public void SetAction(IActionCommand command)
        {
            queuedCommand = command;
        }

        public void PerformAction()
        {
            Debug.Log($"[Player] {Info.Nickname} performed action.");

            //if (queuedCommand == null)
            //    queuedCommand = new FoldCommand(this);

            //if (queuedCommand is FoldCommand)
            //    _isFolded = true;

            ActionInvoker.Instance.Execute(queuedCommand);
            EndTurn();
        }

        public void PerformAutoAction()
        {
            if (IsFolded)
            {
                Debug.Log($"[Player] Skipping auto-action (folded): {Info.Nickname}");
                EndTurn();
                return;
            }

            if (Info.IsLocal && Info.AllowAutoAction)
            {
                Debug.Log($"[Player] {Info.Nickname} auto-acted (timeout).");
                PerformAction();
            }
        }

        public void EndTurn()
        {
            if (Info.IsLocal)
                TurnManager.Instance.EndTurn(this);
        }
    }
}
