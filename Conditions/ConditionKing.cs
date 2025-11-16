using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine.Client;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Condition that check if the card is a king (or is not a king)
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/ConditionKing")]
    public class ConditionKing : ConditionData
    {
        [Header("Card is a king")]
        public bool isKing;
        public bool checkTriggerer;

        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            Card target = args.CardTarget;
            if (checkTriggerer)
            {
                target = (Card)args.triggerer;
            }
            
            if (target == null)
            {
                Debug.LogError("Tried to apply ConditionKing on null target");
                return false;
            }
            
            return isKing == target.IsMonarch();
        }
        
        public override bool IsTriggerConditionMetNoTarget(Game data, AbilityArgs args)
        {
            if (args.target is Slot)
            {
                args.target = data.GetSlotCard(args.SlotTarget);
            }

            return IsTargetConditionMetCardTarget(data, args);
        }
    }
}