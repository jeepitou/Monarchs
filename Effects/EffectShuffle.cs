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

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Shuffle", order = 10)]
    public class EffectShuffle : EffectData
    {
        public override void DoEffectPlayerTarget(GameLogic logic, AbilityArgs args)
        {
            logic.ShuffleDeck(args.PlayerTarget.cards_deck);
        }
    }
}