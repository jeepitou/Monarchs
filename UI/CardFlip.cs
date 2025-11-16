using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Monarchs.UI
{
    public class CardFlip : MonoBehaviour
    {
        [Required] public GameObject cardFront;
        [Required] public GameObject cardBack;
        public float duration = 1f;
        public bool isFlipped;
        
        private void Start()
        {
            cardFront.SetActive(true);
            cardBack.SetActive(false);
        }
        
        [Button]
        public void Flip()
        {
            StartCoroutine(FlipCard());
        }
        
        public IEnumerator FlipCard()
        {
            transform.DOLocalRotate(new Vector3(0, 180, 0), duration).SetEase(Ease.Flash);
            
            float angle = 0;
            while (angle < 180)
            {
                if (angle > 90 && !isFlipped)
                {
                    cardFront.SetActive(false);
                    cardBack.SetActive(true);
                    isFlipped = true;
                }
                
                angle = transform.localEulerAngles.y;
                yield return null;
            }
            isFlipped = !isFlipped;
        }
    }
}
