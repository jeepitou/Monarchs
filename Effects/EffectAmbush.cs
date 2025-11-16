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

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Ambush", order = 10)]
    public class EffectAmbush : EffectData
    {
        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            logic.Game.initiativeManager.AddAmbushToCard(args.CardTarget);
        }
    }
}