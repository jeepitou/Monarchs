using System;
using System.Collections;
using System.Collections.Generic;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;
using TcgEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Monarchs
{
    public class DemoMenuDeckOption : MonoBehaviour
    {
        public bool showInfoOnRight = false;
        public GameObject infoPanel;
        public GameObject deckPanel;
        
        public TMP_Text title;
        public TMP_Text description;
        public Image image;
        
        public TMP_Text championTitle;
        public TMP_Text championName;
        public Image championImage;
        public TMP_Text championDescription;

        public TMP_Text activeAbilityDescription;
        public TMP_Text passiveAbilityDescription;
        public static Action<DemoMenuDeckOption> onHover;
        
        public DeckData deck;
        
        void Start()
        {
            if (deck != null)
            {
                title.text = deck.title;
                description.text = deck.description;
                image.sprite = deck.monarch.artFull;
                championTitle.text = deck.title + "'s champion";
                championName.text = deck.champion.title;
                championImage.sprite = deck.champion.artFull;
                championDescription.text = deck.championDescription;
                activeAbilityDescription.text = deck.activeAbilityDescription;
                passiveAbilityDescription.text = deck.passiveAbilityDescription;
                if (showInfoOnRight)
                {
                    infoPanel.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
                    infoPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
                    infoPanel.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0);
                    infoPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(30, -15, 0);
                }
            }
            DemoMenuDeckOption.onHover += onHoverAny;
        }
        
        void OnDestroy()
        {
            DemoMenuDeckOption.onHover -= onHoverAny;
        }
        
        private void onHoverAny(DemoMenuDeckOption option)
        {
            if (option != this)
            {
                GetComponent<CanvasGroup>().alpha = 0.5f;
                deckPanel.transform.localScale = new Vector3(.97f, .97f, .97f);
            }

            if (option == null)
            {
                deckPanel.transform.localScale = new Vector3(1, 1, 1);
                GetComponent<CanvasGroup>().alpha = 1f;
            }
            
        }
        
        public void OnHover()
        {
            deckPanel.transform.localScale = new Vector3(1.03f, 1.03f, 1.03f);
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            onHover?.Invoke(this);
        }
        
        public void OnExit()
        {
            onHover?.Invoke(null);
        }
        
        public void SetDeck(DeckData deck)
        {
            this.deck = deck;
        }
        
        public void OnClick()
        {
            DemoMenu.Get().StartMatchmaking(deck);
        }
    }
}
