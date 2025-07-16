using UnityEngine;
using UnityEngine.UI;
using System;

public class SettingsView : MonoBehaviour
{
    public Toggle musicToggle;
    public Slider volumeSlider;
    public Button backButton;

    public void Bind(Action<bool> onToggleMusic, Action<float> onVolumeChanged, Action onBack)
    {
        musicToggle.onValueChanged.AddListener(val => onToggleMusic?.Invoke(val));
        volumeSlider.onValueChanged.AddListener(val => onVolumeChanged?.Invoke(val));
        backButton.onClick.AddListener(() => onBack?.Invoke());
    }

    public void SetInitialValues(bool musicOn, float volume)
    {
        musicToggle.isOn = musicOn;
        volumeSlider.value = volume;
    }
}
