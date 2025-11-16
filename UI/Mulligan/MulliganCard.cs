using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Monarchs.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace Monarchs
{
    public class MulliganCard : MonoBehaviour
    {
        public HandCardUIManager handCardUIManager;
        public GameObject currentStatusIcon;
        public Sprite toBeKeptSprite;
        public Sprite toBeDiscardedSprite;
        public float hoverScale = 1.1f;
        public float hoverDuration = 0.1f;
        public float initialScale = 1f;
        public Image glow;
        public Color toBeKeptColor;
        public Color toBeDiscardedColor;
        [HideInInspector]public Card card;
        [HideInInspector]public bool isToBeDiscarded = false;

        public void Start()
        {
            handCardUIManager = GetComponent<HandCardUIManager>();
            glow.color = toBeKeptColor;
            initialScale = transform.localScale.x;
        }

        public void SetCard(Card card)
        {
            this.card = card;
            handCardUIManager.SetCard(card);
        }
        
        public IEnumerator DiscardAndSetNewCard(Card newCard, GameObject deckPosition, float discardDuration, float delay, float drawDuration)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            Vector3 initialPosition = rectTransform.localPosition;
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            rectTransform.DOScale(0.9f, discardDuration);
            rectTransform.DOLocalMoveY(rectTransform.position.y + 50, discardDuration);
            canvasGroup.DOFade(0, discardDuration).onComplete += () =>
            {
                card = newCard;
                handCardUIManager.SetCard(newCard);
                isToBeDiscarded = false;
                currentStatusIcon.SetActive(false);
                glow.gameObject.SetActive(false);

                StartCoroutine(DrawCard(initialPosition, deckPosition, delay, drawDuration));
            };
            yield return null;
        }

        public IEnumerator DrawCard(Vector3 initialPosition, GameObject deckPosition, float delay, float drawDuration)
        {
            yield return new WaitForSeconds(delay);
            
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1;
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            
            rectTransform.localPosition = deckPosition.transform.localPosition;
            
            rectTransform.DOLocalMove(initialPosition, drawDuration);
            rectTransform.DOScale(initialScale, drawDuration);
        }
        
        public void OnMulliganCardClicked()
        {
            isToBeDiscarded = !isToBeDiscarded;
            if (isToBeDiscarded)
            {
                currentStatusIcon.GetComponent<Image>().sprite = toBeDiscardedSprite;
                glow.color = toBeDiscardedColor;
            }
            else
            {
                currentStatusIcon.GetComponent<Image>().sprite = toBeKeptSprite;
                glow.color = toBeKeptColor;
            }
        }
        
        public void OnMulliganCardHover()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.DOScale(hoverScale, hoverDuration);
            currentStatusIcon.GetComponent<Image>().DOFade(0, 0.2f);
        }
        
        public void OnMulliganCardExit()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.DOScale(initialScale, hoverDuration);
            currentStatusIcon.GetComponent<Image>().DOFade(1, 0.2f);
        }
    }
}
