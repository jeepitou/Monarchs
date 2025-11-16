using System.Collections;
using System.Collections.Generic;
using Monarchs.Logic;
using UnityEngine;

namespace Monarchs
{
    public class TooltipManager : MonoBehaviour
    {
        public TextTooltip textTooltip;
        public CardTooltip cardTooltip;
        
        public static TooltipManager instance;
        
        // Start is called before the first frame update
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void ShowTextTooltip(string text)
        {
            textTooltip.ShowTooltip(text);
        }
        
        public static void ShowTooltip(string text)
        {
            instance.ShowTextTooltip(text);
        }
        
        public static void ShowTooltip(Card card, Transform target)
        {
            instance.cardTooltip.ShowTooltip(card, target);
        }
        
        public static void HideTooltip()
        {
            instance.textTooltip.HideTooltip();
            instance.cardTooltip.HideTooltip();
        }
    }
}
