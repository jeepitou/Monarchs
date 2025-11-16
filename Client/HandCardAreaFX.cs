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
            GameObject handCard = _handCardArea.SpawnNewCard(card, true);
            HandCard handCardComponent = handCard.GetComponent<HandCard>();
            handCardComponent.puttingCardBackInHand = true;
            handCardComponent.SetInitialPosition();
            
            Transform cardChild = handCard.transform.GetChild(0);
            cardChild.localScale = Vector3.one * ReturnCardToHandConst.CARD_DISPLAY_SCALE;
            
            _handCardArea.RefreshHand(card.uid);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_handCardArea.GetComponent<RectTransform>());

            yield return handCard.GetComponent<HandCardFX>().AppearOnTopOf(rectTransform, ReturnCardToHandConst.CARD_FADE_DURATION);
            handCardComponent.HideAdditionnalInfo();

            yield return new WaitForSeconds(ReturnCardToHandConst.DELAY_BEFORE_MOVING_CARD);

            yield return handCard.GetComponent<HandCardFX>().SendToHand(ReturnCardToHandConst.CARD_VELOCITY);

            handCardComponent.puttingCardBackInHand = false;
        }
    }
}
