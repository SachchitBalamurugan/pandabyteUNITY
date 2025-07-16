using UnityEngine;
using UnityEngine.UI;
using System;

public class PopupView : MonoBehaviour
{
    public Text messageText;
    public Button confirmButton;
    public Button cancelButton;

    public void Setup(string message, Action onConfirm, Action onCancel = null)
    {
        messageText.text = message;

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
            Close();
        });

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() =>
            {
                onCancel?.Invoke();
                Close();
            });
        }
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }
}
