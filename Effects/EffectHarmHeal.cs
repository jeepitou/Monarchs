using System.Collections;
using System.Collections.Generic;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using UnityEngine;

namespace Monarchs
{
    /// <summary>
    /// Effect that will heal allies, or harm enemies (ex. Holy Word)
    /// </summary>
    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/HarmHeal", order = 10)]
    public class EffectHarmHeal : EffectData
    {
        public int harmValue;
        public int healValue;
        
        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            if (args.CardTarget.playerID == args.caster.playerID)
            {
                logic.HealCard(args.CardTarget, healValue);
            }
            else
            {
                logic.DamageCard(args.CardTarget, harmValue);
            }
        }
    }
}
