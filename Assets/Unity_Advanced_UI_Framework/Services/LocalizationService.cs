using System.Collections.Generic;
using UnityEngine;

public class LocalizationService
{
    private Dictionary<string, string> _localizedStrings;
    private string _currentLanguage;

    private readonly Dictionary<string, LocalizationConfigSO> _languages;

    public LocalizationService(List<LocalizationConfigSO> configs)
    {
        _languages = new Dictionary<string, LocalizationConfigSO>();

        foreach (var config in configs)
        {
            _languages[config.languageCode] = config;
        }

        // Default to English
        LoadLanguage("en");
    }

    public void LoadLanguage(string languageCode)
    {
        if (_languages.TryGetValue(languageCode, out var config))
        {
            _localizedStrings = config.GetDictionary();
            _currentLanguage = languageCode;
        }
        else
        {
            Debug.LogWarning($"Language {languageCode} not found. Falling back to English.");
            LoadLanguage("en");
        }
    }

    public string Get(string key)
    {
        return _localizedStrings != null && _localizedStrings.TryGetValue(key, out var val) ? val : $"[{key}]";
    }

    public string CurrentLanguage => _currentLanguage;
}
