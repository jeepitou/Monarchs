using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    //Effect to Summon an entirely new card (not in anyones deck)
    //And places it on the board

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/EffectBoundSpell")]
    public class EffectBoundSpell : EffectData
    {
        public override void DoEffectPlayerTarget(GameLogic logic, AbilityArgs args)
        {
            foreach (var cardData in args.castedCard.CardData.boundSpells)
            {
                Card card = logic.SummonCardHand(args.PlayerTarget.playerID, cardData, VariantData.GetDefault(), "");
                logic.onAbilitySummonedCardToHand?.Invoke(card.uid);
            }
            
        }
    }
}