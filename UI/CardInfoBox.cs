using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.UI
{
    /// <summary>
    /// In the CardPreviewUI, a list of current status appear
    /// This is one of those
    /// </summary>

    public class CardInfoBox : MonoBehaviour
    {
        public GameObject infoTypeLabel;
        public TMP_Text infoType;
        public TMP_Text description;
        
        public Color activeAbilityLabelColor;
        public Color activeAbilityDescColor;
        public Color passiveAbilityDescColor;
        public Color statusLabelColor;
        public Color statusDescColor;

        private float timer = 0f;
        
        void Update()
        {
            timer += Time.deltaTime;
        }

        public void SetInfoBox(string desc, string type = "")
        {
            if (type == "")
            {
                infoTypeLabel.SetActive(false);
            }
            else
            {
                infoTypeLabel.SetActive(true);
                infoType.text = type;
            }
            description.text = desc;

            if (type == "Active")
            {
                infoTypeLabel.GetComponent<Image>().color = activeAbilityLabelColor;
                description.transform.parent.GetComponent<Image>().color = activeAbilityDescColor;
            }
            else if (type == "")
            {
                description.transform.parent.GetComponent<Image>().color = passiveAbilityDescColor;
            }
            else if (type == "Status")
            {
                infoTypeLabel.GetComponent<Image>().color = statusLabelColor;
                description.transform.parent.GetComponent<Image>().color = statusDescColor;
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        public void Hide()
        {
            if (timer > 0.05f)
                gameObject.SetActive(false);
        }
    }
}
