using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;

public class LoadingController : UIBaseController
{
    private LoadingView _view;
    [Inject] private UIManager _uiManager;

    public override void Init()
    {
        _view = GetComponent<LoadingView>();

        if (_view == null)
        {
            Debug.LogError("❌ LoadingView not found on this GameObject.");
            return;
        }

        _view.SetProgress(0);
        StartFakeLoading().Forget(); // fire & forget async
    }

    private async UniTaskVoid StartFakeLoading()
    {
        float duration = 5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            _view.SetProgress(progress);

            await UniTask.Yield();
        }

        _view.SetProgress(1f);
        Debug.Log("✅ Loading complete.");
        _uiManager.ShowPageAsync(UIPageType.Login).Forget();
    }

    public void SetProgress(float value)
    {
        if (_view != null)
            _view.SetProgress(value);
    }
}
