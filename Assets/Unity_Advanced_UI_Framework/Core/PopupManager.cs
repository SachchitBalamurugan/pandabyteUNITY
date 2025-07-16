using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;

public class PopupManager : MonoBehaviour
{
    [Inject] private UIFactory _uiFactory;
    [Inject] private PoolManager _poolManager;
    [Inject] private UIManager _uiManager;

    [SerializeField] private UIPageConfigSO popupConfig;
    [SerializeField] private Transform popupLayer;

    public async UniTask ShowPopup(string message, System.Action onConfirm, System.Action onCancel = null)
    {
        GameObject popupGO = await _uiFactory.CreatePageAsync(popupConfig, popupLayer);
        var view = popupGO.GetComponent<PopupView>();
        view.Setup(message, onConfirm, onCancel);
    }
}
