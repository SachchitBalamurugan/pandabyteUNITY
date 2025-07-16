using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class NotificationView : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public Image backgroundImage;

    private Action _onHide;

    public void Show(string message, Color background, float duration, Action onHide = null)
    {
        messageText.text = message;
        backgroundImage.color = background;
        gameObject.SetActive(true);
        _onHide = onHide;
        Invoke(nameof(Hide), duration);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        _onHide?.Invoke();
    }
}
