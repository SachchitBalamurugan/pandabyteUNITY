using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LessonCardView : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public Button selectButton;

    public void Bind(LessonDataSO lesson, System.Action onClick)
    {
        titleText.text = lesson.title;
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onClick?.Invoke());
    }
}
