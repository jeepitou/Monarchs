using System.Collections;
using System.Collections.Generic;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using UnityEngine;

namespace Monarchs
{
    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/AddIntimidate", order = 10)]
    public class EffectAddToIntimidate : EffectData
    {
        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            logic.Game.intimidateManager.AddCardWithIntimidation(args.CardTarget);
        }
    }
}
