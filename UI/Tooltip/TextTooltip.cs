using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Monarchs
{
    public class TextTooltip : MonoBehaviour
    {
        public Camera uiCamera;
        public TMP_Text description;
        public RectTransform background;
        public RectTransform canvasRectTransform;
        public float padding = 10;

        private void Update()
        {
            Vector2 localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), Input.mousePosition, uiCamera, out localPosition);
            transform.localPosition = localPosition;
            
            Vector2 anchoredPosition = transform.GetComponent<RectTransform>().anchoredPosition;

            if (anchoredPosition.x - background.rect.width < -canvasRectTransform.rect.width/2)
            {
                anchoredPosition.x = -canvasRectTransform.rect.width/2 + background.rect.width;
            }

            if (anchoredPosition.x > canvasRectTransform.rect.width/2)
            {
                anchoredPosition.x = canvasRectTransform.rect.width/2;
            }

            if (anchoredPosition.y - background.rect.height < -canvasRectTransform.rect.height/2)
            {
                anchoredPosition.y = -canvasRectTransform.rect.height/2 + background.rect.height;
            }

            if (anchoredPosition.y > canvasRectTransform.rect.height/2)
            {
                anchoredPosition.y = canvasRectTransform.rect.height/2;
            }

            transform.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
        }
        
        public void ShowTooltip(string text)
        {
            gameObject.SetActive(true);
            
            description.text = text;
            Vector2 backgroundSize = new Vector2(description.preferredWidth + padding*2f, description.preferredHeight + padding*2f);
            background.sizeDelta = backgroundSize;
        }
        
        public void HideTooltip()
        {
           gameObject.SetActive(false);
        }
    }
}
