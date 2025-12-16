using System.Collections.Generic;
using Monarchs;
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
        public bool randomSpells = false;
        public override void DoEffectPlayerTarget(GameLogic logic, AbilityArgs args)
        {
            if (randomSpells)
            {
                CardData randomSpellCardData = GetRandomSpellCardData(logic);
                if (randomSpellCardData != null)
                {
                    Card card = logic.SummonCardHand(args.PlayerTarget.playerID, randomSpellCardData, VariantData.GetDefault(), "");
                    logic.onAbilitySummonedCardToHand?.Invoke(card.uid, card.cardID);
                }
                return;
            }
            foreach (var cardData in args.castedCard.CardData.boundSpells)
            {
                Card card = logic.SummonCardHand(args.PlayerTarget.playerID, cardData, VariantData.GetDefault(), "");
                logic.onAbilitySummonedCardToHand?.Invoke(card.uid, card.cardID);
            }
            
        }
        
        private CardData GetRandomSpellCardData(GameLogic logic)
        {
            List<CardData> spellCards = CardData.GetAllInterventions();
            
            if (spellCards.Count == 0)
                return null;

            int randomIndex = Random.Range(0, spellCards.Count);
            return spellCards[randomIndex];
        }
    }
}