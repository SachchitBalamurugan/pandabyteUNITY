using System.Collections.Generic;
using UnityEngine;

public class LessonProgressManager
{
    private Dictionary<string, HashSet<string>> _completedTopics = new();
    private const string SaveKey = "LessonProgress";

    public void MarkTopicCompleted(string lessonId, string topicId)
    {
        if (!_completedTopics.ContainsKey(lessonId))
            _completedTopics[lessonId] = new HashSet<string>();

        _completedTopics[lessonId].Add(topicId);
        SaveProgress();
    }

    public bool IsTopicCompleted(string lessonId, string topicId)
    {
        return _completedTopics.ContainsKey(lessonId) && _completedTopics[lessonId].Contains(topicId);
    }

    public float GetLessonProgress(string lessonId, int totalTopics)
    {
        if (!_completedTopics.ContainsKey(lessonId)) return 0f;
        return Mathf.Clamp01(_completedTopics[lessonId].Count / (float)totalTopics);
    }

    public void SaveProgress()
    {
        string json = JsonUtility.ToJson(new SaveWrapper(_completedTopics));
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public void LoadProgress()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return;

        string json = PlayerPrefs.GetString(SaveKey);
        var wrapper = JsonUtility.FromJson<SaveWrapper>(json);

        _completedTopics.Clear();
        foreach (var entry in wrapper.entries)
        {
            _completedTopics[entry.lessonId] = new HashSet<string>(entry.completedTopics);
        }
    }

    [System.Serializable]
    private class SaveWrapper
    {
        public List<Entry> entries = new();

        public SaveWrapper(Dictionary<string, HashSet<string>> data)
        {
            foreach (var kvp in data)
                entries.Add(new Entry { lessonId = kvp.Key, completedTopics = new List<string>(kvp.Value) });
        }

        [System.Serializable]
        public class Entry
        {
            public string lessonId;
            public List<string> completedTopics;
        }
    }
}
