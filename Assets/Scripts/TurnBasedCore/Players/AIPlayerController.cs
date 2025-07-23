using System.Collections;
using UnityEngine;
using TurnBasedCore.Core.TurnSystem;
using TurnBasedCore.Core.Actions;


namespace TurnBasedCore.Core.Players
{
    public class AIPlayerController : MonoBehaviour, IPlayerController
    {
        public PlayerInfo Info { get; private set; }
        

        
        private IActionCommand queuedCommand;

        public void Initialize(PlayerInfo info)
        {
            Info = info;
            
            queuedCommand = null;
        }

        public void StartTurn()
        {
           

            Debug.Log($"[AI] Turn started: {Info.Nickname}");
            StartCoroutine(PerformActionWithDelay());
        }

        private IEnumerator PerformActionWithDelay()
        {
            yield return new WaitForSeconds(Random.Range(1f, 2.5f));  // Simulate thinking

            //IActionCommand command = Random.value > 0.5f
            //    ? (IActionCommand)new FoldCommand(this)
            //    : new CheckCommand(this);

           // SetAction(command);
            PerformAction();
        }


        public void SetAction(IActionCommand command)
        {
            queuedCommand = command;
        }

        public void PerformAction()
        {
            if (queuedCommand == null)
            {
                Debug.LogWarning($"[AI] No action selected for {Info.Nickname}, defaulting to Fold.");
               // queuedCommand = new AttackCommand(this);
            }

          

            Debug.Log($"[AI] {Info.Nickname} performed action: {queuedCommand.GetType().Name}");
            ActionInvoker.Instance.Execute(queuedCommand);
            EndTurn();
        }

        public void PerformAutoAction()
        {
           

            Debug.Log($"[AI] {Info.Nickname} auto-acted (timeout).");
            PerformAction();
        }

        public void EndTurn()
        {
            TurnManager.Instance.EndTurn(this);
        }
    }
}
