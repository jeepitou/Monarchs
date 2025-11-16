using System;
using Monarchs.Logic;
using TMPro;
using UnityEngine;

namespace Monarchs
{
    public class CardTooltip : MonoBehaviour
    {
        public RectTransform cardRect;
        public RectTransform canvasRectTransform;
        public float padding = 50;
        public HandCardUIManager handCardUIManager;

        public void ShowTooltip(Card card, Transform target)
        {
            gameObject.SetActive(true);
            handCardUIManager.SetCard(card);
            
            UpdatePosition(target);
        }

        private void UpdatePosition(Transform target)
        {
            RectTransform rect = GetComponent<RectTransform>();
            
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, target.position);
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), screenPoint, Camera.main, out Vector2 localPosition);
            
            rect.localPosition = localPosition;
            
            Vector2 anchoredPosition = rect.anchoredPosition;

            if (anchoredPosition.x< -canvasRectTransform.rect.width/2)
            {
                anchoredPosition.x = -canvasRectTransform.rect.width/2; ;
            }
            
            if (anchoredPosition.x + cardRect.rect.width  > canvasRectTransform.rect.width/2)
            {
                anchoredPosition.x = canvasRectTransform.rect.width/2 - cardRect.rect.width - padding;
            }
            
            if (anchoredPosition.y - cardRect.rect.height < -canvasRectTransform.rect.height/2)
            {
                anchoredPosition.y = -canvasRectTransform.rect.height/2 + cardRect.rect.height + padding;
            }
            
            if (anchoredPosition.y > canvasRectTransform.rect.height/2)
            {
                anchoredPosition.y = canvasRectTransform.rect.height/2;
            }
            
            transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(anchoredPosition.x, anchoredPosition.y, 0);
        }
    
        public void HideTooltip()
        {
            gameObject.SetActive(false);
        }
    }
}
