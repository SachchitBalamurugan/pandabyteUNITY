using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using PandaBytes;
using System.Threading.Tasks;

public class LoginController : UIBaseController
{
    private LoginView _view;

    [Inject] private UIManager _uiManager;

    public override void Init()
    {

        _view = GetComponent<LoginView>();
        if (_view != null)
            _view.Bind(OnLoginClicked);
        else
            Debug.Log("view is null");

        CheckLoggedIn().Forget();
    }

   

    private  async UniTask CheckLoggedIn()
    {
        bool isAutoLoggedIn = await AuthManager.Instance.TryAutoLoginAsync();

        // 🔹 Navigate after auth check
        if (isAutoLoggedIn)
        {
            Debug.Log("🎉 Auto-login success");
            await _uiManager.ShowPageAsync(UIPageType.MainMenu);
        }
        else
        {
            Debug.Log("🔐 No session, showing login");
            await _uiManager.ShowPageAsync(UIPageType.Login);
        }
    }
    private async void OnLoginClicked()
    {
        _view.ShowLoading(true);

        bool success = await AuthManager.Instance.SignInWithGoogle(); // Your method
        _view.ShowLoading(false);

        if (success)
        {
            await _uiManager.ShowPageAsync(UIPageType.MainMenu);
        }
        else
        {
            _view.ShowError("Login Failed");
        }
    }

    private async void Login()
    {
       await _uiManager.ShowPageAsync(UIPageType.MainMenu);
    }
}
