namespace TurnBasedCore.Core.Players
{
    public class PlayerInfo
    {
        public int ActorNumber { get; set; }
        public string Nickname { get; set; }
        public bool IsAI { get; set; }
        public bool IsLocal { get; set; }
        public bool AllowAutoAction { get; set; } = true;

        // ✅ NEW: Add ChipCount for use in UI, seat info, etc.
        public int ChipCount { get; set; }

        public PlayerInfo(int actorNumber, string nickname, bool isAI, bool isLocal)
        {
            ActorNumber = actorNumber;
            Nickname = nickname;
            IsAI = isAI;
            IsLocal = isLocal;
            ChipCount = 0; // Default; can be set after instantiation
        }
    }
}
