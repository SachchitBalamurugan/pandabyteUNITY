using Photon.Pun;
using System.Collections.Generic;
using TurnBasedCore.Core.Players;
using UnityEngine;
using TurnBasedCore.Core.TurnSystem;
using Unity.VisualScripting;
using System.Linq;
using System.Collections;
using PandaGame;
using DG.Tweening;

public class Entity : MonoBehaviourPun, IPlayerController
{
    public bool isAi;

    public BaseAction[] actions;
    public PlayerInfo Info { get; set; }

    public List<FighterData> Fighters;
    public FighterDisplay prefab;

    private List<FighterDisplay> fighterDisplays = new();

    private BaseAction currentAction;
    public PandaNetworkController controller; // drag & assign in inspector
    [Header("Attack Animation")]
    public GameObject attackParticlePrefab; // Particle system (e.g., fireball, leaf, bamboo)
    public Transform attackSpawnPoint;      // Where projectile spawns (e.g., in front of player)
    public GameObject hitEffectPrefab; // Optional hit FX prefab

    public float attackMoveDistance = 0.5f;
    public float attackMoveDuration = 0.25f;




    int juice = 1;

    private void Awake()
    {
        controller = GetComponent<PandaNetworkController>();
    }

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
        // AI or offline entities won't have a Photon owner
        if (isAi)
        {
            Info = new PlayerInfo(photonView.ViewID, "AI Bot", false, false);
            Debug.Log("[Entity] AI Info set manually.");
            yield break;
        }

        // Wait for Photon to assign owner
        while (photonView.Owner == null)
            yield return null;

        Info = new PlayerInfo(photonView.ViewID, photonView.Owner.NickName, false, photonView.IsMine);
        Debug.Log($"[Entity] Info ready for {Info.Nickname}");
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

            FighterDisplay fighter = Instantiate(prefab, points[i].position, Quaternion.identity, transform);


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
                if (this.entity != null && this.entity != entity)
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

    private bool hasTakenFirstTurn = false;

    public void StartTurn()
    {
        Debug.Log($"[Entity] Turn started for {Info.Nickname}");

        currentAction = null;

        BattleUI.Instance.ShowTurnInfo(this);

        if (isAi)
        {
            StartCoroutine(AutoAttackRoutine()); // ← AI skips quiz and acts automatically
            return;
        }

        if (juice <= 0)
        {
            if (photonView.IsMine)
                StartCoroutine(DelayedQuizStart());
        }
        else
        {
            if (photonView.IsMine)
                BattleUI.Instance.ShowActionUIFor(this);
        }
    }

    private IEnumerator AutoAttackRoutine()
    {
        yield return new WaitForSeconds(1.5f); // Optional delay for realism

        // Find valid target
        Entity target = (this == BattleConnector.Instance.playerA)
            ? BattleConnector.Instance.playerB
            : BattleConnector.Instance.playerA;

        // Pick random action
        currentAction = actions[Random.Range(0, actions.Length)];

        // Pick random target Fighter (you can make this smarter later)
        var possibleTargets = target.GetComponentsInChildren<FighterDisplay>();
        if (possibleTargets.Length > 0)
        {
            var selected = possibleTargets[Random.Range(0, possibleTargets.Length)];
            PerformAction(selected);
        }
        else
        {
            Debug.LogWarning("[AI] No valid targets found!");
            EndTurn();
        }
    }

    public bool CanUseAction(BaseAction action)
    {
        return juice >= action.actionCost;
    }


    private IEnumerator DelayedQuizStart()
    {
        Debug.Log("show quiz");
        yield return new WaitForSeconds(.5f);

        BattleConnector.Instance.InitializeQuiz(
            () =>
            {
                RecoverJuice(); // refills juice
                BattleUI.Instance.ShowActionUIFor(this); // shows attack buttons
            },
            EndTurn // wrong answer skips turn
        );
    }




    public void EndTurn()
    {
        if (photonView.IsMine)
        {
            ShowActionUI(false);
            BattleUI.Instance.HideActionPanel();
            Debug.Log($"[Entity] {Info.Nickname} ending turn");
            TurnManager.Instance.EndTurn(this);
        }
    }


    public void PerformAction(FighterDisplay target)
    {
        if (actions.Contains(currentAction) && currentAction.IsTarget(this, target.Fighter))
        {
            StartCoroutine(AnimateAttackOnlyParticle(target));
        }
    }

    private IEnumerator AnimateAttackOnlyParticle(FighterDisplay target)
    {
        if (attackParticlePrefab && attackSpawnPoint)
        {
            GameObject projectile = Instantiate(attackParticlePrefab, attackSpawnPoint.position, Quaternion.identity);
            projectile.transform.DOMove(target.transform.position, 0.4f).SetEase(Ease.Linear).OnComplete(() =>
            {
                Destroy(projectile);
            });
        }

        yield return new WaitForSeconds(0.4f);

        target.transform.DOShakePosition(0.2f, 0.2f, 10, 90);

        currentAction.PerformAction(this, target.Fighter);
        target.UpdateDisplay();

        // ✅ Clamp HP and check for 0
        if (target.Fighter.CurrentHp <= 0)
        {
            string winner = Info.Nickname;
            BattleConnector.Instance.EndBattle(winner);
            yield break;
        }

        juice -= currentAction.actionCost;
        currentAction = null;
        target.Hover(false);

        yield return new WaitForSeconds(0.2f);

        EndTurn();
    }

    private void CheckBattleEnd()
    {
        bool aDead = BattleConnector.Instance.playerA.Fighters.All(f => f.CurrentHp <= 0);
        bool bDead = BattleConnector.Instance.playerB.Fighters.All(f => f.CurrentHp <= 0);

        if (aDead || bDead)
        {
            string winner = !aDead
                ? BattleConnector.Instance.playerA.Info.Nickname
                : BattleConnector.Instance.playerB.Info.Nickname;

            BattleConnector.Instance.EndBattle(winner);
        }
    }


    public void RecoverJuice()
    {
        juice = 1;
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

    public void StopMovement()
    {
        if (controller != null)
        {
            controller.StopMovement(); // Real player movement stop
        }

        // Disable collider (for both real and AI)
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
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
    {
        CurrentHp -= amount;
        CurrentHp = Mathf.Max(0, CurrentHp); // Clamp at 0
    }


    // rpc or a way to update health
    public void Heal(float amount)
        => CurrentHp += amount;
}
