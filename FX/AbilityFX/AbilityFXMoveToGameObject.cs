using UnityEngine;

namespace Monarchs
{
    public class AbilityFXMoveToGameObject : AbilityFXMultipleTarget
    {
        public Vector3 offset;

        void Start()
        {
            transform.position = GetTargetGameObject().transform.position + offset;
        }
    }
}
