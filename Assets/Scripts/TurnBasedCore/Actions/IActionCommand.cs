namespace TurnBasedCore.Core.Actions
{
    public interface IActionCommand
    {
        void Execute();
        void Undo();  // Optional - for rollback (can be empty if unused)
    }
}
