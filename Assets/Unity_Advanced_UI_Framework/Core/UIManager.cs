using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Zenject;

public enum UIPageType
{
    Loading,
    Login,
    MainMenu,
    HUD,
    Popup,
    Settings,
    Unknown ,
    Battle,
    LearningMain,
    ModeSelection,
    LessonView ,
    LessonList,
    LessonDetails,
    TopicGameplay ,
    WorldSelection
}

public class UIManager : MonoBehaviour
{
    [Inject] private UIFactory _uiFactory;
    [Inject] private UIStateMachine _stateMachine;

    [SerializeField] private List<UIPageConfigSO> pageConfigs;
    [SerializeField] private Transform mainLayer;
    [SerializeField] private Transform popupLayer;
    [SerializeField] private Transform overlayLayer;

    private readonly Dictionary<UIPageType, GameObject> _activePages = new();
    private readonly Dictionary<UIPageType, UIPageConfigSO> _configLookup = new();

    private readonly Stack<UIPageType> _navigationStack = new();

    private UIPageType _currentPageType = UIPageType.Unknown;

    private void Awake()
    {
        foreach (var config in pageConfigs)
        {
            if (!_configLookup.ContainsKey(config.pageType))
                _configLookup.Add(config.pageType, config);
        }
    }

    public async UniTask ShowPageAsync(UIPageType pageType, bool addToStack = true)
    {
        if (_activePages.ContainsKey(_currentPageType) && _currentPageType != pageType)
        {
            // Hide previous page if it's not an overlay
            var currentConfig = GetPageConfig(_currentPageType);
            if (currentConfig.layer != UILayer.Overlay)
            {
                _activePages[_currentPageType].SetActive(false);

                if (addToStack)
                    _navigationStack.Push(_currentPageType);
            }
        }

        if (_activePages.ContainsKey(pageType))
        {
            _activePages[pageType].SetActive(true);
        }
        else
        {
            var config = GetPageConfig(pageType);
            if (config == null)
            {
                Debug.LogError($"No config found for page type: {pageType}");
                return;
            }

            Transform parent = GetParentLayer(config.layer);
            var pageGO = await _uiFactory.CreatePageAsync(config, parent);
            _activePages[pageType] = pageGO;

            var controller = pageGO.GetComponent<UIBaseController>();
            controller?.Init();
        }

        _currentPageType = pageType;
        _stateMachine.SetState(pageType);
    }

    public void HidePage(UIPageType pageType)
    {
        if (_activePages.ContainsKey(pageType))
        {
            _activePages[pageType].SetActive(false);
        }
    }

    public async UniTask<bool> GoBackAsync()
    {
        if (_navigationStack.Count == 0)
        {
            Debug.Log("No previous page in navigation stack.");
            return false;
        }

        var previousPage = _navigationStack.Pop();
        await ShowPageAsync(previousPage, addToStack: false);
        return true;
    }

    private UIPageConfigSO GetPageConfig(UIPageType type)
    {
        return _configLookup.TryGetValue(type, out var config) ? config : null;
    }

    private Transform GetParentLayer(UILayer layer)
    {
        return layer switch
        {
            UILayer.Main => mainLayer,
            UILayer.Popup => popupLayer,
            UILayer.Overlay => overlayLayer,
            _ => mainLayer
        };
    }

    public void ClearNavigationStack()
    {
        _navigationStack.Clear();
    }
}
