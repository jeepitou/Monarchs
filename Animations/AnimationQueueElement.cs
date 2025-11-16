using System.Collections;
using UnityEngine;

namespace Monarchs
{
    public class AnimationQueueElement: MonoBehaviour
    {
        public Animator animator;
        public string animationName;
        public virtual IEnumerator AnimationCoroutine()
        {
            yield return null;
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}