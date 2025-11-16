using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect to make a card attack a target
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Attack", order = 10)]
    public class EffectAttack : EffectData
    {
        public EffectAttackerType attacker_type;

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            Card attack = GetAttacker(logic.GetGameData(), args.caster);
            if (attack != null)
            {
                logic.AttackTarget(attack, ((Card)args.target).slot, true);
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

    public enum EffectAttackerType
    {
        Self = 1,                  
        AbilityTriggerer = 25, 
        LastPlayed = 70,  
        LastTargeted = 72, 
    }
}