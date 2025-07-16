using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;


public class MainMenuController : UIBaseController
{
    [Inject] private UIManager _uiManager;

    private MainMenuView _view;

    public override void Init()
    {
        _view = GetComponent<MainMenuView>();
        _view.Bind(OnPlayClicked, OnSettingsClicked, OnQuitClicked);
    }

    private void OnPlayClicked()
    {
        Debug.Log("Play clicked");
        _uiManager.ShowPageAsync(UIPageType.ModeSelection).Forget(); // HUD = game UI
    }

    private void OnSettingsClicked()
    {
        _uiManager.ShowPageAsync(UIPageType.Settings).Forget();
    }

    private void OnQuitClicked()
    {
        Application.Quit();
    }
}
