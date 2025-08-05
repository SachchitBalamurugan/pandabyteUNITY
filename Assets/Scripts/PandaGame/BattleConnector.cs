using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;
using PandaGame;
using TMPro;
using TurnBasedCore.Core.Players;
using TurnBasedCore.Core.TurnSystem;
using System.Collections.Generic;

public class BattleConnector : MonoBehaviourPunCallbacks
{
    public static BattleConnector Instance;

    [Header("Data")]
    public TurnSettingsSO settings;
    public LessonDataSO[] questions;
    public GameObject cam;

    [Header("UI")]
    [SerializeField] Button[] options;
    [SerializeField] TextMeshProUGUI quiz;
    [SerializeField] GameObject quizPanel;

    private TopicData current;

    private Entity playerA;
    private Entity playerB;

    public Transform[] playerApoints;
    public Transform[] playerBpoints;



    private void Awake()
    {
        Instance = this;
    }


    #region SetUp Online Battle
    public void InitializeBattle(Entity playerA, Entity playerB)
    {
        // Get PhotonView IDs of each player
        int idA = playerA.photonView.ViewID;
        int idB = playerB.photonView.ViewID;

        // Send IDs instead of whole objects
        photonView.RPC(nameof(SetUpBattle), RpcTarget.All, idA, idB);
    }
    [PunRPC]
    private void SetUpBattle(int viewIdA, int viewIdB)
    {
        // Find the PhotonViews by their IDs
        PhotonView viewA = PhotonView.Find(viewIdA);
        PhotonView viewB = PhotonView.Find(viewIdB);


        if (viewA != null && viewB != null)
        {
            var playerA = viewA.GetComponent<Entity>();
            var playerB = viewB.GetComponent<Entity>();

            cam.SetActive(true);

            List<IPlayerController> players = new List<IPlayerController>();


            players.Add(playerA);
            players.Add(playerB);


            this.playerA = playerA;
            this.playerB = playerB;




            TurnManager.Instance.Initialize(settings , players);
            BattleUI.Instance.Initialize(playerA, playerB);

            foreach (var item in players)
                item.StartBattle();
        }
        else
        {
            Debug.LogError("Could not find players for battle setup!");
        }
    }
    #endregion

    #region Quiz

    System.Action OnComplete, OnFailed;



    public void InitializeQuiz(System.Action OnComplete , System.Action OnFailed)
    {
        this.OnComplete = OnComplete;
        this.OnFailed = OnFailed;

        SelectQuiz();
        ShowQuiz();
    }



    private void SelectQuiz()
    {
        var question = questions[Random.Range(0, questions.Length)];

        current = question.topics[Random.Range(0, question.topics.Count)];
    }

    private void ShowQuiz()
    {
        if ((TurnManager.Instance.GetCurrentPlayer() as PandaNetworkController).photonView.IsMine)
        {
            quiz.text = current.questionText;

            foreach (var item in options)
                item.gameObject.SetActive(false);

            for (int i = 0; i < current.options.Count; i++)
            {
                options[i].GetComponentInChildren<TextMeshProUGUI>().text = current.options[i];
                options[i].onClick.AddListener(() => SelectOption(i));
            }

            quizPanel.SetActive(true);
        }
        else
        {
            HideQuiz();
        }
    }
    public void HideQuiz()
    {
        quizPanel.SetActive(false);

    }

    public void SelectOption(int option)
    {
        if (option == current.correctOptionIndex)
        {
            OnComplete?.Invoke();
        }
        else
        {
            OnFailed?.Invoke();
        }

        quizPanel.SetActive(false);

    }

    #endregion

    #region EndBattle
    private void CheckBattleEnd()
    {
        
    }

    private void EndBattle(string winnerName)
    {
        playerA.GameComplete();
        playerB.GameComplete();

        Debug.Log($"Winner: {winnerName}");
    }

    #endregion
}
