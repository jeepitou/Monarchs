using System;
using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;
using Random = System.Random;

namespace TcgEngine
{
    /// <summary>
    /// Effect that damages a card or a player (lose hp)
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/DamageRandom")]
    public class EffectDamageRandom : EffectData
    {
        public TraitData bonus_damage;

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            int damage = GetDamage(logic.Game, args.caster, args.ability.value, args.ability.value2);
            
            logic.DamageCard(args.caster, args.CardTarget, damage);
        }

        private int GetDamage(Game data, Card caster, int minValue, int maxValue)
        {
            Player player = data.GetPlayer(caster.playerID);
            Random random = new Random();
            int damage = minValue + caster.GetTraitValue(bonus_damage) + player.GetTraitValue(bonus_damage) +
                         random.Next(0, maxValue - minValue+1);
            return damage;
        }

        public override int GetAiValue(AbilityData ability)
        {
            return -1;
        }
    }
}