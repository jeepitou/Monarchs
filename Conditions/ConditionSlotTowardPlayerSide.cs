using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{

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
                slotToCheckFrom = ((Card)data.selectorTargets[checkFromTargetNumber - 1]).slot;
                playerID = ((Card)data.selectorTargets[checkFromTargetNumber - 1]).playerID;
            }
            
            if (isTowardOpponentSide)
            {
                if (playerID == 1)
                {
                    return args.SlotTarget.y > slotToCheckFrom.y;
                }
                else
                {
                    return args.SlotTarget.y < slotToCheckFrom.y;
                }
            }
            else
            {
                if (playerID == 1)
                {
                    return args.SlotTarget.y < slotToCheckFrom.y;
                }
                else
                {
                    return args.SlotTarget.y > slotToCheckFrom.y;
                }
            }
            
        }
        
    }
}