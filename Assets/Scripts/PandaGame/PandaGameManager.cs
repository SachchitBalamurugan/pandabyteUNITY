using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TurnBasedCore.Core.Network;

namespace PandaGame
{
    public class PandaGameManager : MonoBehaviourPunCallbacks
    {
        public GameObject pandaPrefab;
        public Transform spawnPoint;

        private void Start()
        {
            PhotonNetwork.AutomaticallySyncScene = true;

            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinOrCreateRoom("GlobalLobbyRoom", new RoomOptions
            {
                MaxPlayers = 500, // 0 = Photon default max (usually 500)
                IsVisible = true,
                IsOpen = true,
                CleanupCacheOnLeave = true
            }, TypedLobby.Default);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("[PandaGameManager] Joined GlobalLobbyRoom.");

            PhotonNetwork.Instantiate(pandaPrefab.name, spawnPoint.position, Quaternion.identity);
        }
    }
}
