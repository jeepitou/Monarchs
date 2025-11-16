using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// SlotRange check each axis variable individualy for range between the caster and target
    /// If you want to check the travel distance instead (all at once) use SlotDist
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/SlotRange", order = 11)]
    public class ConditionSlotRange : ConditionData
    {
        [Header("Slot Range")]
        public int range_x = 1;
        public int range_y = 1;

        public bool checkTriggererPosition;
        public int checkFromTargetNumber = -1;

        public override bool IsTriggerConditionMetNoTarget(Game data, AbilityArgs args)
        {
            Slot targetSlot = args.target.GetSlot();
            if (checkTriggererPosition)
            {
                targetSlot = ((Card)args.triggerer).slot;
            }

            Slot casterSlot = args.caster.slot;
            if (checkFromTargetNumber >= 1)
            {
                casterSlot = ((Card)data.selectorTargets[checkFromTargetNumber - 1]).slot;
            }
            
            int dist_x = Mathf.Abs(casterSlot.x - targetSlot.x);
            int dist_y = Mathf.Abs(casterSlot.y - targetSlot.y);
            return dist_x <= range_x && dist_y <= range_y;
        }

        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            args.target = args.CardTarget.slot;
            return IsTargetConditionMetSlotTarget(data, args);
        }

        public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
        {          
            Slot slotTarget = args.SlotTarget;
            
            if (checkTriggererPosition)
            {
                slotTarget = ((Card)args.triggerer).slot;
            }
            
            Slot casterSlot = args.caster.slot;
            if (checkFromTargetNumber >= 1)
            {
                casterSlot = ((Card)data.selectorTargets[checkFromTargetNumber - 1]).slot;
            }
            
            int dist_x = Mathf.Abs(casterSlot.x - slotTarget.x);
            int dist_y = Mathf.Abs(casterSlot.y - slotTarget.y);
            return dist_x <= range_x && dist_y <= range_y;
        }
    }
}