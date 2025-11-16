using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Condition that check if the target is the same as the caster
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardSelf", order = 10)]
    public class ConditionSelf : ConditionData
    {
        [Header("Target is caster")]
        public ConditionOperatorBool oper;

        
        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            if (args.caster == null || args.target == null)
            {
                Debug.LogError("Tried to apply ConditionSelf on null caster or target");
                return false;
            }
            return CompareBool(args.caster == args.CardTarget, oper);
        }

        public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
        {
            if (args.caster == null || !args.SlotTarget.IsValid())
            {
                Debug.LogError("Tried to apply ConditionSelf on null caster or target");
                return false;
            }
            bool same_owner = args.SlotTarget == args.caster.slot;
            return CompareBool(same_owner, oper);
        }
    }
}