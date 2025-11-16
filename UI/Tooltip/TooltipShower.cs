using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monarchs
{
    public class TooltipShower : MonoBehaviour
    {
        
        public void OnHoverEnter()
        {
            TooltipManager.ShowTooltip("Test 231564564as1dsad1 ");
        }
        
        public void OnHoverExit()
        {
            TooltipManager.HideTooltip();
        }
    }
    
    
}
