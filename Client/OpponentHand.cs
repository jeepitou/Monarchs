using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Monarchs.Logic;
using TcgEngine;
using UnityEngine;

namespace Monarchs.Client
{
    /// <summary>
    /// Same as HandCardArea but for the opponents hand
    /// Simpler version with display only (no draging of card)
    /// </summary>

    public class OpponentHand : MonoBehaviour
    {
        public HandCardUIManager playedCard;
        public RectTransform card_area;
        public GameObject card_template;
        public float card_spacing = 100f;
        public float card_angle = 10f;
        public float card_offset_y = 10f;

        private List<HandCardBack> cards = new List<HandCardBack>();
        private OpponentHandFX _opponentHandFX;
        private static OpponentHand _instance;

        void Start()
        {
            card_template.SetActive(false);
            GameClient.Get().onCardPlayed += OnCardPlayed;
            GameClient.Get().onTrapResolve += OnTrapResolved;
            _opponentHandFX = new OpponentHandFX(this);
            
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(this);
            }
        }
        
        public OpponentHandFX GetOpponentHandFX()
        {
            return _opponentHandFX;
        }
        
        public static OpponentHand Get()
        {
            return _instance;
        }

        private void OnTrapResolved(Card trap, Card trigerer)
        {
            //StartCoroutine(ShowCard(trap, true));
        }

        private void OnCardPlayed(Card card, Slot slot)
        {
            if (card?.playerID != GameClient.Get().GetPlayerID())
            {
                GameClient.Get().animationManager.AddToQueue(OnCardPlayedCoroutine(card, slot), gameObject);
            }
        }
        
        private IEnumerator OnCardPlayedCoroutine(Card card, Slot slot)
        {
            if (card == null)
            {
                yield break;
            }
            
            int index = GameClient.Get().GetOpponentPlayer().cards_hand.FindIndex(a => a.uid == card.uid);
            if (index >= cards.Count || index == -1)
            {
                yield break;
            }
            HandCardBack handCardBack = cards[index];
            cards.Remove(handCardBack);
            yield return handCardBack.PlayCard(handCardBack.gameObject);

            if (card.CardData.cardType == CardType.Trap)
            {
                yield break;
            }
            
            yield return StartCoroutine(ShowCard(card));
        }
        
        private IEnumerator ShowCard(Card card, bool isTrap=false)
        {
            playedCard.SetCard(card);
            CanvasGroup canvas = playedCard.gameObject.GetComponent<CanvasGroup>();
            canvas.DOFade(1, 0.5f);
            yield return new WaitForSeconds(1.5f);
            canvas.DOFade(0, 2f);
        }

        void Update()
        {
            if (!GameClient.Get().IsReady())
                return;

            Game gdata = GameClient.GetGameData();
            Player player = gdata.GetPlayer(GameClient.Get().GetOpponentPlayerID());

            if (cards.Count < player.cards_hand.Count)
            {
                SpawnNewCard();
                
            }

            if (cards.Count > player.cards_hand.Count)
            {
                RemoveCard();
            }

            int nb_cards = Mathf.Min(cards.Count, player.cards_hand.Count);

            for (int i = 0; i < nb_cards; i++)
            {
                HandCardBack card = cards[i];
                RectTransform crect = card.GetRect();
                float half = nb_cards / 2f;
                Vector3 tpos = new Vector3((i - half) * card_spacing, (i - half) * (i - half) * card_offset_y);
                float tangle = (i - half) * card_angle;
                crect.anchoredPosition = Vector3.Lerp(crect.anchoredPosition, tpos, 4f * Time.deltaTime);
                card.transform.localRotation = Quaternion.Slerp(card.transform.localRotation, Quaternion.Euler(0f, 0f, tangle), 4f * Time.deltaTime);
            }

        }
        
        public void PutCardAtFinalPositionInHand(HandCardBack card)
        {
            int i = cards.IndexOf(card);
            if (i == -1)
                return;
            int nb_cards = cards.Count;
            RectTransform crect = card.GetRect();
            float half = nb_cards / 2f;
            Vector3 tpos = new Vector3((i - half) * card_spacing, (i - half) * (i - half) * card_offset_y);
            float tangle = (i - half) * card_angle;
            crect.anchoredPosition = tpos;
            card.transform.localRotation = Quaternion.Euler(0f, 0f, tangle);
        }

        public GameObject SpawnNewCard(bool replaceExisting = false)
        {
            Game gdata = GameClient.GetGameData();
            Player player = gdata.GetPlayer(GameClient.Get().GetOpponentPlayerID());
            
            GameObject new_card = Instantiate(card_template, card_area);
            new_card.SetActive(true);
            HandCardBack hand_card = new_card.GetComponent<HandCardBack>();
            CardbackData cbdata = CardbackData.Get(player.cardback);
            hand_card.SetCardback(cbdata);
            RectTransform card_rect = new_card.GetComponent<RectTransform>();
            card_rect.anchoredPosition = new Vector2(0f, 100f);

            if (replaceExisting)
            {
                RemoveCard();
            }
            cards.Add(hand_card);
            return new_card;
        }
        
        public void RemoveCard()
        {
            HandCardBack card = cards[cards.Count - 1];
            cards.RemoveAt(cards.Count - 1);
            Destroy(card.gameObject);
        }
    }
}