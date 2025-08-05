using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerAction", menuName = "TurnBased/Attack Action")]
public class AttackAction : BaseAction
{
    [SerializeField] float damageAmount;
    public override ActionType Type { get => ActionType.AttackAction; }

    public override void PerformAction(Entity sender, FighterData target)
    {
        if (!sender.Fighters.Contains(target))
        {
            target.TakeDamage(damageAmount);
        }
    }

    public override bool IsTarget(Entity sender, FighterData target)
    {
        return !sender.Fighters.Contains(target);
    }
}
