using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect to discard cards from hand
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Discard", order = 10)]
    public class EffectDiscard : EffectData
    {
        public override void DoEffectPlayerTarget(GameLogic logic, AbilityArgs args)
        {
            logic.DrawDiscardCard(args.PlayerTarget.playerID, args.ability.value);
        }

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            logic.DiscardCard(args.CardTarget);
        }

        public override int GetAiValue(AbilityData ability)
        {
            return -1;
        }
    }
}