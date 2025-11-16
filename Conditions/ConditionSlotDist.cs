using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// SlotDist is the travel distance from the caster to the target
    /// Unlike SlotRange which is just checking each X,Y,P separately
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/SlotDist", order = 11)]
    public class ConditionSlotDist : ConditionData
    {
        [Header("Slot Distance")]
        public int distance = 1;
        public bool diagonals;
        
        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            if (args.caster == null || args.target == null)
            {
                Debug.LogError("Tried to apply ConditionSlotDist on null caster or target");
                return false;
            }

            args.target = args.target.GetSlot();
            return IsTargetConditionMetSlotTarget(data, args);
        }

        public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
        {
            if (args.caster == null || !args.SlotTarget.IsValid() || !args.caster.slot.IsValid() )
            {
                Debug.LogError("Tried to apply ConditionSlotDist on null caster or target");
                return false;
            }

            if (distance <= 0)
            {
                Debug.LogError("Tried to apply ConditionSlotDist with a distance of 0 or less");
                return false;
            }
            
            Slot cslot = args.caster.slot;
            if (diagonals)
                return cslot.IsInDistance(args.SlotTarget, distance);
            return cslot.IsInDistanceStraight(args.SlotTarget, distance);
        }
    }
}