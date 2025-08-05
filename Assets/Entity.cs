using Photon.Pun;
using System.Collections.Generic;
using TurnBasedCore.Core.Players;
using UnityEngine;
using TurnBasedCore.Core.TurnSystem;
using Unity.VisualScripting;
using System.Linq;
using System.Collections;

public class Entity : MonoBehaviourPun, IPlayerController
{
    public bool isAi;

    public BaseAction[] actions;
    public PlayerInfo Info { get; set; }

    public List<FighterData> Fighters;
    public FighterDisplay prefab;

    private List<FighterDisplay> fighterDisplays = new();

    private BaseAction currentAction;


    int juice = 3;

    private void Start()
    {
        //Info = new PlayerInfo(photonView.ViewID, photonView.Owner.NickName, false, photonView.IsMine);
    }

    private void OnEnable()
    {
        StartCoroutine(InitAfterPhotonReady());
    }

    private IEnumerator InitAfterPhotonReady()
    {
        // Wait until PhotonView has a valid Owner
        while (photonView.Owner == null)
            yield return null;

        Info = new PlayerInfo(photonView.ViewID, photonView.Owner.NickName, false, photonView.IsMine);
        Debug.Log(photonView.Owner.NickName);
    }

    public void StartBattle()
    {
        foreach (FighterData fighter in Fighters)
            fighter.Setup();




        foreach (var item in fighterDisplays)
            Destroy(item.gameObject);

        fighterDisplays.Clear();

        juice = 3;


        var points = BattleConnector.Instance.playerApoints;

        if (isAi || !Info.IsMine)
            points = BattleConnector.Instance.playerBpoints;

        for (var i = 0; i < Fighters.Count; i++)
        {
            if (i > points.Length)
                break;

            FighterDisplay fighter = Instantiate(prefab, points[i]);

            fighter.Setup(Fighters[i]);

            fighterDisplays.Add(fighter);
        }
    }


    FighterDisplay entity;

    private void Update()
    {
        if (currentAction == null)
            return;
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent<FighterDisplay>(out var entity))
            {
                if (this.entity != null&&this.entity != entity)
                {
                    this.entity.Hover(false);
                    this.entity = entity;
                }

                entity.Hover(true);

                if (Input.GetMouseButtonDown(0))
                    PerformAction(entity);
            }
            else
            {
                if (this.entity != null)
                {
                    this.entity.Hover(false);
                    this.entity = null;
                }
            }
        }
        if (this.entity != null)
        {
            this.entity.Hover(false);
            this.entity = null;
        }
    }

    public void StartTurn()
    {
        Debug.Log($"[Entity] Turn started for player {Info.Nickname}");

        currentAction = null;

        if (juice <= 0)
        {
            BattleConnector.Instance.InitializeQuiz(() => juice = 3 , EndTurn);
        }


        if (photonView.IsMine)
        {
            ShowActionUI(true);
        }
    }

    public void EndTurn()
    {
        if (photonView.IsMine)
        {
            ShowActionUI(false);
            Debug.Log($"[Entity] {Info.Nickname} ending turn");
            TurnManager.Instance.EndTurn(this);
        }
    }

    public void PerformAction(FighterDisplay target)
    {
        if (actions.Contains(currentAction) && currentAction.IsTarget(this, target.Fighter))
        {
            currentAction.PerformAction(this , target.Fighter); // Pass self as executor
            target.UpdateDisplay();

            juice -= currentAction.actionCost;

            currentAction = null;

            target.Hover(false);

            EndTurn();
        }
    }

    public void PerformAutoAction()
    {
    }

    public void GameComplete()
    {
        Debug.Log($"[Entity] Game completed for player {Info.Nickname}");
        // Cleanup, hide UI, disable input
        ShowActionUI(false);
    }

    private void OnTurnTimeout()
    {

    }

    private void ShowActionUI(bool show)
    {
        // Logic to toggle action UI based on local player
        if (photonView.IsMine)
        {
            // Show or hide action buttons, fighter display, etc.
        }
    }

    public void PerformAction()
    {
        // no need;


    }

    public void SelectAction(BaseAction action)
    {
        if (juice >= action.actionCost)
        {
            currentAction = action;
        }
    }
}


[System.Serializable]
public class FighterData
{
    // add stats stuff

    public string name;
    public float Hp;

    public float CurrentHp { get; private set; }

    public void Setup()
        => CurrentHp = Hp;
    // rpc or a way to update health
    public void TakeDamage(float amount)
        => CurrentHp -= amount;

    // rpc or a way to update health
    public void Heal(float amount)
        => CurrentHp += amount;
}
