using UnityEngine;
using UnityEngine.UI;
using System;

public class HUDView : MonoBehaviour
{
    public Text scoreText;
    public Text livesText;
    public Button pauseButton;

    public void Bind(Action onPauseClicked)
    {
        pauseButton.onClick.AddListener(() => onPauseClicked?.Invoke());
    }

    public void SetScore(int score)
    {
        scoreText.text = $"Score: {score}";
    }

    public void SetLives(int lives)
    {
        livesText.text = $"Lives: {lives}";
    }
}
