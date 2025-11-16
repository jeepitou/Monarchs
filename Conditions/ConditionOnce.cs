using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Add this to an ability to prevent it from being cast more than once per turn
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/OncePerTurn", order = 10)]
    public class ConditionOnce : ConditionData
    {
        public override bool IsTriggerConditionMetNoTarget(Game data, AbilityArgs args)
        {
            if (args.ability == null || data == null)
            {
                Debug.LogError("Tried to apply ConditionOnce on null ability or game");
                return false;
            }
            return !data.abilityPlayed.Contains(args.ability.id);
        }

    }
}