using UnityEngine;

namespace Monarchs.UI
{
    /// <summary>
    /// Bar that contain multiple icons to represent a value
    /// Such as the mana bar during the game
    /// </summary>

    public class ManaBarHandCard : MonoBehaviour
    {
        public ManaBar.ManaIconLink[] icons;
        public GameObject anyMana;

        public void UpdateMana(PlayerMana mana)
        {
            if (mana == null)
            {
                return;
            }
            
            foreach (ManaBar.ManaIconLink iconLink in icons)
            {
                iconLink.icon.enabled = (mana.HasMana(iconLink.manaType));
            }
        }
        
        public void UpdateMana(PlayerMana.ManaType mana)
        {
            foreach (ManaBar.ManaIconLink iconLink in icons)
            {
                iconLink.icon.gameObject.SetActive(mana.HasFlag(iconLink.manaType));
            }
            
            
        }

        public void SetOpacity(float opacity)
        {
            foreach (ManaBar.ManaIconLink iconLink in icons)
            {
                iconLink.icon.color = new Color(iconLink.icon.color.r, iconLink.icon.color.g, iconLink.icon.color.b, opacity);
            }
        }

        public void SetAnyMana(bool newState)
        {
            if (anyMana != null)
                anyMana.SetActive(newState);
        }
    }
}