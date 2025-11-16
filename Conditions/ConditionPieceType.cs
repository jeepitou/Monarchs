using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Condition that checks the type, team and traits of a card
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/PieceType", order = 10)]
    public class ConditionPieceType : ConditionData
    {
        [Header("Piece is of type")] 
        public PieceType has_type;

        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            if (args.target == null)
            {
                Debug.LogError("Tried to apply ConditionCardType on null target.");
                return false;
            }
            
            return CompareBool(args.CardTarget.CardData.type == has_type, oper);
        }

        public override bool IsTargetConditionMetPlayerTarget(Game data, AbilityArgs args)
        {
            return false; //Not a card
        }

        public override bool IsTargetConditionMetCardDataTarget(Game data, AbilityArgs args)
        {
            if (args.target == null)
            {
                Debug.LogError("Tried to apply ConditionCardType on null target.");
                return false;
            }
            
            return CompareBool(args.CardDataTarget.type == has_type, oper);
        }
    }
}