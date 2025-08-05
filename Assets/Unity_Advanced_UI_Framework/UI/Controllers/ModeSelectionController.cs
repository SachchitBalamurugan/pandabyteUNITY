using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Zenject;

public class ModeSelectionController : UIBaseController
{
    [Inject] private SceneLoader _sceneLoader;

    [Inject] UIManager _uiManager;
    private ModeSelectionView _view;
    public override void Init()
    {
        _view = GetComponent<ModeSelectionView>();
        _view.Bind(OnLearn, OnBattle);
    }

    private  void OnBattle()
    {
        //  await ConnectAndJoinGlobalLobbyRoom();
        //await _sceneLoader.LoadSceneAsync("Lobby"); // ← use your actual lobby scene name
        _uiManager.ShowPageAsync(UIPageType.WorldSelection).Forget();
    }


    private void OnLearn()
    {
        _uiManager.ShowPageAsync(UIPageType.LessonList).Forget();
    }

    private async UniTask ConnectAndJoinGlobalLobbyRoom()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();

            await UniTask.WaitUntil(() =>
                PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer);
        }

        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
            await UniTask.WaitUntil(() => PhotonNetwork.InLobby);
        }

        if (!PhotonNetwork.InRoom)
        {
            PhotonNetwork.JoinOrCreateRoom("GlobalLobbyRoom", new RoomOptions
            {
                MaxPlayers = 0,
                IsVisible = true,
                IsOpen = true,
                CleanupCacheOnLeave = true
            }, TypedLobby.Default);

            await UniTask.WaitUntil(() => PhotonNetwork.InRoom);
        }

        Debug.Log("[ModeSelectionController] Joined GlobalLobbyRoom.");
    }


}
