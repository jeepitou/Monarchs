using System.Collections;
using DG.Tweening;
using Monarchs.Tools;
using UnityEngine;

namespace Monarchs.Client
{
    public class HandCardFX : MonoBehaviour
    {
        public float GetDurationToHand(float velocity)
        {
            RectTransform childRectTransform = transform.GetChild(0).GetComponent<RectTransform>();
            float distance = Vector3.Distance(childRectTransform.localPosition, Vector3.zero);
            return distance / velocity;
        }
        
        public IEnumerator SendToHand(float velocity)
        {
            RectTransform childRectTransform = transform.GetChild(0).GetComponent<RectTransform>();
            float duration = GetDurationToHand(velocity);
            
            childRectTransform.DOLocalMove(Vector3.zero, duration).SetEase(Ease.InOutCubic);
            GetComponent<RectTransform>().DOLocalRotate(GetComponent<HandCard>().deckAngle*Vector3.forward, duration).SetEase(Ease.InOutCubic);
            childRectTransform.DOScale(Vector3.one, duration).SetEase(Ease.InOutCubic);
            
            yield return new WaitForSeconds(duration);
        }
        
        public IEnumerator AppearOnTopOf(RectTransform target, float fadeDuration)
        {
            GetComponent<RectTransform>().localRotation = Quaternion.Euler(Vector3.zero);
            transform.GetChild(0).gameObject.SetActive(true);
            
            RectTransform childRectTransform = transform.GetChild(0).GetComponent<RectTransform>();
            RectTransformTools.MoveRectTransformOnTopOfAnotherRectTransform(childRectTransform, target);
            
            GetComponent<CanvasGroup>().alpha = 0;
            GetComponent<HandCard>().HideAdditionnalInfo();
            GetComponent<CanvasGroup>().DOFade(1, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }
    }
}
