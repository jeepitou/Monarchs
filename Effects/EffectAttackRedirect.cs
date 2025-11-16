using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect to redirect an attack (usually triggered with OnBeforeAttack or OnBeforeDefend)
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/AttackRedirect", order = 10)]
    public class EffectAttackRedirect : EffectData
    {
        public EffectAttackerType attacker_type;

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            Card attacker = GetAttacker(logic.GetGameData(), args.caster);
            if (attacker != null)
            {
                logic.RedirectAttack(attacker, args.CardTarget);
            }
        }

        public Card GetAttacker(Game gdata, Card caster)
        {
            if (attacker_type == EffectAttackerType.Self)
                return caster;
            if (attacker_type == EffectAttackerType.AbilityTriggerer)
                return gdata.abilityTriggerer;
            if (attacker_type == EffectAttackerType.LastPlayed)
                return gdata.lastPlayed;
            if (attacker_type == EffectAttackerType.LastPlayed)
                return (Card)gdata.lastTarget;
            return null;
        }
    }
}