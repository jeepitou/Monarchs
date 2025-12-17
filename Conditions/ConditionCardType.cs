using System.Collections;
using System.Collections.Generic;
using Monarchs;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Condition that checks the type, team and traits of a card
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardType", order = 10)]
    public class ConditionCardType : ConditionData
    {
        [Header("Card is of type")] 
        public CardType has_type;
        public GuildData hasGuild;
        public TraitData has_trait;
        public SubtypeData has_subtype;

        public ConditionOperatorBool oper;

        public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
        {
            if (args.target == null)
            {
                Debug.LogError("Tried to apply ConditionCardType on null target.");
                return false;
            }
            
            bool hasTrait = args.CardTarget.IsTeamTraitAndType(hasGuild, has_trait, has_subtype, has_type);
            
            return CompareBool(hasTrait, oper);
        }

        public override bool IsTargetConditionMetPlayerTarget(Game data, AbilityArgs args)
        {
            return false; //Not a card
        }

        public override bool IsTargetConditionMetSlotTarget(Game data, AbilityArgs args)
        {
            return false; //Not a card
        }

        public override bool IsTargetConditionMetCardDataTarget(Game data, AbilityArgs args)
        {
            if (args.target == null)
            {
                Debug.LogError("Tried to apply ConditionCardType on null target.");
                return false;
            }

            bool hasTrait = args.CardDataTarget.IsTeamTraitAndType(hasGuild, has_trait, has_subtype, has_type);
            return CompareBool(hasTrait, oper);
        }
    }
}