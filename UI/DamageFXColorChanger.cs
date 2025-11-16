using System;
using TMPro;
using UnityEngine;

namespace Monarchs
{
    public class DamageFXColorChanger : MonoBehaviour
    {
        public TMP_Text text;
        public Color damageColor;
        public Color damageOutlineColor;
        public Color healColor;
        public Color healOutlineColor;

        private bool healing = false;
        
        private void Update()
        {
            if (text.text[0] == '+' && !healing)
            {
                text.color = healColor;
                text.fontMaterial.SetColor("_OutlineColor", healOutlineColor);
                healing = true;
            }
            else if (text.text[0] != '+' && healing)
            {
                text.color = damageColor;
                text.fontMaterial.SetColor("_OutlineColor", damageOutlineColor);
                healing = false;
            }
        }
    }
}
