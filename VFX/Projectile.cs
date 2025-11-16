using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Projectile : MonoBehaviour
{
    public GameObject impactVFX;
    public float speed;
    public float verticalAmplitude;
    public float horizontalAmplitude;
    public AnimationCurve speedCurve;
    public AnimationCurve verticalCurve;
    public AnimationCurve horizontalCurve;
    public UnityAction onImpact;
    
    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private Vector3 _currentDirection;
    private Vector3 _straightDirection;
    private Vector3 _horizontalDirection;
    private Vector3 _verticalDirection = Vector3.up;
    private void Start()
    {
        _startPosition = transform.position;
    }

    public void SetTargetPosition(Vector3 targetPosition)
    {
        transform.position += Vector3.up*.2f;
        _targetPosition = targetPosition + Vector3.up*.2f; 
        float distance = (_targetPosition - transform.position).magnitude;
        float duration = distance / speed;
        Vector3 middlePosition = (_startPosition + _targetPosition) / 2;
        
        _straightDirection = (_targetPosition - _startPosition).normalized;
        _horizontalDirection = Vector3.Cross(_straightDirection, _verticalDirection);

        transform.DOMove(_targetPosition, duration).SetEase(speedCurve)
            .OnUpdate(() =>
        {
            transform.position += _horizontalDirection * horizontalCurve.Evaluate(1 - (transform.position - _startPosition).magnitude / distance) * horizontalAmplitude;
            transform.position += _verticalDirection * verticalCurve.Evaluate(1 - (transform.position - _startPosition).magnitude / distance) * verticalAmplitude;
            transform.LookAt(_targetPosition);
        })
            .OnComplete(() =>
        {
            onImpact?.Invoke();
            if (impactVFX != null)
            {
                GameObject g = Instantiate(impactVFX, _targetPosition, Quaternion.identity);
                Destroy(g, 10f);
            }

            Destroy(gameObject);
        });
    }
}
