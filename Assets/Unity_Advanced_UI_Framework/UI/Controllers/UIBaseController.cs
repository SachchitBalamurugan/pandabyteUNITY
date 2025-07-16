using UnityEngine;

public abstract class UIBaseController : MonoBehaviour
{
    /// <summary>
    /// Called when the UI prefab is instantiated.
    /// Use this to bind view logic and setup listeners.
    /// </summary>
    public abstract void Init();

    /// <summary>
    /// Called when the page is shown (optional).
    /// </summary>
    public virtual void OnShow() { }

    /// <summary>
    /// Called when the page is hidden (optional).
    /// </summary>
    public virtual void OnHide() { }
}
