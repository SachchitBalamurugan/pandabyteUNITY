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
        public Vector3 bounds;

        public GameObject loading;

        private void Start()
        {
            PhotonNetwork.AutomaticallySyncScene = true;

            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
                loading.SetActive(true);
            }
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.LocalPlayer.NickName = "Player" + Random.Range(1, 11).ToString();


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
            loading.SetActive(false);


            Debug.Log($"[PandaGameManager] Player{PhotonNetwork.LocalPlayer.ActorNumber} Joined GlobalLobbyRoom.");

            PhotonNetwork.Instantiate(pandaPrefab.name, GetRandomPoint(), Quaternion.identity);

        }


        Vector3 GetRandomPoint()
        {
            int miniX  = (int)(-bounds.x / 2);
            int maxX  = (int)(bounds.x / 2);

            int miniY = (int)(-bounds.y / 2);
            int maxY = (int)(bounds.y / 2);


            return new Vector3(Random.Range(miniX , maxX ) , Random.Range(miniY , maxY) , spawnPoint.position.z);
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireCube(spawnPoint.position, bounds);
        }
    }
}
