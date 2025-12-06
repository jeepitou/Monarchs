using Monarchs.Ability.Target;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using UnityEngine;

namespace Monarchs.Conditions
{
    /// <summary>
    /// This condition checks if the slot target is toward the player side or opponent side from the caster or another target.
    /// </summary>
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/ConditionSlotTowardPlayerSide", order = 10)]
    public class ConditionSlotTowardPlayerSide : ConditionData
    {
        public bool isTowardOpponentSide = true;
        public int checkFromTargetNumber = -1;
 
        
        public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
        {
            int playerID = args.caster.playerID;
            Slot slotToCheckFrom = args.caster.slot;
            if (checkFromTargetNumber >= 1)
            {
                ITargetable target = data.selectorTargets[checkFromTargetNumber - 1];
                
                if (!IsTargetNumberValid(data) || !IsTargetValid(target))
                {
                    Debug.LogWarning("ConditionSlotTowardPlayerSide: Invalid target number or target type.");
                    return false;
                }
                
                slotToCheckFrom = ((Card)target).slot;
                playerID = ((Card)target).playerID;
            }
            
            return isTowardOpponentSide
                ? (playerID == 1
                    ? args.SlotTarget.y > slotToCheckFrom.y
                    : args.SlotTarget.y < slotToCheckFrom.y)
                : (playerID == 1
                    ? args.SlotTarget.y < slotToCheckFrom.y
                    : args.SlotTarget.y > slotToCheckFrom.y);
            
        }
        
        private bool IsTargetNumberValid(Game data)
        {
            return data.selectorTargets.Count >= checkFromTargetNumber;
        }
        
        private bool IsTargetValid(ITargetable target)
        {
            return target != null && target is Card;
        }
    }
}