using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Shuffle Deck
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/RollDice", order = 10)]
    public class EffectRoll : EffectData
    {
        public int dice = 6;

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            logic.RollRandomValue(dice);
        }

        public override void DoEffectPlayerTarget(GameLogic logic, AbilityArgs args)
        {
            logic.RollRandomValue(dice);
        }

        public override void DoEffectSlotTarget(GameLogic logic, AbilityArgs args)
        {
            logic.RollRandomValue(dice);
        }
    }
}