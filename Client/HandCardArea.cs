using System.Collections.Generic;
using Monarchs.Logic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Monarchs.Client
{
    /// <summary>
    /// Area where all the hand cards are
    /// Will take card of spawning/despawning hand cards based on the refresh data received from server
    /// </summary>

    public class HandCardArea : MonoBehaviour
    {
        [Required] public GameObject deck;
        [Required] public RectTransform cardArea;
        [Required] public GameObject cardTemplate;
        public float cardSpacing = 150f;
        public float cardAngle = 5f;
        public float cardOffsetY = 10f;

        public bool isCardExporter;
        public bool isFocusInCardExport;

        public List<HandCard> cards = new ();

        private float _refreshDelay = 0.1f; 
        private float _currentTime = 0f;
        private bool _isDragging;
        private HandCardAreaFX _handCardAreaFX;

        private string _lastDestroyed;
        private float _lastDestroyedTimer;

        private static HandCardArea _instance;

        void Awake()
        {
            if (isCardExporter)
                return;
            _instance = this;
            cardTemplate.SetActive(false);
            _handCardAreaFX = new HandCardAreaFX(this);
        }

        private void Start()
        {
            if (isCardExporter)
                return;
            GameClient.Get().onRefreshAll += RefreshHand;
        }

        void Update()
        {
            if (isCardExporter)
                return;
            
            if (!GameClient.Get().IsReady())
                return;

            _lastDestroyedTimer += Time.deltaTime;

            if (_currentTime < _refreshDelay)
            {
                _currentTime += Time.deltaTime;
            }
            else
            {
                _currentTime = 0f;
                RefreshHand();
            }

            //Set target focus
            HandCard dragCard = HandCard.GetDrag();
            _isDragging = dragCard != null;
        }

        public void RefreshHand()
        {
            int playerID = GameClient.Get().GetPlayerID();
            Game data = GameClient.GetGameData();
            if (data.selector != SelectorType.None)
            {
                return;
            }
            
            Player player = data.GetPlayer(playerID);
            
            //Add missing cards
            foreach (Card card in player.cards_hand)
            {
                if (!HasCard(card.uid))
                    SpawnNewCard(card);
            }

            //Remove removed cards
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                HandCard card = cards[i];
                if (card == null || player.GetHandCard(card.GetCard().uid) == null)
                {
                    cards.RemoveAt(i);
                    if (card)
                        card.Kill();
                }
            }

            //Set card index
            int index = 0;
            float countHalf = cards.Count / 2f;
            foreach (HandCard card in cards)
            {
                card.handIndex = index;
                card.deckPosition = new Vector2((index - countHalf) * cardSpacing, (index - countHalf) * (index - countHalf) * -cardOffsetY);
                card.deckAngle = (index - countHalf) * -cardAngle;
                index++;
            }
        }

        public void RefreshHand(string uidToIgnore)
        {
            int playerID = GameClient.Get().GetPlayerID();
            Game data = GameClient.GetGameData();
            if (data.selector != SelectorType.None)
            {
                return;
            }
            
            Player player = data.GetPlayer(playerID);
            
            //Add missing cards
            foreach (Card card in player.cards_hand)
            {
                if (!HasCard(card.uid) && card.uid != uidToIgnore)
                    SpawnNewCard(card);
            }

            //Remove removed cards
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                HandCard card = cards[i];
                if (card == null || (player.GetHandCard(card.GetCard().uid) == null && card.GetCard().uid != uidToIgnore))
                {
                    cards.RemoveAt(i);
                    if (card)
                        card.Kill();
                }
            }

            //Set card index
            int index = 0;
            float countHalf = cards.Count / 2f;
            foreach (HandCard card in cards)
            {
                card.handIndex = index;
                card.deckPosition = new Vector2((index - countHalf) * cardSpacing, (index - countHalf) * (index - countHalf) * -cardOffsetY);
                card.deckAngle = (index - countHalf) * -cardAngle;
                index++;
            }
        }

        public GameObject SpawnNewCard(Card card, bool removeExisting = false)
        {
            if (removeExisting)
            {
                HandCard existing = HandCard.Get(card.uid);
                if (existing)
                {
                    cards.Remove(existing);
                    existing.Kill();
                }
            }
            GameObject cardObj = Instantiate(cardTemplate, cardArea.transform);
            cardObj.SetActive(true);
            HandCard handCard = cardObj.GetComponentInChildren<HandCard>();
            handCard.SetCard(card);
            handCard.deckPositionGameObject = deck;
            cards.Add(handCard);
            
            float countHalf = cards.Count / 2f;
            int index = cards.Count - 1;
            cardObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -100f);
            handCard.handIndex = index;
            handCard.deckPosition = new Vector2((index - countHalf) * cardSpacing, (index - countHalf) * (index - countHalf) * -cardOffsetY);
            handCard.deckAngle = (index - countHalf) * -cardAngle;
            
            return cardObj;
        }
        
        public void DelayRefresh(Card card)
        {
            _lastDestroyedTimer = 0f;
            _lastDestroyed = card.uid;
        }

		public void SortCards()
        {
            cards.Sort(SortFunc);

            int i = 0;
            foreach (HandCard handCard in cards)
            {
                handCard.transform.SetSiblingIndex(i);
                i++;
            }
        }

        private int SortFunc(HandCard a, HandCard b)
        {
            return a.transform.position.x.CompareTo(b.transform.position.x);
        }

        public bool HasCard(string cardUID)
        {
            HandCard card = HandCard.Get(cardUID);
            bool justDestroyed = cardUID == _lastDestroyed && _lastDestroyedTimer < 0.7f;
            return card != null || justDestroyed;
        }

        public void SetCardForExport(Card card)
        {
            cardTemplate.GetComponent<HandCard>().SetCard(card);
            if (isFocusInCardExport)
            {
                cardTemplate.GetComponent<HandCardExporting>().SetFocus();
            }
        }

        public bool IsDragging()
        {
            return _isDragging;
        }
        
        public HandCardAreaFX GetHandCardAreaFX()
        {
            return _handCardAreaFX;
        }

        public static HandCardArea Get()
        {
            return _instance;
        }
    }
}