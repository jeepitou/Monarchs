using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Client;
using Monarchs.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace Monarchs
{
    public class HistoryCard : MonoBehaviour
    {
        public GameObject coordinatePrefab;
        public Image background;
        public Image cardImage;
        public HistoryCoordinate coordinate;
        public TooltipCardShower tooltipCardShower;

        public void SetCard(Card card, Slot slot)
        {
            SetCard(card, new List<Slot>{slot});
        }
        
        public void SetCard(Card card, List<Slot> slots)
        {
            if (card == null)
            {
                cardImage.transform.gameObject.SetActive(false);
            }
            else
            {
                cardImage.sprite = card.CardData.artBoard;
                if (card.playerID == GameClient.Get().GetPlayerID())
                {
                    background.color = Color.green;
                }
                else
                {
                    background.color = Color.red;
                }
            }

            foreach (var slot in slots)
            {
                if (slot != Slot.None)
                {
                    HistoryCoordinate coordinate =
                        Instantiate(coordinatePrefab, transform).GetComponent<HistoryCoordinate>();
                    coordinate.SetCoordinates(slot);
                }
            }
            
            tooltipCardShower.SetCard(card);
        }

        public void SetAbility(AbilityData ability, List<Slot> slots)
        {
            cardImage.sprite = ability.icon;
            
            foreach (var slot in slots)
            {
                if (slot != Slot.None)
                {
                    HistoryCoordinate coordinate = Instantiate(coordinatePrefab, transform).GetComponent<HistoryCoordinate>();
                    coordinate.SetCoordinates(slot);
                }
            }
        }
    }
}
