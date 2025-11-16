using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    public enum ConditionStatType
    {
        None = 0,
        Attack = 10,
        HP = 20,
        Mana = 30,
        Cohort = 31,
    }

    /// <summary>
    /// Compares basic card or player stats such as attack/hp/mana
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Stat", order = 10)]
    public class ConditionStat : ConditionData
    {
        [Header("Card stat is")]
        public ConditionStatType type;
        public ConditionOperatorInt oper;
        public int value;

        public override bool IsTriggerConditionMetNoTarget(Game data, AbilityArgs args)
        {
            if (type == ConditionStatType.Cohort)
            {
                return CompareInt(args.castedCard.cohortSize, oper, value);
            }

            return true;
        }

        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            if (type == ConditionStatType.Attack)
            {
                return CompareInt(args.CardTarget.attack, oper, value);
            }

            if (type == ConditionStatType.HP)
            {
                return CompareInt(args.CardTarget.GetHP(), oper, value);
            }
            
            if (type == ConditionStatType.Cohort)
            {
                return CompareInt(args.castedCard.cohortSize, oper, value);
            }

            return false;
        }

        public override bool IsTargetConditionMetPlayerTarget(Game data, AbilityArgs args)
        {
            if (type == ConditionStatType.HP)
            {
                return CompareInt(args.PlayerTarget.king.hpOngoing, oper, value);
            }

            return false;
        }
    }
}