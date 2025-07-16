using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;

public class NotificationSystem : MonoBehaviour
{
    [Inject] private UIFactory _uiFactory;
    [Inject] private PoolManager _poolManager;

    [SerializeField] private UIPageConfigSO notificationConfig;
    [SerializeField] private Transform notificationLayer;

    private const float DEFAULT_DURATION = 2f;

    public async UniTask ShowNotification(string message, float duration = DEFAULT_DURATION, Color? bgColor = null)
    {
        GameObject go = await _uiFactory.CreatePageAsync(notificationConfig, notificationLayer);
        var view = go.GetComponent<NotificationView>();

        view.Show(message, bgColor ?? Color.black, duration, () =>
        {
            _poolManager.ReturnToPool(UIPageType.Popup, go); // Re-pool
        });
    }
}
