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

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/EffectDamageMonarch", order = 10)]
    public class EffectDamageMonarch : EffectData
    {
        public bool damageOwnMonarch = false;
        public TraitData bonusDamage;

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            int damage = GetDamage(logic.Game, args.caster, args.ability.value);
            Card monarch = null;
            if (damageOwnMonarch)
            {
                monarch = logic.Game.GetPlayer(args.caster.playerID).king;
            }
            else
            {
                monarch = logic.Game.GetOpponentPlayer(args.caster.playerID).king;
            }
            logic.DamageCard(args.caster, monarch, damage);
        }

        private int GetDamage(Game data, Card caster, int value)
        {
            Player player = data.GetPlayer(caster.playerID);
            int damage = value + caster.GetTraitValue(bonusDamage) + player.GetTraitValue(bonusDamage);
            return damage;
        }

        public override int GetAiValue(AbilityData ability)
        {
            return -1;
        }
    }
}