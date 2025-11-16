using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect that adds or removes card/player custom stats
    /// </summary>
    
    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/AddStatCustom", order = 10)]
    public class EffectAddStatCustom : EffectData
    {
        public TraitData trait;

        public override void DoEffectPlayerTarget(GameLogic logic, AbilityArgs args)
        {
            ((Player)args.target).AddTrait(trait.id, args.ability.value);
        }

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            ((Card)args.target).AddTrait(trait.id, args.ability.value);
        }

        public override void DoOngoingEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            ((Card)args.target).AddOngoingTrait(trait.id, args.ability.value);
        }

        public override void DoOngoingEffectPlayerTarget(GameLogic logic, AbilityArgs args)
        {
            ((Player)args.target).AddOngoingTrait(trait.id, args.ability.value);
        }
    }
}