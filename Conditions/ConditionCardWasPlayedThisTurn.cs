using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using UnityEngine;

namespace Monarchs
{
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/WasPlayedThisTurn", order = 10)]
    public class ConditionCardWasPlayedThisTurn : ConditionData
    {
        public bool wasPlayedThisTurn;

        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            return args.CardTarget.wasPlayedThisTurn == wasPlayedThisTurn;
        }
    }
}
