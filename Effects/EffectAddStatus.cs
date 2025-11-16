using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect that adds or removes basic card/player stats such as hp, attack, mana
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/AddStatus")]
    public class EffectAddStatus : EffectData
    {
        public StatusType type;
        public bool removeOnApplierTurn = false;

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            Card target = (Card) args.target;

            int duration = args.ability.duration;
            if (removeOnApplierTurn && duration != 0)
            {
                duration++; // Otherwise the status is removed directly when we finish the caster turn instead of on its next turn.
            }
            target.AddStatus(type, args.ability.value, duration, args.caster, removeOnApplierTurn);
            
        }
    }
}