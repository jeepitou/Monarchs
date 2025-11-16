using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect that sets basic stats (hp/attack/mana) to a specific value
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/SetStat", order = 10)]
    public class EffectSetStat : EffectData
    {
        public EffectStatType type;
        
        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            if (type == EffectStatType.Attack)
                args.CardTarget.attack = args.ability.value;
            if (type == EffectStatType.HP)
            {
                args.CardTarget.hp = args.ability.value;
                args.CardTarget.damage = 0;
            }
        }

        public override void DoOngoingEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            if (type == EffectStatType.Attack)
                args.CardTarget.attack = args.ability.value;

            if (type == EffectStatType.HP)
                args.CardTarget.hp = args.ability.value;
        }

        public override int GetAiValue(AbilityData ability)
        {
            if (type == EffectStatType.Mana)
                return 0; //Mana unclear, depend of target (good for player, bad for card)

            if (ability.value <= 3)
                return -1; //Set to low value
            if (ability.value >= 7)
                return 1; //Set to high value
            return 0;
        }
    }
}