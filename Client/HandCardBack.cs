using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Monarchs.Logic;
using Monarchs.Tools;
using Monarchs.UI;
using Sirenix.OdinInspector;
using TcgEngine;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Monarchs.Client
{
    public class HandCardBack : MonoBehaviour
    {
        [Required] public Image cardSprite;
        [Required] public Image glow;
        [Required] public GameObject handCard;
        [Required] public CardFlip cardFlip;

        private RectTransform _rect;

        private static readonly List<HandCardBack> CardList = new ();

        void Awake()
        {
            CardList.Add(this);
            _rect = GetComponent<RectTransform>();
            SetCardback(null);
        }

        private void Start()
        {
            GameClient.Get().onHandCardHoveredByOpponent += OnHandCardHovered;
        }

        private void OnHandCardHovered(Card card)
        {
            if (card == null)
            {
                glow.enabled = false;
                return;
            }
            
            int index = GameClient.Get().GetOpponentPlayer().cards_hand.FindIndex(a => a.uid == card.uid);
            
            glow.enabled = transform.GetSiblingIndex()-1 == index; //We add 1 because of the card template
        }

        private void OnDestroy()
        {
            if (CardList.Contains(this))
            {
                CardList.Remove(this);
            }
            
            GameClient.Get().onHandCardHoveredByOpponent -= OnHandCardHovered;
        }

        public IEnumerator PlayCard(GameObject displayPosition)
        {
            Vector3 start = transform.localPosition;
            Vector3 end = start - Vector3.back*300;
            Color color = cardSprite.color;
            color.a = 0;
            float duration = 0.5f;
            CardList.Remove(this);
            transform.DOLocalMove(end, duration);
            cardSprite.DOColor(color, duration);

            yield return new WaitForSeconds(duration);
            
            Destroy(gameObject);
        }
        
        public IEnumerator SendToHand(float velocity)
        {
            RectTransform childRectTransform = transform.GetChild(0).GetComponent<RectTransform>();
            float distance = Vector3.Distance(childRectTransform.localPosition, Vector3.zero);
            float duration = distance / velocity;
            
            childRectTransform.DOLocalMove(Vector3.zero, duration).SetEase(Ease.InOutCubic);
            childRectTransform.DOLocalRotate(0 * Vector3.forward, duration).SetEase(Ease.InOutCubic);
            childRectTransform.DOScale(Vector3.one, duration).SetEase(Ease.InOutCubic);
            
            yield return new WaitForSeconds(duration);
        }
        
        public IEnumerator AppearOnTopOf(RectTransform target, float fadeDuration)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            
            RectTransform childRectTransform = transform.GetChild(0).GetComponent<RectTransform>();
            RectTransformTools.MoveRectTransformOnTopOfAnotherRectTransform(childRectTransform, target);
            
            childRectTransform.localRotation = Quaternion.Euler(0, 0, -GetComponent<RectTransform>().localRotation.eulerAngles.z);
            GetComponent<CanvasGroup>().alpha = 0;
            GetComponent<CanvasGroup>().DOFade(1, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }
        
        public void ShowCard(Card card)
        {
            handCard.SetActive(true);
            cardFlip.cardBack.SetActive(false);
            handCard.GetComponent<HandCardUIManager>().SetCard(card);
        }
        
        public IEnumerator FlipCard()
        {
            cardFlip.enabled = true;
            yield return cardFlip.FlipCard();
        }

        public void SetCardback(CardbackData cb)
        {
            if (cb != null && cb.cardback != null)
                cardSprite.sprite = cb.cardback;
        }

        public RectTransform GetRect()
        {
            if (_rect == null)
                return GetComponent<RectTransform>();
            return _rect;
        }

    }
}
