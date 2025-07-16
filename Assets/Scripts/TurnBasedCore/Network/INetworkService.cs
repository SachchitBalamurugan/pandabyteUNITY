namespace TurnBasedCore.Core.Network
{
    public interface INetworkService
    {
        bool IsMasterClient { get; }
        int LocalActorNumber { get; }

        void RPC(string methodName, object[] parameters, RpcTargetType targetType, int targetActorNumber = -1);
    }

    public enum RpcTargetType
    {
        All,
        Others,
        MasterClient,
        SpecificPlayer
    }
}
