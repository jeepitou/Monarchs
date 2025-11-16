using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Condition that check if the card is exhausted or not
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardExhausted", order = 10)]
    public class ConditionExhaust : ConditionData
    {
        [Header("Target is exhausted")]
        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            if (args.target == null)
            {
                Debug.LogError("Tried to apply ConditionExhaust on null target");
                return false;
            }
            return CompareBool(args.CardTarget.exhausted, oper);
        }

        public override bool IsTargetConditionMetPlayerTarget(Game data, AbilityArgs args)
        {
            return false;
        }

        public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
        {
            if (data == null || !args.SlotTarget.IsValid())
            {
                Debug.LogError("Tried to apply ConditionExhaust on null data or target");
                return false;
            }
            
            Card card = data.GetSlotCard(args.SlotTarget);
            if (card != null)
            {
                return CompareBool(card.exhausted, oper);
            }
            return false;
        }
    }
}