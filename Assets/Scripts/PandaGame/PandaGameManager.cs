using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace PandaGame
{
    public class PandaGameManager : MonoBehaviourPunCallbacks
    {
        public GameObject pandaPrefab;
        public Transform spawnPoint;
        public Vector3 bounds;

        public GameObject loading;

        [Header("World Backgrounds")]
        public GameObject world1Environment;
        public GameObject world2Environment;
        public GameObject world3Environment;

        private string selectedWorld;

        private void Start()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            selectedWorld = WorldSelectionState.SelectedWorld ?? "World1"; // fallback

            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
                loading.SetActive(true);
            }
            else
            {
                JoinWorldRoom();
            }
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.LocalPlayer.NickName = "Player" + Random.Range(1, 1000);
            JoinWorldRoom();
        }

        private void JoinWorldRoom()
        {
            string roomName = selectedWorld + "Room";

            PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions
            {
                MaxPlayers = 10,
                IsVisible = true,
                IsOpen = true,
                CleanupCacheOnLeave = true
            }, TypedLobby.Default);
        }

        public override void OnJoinedRoom()
        {
            loading.SetActive(false);

            Debug.Log($"[PandaGameManager] Player {PhotonNetwork.LocalPlayer.ActorNumber} joined {PhotonNetwork.CurrentRoom.Name}");

            ActivateWorldEnvironment();

            PhotonNetwork.Instantiate(pandaPrefab.name, GetRandomPoint(), Quaternion.identity);
        }

        private void ActivateWorldEnvironment()
        {
            if (selectedWorld == "World1")
                world1Environment?.SetActive(true);
            else if (selectedWorld == "World2")
                world2Environment?.SetActive(true);
            else if (selectedWorld == "World3")
                world3Environment?.SetActive(true);
        }

        Vector3 GetRandomPoint()
        {
            float minX = -bounds.x / 2f;
            float maxX = bounds.x / 2f;
            float minY = -bounds.y / 2f;
            float maxY = bounds.y / 2f;

            return new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), spawnPoint.position.z);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(spawnPoint.position, bounds);
        }
    }
}
