using UnityEngine;

public class BaseAction : ScriptableObject
{
    public string actionName = "New Action";
    public int actionCost = 1; // e.g., action points

    public Sprite icon;
    public AudioClip actionSound;
    public AnimationClip animationClip;

    [TextArea]
    public string description;

    public virtual ActionType Type { get; protected set; }

    public enum ActionType
    {
        SelfAction,
        AttackAction
    }// for ui stuff i gess;

    public virtual void PerformAction(Entity sender ,FighterData target)
    {

    }

    public virtual bool IsTarget(Entity sender, FighterData target)
    {
        return false;
    }
}
