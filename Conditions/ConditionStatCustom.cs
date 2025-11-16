using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Compares cards or players custom stats
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/StatCustom", order = 10)]
    public class ConditionStatCustom : ConditionData
    {
        [Header("Card stat is")]
        public TraitData trait;
        public ConditionOperatorInt oper;
        public int value;

        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            return CompareInt(args.CardTarget.GetTraitValue(trait.id), oper, value);
        }

        public override bool IsTargetConditionMetPlayerTarget(Game data, AbilityArgs args)
        {
            return CompareInt(args.PlayerTarget.GetTraitValue(trait.id), oper, value);
        }
    }
}