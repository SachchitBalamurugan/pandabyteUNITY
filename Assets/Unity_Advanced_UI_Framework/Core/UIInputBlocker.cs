using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIInputBlocker : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        Unblock(); // Default state: no blocking
    }

    public void Block()
    {
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = false;
        _canvasGroup.alpha = 0f; // optional: keep transparent
    }

    public void Unblock()
    {
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
        _canvasGroup.alpha = 0f;
    }
}
