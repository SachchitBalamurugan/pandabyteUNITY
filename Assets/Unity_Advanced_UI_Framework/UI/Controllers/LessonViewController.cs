using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LessonViewController : UIBaseController
{
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI explanationText;
    [SerializeField] private Button[] optionButtons;

    private LessonDataSO currentLesson;
    private int currentTopicIndex;

    public override void Init()
    {
        currentLesson = PlayerState.CurrentLesson;
        currentTopicIndex = 0;
        ShowCurrentTopic();
    }

    private void ShowCurrentTopic()
    {
        var topic = currentLesson.topics[currentTopicIndex];

        explanationText.text = topic.explanation;
        questionText.text = topic.questionText;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = topic.options[i];
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
        }
    }

    private void OnOptionSelected(int selectedIndex)
    {
        var topic = currentLesson.topics[currentTopicIndex];
        bool correct = selectedIndex == topic.correctOptionIndex;

        Debug.Log(correct ? "✅ Correct!" : "❌ Wrong!");

        currentTopicIndex++;
        if (currentTopicIndex < currentLesson.topics.Count)
            ShowCurrentTopic();
        else
            Debug.Log("🎉 Lesson Complete!");
    }
}
