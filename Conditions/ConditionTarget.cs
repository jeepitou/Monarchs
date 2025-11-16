using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;
using TcgEngine.AI;

namespace TcgEngine
{
    /// <summary>
    /// Condition that compares the target category of an ability to the actual target (card, player or slot)
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Player", order = 10)]
    public class ConditionTarget : ConditionData
    {
        [Header("Target is of type")]
        public ConditionTargetType type;
        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            return CompareBool(type == ConditionTargetType.Card, oper); //Is Card
        }

        public override bool IsTargetConditionMetPlayerTarget(Game data, AbilityArgs args)
        {
            return CompareBool(type == ConditionTargetType.Player, oper); //Is Player
        }

        public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
        {
            return CompareBool(type == ConditionTargetType.Slot, oper); //Is Player
        }
    }

    public enum ConditionTargetType
    {
        None = 0,
        Card = 10,
        Player = 20,
        Slot = 30,
    }
}