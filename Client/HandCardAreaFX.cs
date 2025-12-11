using System.Collections;
using DG.Tweening;
using Monarchs.Logic;
using Monarchs.UI;
using UnityEngine;
using UnityEngine.UI;
using ReturnCardToHandConst = Monarchs.Animations.AnimConstants.ReturnCardToHand;

namespace Monarchs.Client
{
    public class HandCardAreaFX
    {
        private readonly HandCardArea _handCardArea;

        public HandCardAreaFX(HandCardArea handCardArea)
        {
            _handCardArea = handCardArea;
        }

        public IEnumerator SendBackToHandRoutine(RectTransform rectTransform, Card card)
        {
            GameObject handCard = _handCardArea.SpawnNewCard(card, true, true);
            HandCard handCardComponent = handCard.GetComponent<HandCard>();
            handCardComponent.puttingCardBackInHand = true;
            handCardComponent.SetInitialPosition();
            HandCardFX handCardFX = handCard.GetComponent<HandCardFX>();
            
            Transform cardChild = handCard.transform.GetChild(0);
            cardChild.localScale = Vector3.one * ReturnCardToHandConst.CARD_DISPLAY_SCALE;
            
            _handCardArea.RefreshHand(card.uid);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_handCardArea.GetComponent<RectTransform>());

            yield return handCardFX.AppearOnTopOf(rectTransform, ReturnCardToHandConst.CARD_FADE_DURATION);
            handCardComponent.HideAdditionnalInfo();

            yield return new WaitForSeconds(ReturnCardToHandConst.DELAY_BEFORE_MOVING_CARD);

            float durationToHand = handCardFX.GetDurationToHand(ReturnCardToHandConst.CARD_VELOCITY);
            handCardFX.StartCoroutine(AddCardToHandRoutine(card, durationToHand*.7f));
            handCardFX.StartCoroutine(handCardFX.SendToHand(ReturnCardToHandConst.CARD_VELOCITY));
            yield return new WaitForSeconds(durationToHand*.71f);

            handCardFX.StartCoroutine(ResetFlagsRoutine(handCardComponent, durationToHand*.4f));
        }

        public IEnumerator AddCardToHandRoutine(Card card, float delay)
        {
            yield return new WaitForSeconds(delay);
            // Game game = GameClient.GetGameData();
            // game.players[card.playerID].cards_all[card.uid] = card;
            // game.players[card.playerID].cards_hand.Add(card);
            HandCardArea.CardMovingToHandCount--;
            HandCardArea.Get().RefreshHand();
        }
        
        public IEnumerator ResetFlagsRoutine(HandCard handCardComponent, float delay)
        {
            yield return new WaitForSeconds(delay);
            handCardComponent.puttingCardBackInHand = false;
        }
    }
}
