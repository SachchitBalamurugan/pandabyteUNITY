using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LessonDetailsView : MonoBehaviour
{
    public TMP_Text lessonTitle;
    public TMP_Text topicCount;
    public Image icon;
    public Button startButton;

    private System.Action _onStartClicked;

    public void Bind(System.Action onStartClicked)
    {
        _onStartClicked = onStartClicked;
        startButton.onClick.AddListener(() => _onStartClicked?.Invoke());
    }

    public void ShowLesson(LessonDataSO lesson)
    {
        lessonTitle.text = lesson.title;
        topicCount.text = $"Topics: {lesson.topics.Count}";
        icon.sprite = lesson.icon;
    }
}
