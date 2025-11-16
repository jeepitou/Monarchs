using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Add this to an ability to prevent it from being cast more than once per turn
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/SmokeSteedTargetHasAnEmptySquare", order = 10)]
    public class ConditionSmokeSteedTargetHasAnEmptySquare : ConditionData
    {
        public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
        {
            Slot targetSlot = ((Card)data.savedTargetForAbility).slot;

            Vector2S offset = targetSlot.GetCoordinate() - args.caster.slot.GetCoordinate();
            
            Vector2S newTargetPosition = args.SlotTarget.GetCoordinate() + offset;
            Slot newTargetSlot = Slot.Get(newTargetPosition);
            
            return data.GetSlotCard(newTargetSlot) == null && newTargetSlot.IsValid();
        }

    }
}