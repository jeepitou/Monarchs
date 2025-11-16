using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// SlotValue compare each slot x and y to a specific value, like slot.x >=3 and slot.y < 5
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/SlotValue", order = 11)]
    public class ConditionSlotValue : ConditionData
    {
        [Header("Slot Value")]
        public ConditionOperatorInt oper_x;
        public int value_x = 0;

        public ConditionOperatorInt oper_y;
        public int value_y = 0;
        
        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            args.target = args.CardTarget.slot;
            return IsTargetConditionMetSlotTarget(data, args);
        }

        public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
        {
            bool valid_x = CompareInt(args.SlotTarget.x, oper_x, value_x);
            bool valid_y = CompareInt(args.SlotTarget.y, oper_y, value_y);
            return valid_x && valid_y;
        }
    }
}