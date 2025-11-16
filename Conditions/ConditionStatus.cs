using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    //Checks if a player or card has a status effect
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardStatus", order = 10)]
    public class ConditionStatus : ConditionData
    {
        [Header("Card has status")]
        public StatusType has_status;
        public int value = 0;
        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            bool hstatus = args.CardTarget.HasStatus(has_status) && args.CardTarget.GetStatusValue(has_status) >= value;
            return CompareBool(hstatus, oper);
        }

        public override bool IsTargetConditionMetPlayerTarget(Game data, AbilityArgs args)
        {
            bool hstatus = args.PlayerTarget.HasStatusEffect(has_status) && args.PlayerTarget.GetStatusEffectValue(has_status) >= value;
            return CompareBool(hstatus, oper);
        }

        public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
        {
            Card card = data.GetSlotCard(args.SlotTarget);
            if (card != null)
            {
                args.target = card;
                return IsTargetConditionMetCardTarget(data, args);
            }
                
            return false;
        }
    }
}