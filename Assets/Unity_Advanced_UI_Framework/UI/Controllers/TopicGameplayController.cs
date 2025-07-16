using UnityEngine;
using Zenject;
using PandaBytes;
using Cysharp.Threading.Tasks;

public class TopicGameplayController : UIBaseController
{
    [Inject] private UIManager _uiManager;

    private TopicGameplayView _view;

    private LessonDataSO _lesson;
    private int _currentTopicIndex = 0;

    public override void Init()
    {
        _view = GetComponent<TopicGameplayView>();
        _lesson = LessonState.CurrentLesson;

        if (_lesson == null || _lesson.topics.Count == 0)
        {
            Debug.LogError("🚫 No lesson or topics found!");
            _uiManager.ShowPageAsync(UIPageType.MainMenu).Forget();
            return;
        }

        _view.Bind(OnSubmitClicked);
        ShowCurrentTopic();
    }

    private void ShowCurrentTopic()
    {
        var topic = _lesson.topics[_currentTopicIndex];
        _view.ShowTopic(topic);
    }

    private void OnSubmitClicked(int selectedIndex)
    {
        var topic = _lesson.topics[_currentTopicIndex];
        bool isCorrect = topic.correctOptionIndex == selectedIndex;

        if (isCorrect)
        {
            PlayerState.AddXP(10);
        }

        // Show result feedback
        _view.ShowResult(isCorrect);

        // Move to next topic after delay
        if (_currentTopicIndex + 1 < _lesson.topics.Count)
        {
            _currentTopicIndex++;
            _ = UniTask.Delay(1000).ContinueWith(() => ShowCurrentTopic());
        }
        else
        {
            PlayerState.MarkLessonComplete(_lesson.lessonId);
            _uiManager.ShowPageAsync(UIPageType.MainMenu).Forget();
        }
    }
}
