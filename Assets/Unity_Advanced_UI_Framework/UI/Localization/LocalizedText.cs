using UnityEngine;
using TMPro;
using Zenject;

public class LocalizedText : MonoBehaviour
{
    public string key;
    private TextMeshProUGUI _text;

    [Inject] private LocalizationService _localization;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (_text != null && !string.IsNullOrEmpty(key))
        {
            _text.text = _localization.Get(key);
        }
    }
}
