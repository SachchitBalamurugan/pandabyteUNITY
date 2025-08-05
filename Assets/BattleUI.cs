using System.Collections;
using System.Collections.Generic;
using TMPro;
using TurnBasedCore.Core.TurnSystem;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;
using static UnityEditor.Progress;

public class BattleUI : MonoBehaviour
{
    public static BattleUI Instance;

    public Entity player1, player2;



    [Header("UI References")]
    [SerializeField] GameObject battleUIPanel;
    [SerializeField] GameObject actionsPanel;
    [SerializeField] Button buttonPrefab;

    [SerializeField] TextMeshProUGUI player1Name , player2Name;


    private void Awake()
    {
        Instance = this;
    }



    public void Initialize(Entity player1, Entity player2)
    {
        this.player1 = player1;
        this.player2 = player2;

        battleUIPanel.SetActive(true);

        player1Name.text = player1.Info.Nickname;
        player2Name.text = player1.Info.Nickname;

        LoadButtons();
        UpdateDisplay();
    }
    private void LoadButtons()
    {
        if (TurnManager.Instance.GetCurrentPlayer().Info.IsMine)
        {
            var entity = (TurnManager.Instance.GetCurrentPlayer() as Entity);

            foreach (var item in entity.actions)
            {
                Button btn = Instantiate(buttonPrefab, actionsPanel.transform);

                btn.onClick.AddListener(() => entity.SelectAction(item));
                btn.onClick.AddListener(() => SelectButton(btn.transform));

                btn.GetComponentInChildren<TextMeshProUGUI>().text = item.name;
            }
        }
    }

    private void UpdateDisplay()
    {
        if (TurnManager.Instance.GetCurrentPlayer().Info.IsMine)
        {
            actionsPanel.SetActive(true);
        }
        else
        {
            actionsPanel.SetActive(false);

        }
    }

    private void SelectButton(Transform target)
    {
        foreach (Transform item in actionsPanel.transform)
            item.DOScale(Vector3.one, 1);

        if(target)
            target.DOScale(Vector3.one * 1.15f, 1);
    }

}
