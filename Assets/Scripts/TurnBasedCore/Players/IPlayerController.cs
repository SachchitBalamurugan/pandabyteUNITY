namespace TurnBasedCore.Core.Players
{
    public interface IPlayerController
    {
        PlayerInfo Info { get; }
       

        void StartBattle();              // Called by TurnManager
        void StartTurn();              // Called by TurnManager
        void PerformAction();          // For local (manual) or AI logic
        void SelectAction(BaseAction action);          // For local (manual) or AI logic
        void PerformAutoAction();      // On timeout
        void EndTurn();                // Ends the turn (calls TurnManager)

        void GameComplete();
    }
}
