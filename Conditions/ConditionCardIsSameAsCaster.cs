using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Condition that checks if the card is the same as the caster. (Same CardData)
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardIsSameAsCaster", order = 10)]
    public class ConditionCardIsSameAsCaster : ConditionData
    {
        public bool isSame = true;

        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            return (args.caster.cardID == args.CardTarget.cardID) == isSame;
        }

        public override bool IsTargetConditionMetPlayerTarget(Game data, AbilityArgs args)
        {
            return false; //Not a card
        }

        public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
        {
            return false; //Not a card
        }
    }
}