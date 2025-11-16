using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect that sets custom stats to a specific value
    /// </summary>
    
    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/SetStatCustom", order = 10)]
    public class EffectSetStatCustom : EffectData
    {
        public TraitData trait;

        public override void DoEffectPlayerTarget(GameLogic logic, AbilityArgs args)
        {
            args.PlayerTarget.SetTrait(trait.id, args.ability.value);
        }

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            args.CardTarget.SetTrait(trait.id, args.ability.value);
        }

        public override void DoOngoingEffectPlayerTarget(GameLogic logic, AbilityArgs args)
        {
            args.PlayerTarget.SetTrait(trait.id, args.ability.value);
        }

        public override void DoOngoingEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            args.CardTarget.SetTrait(trait.id, args.ability.value);
        }
    }
}