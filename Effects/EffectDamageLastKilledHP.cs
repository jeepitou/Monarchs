using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect that damages a card or a player (lose hp)
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/DamageLastKilledHP", order = 10)]
    public class EffectDamageLastKillHp : EffectData
    {
        public int additional_damage;
        public int maxDamage;
        public TraitData bonus_damage;
        public bool armor_penetration;

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            int damage = GetDamage(logic.Game, args.caster, additional_damage);
            damage = Mathf.Min(damage, maxDamage);
            logic.DamageCard(args.caster, args.CardTarget, damage, armor_penetration);
        }

        private int GetDamage(Game data, Card caster, int value)
        {
            Player player = data.GetPlayer(caster.playerID);
            int damage = data.lastKilled.GetHPMax() + value + caster.GetTraitValue(bonus_damage) + player.GetTraitValue(bonus_damage);
            return damage;
        }

        public override int GetAiValue(AbilityData ability)
        {
            return -1;
        }
    }
}