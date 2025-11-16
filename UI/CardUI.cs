using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Monarchs;
using Monarchs.Logic;
using Monarchs.UI;
using UnityEngine;
using UnityEngine.UI;
using TcgEngine;
using TcgEngine.Client;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TcgEngine.UI
{
    /// <summary>
    /// Scripts to display all stats of a card, 
    /// is used by other script that display cards like BoardCard, and HandCard, CollectionCard..
    /// </summary>

    public class CardUI : MonoBehaviour, IPointerClickHandler
    {
        public TMP_Text card_title;
        public TMP_Text card_text;
        public TMP_Text card_description;
        public GameObject cardPrefab;
        public Image card_image;
        public Image frame_image;
        public Image team_icon;
        public Image rarity_icon;
        public Image attack_icon;
        public Image range_icon;
        public Image armorPen_icon;
        public Image[] aoe_patterns;
        public ManaBarHandCard manaBar;
        public GuildColors guildColors;
        
        public Text attack;
        public Text range;

        public TraitUI[] stats;

        public UnityAction<CardUI> onClick;
        public UnityAction<CardUI> onClickRight;
        
        protected ICard card;
        protected VariantData variant;
        private bool descriptionShown = false;
        private bool aoePatternsShown = false;

        protected void SetAttackText(Text attack)
        {
            attack.text = card.GetAttack().ToString();
        }

        public virtual void SetCard(Card card)
        {
        }

        public virtual void SetCard(ICard card, VariantData variant)
        {
            
        }

        public void ShowDescription()
        {
            if (descriptionShown || card_description == null || string.IsNullOrWhiteSpace(card_description.text))
                return;
            
            descriptionShown = true;
            CanvasGroup canvas = card_description.transform.parent.GetComponent<CanvasGroup>();
            canvas.DOFade(1, 0.2f);
        }
        
        public void HideDescription()
        {
            if (!descriptionShown || card_description == null)
                return;
            
            descriptionShown = false;
            CanvasGroup canvas = card_description.transform.parent.GetComponent<CanvasGroup>();
            canvas.DOFade(0, 0.2f);
        }

        public void ShowAOEPatterns()
        {
            if (aoePatternsShown || card.GetCardData().aoePatterns.Length == 0 || aoe_patterns == null)
                return;
            
            aoePatternsShown = true;
            aoe_patterns[0].transform.parent.gameObject.SetActive(true);
            CanvasGroup canvas = aoe_patterns[0].transform.parent.GetComponent<CanvasGroup>();
            canvas.DOFade(1, 0.2f);
        }
        
        public void HideAOEPatterns()
        {
            if (!aoePatternsShown || aoe_patterns == null)
                return;
            
            aoePatternsShown = false;
            aoe_patterns[0].transform.parent.gameObject.SetActive(false);
            CanvasGroup canvas = aoe_patterns[0].transform.parent.GetComponent<CanvasGroup>();
            canvas.DOFade(0, 0f);
        }

        protected void SetDescription()
        {
            if (card_description == null) return;
            string cdesc = card.GetCardData().GetDesc();
            string adesc = card.GetCardData().GetAbilitiesDesc();
            if (!string.IsNullOrWhiteSpace(cdesc))
                card_description.text = cdesc + "\n\n" + adesc;
            else
                card_description.text = adesc;
        }

        protected void SetAOEPatterns()
        {
            int cardAOEPatternCount = card.GetCardData().aoePatterns.Length;
            for (int i=0; i<aoe_patterns.Length; i++)
            {
                if (i < card.GetCardData().aoePatterns.Length && aoe_patterns!=null)
                {
                    aoe_patterns[i].gameObject.SetActive(true);
                    aoe_patterns[i].sprite = card.GetCardData().aoePatterns[i];
                }
                else
                {
                    break;
                }
            }

            if (aoe_patterns.Length > cardAOEPatternCount)
            {
                for (int i=aoe_patterns.Length-1; i>=cardAOEPatternCount; i--)
                {
                    aoe_patterns[i].gameObject.SetActive(false);
                }
            }
        }
        
        

        public void Hide()
        {
            if (cardPrefab.activeSelf)
                cardPrefab.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (onClick != null)
                    onClick.Invoke(this);
            }

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (onClickRight != null)
                    onClickRight.Invoke(this);
            }
        }

        public virtual void SetMaterial(Material mat)
        {
        }

        public virtual void SetOpacity(float opacity)
        {
        }

        public ICard GetCard()
        {
            return card;
        }

        public VariantData GetVariant()
        {
            return variant;
        }
    }
}
