using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;

public class UIInputRouter : MonoBehaviour
{
    [Inject] private UIManager _uiManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            _uiManager.GoBackAsync().Forget();
    }

}
