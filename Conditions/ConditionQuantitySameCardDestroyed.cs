using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Condition that checks if enough card died to trigger a blood sacrifice.
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/SameCardDestroyed")]
    public class ConditionQuantitySameCardDestroyed : ConditionData
    {
        public int quantityOfDestroyedCard;
        public bool sameCohort;

        public override bool IsTriggerConditionMetNoTarget(Game data, AbilityArgs args)
        {
            if (args.caster == null || data == null)
            {
                Debug.LogError("Tried to apply ConditionSameCardDestroyed on null caster or Game.");
                return false;
            }

            if (quantityOfDestroyedCard <= 0)
            {
                Debug.LogError("Invalid quantityOfDestroyedCard for ConditionQuantitySameCardDestroyed.");
                return false;
            }

            int destroyedCard = args.caster.numberOfCohortUnitDied;

            if (destroyedCard == quantityOfDestroyedCard)
            {
                return true;
            }
            
            return false; 
        }
    }
}