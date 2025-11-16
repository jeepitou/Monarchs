using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Checks if a slot contains a card or not
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/SlotEmpty", order = 11)]
    public class ConditionSlotEmpty : ConditionData
    {
        [Header("Slot Is Empty")]
        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            return CompareBool(false, oper); //Target is not empty slot
        }

        public override bool IsTargetConditionMetPlayerTarget(Game data, AbilityArgs args)
        {
            return CompareBool(false, oper); //Target is not empty slot
        }

        public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
        { 
            Card slot_card = data.GetSlotCard(args.SlotTarget);
            return CompareBool(slot_card == null, oper);
        }
    }
}