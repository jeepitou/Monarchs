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
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/ConditionPieceDiedOnSlot", order = 10)]
    public class ConditionPieceDiedOnSlot : ConditionData
    {
        public ConditionCardType conditionCardType;
        
        public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
        {
            List<Card> cards = new List<Card>();
            cards = data.GetAllDiedCardOnSlot(args.SlotTarget);

            if (cards.Count == 0)
            {
                return false;
            }
            
            if (conditionCardType == null)
            {
                return true;
            }
            
            foreach (var card in cards)
            {
                AbilityArgs argsCopy = args.Clone();
                argsCopy.target = card;
                
                if (conditionCardType.IsTargetConditionMetCardTarget(data, argsCopy))
                {
                    return true;
                }
            }

            return false;
        }
    }
}