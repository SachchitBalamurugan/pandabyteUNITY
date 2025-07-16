using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LessonListView : MonoBehaviour
{
    public Transform contentParent;
    public GameObject lessonItemPrefab;

    private Action<LessonDataSO> _onLessonClicked;

    public void Bind(Action<LessonDataSO> onLessonClicked)
    {
        _onLessonClicked = onLessonClicked;
    }

    public void ShowLessons(List<LessonDataSO> lessons)
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (var lesson in lessons)
        {
            var item = Instantiate(lessonItemPrefab, contentParent);
            item.GetComponentInChildren<TMP_Text>().text = lesson.title;

            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                _onLessonClicked?.Invoke(lesson);
            });
        }
    }
}
