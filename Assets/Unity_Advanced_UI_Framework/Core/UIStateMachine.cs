using System.Collections.Generic;
using UnityEngine;

public class UIStateMachine
{
    public UIPageType CurrentState { get; private set; }
    public UIPageType? PreviousState => _history.Count > 1 ? _history.Peek() : null;

    private readonly Stack<UIPageType> _history = new();

    public void SetState(UIPageType newState)
    {
        if (_history.Count == 0 || _history.Peek() != newState)
            _history.Push(newState);

        CurrentState = newState;

        Debug.Log($"UI State changed to: {CurrentState}");
    }

    public UIPageType? GoBack()
    {
        if (_history.Count > 1)
        {
            // Pop current
            _history.Pop();
            // Go to previous
            CurrentState = _history.Peek();
            Debug.Log($"UI State rolled back to: {CurrentState}");
            return CurrentState;
        }

        Debug.LogWarning("No previous UI state to go back to.");
        return null;
    }

    public void Reset()
    {
        _history.Clear();
        CurrentState = default;
    }
}
