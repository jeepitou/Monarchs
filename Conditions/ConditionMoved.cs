using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Condition that checks if the caster has already attacked or not.
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Moved", order = 10)]
    public class ConditionMoved : ConditionData
    {
        public bool hasMoved;

        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            return (args.caster.numberOfMoveThisTurn != 0) == hasMoved;
        }
        
        public override bool IsTriggerConditionMetNoTarget(Game data, AbilityArgs args)
        {
            return (args.caster.numberOfMoveThisTurn != 0) == hasMoved;
        }
    }
}