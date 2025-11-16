using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect to draw cards
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Draw", order = 10)]
    public class EffectDraw : EffectData
    {
        public override void DoEffectPlayerTarget(GameLogic logic, AbilityArgs args)
        {
            logic.DrawCard(args.PlayerTarget.playerID, args.ability.value);
        }

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            logic.DrawCard(args.CardTarget.playerID, args.ability.value);
        }

        public override int GetAiValue(AbilityData ability)
        {
            return 1;
        }
    }
}