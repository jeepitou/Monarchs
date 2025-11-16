using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Checks if its your turn
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/RolledValue", order = 10)]
    public class ConditionRolled : ConditionData
    {
        [Header("Value Rolled is")]
        public ConditionOperatorInt oper;
        public int value;

        public override bool IsTriggerConditionMetNoTarget(Game data, AbilityArgs args)
        {
            if (data == null)
            {
                Debug.LogError("Tried to apply ConditionRolled on null Game");
                return false;
            }
            return CompareInt(data.rolledValue, oper, value);
        }

        public override bool IsTargetConditionMetPlayerTarget(Game data, AbilityArgs args)
        {
            if (data == null)
            {
                Debug.LogError("Tried to apply ConditionRolled on null Game");
                return false;
            }
            return CompareInt(data.rolledValue, oper, value);
        }

        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            if (data == null)
            {
                Debug.LogError("Tried to apply ConditionRolled on null Game");
                return false;
            }
            return CompareInt(data.rolledValue, oper, value);
        }

        public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
        {
            if (data == null)
            {
                Debug.LogError("Tried to apply ConditionRolled on null Game");
                return false;
            }
            return CompareInt(data.rolledValue, oper, value);
        }
    }
}