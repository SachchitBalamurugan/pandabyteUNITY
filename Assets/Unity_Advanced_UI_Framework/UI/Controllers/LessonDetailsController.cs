using UnityEngine;
using Zenject;
using PandaBytes;
using Cysharp.Threading.Tasks;

public class LessonDetailsController : UIBaseController
{
    [Inject] private UIManager _uiManager;

    private LessonDetailsView _view;

    public override void Init()
    {
        _view = GetComponent<LessonDetailsView>();
        _view.Bind(OnStartClicked);

        var currentLesson = LessonState.CurrentLesson;
        _view.ShowLesson(currentLesson);
    }

    private void OnStartClicked()
    {
        _uiManager.ShowPageAsync(UIPageType.TopicGameplay).Forget();
    }
}
