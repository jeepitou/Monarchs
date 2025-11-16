using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Destroy", order = 10)]
    public class EffectDestroy : EffectData
    {
        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            logic.KillCard(args.caster, args.CardTarget);
        }

        public override int GetAiValue(AbilityData ability)
        {
            return -1;
        }
    }
}