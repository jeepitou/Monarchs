using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace TcgEngine
{
    /// <summary>
    /// Effect that damages a card or a player (lose hp)
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/EffectDamageSpecificTargetAttack", order = 10)]
    public class EffectDamageSpecificTargetAttack : EffectData
    {
        public int targetNumber;
        public bool armorPenetrating = false;
        public bool triggersDyingWish = true;
        public TraitData bonusDamage;

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
            int damage = ((Card)data.selectorTargets[targetNumber-1]).GetAttack() + caster.GetTraitValue(bonusDamage) + player.GetTraitValue(bonusDamage);
            return damage;
        }

        public override int GetAiValue(AbilityData ability)
        {
            return -1;
        }
    }
}