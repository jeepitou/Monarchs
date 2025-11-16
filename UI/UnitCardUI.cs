using System;
using System.Collections;
using System.Collections.Generic;
using Monarchs;
using Monarchs.Logic;
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

    public class UnitCardUI : CardUI, IPointerClickHandler
    {
        public Image hp_icon;
        public Image cohortSize_icon;
        public Image initiative_icon;
        public Image unitType_icon;
        public Image armor_icon;
        
        public Text hp;
        public Text initiative;
        public Text movement;
        public Text armor;
        public TMP_Text type;
        

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
            SetCardUI(card, variant);
            
            SetDescription();
            SetAOEPatterns();

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        private void SetCardUI(ICard card, VariantData variant)
        {
            CardData cardData = card.GetCardData();

            if (manaBar != null)
            {
                manaBar.UpdateMana(card.GetManaCost());
            }
            

            if (armor_icon != null)
            {
                if (card.GetArmor() != 0)
                {
                    armor.text = card.GetArmor().ToString(); 
                }
                else
                {
                    armor_icon.gameObject.SetActive(false);
                }
            }

            if (armorPen_icon != null)
            {
                armorPen_icon.gameObject.SetActive(card.HasArmorPenetration());
            }

            if (cohortSize_icon != null)
            {
                if (card.GetCohortSize() > 1)
                {
                    cohortSize_icon.gameObject.SetActive(true);
                    cohortSize_icon.sprite = GameplayData.Get().CohortIconLink.GetCohortIcon(card.GetCohortSize());
                }
                else
                {
                    cohortSize_icon.gameObject.SetActive(false);
                }
            }

            if (unitType_icon != null)
            {
                Sprite sprite = GameplayData.Get().MovementIconLink.GetSprite(card.GetPieceType());
                unitType_icon.sprite = sprite;
            }

            if (movement != null)
            {
                if (card.GetPieceType() == PieceType.Knight)
                {
                    movement.text = "";
                }
                else
                {
                    movement.text = card.GetMoveRange().ToString();
                }
            }

            if (initiative != null)
            {
                initiative.text = card.GetInitiative().ToString();
            }
            
            if (range_icon != null)
            {
                if (card.GetMaxAttackRange() > 1)
                {
                    range_icon.gameObject.SetActive(true);
                    if (card.GetMinAttackRange() != 0)
                    {
                        range.text = card.GetMinAttackRange().ToString() + "-" + card.GetMaxAttackRange().ToString();
                    }
                    else
                    {
                        range.text = card.GetMaxAttackRange().ToString();
                    }
                }
                else
                {
                    range_icon.gameObject.SetActive(false);
                }

            }
            
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

            if (type != null )
            {
                if (card.GetSubtypes() != null && card.GetSubtypes().Length >= 0)
                {
                    type.text = "";
                    foreach (var subtype in card.GetSubtypes())
                    {
                        type.text += subtype.title + " ";
                    }
                }
                else
                {
                    type.enabled = false;
                }
            }
            
            if (attack_icon != null)
                attack_icon.enabled = cardData.IsCharacter();
            if (attack != null)
                attack.enabled = cardData.IsCharacter();
            if (hp_icon != null)
                hp_icon.enabled = cardData.IsBoardCard();
            if (hp != null)
                hp.enabled = cardData.IsBoardCard();

            if (manaBar != null)
            {
                manaBar.UpdateMana(card.GetManaCost());
            }

            if (attack != null)
                SetAttackText(attack);
            if (hp != null)
                hp.text = card.GetHP().ToString();

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
            if (hp_icon != null)
                hp_icon.material = mat;
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
            if (hp_icon != null)
                hp_icon.color = new Color(hp_icon.color.r, hp_icon.color.g, hp_icon.color.b, opacity);
            if (manaBar != null)
                manaBar.SetOpacity(opacity);
            if (attack != null)
                attack.color = new Color(attack.color.r, attack.color.g, attack.color.b, opacity);
            if (hp != null)
                hp.color = new Color(hp.color.r, hp.color.g, hp.color.b, opacity);
            if (card_title != null)
                card_title.color = new Color(card_title.color.r, card_title.color.g, card_title.color.b, opacity);
            if (card_text != null)
                card_text.color = new Color(card_text.color.r, card_text.color.g, card_text.color.b, opacity);
        }
    }
}
