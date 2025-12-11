using System.Collections;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine;
using UnityEngine;

namespace Monarchs
{
    // The BoundSpellFX class is responsible for handling the visual and logical effects of a bound spell in the game.
    // It inherits from the AbilityFX base class, which likely provides common functionality for ability effects.
    public class BoundSpellFX : AbilityFX
    {
        // The DoFX method is the entry point for executing the bound spell's effects.
        // It first attempts to find the card data for the bound spell using the FindCardToCreate method.
        // If no card data is found, it logs an error and exits early.
        // Otherwise, it subscribes to the onAbilitySummonedCardToHand event to handle further actions.
        public override void DoFX()
        {
            GameClient.Get().onAbilitySummonedCardToHand += OnAbilitySummonedCardToHand;
        }

        private void OnDestroy()
        {
            GameClient.Get().onAbilitySummonedCardToHand -= OnAbilitySummonedCardToHand;
        }

        // The OnAbilitySummonedCardToHand method is triggered when a card is summoned to the hand.
        // It creates a new card object, updates the game state, and queues animations for the card's movement.
        // The behavior differs based on whether the caster is the local player or an opponent.
        private void OnAbilitySummonedCardToHand(string uid, string id)
        {
            RectTransform rectTransform = BoardCard.Get(_abilityArgs.caster.uid).GetComponent<RectTransform>();
            CardData cardData = CardData.Get(id);
            
            if (cardData == null)
            {
                Debug.LogError("BoundSpellFX: CardData not found for id " + id);
                return;
            }

            Card card = Card.Create(cardData, _abilityArgs.caster.VariantData, _abilityArgs.caster.playerID, uid);
            Game game = GameClient.GetGameData();
            game.players[card.playerID].cards_all[card.uid] = card;
            game.players[card.playerID].cards_hand.Add(card);

            if (_abilityArgs.caster.playerID == GameClient.Get().GetPlayerID())
            {
                GameClient.Get().animationManager.AddToQueue(HandCardArea.Get().GetHandCardAreaFX().SendBackToHandRoutine(rectTransform, card), gameObject);
                GameClient.Get().animationManager.AddToQueue(Destroy(), gameObject);
            }
            else
            {
                GameClient.Get().animationManager.AddToQueue(OpponentHand.Get().GetOpponentHandFX().SendBackToOpponentHandRoutine(rectTransform, card), gameObject);
                GameClient.Get().animationManager.AddToQueue(Destroy(), gameObject);
            }
        }

        // The Destroy method is a coroutine that destroys the game object after completing its tasks.
        private IEnumerator Destroy()
        {
            Destroy(gameObject);
            yield break ;
        }
        
        public override IEnumerator FXEnumerator()
        {
            yield return null;
        }
    }
}
