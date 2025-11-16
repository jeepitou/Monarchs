using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Condition that check if the CardData is a valid deckbuilding card (not a summon token)
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardDeckbuilding", order = 10)]
    public class ConditionDeckbuilding : ConditionData
    {
        [Header("Card is Deckbuilding")]
        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            if (args.target == null)
            {
                Debug.LogError("Tried to apply ConditionDeckbuilding on null target.");
                return false;
            }
            
            return CompareBool(args.CardTarget.CardData.deckBuilding, oper);
        }

        public override bool IsTargetConditionMetCardDataTarget(Game data, AbilityArgs args)
        {
            if (args.target == null)
            {
                Debug.LogError("Tried to apply ConditionDeckbuilding on null target.");
                return false;
            }
            
            return CompareBool(args.CardDataTarget.deckBuilding, oper);
        }
    }
}