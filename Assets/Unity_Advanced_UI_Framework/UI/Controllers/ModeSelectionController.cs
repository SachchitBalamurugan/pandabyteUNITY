using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Zenject;

public class ModeSelectionController : UIBaseController
{
    [Inject] UIManager _uiManager;
    private ModeSelectionView _view;
    public override void Init()
    {
         _view = GetComponent<ModeSelectionView>();
        _view.Bind(OnLearn, OnBattle);
    }

    private void OnBattle()
    {
        _uiManager.ShowPageAsync(UIPageType.Battle).Forget();
    }

    private void OnLearn()
    {
        _uiManager.ShowPageAsync(UIPageType.LessonList).Forget();
    }
}
