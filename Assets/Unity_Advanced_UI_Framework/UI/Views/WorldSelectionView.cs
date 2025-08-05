using System;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class WorldSelectionView : MonoBehaviour
{
    public Button world1;
    public Button world2;
    public Button world3;

    public void Bind(Func<UniTask> onWorld1, Func<UniTask> onWorld2, Func<UniTask> onWorld3)
    {
        world1.onClick.AddListener(() => onWorld1?.Invoke().Forget());
        world2.onClick.AddListener(() => onWorld2?.Invoke().Forget());
        world3.onClick.AddListener(() => onWorld3?.Invoke().Forget());
    }
}
