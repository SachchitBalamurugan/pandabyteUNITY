using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LessonLoader
{
    private const string LESSON_PATH = "Lessons"; // Resources/Lessons/...

    public List<LessonDataSO> LoadLessons(string languageCode)
    {
        List<LessonDataSO> lessons = new();

        var loaded = Resources.LoadAll<LessonDataSO>($"{LESSON_PATH}/{languageCode}");
        foreach (var lesson in loaded)
        {
            lessons.Add(lesson);
        }

        return lessons;
    }

    public LessonDataSO LoadLessonById(string languageCode, string lessonId)
    {
        return Resources.Load<LessonDataSO>($"{LESSON_PATH}/{languageCode}/{lessonId}_Lesson");
    }
}
