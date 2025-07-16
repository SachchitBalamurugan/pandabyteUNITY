using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TopicGameplayView : MonoBehaviour
{
    public TMP_Text topicTitle;
    public TMP_Text questionText;
    public TMP_Text explanationText;
    public List<Button> optionButtons;
    public List<TMP_Text> optionLabels;
    public GameObject resultPanel;
    public TMP_Text resultText;

    private System.Action<int> _onSubmit;

    public void Bind(System.Action<int> onSubmit)
    {
        _onSubmit = onSubmit;

        for (int i = 0; i < optionButtons.Count; i++)
        {
            int index = i;
            optionButtons[i].onClick.AddListener(() => _onSubmit?.Invoke(index));
        }

        resultPanel.SetActive(false);
    }

    public void ShowTopic(TopicData topic)
    {
        topicTitle.text = topic.title;
        questionText.text = topic.questionText;
        explanationText.text = topic.explanation;

        for (int i = 0; i < optionButtons.Count; i++)
        {
            optionButtons[i].gameObject.SetActive(i < topic.options.Count);
            optionLabels[i].text = topic.options[i];
        }

        resultPanel.SetActive(false);
    }

    public void ShowResult(bool isCorrect)
    {
        resultPanel.SetActive(true);
        resultText.text = isCorrect ? "Correct!" : "Incorrect!";
    }
}
