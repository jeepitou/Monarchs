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
    /// Effect that will do the effect of "cardToPlay"
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/PlaySpecificCarc", order = 10)]
    public class EffectPlaySpectificCard : EffectData
    {
        public CardData cardToPlay;
        public override void DoEffectSlotTarget(GameLogic logic, AbilityArgs args)
        {
            Game game = logic.GetGameData();
            Player player = game.GetPlayer(args.caster.playerID);
            Card card = logic.SummonCardHand(args.caster.playerID, cardToPlay, VariantData.GetDefault());
            

            logic.PlayCard(card, args.SlotTarget, true);
        }
    }
}