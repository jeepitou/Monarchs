using System;
using System.Collections;
using System.Collections.Generic;
using Monarchs;
using Monarchs.Ability;
using Monarchs.Client;
using Monarchs.Logic;
using UnityEngine;
using UnityEngine.UI;
using TcgEngine.Client;
using TMPro;

namespace TcgEngine.UI
{
    /// <summary>
    /// Ability button on a BoardCard, let you activate abilities
    /// </summary>

    public class AbilityButton : MonoBehaviour
    {
        public Text titleText;
        public Text descriptionText;
        public GameObject abilityDescriptionObject;
        public Image icon;
        public float hoverTimeDelay;
        public Image glow;

        public static AbilityButton hoveredAbilityButton;
        
        public TextTooltip tooltip;
        public bool isCardExporter = false;
        public Material grayscaleMaterial;
        
        private Card card;
        private AbilityData iability;

        private CanvasGroup canvas_group;
        private float target_alpha = 0f;
        private bool focus = false;
        private bool _active = false;
        private float hoverTimer = 0f;
        

        private static List<AbilityButton> button_list = new List<AbilityButton>();

        void Awake()
        {
            if (isCardExporter)
                return;
            button_list.Add(this);
            canvas_group = GetComponent<CanvasGroup>();
            canvas_group.alpha = 0f;
            if (icon != null)
                icon.enabled = false;
        }

        private void Start()
        {
            if (isCardExporter)
                return;
            GameClient.Get().onAbilityHoveredByOpponent += OnAbilityHovered;
        }

        private void OnAbilityHovered(AbilityData ability, Card card)
        {
            if (ability == null || card == null)
            {
                glow.enabled = false;
                return;
            }
            
            if (ability.id == iability?.id && card.uid == this.card?.uid && _active)
            {
                glow.enabled = true;
            }
            else
            {
                glow.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (isCardExporter)
                return;
            button_list.Remove(this);
        }

        void Update()
        {
            if (isCardExporter)
                return;
            canvas_group.alpha = Mathf.MoveTowards(canvas_group.alpha, target_alpha, 5f * Time.deltaTime);
            
            if (focus && abilityDescriptionObject.activeSelf == false)
            {
                hoverTimer += Time.deltaTime;

                if (hoverTimer >= hoverTimeDelay)
                {
                    abilityDescriptionObject.SetActive(true);
                }
            }
            if (card == null)
                return;
            
            bool playerCard = card.playerID == GameClient.Get().GetPlayerID();
            bool playerTurn = GameClient.GetGameData().IsPlayerTurn(GameClient.Get().GetPlayer());
            _active = !card.exhausted && (isCardExporter || (playerCard && playerTurn));
            
            icon.color = _active ? Color.white : new Color(1, 1, 1, 0.4f);
            //icon.material = _active ? null : grayscaleMaterial;
        }

        public void SetAbility(Card card, AbilityData iability)
        {
            this.card = card;
            this.iability = iability; 
            titleText.text = iability.title;
            descriptionText.text = iability.desc;

            if (isCardExporter || card.playerID == GameClient.Get().GetPlayerID() &&
                !card.exhausted)
            {
                canvas_group.interactable = true;
                
                canvas_group.blocksRaycasts = true;
            }
            
            target_alpha = 1f;
            if (iability.icon != null)
            {
                icon.sprite = iability.icon;
            }

            icon.enabled = true;
            
            if (isCardExporter)
                abilityDescriptionObject.SetActive(true);
        }

        public void Hide()
        {
            if (canvas_group == null)
                canvas_group = GetComponent<CanvasGroup>();

            this.card = null;
            this.iability = null;
            canvas_group.interactable = false;
            target_alpha = 0f;
            icon.enabled = false;
            abilityDescriptionObject.SetActive(false);
        }

        public void OnClick()
        {
            if (card != null && iability != null && !card.exhausted && _active)
            {
                Debug.Log($"Player {GameClient.Get().GetPlayerID()} is casting ability {iability.id} with card {card.CardData.id}");
                GameClient.Get().CastAbility(card, iability);
                PlayerControls.Get().UnselectAll();
            }
        }

        public bool IsVisible()
        {
            return canvas_group.alpha > 0.5f;
        }

        public void OnMouseEnter()
        {
            focus = true;
            hoveredAbilityButton = this;

            bool isPlayerTurn = GameClient.GetGameData().IsPlayerTurn(GameClient.Get().GetPlayer());
            if (isPlayerTurn)
            {
                GameClient.Get().HoverAbility(iability, card);
            }
            
            if (!GameClient.GetGameData().GetCard(card.uid).playedCardThisRound && isPlayerTurn)
            {
                tooltip.ShowTooltip("You won't be able to play a card after using an ability");
            }
        }

        public void OnMouseExit()
        {
            focus = false;
            hoveredAbilityButton = null;
            GameClient.Get().HoverAbility(null, null);
            hoverTimer = 0f;
            if (abilityDescriptionObject.activeSelf == true)
            {
                abilityDescriptionObject.SetActive(false);
            }
            
            bool isPlayerTurn = GameClient.GetGameData().IsPlayerTurn(GameClient.Get().GetPlayer());
            if (isPlayerTurn)
            {
                tooltip.HideTooltip();
            }
        }

        public static AbilityButton GetHover(Vector3 pos, float range = 999f)
        {
            AbilityButton nearest = null;
            float min_dist = range;
            foreach (AbilityButton button in button_list)
            {
                float dist = (button.transform.position - pos).magnitude;
                if (button.focus && button.IsVisible() && dist < min_dist)
                {
                    min_dist = dist;
                    nearest = button;
                }
            }
            return nearest;
        }

        public static AbilityButton GetNearest(Vector3 pos, float range = 999f)
        {
            AbilityButton nearest = null;
            float min_dist = range;
            foreach (AbilityButton button in button_list)
            {
                float dist = (button.transform.position - pos).magnitude;
                if (dist < min_dist)
                {
                    min_dist = dist;
                    nearest = button;
                }
            }
            return nearest;
        }

    }
}
