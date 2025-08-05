using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Zenject;

public class WorldSelectionController : UIBaseController
{
    [Inject] private UIManager _uiManager;
    [Inject] private SceneLoader _sceneLoader;

    private WorldSelectionView _view;

    public override void Init()
    {
        _view = GetComponent<WorldSelectionView>();
        _view.Bind(OnWorld1ClickedAsync, OnWorld2ClickedAsync, OnWorld3ClickedAsync);
    }

    private async UniTask OnWorld1ClickedAsync()
    {
        WorldSelectionState.SelectedWorld = "World1";
        await _sceneLoader.LoadSceneAsync("Lobby");
    }

    private async UniTask OnWorld2ClickedAsync()
    {
        WorldSelectionState.SelectedWorld = "World2";
        await _sceneLoader.LoadSceneAsync("Lobby");
    }

    private async UniTask OnWorld3ClickedAsync()
    {
        WorldSelectionState.SelectedWorld = "World3";
        await _sceneLoader.LoadSceneAsync("Lobby");
    }

}
