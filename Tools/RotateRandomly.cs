using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monarchs
{
    public class RotateRandomly : MonoBehaviour
    {
        public float rotationSpeed;
        private Vector3 _rotationDirection;
        
        private void Start()
        {
            SetRandomRotation();
            GetRandomRotationDirection();
        }
        
        public void SetRandomRotation()
        {
            transform.rotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        }

        private void GetRandomRotationDirection()
        {
            _rotationDirection = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
        }
        
        private void Update()
        {
            transform.Rotate(_rotationDirection * rotationSpeed * Time.deltaTime);
        }
    }
}
