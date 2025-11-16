using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using TcgEngine.Client;
using UnityEngine;

/// <summary>
/// This condition validates if the slot is in the playable square of the card.
/// </summary>

[CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/ConditionCardPlayableOnSlot")]
public class ConditionCardPlayableOnSlot : ConditionData
{
    
    public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
    {
        if (args.caster == null || args.CardTarget == null)
        {
            Debug.LogError("Tried to apply ConditionCardPlayableOnSlot on null target or caster.");
            return false;
        }

        args.target = args.CardTarget.slot;
        return IsTargetConditionMetSlotTarget(data, args);
    }

    public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
    {
        if (args.caster == null || !args.SlotTarget.IsValid())
        {
            Debug.LogError("Tried to apply ConditionCardPlayableOnSlot on null target or caster.");
            return false;
        }
        
        return data.CardCanSpawnOnSlot(args.castedCard, args.SlotTarget, false);
    }
}