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
    /// Condition that check the owner of the target match the owner of the caster
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardOwner", order = 10)]
    public class ConditionOwner : ConditionData
    {
        [Header("Card is Owned by You")]
        public ConditionOperatorBool oper;
        public bool checkTriggerer = false;

        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            if (args.caster == null || args.target == null)
            {
                Debug.LogError("Tried to apply ConditionOwner on null caster or target");
                return false;
            }

            if (checkTriggerer)
            {
                args.target = args.triggerer;
            }
            
            bool same_owner = args.caster.playerID == args.CardTarget.playerID;
            return CompareBool(same_owner, oper);
        }

        public override bool IsTargetConditionMetPlayerTarget(Game data, AbilityArgs args)
        {
            if (args.caster == null || args.target == null)
            {
                Debug.LogError("Tried to apply ConditionOwner on null caster or target");
                return false;
            }
            
            bool same_owner = args.caster.playerID == args.PlayerTarget.playerID;
            return CompareBool(same_owner, oper);
        }
        
        public override bool IsTriggerConditionMetNoTarget(Game data, AbilityArgs args)
        {
            if (checkTriggerer)
            {
                bool same_owner = false;
                if (args.caster == null)
                {
                    same_owner = args.castedCard.GetPlayerId() == args.triggerer.GetPlayerId();
                }
                else
                {
                    same_owner = args.caster.playerID == args.triggerer.GetPlayerId();
                
                }
                return CompareBool(same_owner, oper);
            }

            return true;
        }
    }
}