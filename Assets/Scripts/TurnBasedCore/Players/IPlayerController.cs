namespace TurnBasedCore.Core.Players
{
    public interface IPlayerController
    {
        PlayerInfo Info { get; }
        bool IsFolded { get; }

        void StartTurn();              // Called by TurnManager
        void PerformAction();          // For local (manual) or AI logic
        void PerformAutoAction();      // On timeout
        void EndTurn();                // Ends the turn (calls TurnManager)
    }
}
