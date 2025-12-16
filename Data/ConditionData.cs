using System;
using System.Collections;
using System.Collections.Generic;
using Monarchs;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Base class for all ability conditions, override the IsConditionMet function
    /// </summary>

    public class ConditionData : ScriptableObject
    {
        public virtual bool IsTargetConditionMet(Game game, AbilityArgs args)
        {
            if (args.target is Card)
            {
                return IsTargetConditionMetCardTarget(game, args);
            }
            else if (args.target is Player)
            {
                return IsTargetConditionMetPlayerTarget(game, args);
            }
            else if (args.target is Slot)
            {
                return IsTargetConditionMetSlotTarget(game, args);
            }
            else if (args.target is CardData)
            {
                return IsTargetConditionMetCardDataTarget(game, args);
            }
            else
            {
                if (args.ability.targetType == AbilityTargetType.SelectTarget)
                {
                    return true;
                }
                throw new ArgumentException("Unsupported target type");
            }
        }
        
        public virtual bool IsTriggerConditionMetNoTarget(Game data, AbilityArgs args)
        {
            return true; //Override this, applies to any target, always checked
        }

        public virtual bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            Slot target = args.CardTarget.slot;
            AbilityArgs newArgs = args.Clone();
            newArgs.target = target;
            return IsTargetConditionMetSlotTarget(data, newArgs); //Override this, condition targeting card
        }

        public virtual bool IsTargetConditionMetPlayerTarget(Game data, AbilityArgs args)
        {
            return true; //Override this, condition targeting player
        }

        public virtual bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
        {
            Card target = data.GetSlotCard(args.SlotTarget);

            if (target != null)
            {
                args.target = target;
                return IsTargetConditionMetCardTarget(data, args);
            }
                
            return false; //Override this, condition targeting slot
        }

        public virtual bool IsTargetConditionMetCardDataTarget(Game data, AbilityArgs args)
        {
            return true; //Override this, for effects that create new cards
        }

        public virtual bool CompareBool(bool condition, ConditionOperatorBool oper)
        {
            if (oper == ConditionOperatorBool.IsFalse)
                return !condition;
            return condition;
        }

        public virtual bool CompareInt(int ival1, ConditionOperatorInt oper, int ival2)
        {
            if (oper == ConditionOperatorInt.Equal)
            {
                return ival1 == ival2;
            }
            if (oper == ConditionOperatorInt.NotEqual)
            {
                return ival1 != ival2;
            }
            if (oper == ConditionOperatorInt.GreaterEqual)
            {
                return ival1 >= ival2;
            }
            if (oper == ConditionOperatorInt.LessEqual)
            {
                return ival1 <= ival2;
            }
            if (oper == ConditionOperatorInt.Greater)
            {
                return ival1 > ival2;
            }
            if (oper == ConditionOperatorInt.Less)
            {
                return ival1 < ival2; ;
            }
            return false;
        }
    }

    public enum ConditionOperatorInt
    {
        Equal,
        NotEqual,
        GreaterEqual,
        LessEqual,
        Greater,
        Less,
    }

    public enum ConditionOperatorBool
    {
        IsTrue,
        IsFalse,
    }
}