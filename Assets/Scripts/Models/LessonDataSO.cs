using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Learning/LessonData", fileName = "LessonData")]
public class LessonDataSO : ScriptableObject
{
    public string lessonId;           // e.g. "python_intro"
    public string title;              // "Intro to Python"
    public Sprite icon;               // Icon for UI
    public string languageCode;       // "python", "html", etc.
    public List<TopicData> topics;    // Topics under this lesson
}

[Serializable]
public class TopicData
{
    public string topicId;            // "variables", "loops"
    public string title;
    [TextArea] public string explanation;
    public QuestionType questionType;
    public string questionText;
    public List<string> options;
    public int correctOptionIndex;
}

public enum QuestionType
{
    MultipleChoice,
    FillInTheBlank
}
