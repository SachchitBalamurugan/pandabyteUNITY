using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LessonBuilderWindow : EditorWindow
{
    private LessonDataSO lessonData;
    private string lessonTitle = "New Lesson";
    private string lessonId = "lesson_id";
    private string languageCode = "python";
    private Sprite icon;

    private List<TopicData> topics = new();

    private Vector2 scrollPos;

    [MenuItem("Tools/Lesson Builder")]
    public static void ShowWindow()
    {
        GetWindow<LessonBuilderWindow>("Lesson Builder");
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Lesson Settings", EditorStyles.boldLabel);

        lessonTitle = EditorGUILayout.TextField("Title", lessonTitle);
        lessonId = EditorGUILayout.TextField("Lesson ID", lessonId);
        languageCode = EditorGUILayout.TextField("Language Code", languageCode);
        icon = (Sprite)EditorGUILayout.ObjectField("Icon", icon, typeof(Sprite), false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Topics", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Topic"))
            topics.Add(new TopicData() { options = new List<string> { "", "", "", "" } });

        for (int i = 0; i < topics.Count; i++)
        {
            var topic = topics[i];

            if (topic.options == null || topic.options.Count < 2)
            {
                topic.options = new List<string> { "", "" };
            }

            EditorGUILayout.BeginVertical("box");

            topic.topicId = EditorGUILayout.TextField("Topic ID", topic.topicId);
            topic.title = EditorGUILayout.TextField("Title", topic.title);
            topic.explanation = EditorGUILayout.TextArea(topic.explanation, GUILayout.Height(60));
            topic.questionType = (QuestionType)EditorGUILayout.EnumPopup("Type", topic.questionType);
            topic.questionText = EditorGUILayout.TextField("Question", topic.questionText);

            int optionCount = Mathf.Max(2, topic.options.Count);

            // Ensure option list has fixed size (4)
            while (topic.options.Count < 4) topic.options.Add("");
            while (topic.options.Count > 4) topic.options.RemoveAt(topic.options.Count - 1);

            for (int j = 0; j < topic.options.Count; j++)
                topic.options[j] = EditorGUILayout.TextField($"Option {j + 1}", topic.options[j]);

            topic.correctOptionIndex = EditorGUILayout.IntSlider("Correct Option Index", topic.correctOptionIndex, 0, topic.options.Count - 1);

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Remove Topic"))
            {
                topics.RemoveAt(i);
                GUI.backgroundColor = Color.white;
                break; // Avoid layout error after modification
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Create LessonDataSO"))
        {
            CreateLessonDataAsset();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndScrollView();
    }

    private void CreateLessonDataAsset()
    {
        var asset = ScriptableObject.CreateInstance<LessonDataSO>();
        asset.lessonId = lessonId;
        asset.title = lessonTitle;
        asset.languageCode = languageCode;
        asset.icon = icon;
        asset.topics = new List<TopicData>(topics);

        string dir = "Assets/Lessons/";
        if (!AssetDatabase.IsValidFolder(dir)) System.IO.Directory.CreateDirectory(dir);

        string path = $"{dir}{lessonId}_Lesson.asset";
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"✅ Lesson created at: {path}");
    }
}
