using System.Collections.Generic;
using TMPro;
using TurnBasedCore.Core.TurnSystem;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUI : MonoBehaviour
{
    public static BattleUI Instance;
    [SerializeField] private TextMeshProUGUI turnText;
    
    public Entity player1, player2;

    [Header("UI References")]
    [SerializeField] GameObject battleUIPanel;
    [SerializeField] GameObject actionsPanel;
    [SerializeField] Button buttonPrefab;

    

    private void Awake()
    {
        Instance = this;
        actionsPanel.SetActive(false);
    }

    public void ShowTurnInfo(Entity entity)
    {
        if (entity == null || entity.Info == null)
        {
            Debug.LogError("[BattleUI] Cannot show turn info — entity or entity.Info is null.");
            return;
        }

        turnText.text = entity.Info.IsMine ? "Your Turn" : "Opponent's Turn";
        turnText.gameObject.SetActive(true);
    }

    public void HideTurnInfo()
    {
        turnText.gameObject.SetActive(false);
    }

    public void Initialize(Entity player1, Entity player2)
    {
        this.player1 = player1;
        this.player2 = player2;

       // battleUIPanel.SetActive(true);

       // player1Name.text = player1.Info.Nickname;
       //player2Name.text = player2.Info.Nickname; // ✅ Fixed typo

       // HideActionPanel(); // Start with actions hidden
    }

    /// <summary>
    /// Call this after quiz is correctly answered to show action UI.
    /// </summary>
    public void ShowActionUIFor(Entity entity)
    {
        if (!entity.Info.IsMine) return;

        ClearActions();
        actionsPanel.SetActive(true);
        Debug.Log("show action panel");

        foreach (var item in entity.actions)
        {
            Button btn = Instantiate(buttonPrefab, actionsPanel.transform);
            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = item.name;

            bool canUse = entity.CanUseAction(item);

            btn.interactable = canUse;

            if (canUse)
            {
                btn.onClick.AddListener(() => entity.SelectAction(item));
                btn.onClick.AddListener(() => SelectButton(btn.transform));
            }
            else
            {
                btnText.text += " (Not enough power)";
            }
        }
    }


    public void HideActionPanel()
    {
        actionsPanel.SetActive(false);
        ClearActions();
    }

    private void ClearActions()
    {
        foreach (Transform child in actionsPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void SelectButton(Transform target)
    {
        foreach (Transform item in actionsPanel.transform)
            item.DOScale(Vector3.one, 0.25f);

        if (target)
            target.DOScale(Vector3.one * 1.15f, 0.25f);
    }
}
