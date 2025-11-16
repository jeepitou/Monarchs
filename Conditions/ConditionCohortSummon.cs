using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/ConditionCohortSummon", order = 10)]
    public class ConditionCohortSummon : ConditionData
    {
        public bool checkCaster = true;
        public bool isCohortSummon = false;

        public override bool IsTriggerConditionMetNoTarget(Game data, AbilityArgs args)
        {
            return CheckCaster(args);
        }
        
        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            return CheckCaster(args);
        }

        public override bool IsTargetConditionMetPlayerTarget(Game data, AbilityArgs args)
        {
            return CheckCaster(args);
        }

        public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
        {
            return CheckCaster(args);
        }

        public override bool IsTargetConditionMetCardDataTarget(Game data, AbilityArgs args)
        {
            return CheckCaster(args);
        }
        
        private bool CheckCaster(AbilityArgs args)
        {
            if (checkCaster && args.caster != null)
            {
                return args.caster.cohortSummon == isCohortSummon;
            }
            
            Debug.LogError("ConditionCohortSummon: Only checkCaster is supported");
            return false;
        }
    }
}