using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;

public class SettingsController : UIBaseController
{
    [Inject] private AudioService _audioService;
    [Inject] private SaveService _saveService;
    [Inject] private SettingsStateSO _settings;
    [Inject] private UIManager _uiManager;

    private SettingsView _view;

    public override void Init()
    {
        _view = GetComponent<SettingsView>();
        _view.Bind(OnToggleMusic, OnVolumeChanged, OnBackClicked);
        _view.SetInitialValues(_settings.musicOn, _settings.masterVolume);
    }

    private void OnToggleMusic(bool isOn)
    {
        _audioService.ToggleMusic(isOn);
        _saveService.SaveSettings(_settings);
    }

    private void OnVolumeChanged(float value)
    {
        _audioService.SetVolume(value);
        _saveService.SaveSettings(_settings);
    }

    private void OnBackClicked()
    {
        _uiManager.ShowPageAsync(UIPageType.MainMenu).Forget();
    }
}
