using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;
using PandaGame;
using TMPro;
using TurnBasedCore.Core.Players;
using TurnBasedCore.Core.TurnSystem;
using System.Collections.Generic;
using System.Collections;
using Zenject;
using Cysharp.Threading.Tasks;

public class BattleConnector : MonoBehaviourPunCallbacks
{
    public static BattleConnector Instance;
    [Inject] private UIManager _uimanager;

    [Header("Data")]
    public TurnSettingsSO settings;
    public LessonDataSO[] questions;
    public GameObject cam;

    [Header("UI")]
    [SerializeField] Button[] options;
    [SerializeField] TextMeshProUGUI quiz;
    [SerializeField] GameObject quizPanel;
    [SerializeField] GameObject winnerPanel;
    [SerializeField] TextMeshProUGUI winnerNameTeext;



    private TopicData current;

    public Entity playerA { get; private set; }
    public Entity playerB { get; private set; }


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
        PhotonView viewA = PhotonView.Find(viewIdA);
        PhotonView viewB = PhotonView.Find(viewIdB);

        if (viewA != null && viewB != null)
        {
            var playerA = viewA.GetComponent<Entity>();
            var playerB = viewB.GetComponent<Entity>();

            cam.SetActive(true);

            this.playerA = playerA;
            this.playerB = playerB;

            // ✅ Step 1: SNAP players to battle positions
            Transform pointA = playerApoints[0]; // Use any available slot logic if needed
            Transform pointB = playerBpoints[0];



            playerA.transform.position = pointA.position;
            playerB.transform.position = pointB.position;

            playerA.StopMovement();
            playerB.StopMovement();



            // 🚀 Animate "Battle Start"
            StartCoroutine(ShowBattleStartAndStart(playerA, playerB));
        }
    }

    [SerializeField] private GameObject battleStartText;

    private IEnumerator ShowBattleStartAndStart(Entity playerA, Entity playerB)
    {
        battleStartText.SetActive(true);
        yield return new WaitForSeconds(2f);
        battleStartText.SetActive(false);

        while (playerA.Info == null || playerB.Info == null)
            yield return null;

        playerA.StartBattle();
        playerB.StartBattle();

        List<IPlayerController> players = new() { playerA, playerB };
        TurnManager.Instance.Initialize(settings, players); // this will trigger StartTurn()

        BattleUI.Instance.Initialize(playerA, playerB);

        // ✅ NO NEED TO CALL QUIZ HERE! StartTurn handles it
    }




    #endregion

    #region Quiz

    System.Action OnComplete, OnFailed;



    public void InitializeQuiz(System.Action OnComplete, System.Action OnFailed)
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
        // Only the local current player should see the quiz
        var current = TurnManager.Instance.GetCurrentPlayer() as Entity;

        if (current != null && current.photonView.IsMine)
        {
            quiz.text = this.current.questionText;

            for (int i = 0; i < options.Length; i++)
            {
                if (i < this.current.options.Count)
                {
                    options[i].gameObject.SetActive(true);
                    options[i].GetComponentInChildren<TextMeshProUGUI>().text = this.current.options[i];

                    // VERY IMPORTANT: Remove old listeners before adding new ones
                    options[i].onClick.RemoveAllListeners();

                    int index = i; // capture index correctly in lambda
                    options[i].onClick.AddListener(() => SelectOption(index));
                }
                else
                {
                    options[i].gameObject.SetActive(false);
                    options[i].onClick.RemoveAllListeners();
                }
            }

            quizPanel.SetActive(true);
        }
        else
        {
            HideQuiz(); // Hide on all other clients
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

    public void EndBattle(string winnerName)
    {
        playerA.GameComplete();
        playerB.GameComplete();

        Debug.Log($"Winner: {winnerName}");
        winnerNameTeext.text = winnerName;
        winnerPanel.SetActive(true);
    }

    public void Back()
    {
        _uimanager.ShowPageAsync(UIPageType.WorldSelection).Forget();
    }

    #endregion
}
