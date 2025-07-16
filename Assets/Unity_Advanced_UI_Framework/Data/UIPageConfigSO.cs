using UnityEngine;

public enum UILayer
{
    Main,
    Popup,
    Overlay
}

[CreateAssetMenu(fileName = "UIPageConfig", menuName = "UI/Page Config")]
public class UIPageConfigSO : ScriptableObject
{
    public UIPageType pageType;
    public GameObject prefab;
    public UILayer layer = UILayer.Main;
    public bool usePooling = true;
}
