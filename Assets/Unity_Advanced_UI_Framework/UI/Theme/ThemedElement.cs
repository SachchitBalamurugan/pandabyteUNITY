using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

public class ThemedElement : MonoBehaviour
{
    public enum TargetType { Background, Text, Accent }
    public TargetType type;

    private Image _image;
    private TextMeshProUGUI _text;

    [Inject] private ThemeService _themeService;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        var theme = _themeService.ActiveTheme;

        switch (type)
        {
            case TargetType.Background:
                if (_image) _image.color = theme.backgroundColor;
                break;
            case TargetType.Text:
                if (_text) _text.color = theme.textColor;
                break;
            case TargetType.Accent:
                if (_image) _image.color = theme.accentColor;
                break;
        }
    }
}
