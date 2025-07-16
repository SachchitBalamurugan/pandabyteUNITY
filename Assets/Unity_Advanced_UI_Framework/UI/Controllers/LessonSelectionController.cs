using UnityEngine;
using Zenject;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class LessonSelectionController : UIBaseController
{
    [Inject] UIManager _uiManager;

    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject lessonCardPrefab;

    private LessonLoader lessonLoader = new();
    private List<LessonDataSO> loadedLessons;

    public override void Init()
    {
        string selectedLanguage = "python"; // Replace with dynamic from player settings
        loadedLessons = lessonLoader.LoadLessons(selectedLanguage);

        foreach (var lesson in loadedLessons)
        {
            GameObject card = Instantiate(lessonCardPrefab, contentParent);
            var view = card.GetComponent<LessonCardView>();
            view.Bind(lesson, () => OnLessonSelected(lesson));
        }
    }

    private void OnLessonSelected(LessonDataSO lesson)
    {
        PlayerState.CurrentLesson = lesson; // store selected lesson globally
        _uiManager.ShowPageAsync(UIPageType.LessonView).Forget();
    }
}
