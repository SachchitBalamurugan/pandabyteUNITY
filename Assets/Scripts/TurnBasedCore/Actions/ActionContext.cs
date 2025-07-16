using TurnBasedCore.Core.Players;

namespace TurnBasedCore.Core.Actions
{
    public class ActionContext
    {
        public IPlayerController SourcePlayer { get; private set; }
        public IPlayerController TargetPlayer { get; private set; }
        public object Data { get; private set; }

        public ActionContext(IPlayerController source, IPlayerController target = null, object data = null)
        {
            SourcePlayer = source;
            TargetPlayer = target;
            Data = data;
        }
    }
}
