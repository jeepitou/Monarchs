using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Monarchs
{
    public class HistoryDamage : MonoBehaviour
    {
        public TextMeshProUGUI damage;

        public void SetDamage(int damage)
        {
            this.damage.text = "-" + damage.ToString();
        }
    }
}