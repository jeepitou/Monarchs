using System.Collections;
using DG.Tweening;
using Monarchs.Logic;
using UnityEngine;
using ReturnCardToHandConst = Monarchs.Animations.AnimConstants.ReturnCardToHand;

namespace Monarchs.Client
{
    public class OpponentHandFX
    {
        private readonly OpponentHand _opponentHand;

        public OpponentHandFX(OpponentHand opponentHand)
        {
            _opponentHand = opponentHand;
        }

        public IEnumerator SendBackToOpponentHandRoutine(RectTransform rectTransform, Card card)
        {
            float cardFadeDuration = ReturnCardToHandConst.CARD_FADE_DURATION;
            float delayBeforeMoving = ReturnCardToHandConst.DELAY_BEFORE_MOVING_CARD;
            float cardVelocity = ReturnCardToHandConst.CARD_VELOCITY;

            GameObject handCard = _opponentHand.SpawnNewCard(true);
            _opponentHand.PutCardAtFinalPositionInHand(handCard.GetComponent<HandCardBack>());

            handCard.transform.GetChild(0).localScale = Vector3.one * ReturnCardToHandConst.CARD_DISPLAY_SCALE;

            handCard.GetComponent<HandCardBack>().ShowCard(card);
            yield return handCard.GetComponent<HandCardBack>().AppearOnTopOf(rectTransform, cardFadeDuration);

            yield return new WaitForSeconds(delayBeforeMoving);

            yield return handCard.GetComponent<HandCardBack>().SendToHand(cardVelocity);
        }
    }
}
