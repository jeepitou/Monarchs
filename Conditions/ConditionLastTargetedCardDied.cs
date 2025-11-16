using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{


    /// <summary>
    /// Trigger condition that count the amount of cards in pile of your choise (deck/discard/hand/board...)
    /// Can also only count cards of a specific type/team/trait
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/ConditionLastTargetedCardDied", order = 10)]
    public class ConditionLastTargetedCardDied : ConditionData
    {
        public override bool IsTriggerConditionMetNoTarget(Game data, AbilityArgs args)
        {
            Slot targetOfCard = args.castedCard.slot;
            
            return data.GetSlotCard(targetOfCard) == null;
        }
    }
}