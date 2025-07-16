using System.Collections.Generic;
using UnityEngine;

namespace TurnBasedCore.Core.Actions
{
    public class ActionInvoker : MonoBehaviour
    {
        public static ActionInvoker Instance { get; private set; }

        private readonly Stack<IActionCommand> _commandHistory = new();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void Execute(IActionCommand command)
        {
            command.Execute();
            _commandHistory.Push(command);
        }

        public void UndoLast()
        {
            if (_commandHistory.Count > 0)
            {
                var lastCommand = _commandHistory.Pop();
                lastCommand.Undo();
            }
        }

        public void ClearHistory()
        {
            _commandHistory.Clear();
        }
    }
}
