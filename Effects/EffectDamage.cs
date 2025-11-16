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

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Damage", order = 10)]
    public class EffectDamage : EffectData
    {
        public bool armorPenetrating = false;
        public bool triggersDyingWish = true;
        public TraitData bonus_damage;

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            int damage = GetDamage(logic.Game, args.caster, args.ability.value);
            logic.DamageCard(args.caster, args.CardTarget, damage, armorPenetrating, false, triggersDyingWish);
        }
        
        public override void DoEffect(GameLogic logic, SlotStatusData slotStatusData, Card target, Slot slotWithStatus, Slot destinationSlot)
        {
            int damage = slotStatusData.value;
            logic.DamageCard(target, damage);
        }

        private int GetDamage(Game data, Card caster, int value)
        {
            Player player = data.GetPlayer(caster.playerID);
            int damage = value + caster.GetTraitValue(bonus_damage) + player.GetTraitValue(bonus_damage);
            return damage;
        }

        public override int GetAiValue(AbilityData ability)
        {
            return -1;
        }
    }
}