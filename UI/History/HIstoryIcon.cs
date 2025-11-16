using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monarchs
{
    public class HistoryIcon : MonoBehaviour
    {
        public Image image;
        public Sprite moveIcon;
        public Sprite attackIcon;
        public Sprite rangedAttackIcon;
        public Sprite cardPlayedIcon;
        
        
        public void SetIcon(HistoryIconType iconType)
        {
            if (iconType == HistoryIconType.Move)
            {
                image.sprite = moveIcon;
            }
            else if (iconType == HistoryIconType.Attack)
            {
                image.sprite = attackIcon;
            }
            else if (iconType == HistoryIconType.RangedAttack)
            {
                image.sprite = rangedAttackIcon;
            }
            else if (iconType == HistoryIconType.CardPlayed)
            {
                image.sprite = cardPlayedIcon;
            }
        }

        public void SetIcon(Sprite icon)
        {
            image.sprite = icon;
        }

        public enum HistoryIconType
        {
            Move,
            Attack,
            RangedAttack,
            CardPlayed
        }
    }
}
