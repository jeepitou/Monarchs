using System;
using System.Collections;
using System.Collections.Generic;
using Monarchs;
using Monarchs.Client;
using Monarchs.Logic;
using UnityEngine;
using UnityEngine.UI;
using TcgEngine;
using TcgEngine.Client;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TcgEngine.UI
{
    /// <summary>
    /// Scripts to display all stats of a card, 
    /// is used by other script that display cards like BoardCard, and HandCard, CollectionCard..
    /// </summary>

    public class InterventionCardUI : CardUI, IPointerClickHandler
    {
        public HandCardPossibleCasters handCardPossibleCasters;
        public InterventionTypeIcon interventionTypeIcon;
        private bool _subscribed = false;

        public override void SetCard(Card card)
        {
            if (card == null)
                return;

            SetCard(card, card.VariantData);

            foreach (TraitUI stat in stats)
                stat.SetCard(card);
        }

        public override void SetCard(ICard card, VariantData variant)
        {
            if (card == null)
                return;

            this.card = card;
            this.variant = variant;

            cardPrefab.SetActive(true);
            SetInterventionCard(card, variant);
            SetDescription();
            SetAOEPatterns();
            if (card.GetCardData().cardType == CardType.Trap)
            {
                interventionTypeIcon.SetTrapIcon();
            }
            else
            {
                interventionTypeIcon.SetIcon(card.GetCardData().interventionType);
            }

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        private void SetInterventionCard(ICard card, VariantData variant, Card currentCardRound=null)
        {
            CardData cardData = card.GetCardData();
            
            if(card_image != null)
                card_image.sprite = cardData.GetFullArt(variant);
            if (frame_image != null)
            {
                ColorSO colorsSo = guildColors.GetColor(cardData.guild);
                if (colorsSo != null)
                {
                    frame_image.color = colorsSo.color;
                }
            }
            if (card_title != null)
                card_title.text = cardData.GetTitle().ToUpper();
            if (card_text != null)
                card_text.text = cardData.GetText();
            if (manaBar != null)
            {
                manaBar.UpdateMana(card.GetManaCost());
                
                bool needToPayAdditionalMana = currentCardRound != null &&
                                                   currentCardRound.GetPieceType() != PieceType.Monarch &&
                                                   card.GetCardData().possibleCasters.HasFlag(currentCardRound.GetPieceType());
                manaBar.SetAnyMana(needToPayAdditionalMana);
            }

            if (!GameplayData.Get().EveryPieceCanCastInterventions && this.card.GetCardData().possibleCasters != PieceType.Monarch)
            {
                handCardPossibleCasters.SetPossibleCasters(this.card.GetCardData().possibleCasters);
            }
            else
            {
                handCardPossibleCasters.gameObject.SetActive(false);
            }
            
            
            foreach (TraitUI stat in stats)
                stat.SetCard(cardData);
        }

        public override void SetMaterial(Material mat)
        {
            if (card_image != null)
                card_image.material = mat;
            if (frame_image != null)
                frame_image.material = mat;
            if (team_icon != null)
                team_icon.material = mat;
            if (rarity_icon != null)
                rarity_icon.material = mat;
            if (attack_icon != null)
                attack_icon.material = mat;
        }

        public override void SetOpacity(float opacity)
        {
            if (card_image != null)
                card_image.color = new Color(card_image.color.r, card_image.color.g, card_image.color.b, opacity);
            if (frame_image != null)
                frame_image.color = new Color(frame_image.color.r, frame_image.color.g, frame_image.color.b, opacity);
            if (team_icon != null)
                team_icon.color = new Color(team_icon.color.r, team_icon.color.g, team_icon.color.b, opacity);
            if (rarity_icon != null)
                rarity_icon.color = new Color(rarity_icon.color.r, rarity_icon.color.g, rarity_icon.color.b, opacity);
            if (attack_icon != null)
                attack_icon.color = new Color(attack_icon.color.r, attack_icon.color.g, attack_icon.color.b, opacity);
            if (manaBar != null)
                manaBar.SetOpacity(opacity);
            if (attack != null)
                attack.color = new Color(attack.color.r, attack.color.g, attack.color.b, opacity);
            if (card_title != null)
                card_title.color = new Color(card_title.color.r, card_title.color.g, card_title.color.b, opacity);
            if (card_text != null)
                card_text.color = new Color(card_text.color.r, card_text.color.g, card_text.color.b, opacity);
        }
    }
}
