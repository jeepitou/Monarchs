using System.Collections;
using DG.Tweening;
using Monarchs.Board;
using Monarchs.Client;
using Monarchs.Logic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using ReturnCardToHandConst = Monarchs.Animations.AnimConstants.ReturnCardToHand;

namespace Monarchs.UI
{
    public class BoardCardTrapFx : MonoBehaviour
    {
        [Required] public GameObject trapTriggeredFX;
        private BoardCard _boardCard;
        private HandCardArea _handCardArea;
        private OpponentHand _opponentHand;
        
        protected void Start()
        {
            _boardCard = GetComponent<BoardCard>();
            GameClient.Get().animationManager.onTrapTriggerBoardCard += OnTrapTrigger; 
            _handCardArea = HandCardArea.Get();
            _opponentHand = OpponentHand.Get();
        }
        
        protected void OnDestroy()
        {
            GameClient.Get().animationManager.onTrapTriggerBoardCard -= OnTrapTrigger;
        }

        private void OnTrapTrigger(Card trap, Card triggerer)
        {
            if (triggerer.uid == _boardCard.GetCardUID())
            {
                GameClient.Get().animationManager.AddToQueue(MoveToSlotCoroutine(trap.slot), gameObject);
            }
        }

        private IEnumerator MoveToSlotCoroutine(Slot slot)
        {
            _boardCard.SetMovingInCoroutine(true);
            
            Vector3 targetPos = BoardManager.Instance.GetPositionFromCoordinate(slot.GetCoordinate());
            float startTime = Time.time;
            
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                float deltaTime = Time.time - startTime;
                transform.position = Vector3.MoveTowards(transform.position, targetPos, 12f * deltaTime);
                startTime = Time.time;
                yield return null;
            }

            if (trapTriggeredFX != null)
            {
                trapTriggeredFX.GetComponent<CanvasGroup>().DOFade(1, 0.2f);
            }
            
            yield return new WaitForSeconds(0.2f);
            
            if (trapTriggeredFX != null)
            {
                StartCoroutine(HideTrapExclamation());
            }
            
            _boardCard.SetMovingInCoroutine(false);
        }
        
        private IEnumerator HideTrapExclamation()
        {
            yield return new WaitForSeconds(0.5f);
            trapTriggeredFX.GetComponent<CanvasGroup>().DOFade(0, 0.2f);
        }
        
        public void SendBackToHand()
        {
            if (_boardCard.GetMovingInCoroutine())
                return;
            _boardCard.SetMovingInCoroutine(true);
            
            IEnumerator coroutineToAdd = _boardCard.GetCard().playerID == GameClient.Get().GetPlayerID() ? 
                SendBackToHandPlayerRoutine() : SendBackToOpponentHandRoutine();
            
            GameClient.Get().animationManager.AddToQueue(coroutineToAdd, gameObject);
        }

        private IEnumerator SendBackToHandPlayerRoutine()
        {
            float moveZDuration = ReturnCardToHandConst.BOARD_CARD_MOVE_Z_DURATION;
            float moveZAmount = ReturnCardToHandConst.BOARD_CARD_MOVE_Z_AMOUNT;
            float cardFadeDuration = ReturnCardToHandConst.CARD_FADE_DURATION;
            float delayBeforeMoving = ReturnCardToHandConst.DELAY_BEFORE_MOVING_CARD;
            float cardVelocity = ReturnCardToHandConst.CARD_VELOCITY;
            
            
            transform.DOMoveZ(transform.position.z + moveZAmount, moveZDuration);
            yield return new WaitForSeconds(moveZDuration);
            
            GameObject handCard = _handCardArea.SpawnNewCard(_boardCard.GetCard(), true);
            HandCard handCardComponent = handCard.GetComponent<HandCard>();
            handCardComponent.puttingCardBackInHand = true;
            handCardComponent.SetInitialPosition();
            _handCardArea.RefreshHand();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_handCardArea.GetComponent<RectTransform>());
            
            
            yield return handCard.GetComponent<HandCardFX>().AppearOnTopOf(GetComponent<RectTransform>(), cardFadeDuration);
            
            Hide();
            
            yield return new WaitForSeconds(delayBeforeMoving);
            
            yield return handCard.GetComponent<HandCardFX>().SendToHand(cardVelocity);
            
            
            handCardComponent.puttingCardBackInHand = false;
            
            _boardCard.Kill(false);
        }
        
        private IEnumerator SendBackToOpponentHandRoutine()
        {
            float moveZDuration = ReturnCardToHandConst.BOARD_CARD_MOVE_Z_DURATION;
            float moveZAmount = ReturnCardToHandConst.BOARD_CARD_MOVE_Z_AMOUNT;
            float cardFadeDuration = ReturnCardToHandConst.CARD_FADE_DURATION;
            float delayBeforeMoving = ReturnCardToHandConst.DELAY_BEFORE_MOVING_CARD;
            float cardVelocity = ReturnCardToHandConst.CARD_VELOCITY;
            
            
            transform.DOMoveZ(transform.position.z + moveZAmount, moveZDuration);
            yield return new WaitForSeconds(moveZDuration);
            
            GameObject handCard = _opponentHand.SpawnNewCard(true);
            _opponentHand.PutCardAtFinalPositionInHand(handCard.GetComponent<HandCardBack>());
            
            handCard.GetComponent<HandCardBack>().ShowCard(_boardCard.GetCard());
            yield return handCard.GetComponent<HandCardBack>().AppearOnTopOf(GetComponent<RectTransform>(), cardFadeDuration);
            
            
            Hide();
            
            yield return new WaitForSeconds(delayBeforeMoving);
            
            StartCoroutine(handCard.GetComponent<HandCardBack>().FlipCard());
            yield return handCard.GetComponent<HandCardBack>().SendToHand(cardVelocity);
            
            _boardCard.Kill(false);
        }
        
        private void Hide()
        {
            for (int i=0; i<transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}