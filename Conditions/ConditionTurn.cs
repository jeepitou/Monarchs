using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Checks if its your turn
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Turn", order = 10)]
    public class ConditionTurn : ConditionData
    {
        public TurnType turnType;
        public ConditionOperatorBool oper;

        public override bool IsTriggerConditionMetNoTarget(Game data, AbilityArgs args)
        {
            bool yourTurn = false;
            if (turnType == TurnType.Player)
            {
                yourTurn = args.caster.playerID == data.CurrentPlayer;    
            }
            else
            {
                yourTurn = data.GetCurrentCardTurn().Contains(args.castedCard);
            }
            
            return CompareBool(yourTurn, oper);
        }

        public enum TurnType
        {
            Player,
            Card
        }
    }
}