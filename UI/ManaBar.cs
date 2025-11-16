using System;
using Monarchs.Client;
using Monarchs.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace Monarchs.UI
{
    /// <summary>
    /// Bar that contain multiple icons to represent a value
    /// Such as the mana bar during the game
    /// </summary>

    public class ManaBar : MonoBehaviour
    {
        public ManaIconLink[] icons;
        public Image avatar;
        public bool playerSelf;
        
        private void Start()
        {
            GameClient.Get().onGameStart += SetAvatar;
        }

        public void SetAvatar()
        {
            Player player = GameClient.Get().GetPlayer();
            if (!playerSelf)
            {
                player = GameClient.Get().GetOpponentPlayer();
            }
            avatar.sprite = player.king.CardData.artFull;
        }
        
        public void UpdateMana(PlayerMana mana)
        {
            if (mana == null)
            {
                return;
            }
            
            foreach (ManaIconLink iconLink in icons)
            {
                if (mana.HasMana(iconLink.manaType))
                {
                    iconLink.iconColor.ApplyAlternateColor(0);
                    SetGlowActive(iconLink, true);
                }
                else
                {
                    iconLink.iconColor.ApplyDefaultColor();
                    SetGlowActive(iconLink, false);
                }
                
            }
        }

        private void SetGlowActive(ManaIconLink iconLink, bool active)
        {
            iconLink.glow.enabled = active;
        }

        [Serializable]
        public struct ManaIconLink
        {
            public Image icon;
            public Image glow;
            public SetImageColor iconColor;
            public PlayerMana.ManaType manaType;
        }
    }
}