using System.Collections;
using System.Collections.Generic;
using Monarchs.Logic;
using UnityEngine;

namespace Monarchs
{
    public class TooltipCardShower : MonoBehaviour
    {
        private Card _card;

        public void SetCard(Card card)
        {
            _card = card;
        }
        
        public void OnHoverEnter()
        {
            TooltipManager.ShowTooltip(_card, transform);
        }
        
        public void OnHoverExit()
        {
            TooltipManager.HideTooltip();
        }
    }
    
    
}
