using System.Collections.Generic;
using UnityEngine;

public class ThemeService
{
    private ThemeDataSO _activeTheme;
    public ThemeDataSO ActiveTheme => _activeTheme;

    private readonly Dictionary<string, ThemeDataSO> _themes;

    public ThemeService(List<ThemeDataSO> availableThemes, string defaultKey = "Light")
    {
        _themes = new();

        foreach (var theme in availableThemes)
        {
            _themes[theme.name] = theme;
        }

        LoadTheme(defaultKey);
    }

    public void LoadTheme(string key)
    {
        if (_themes.TryGetValue(key, out var theme))
        {
            _activeTheme = theme;
        }
        else
        {
            Debug.LogWarning($"Theme {key} not found.");
        }
    }
}
