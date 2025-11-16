using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect to reset all stats to their original values
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/ResetStat", order = 10)]
    public class EffectResetStat : EffectData
    {
        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            args.CardTarget.SetCard(args.CardTarget.CardData, args.CardTarget.VariantData);
        }
    }
}