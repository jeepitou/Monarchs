using System.Collections;
using UnityEngine;

namespace Monarchs
{
    public class AnimationQueueTimeElement : AnimationQueueElement
    {
        public float timeToWait = 0.5f;
        public override IEnumerator AnimationCoroutine()
        {
            yield return new WaitForSeconds(timeToWait);
        }
    }
}