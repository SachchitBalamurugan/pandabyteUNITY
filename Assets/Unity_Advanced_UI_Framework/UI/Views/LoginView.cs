using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginView : MonoBehaviour
{
    public Button loginButton;
    public GameObject loadingSpinner;
    public TextMeshProUGUI errorText;

    public void Bind(Action onClick)
    {
        loginButton.onClick.AddListener(() => onClick?.Invoke());
    }

    public void ShowLoading(bool show)
    {
        loadingSpinner.SetActive(show);
    }

    public void ShowError(string message)
    {
        errorText.text = message;
    }
}
