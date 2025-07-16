using UnityEngine;
using UnityEngine.UI;
using System;

public class MainMenuView : MonoBehaviour
{
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;

    public void Bind(Action onPlay, Action onSettings, Action onQuit)
    {
        playButton.onClick.AddListener(() => onPlay?.Invoke());
        settingsButton.onClick.AddListener(() => onSettings?.Invoke());
        quitButton.onClick.AddListener(() => onQuit?.Invoke());
    }
}
