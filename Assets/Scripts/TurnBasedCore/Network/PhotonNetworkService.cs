using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace TurnBasedCore.Core.Network
{
    public class PhotonNetworkService : MonoBehaviourPun, INetworkService
    {
        public static PhotonNetworkService Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public bool IsMasterClient => PhotonNetwork.IsMasterClient;
        public int LocalActorNumber => PhotonNetwork.LocalPlayer.ActorNumber;

        public void RPC(string methodName, object[] parameters, RpcTargetType targetType, int targetActorNumber = -1)
        {
            switch (targetType)
            {
                case RpcTargetType.All:
                    photonView.RPC(methodName, RpcTarget.All, parameters);
                    break;
                case RpcTargetType.Others:
                    photonView.RPC(methodName, RpcTarget.Others, parameters);
                    break;
                case RpcTargetType.MasterClient:
                    photonView.RPC(methodName, RpcTarget.MasterClient, parameters);
                    break;
                case RpcTargetType.SpecificPlayer:
                    if (targetActorNumber > 0)
                    {
                        Player target = GetPlayerByActorNumber(targetActorNumber);
                        if (target != null)
                            photonView.RPC(methodName, target, parameters);
                        else
                            Debug.LogWarning($"[PhotonNetworkService] Player {targetActorNumber} not found.");
                    }
                    break;
            }
        }

        private Player GetPlayerByActorNumber(int actorNumber)
        {
            foreach (var player in PhotonNetwork.PlayerList)
                if (player.ActorNumber == actorNumber)
                    return player;

            return null;
        }
    }
}
