using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/NotInCurrentTargets", order = 10)]
    public class ConditionNotInCurrentTargets : ConditionData
    {
        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            if (data.selectorTargets.Contains(args.target) || data.selectorTargets.Contains(args.target.GetSlot()))
            {
                return false;
            }

            return true;
        }
        
        public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
        {
            Card card = data.GetSlotCard(args.target.GetSlot());
            
            if (data.selectorTargets.Contains(args.target) || 
                (card != null && data.selectorTargets.Contains(card)))
            {
                return false;
            }
                
            return true; 
        }

    }
}