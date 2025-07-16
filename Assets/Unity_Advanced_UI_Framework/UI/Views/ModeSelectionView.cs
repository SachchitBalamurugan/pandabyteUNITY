using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeSelectionView : MonoBehaviour
{
    [SerializeField] private Button learnMode, BattleMode;

    public void Bind(Action onLearnMode, Action onBattleMode)
    {
        learnMode.onClick.AddListener(() => onLearnMode?.Invoke());
        BattleMode.onClick.AddListener(() => onBattleMode?.Invoke());
    }
}
