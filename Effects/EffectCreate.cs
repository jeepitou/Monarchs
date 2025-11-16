using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effects that creates a new card from a CardData
    /// Use for discover effects
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Create", order = 10)]
    public class EffectCreate : EffectData
    {
        public override void DoEffectCardDataTarget(GameLogic logic, AbilityArgs args)
        {
            Player player = logic.Game.GetPlayer(args.caster.playerID);
            Card card = Card.Create(args.CardDataTarget, args.caster.VariantData, args.caster.playerID);
            player.cards_all[card.uid] = card;
            player.cards_temp.Add(card);
        }
    }
}