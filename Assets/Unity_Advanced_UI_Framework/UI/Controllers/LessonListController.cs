using UnityEngine;
using Zenject;
using System.Collections.Generic;
using UnityEditor;
using Cysharp.Threading.Tasks;

public class LessonListController : UIBaseController
{
    [Inject] private UIManager _uiManager;

    private LessonListView _view;
    private LessonLoader _lessonLoader;

    public override void Init()
    {
        _view = GetComponent<LessonListView>();
        _lessonLoader = new LessonLoader();

        string languageCode = PlayerState.CurrentLanguage;
        List<LessonDataSO> lessons = _lessonLoader.LoadLessons(languageCode);

        _view.Bind(OnLessonClicked);
        _view.ShowLessons(lessons);
    }

    private void OnLessonClicked(LessonDataSO lesson)
    {
        LessonState.CurrentLesson = lesson;
        _uiManager.ShowPageAsync(UIPageType.LessonDetails).Forget();
    }
}
