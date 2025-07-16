using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using System; // ✅ Required for Enum.Parse

public class Bootstrapper : MonoBehaviour
{
    [Inject] private UIManager _uiManager;
    [Inject] private LocalizationService _localizationService;
    [Inject] private ThemeService _themeService;
    [Inject] private SaveService _saveService;
    [Inject] private SettingsStateSO _settings;

    [SerializeField] private string startPage = "MainMenu"; // e.g., MainMenu, HUD, Splash

    private async void Start()
    {
        // 🔹 Load saved settings (no var assignment)
        _saveService.LoadSettings(_settings);

        // 🔹 Apply language and theme from saved state
        _localizationService.LoadLanguage(_settings.languageCode);
        _themeService.LoadTheme(_settings.themeName);

        // 🔹 Show initial UI page
        await _uiManager.ShowPageAsync(Enum.Parse<UIPageType>(startPage));
    }
}
