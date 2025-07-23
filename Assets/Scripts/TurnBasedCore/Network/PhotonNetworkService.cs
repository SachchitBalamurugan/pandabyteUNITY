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
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                if (photonView == null)
                    Debug.LogError("[PhotonNetworkService] Missing PhotonView component!");
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        public bool IsMasterClient => PhotonNetwork.IsMasterClient;
        public int LocalActorNumber => PhotonNetwork.LocalPlayer.ActorNumber;

        public void RPC(string methodName, object[] parameters, RpcTargetType targetType, int targetActorNumber = -1)
        {
            if (photonView == null)
            {
                Debug.LogError($"[PhotonNetworkService] Cannot send RPC '{methodName}' — PhotonView missing.");
                return;
            }

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
                    else
                    {
                        Debug.LogWarning($"[PhotonNetworkService] Invalid actor number for specific player RPC.");
                    }
                    break;
                default:
                    Debug.LogError($"[PhotonNetworkService] Unknown RpcTargetType: {targetType}");
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

        // Optional hooks for future use
        // public event Action<Player> OnPlayerJoined;
        // public event Action<Player> OnPlayerLeft;
    }
}
