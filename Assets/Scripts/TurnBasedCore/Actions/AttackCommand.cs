using System.Collections;
using System.Collections.Generic;
using TurnBasedCore.Core.Actions;
using UnityEngine;

public class AttackCommand : IActionCommand
{
    private ActionContext context;

    public AttackCommand(ActionContext ctx)
    {
        context = ctx;
    }

    public void Execute()
    {
        // Damage context.TargetPlayer with context.Data (e.g. 10 damage)
    }

    public void Undo() { }
}

