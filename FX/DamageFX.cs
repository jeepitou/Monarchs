using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TcgEngine.UI;

namespace TcgEngine.FX
{
    /// <summary>
    /// Text number FX that appear when a card receives damage
    /// </summary>

    public class DamageFX : MonoBehaviour
    {
        public Text text_value;

        void Start()
        {

        }

        void Update()
        {

        }

        public void SetValue(int value)
        {
            string valueString = "";
            if (value > 0)
            {
                valueString = "+";
            }

            valueString += value.ToString();
            
            if (text_value != null)
                text_value.text = valueString;
        }

        public void SetValue(string value)
        {
            if (text_value != null)
                text_value.text = value;
        }
    }
}