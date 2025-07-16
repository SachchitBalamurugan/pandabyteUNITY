using UnityEngine;
using Zenject;

public class HUDController : UIBaseController
{
    private HUDView _view;

    private int _score = 0;
    private int _lives = 3;

    [Inject] private UIManager _uiManager;

    public override void Init()
    {
        _view = GetComponent<HUDView>();
        _view.Bind(OnPauseClicked);
        UpdateHUD();
    }

    private void OnPauseClicked()
    {
        // Optionally open a pause popup
        Debug.Log("Pause clicked");
        // _uiManager.ShowPageAsync(UIPageType.Pause).Forget();
    }

    private void UpdateHUD()
    {
        _view.SetScore(_score);
        _view.SetLives(_lives);
    }

    // Call these from gameplay system
    public void AddScore(int amount)
    {
        _score += amount;
        _view.SetScore(_score);
    }

    public void ReduceLife()
    {
        _lives = Mathf.Max(0, _lives - 1);
        _view.SetLives(_lives);
    }
}
