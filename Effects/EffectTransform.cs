using System.Collections;
using System.Collections.Generic;
using Monarchs;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect to transform a card into another card
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Transform", order = 10)]
    public class EffectTransform : EffectData
    {
        public CardData transform_to;

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            logic.TransformCard(args.CardTarget, transform_to);
        }
    }
}