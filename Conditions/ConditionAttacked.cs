using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Condition that checks if the caster has already attacked or not.
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Attacked", order = 10)]
    public class ConditionAttacked : ConditionData
    {
        public bool hasAttacked;

        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            return args.caster.hasAttacked == hasAttacked;
        }
        
        public override bool IsTriggerConditionMetNoTarget(Game data, AbilityArgs args)
        {
            return args.caster.hasAttacked == hasAttacked;
        }
    }
}