using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Monarchs
{
    public class AbilityFXMoveToGameObjectAfterDelay : AbilityFXMultipleTarget
    {
        public Vector3 offset;
        public float delay;
        public float speed;
        public Ease easeType;
        public GameObject impactPrefab;

        private float _time = 0f;
        private bool _triggered = false;


        // Update is called once per frame
        void Update()
        {
            if (_triggered)
                return;

            if (_time >= delay)
            {
                _triggered = true;
                DoMovement();
            }

            _time += Time.deltaTime;
        }

        void DoMovement()
        {
            Vector3 targetPosition = GetTargetGameObject().transform.position + offset;
            float distance = (transform.position - targetPosition).magnitude;
            float duration = distance/speed;

            transform.DOMove(targetPosition, duration).SetEase(easeType).OnComplete(()=>{
                Instantiate(impactPrefab, targetPosition, Quaternion.identity);
                Destroy(gameObject);
            });
        }
    }
}
