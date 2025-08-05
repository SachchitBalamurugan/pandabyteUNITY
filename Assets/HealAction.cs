using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerAction", menuName = "TurnBased/Heal Action")]
public class HealAction : BaseAction
{
    [SerializeField] float healAmount;

    public override ActionType Type { get => ActionType.SelfAction; }

    public override void PerformAction(Entity sender, FighterData target)
    {
        if (sender.Fighters.Contains(target))
        {
            target.TakeDamage(healAmount);
        }
    }

    public override bool IsTarget(Entity sender, FighterData target)
    {
        return !sender.Fighters.Contains(target);
    }
}