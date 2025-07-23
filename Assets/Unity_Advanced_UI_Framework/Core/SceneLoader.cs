using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using Zenject;

public class SceneLoader : MonoBehaviour
{
    [Inject] private UIFactory _uiFactory;
    [Inject] private PoolManager _poolManager;

    [SerializeField] private UIPageConfigSO loadingConfig;
    [SerializeField] private Transform loadingLayer;

    private LoadingController _loadingController;

    public async UniTask LoadSceneAsync(string sceneName)
    {
        // 🔹 Show loading screen via controller
        GameObject loadingGO = await _uiFactory.CreatePageAsync(loadingConfig, loadingLayer);
        DontDestroyOnLoad(loadingGO);
        _loadingController = loadingGO.GetComponent<LoadingController>();

        // 🔹 Start async scene loading
        var asyncOp = SceneManager.LoadSceneAsync(sceneName);
        asyncOp.allowSceneActivation = false;

        while (!asyncOp.isDone)
        {
            float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
            _loadingController.SetProgress(progress);

            if (asyncOp.progress >= 0.9f)
            {
                await UniTask.Delay(300); // Optional fake wait
                asyncOp.allowSceneActivation = true;
            }

            await UniTask.Yield();
        }

        if (loadingGO != null)
        {
            _poolManager.ReturnToPool(UIPageType.Loading, loadingGO);
        }
    }
}
