using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class EnemyCharacter : Character
{
    [SerializeField] private Transform _head;
    public Vector3 targetPosition { get; set; } = Vector3.zero;
    public float targetRotationY { get; set; } = 0f;

    private float _velocityMagnitude = 0f;
    private float _angleVelocityMagnitudeY = 0f;
    private float shift = 0f;

    private void Start()
    {
        targetPosition = transform.position;
        targetRotationY = transform.rotation.eulerAngles.y;
    }

    private void Update()
    {
        if (_velocityMagnitude > .1f)
        {
            float maxDistance = _velocityMagnitude * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, maxDistance);
        }
        else
            transform.position = targetPosition;

        if (_angleVelocityMagnitudeY > .1f)
        {
            shift += _angleVelocityMagnitudeY * Mathf.Rad2Deg * Time.deltaTime;
            Vector3 newRotation = transform.eulerAngles;
            newRotation.y = targetRotationY;

            newRotation.y = Mathf.LerpAngle(transform.eulerAngles.y, targetRotationY, shift);
            transform.eulerAngles = newRotation;
        }
        else
        {
            transform.eulerAngles = new Vector3(0, targetRotationY, 0);
        }

    }

    public void SetSpeed(float value) => speed = value;

    public void SetMovement(in Vector3 position, in Vector3 velocity, in float avarageInterval)
    {
        targetPosition = position + (velocity*avarageInterval);
        _velocityMagnitude = velocity.magnitude;
        
        this.velocity = velocity;
    }

    public void SetRotateHeadX(float value)
    {              
        _head.localEulerAngles = new Vector3(value, 0, 0);
    }

    public void SetRotateBodyY(float value, in Vector3 angVelocity, in float avarageInterval)
    {
        targetRotationY = value + (angVelocity.y * avarageInterval);
        _angleVelocityMagnitudeY = angVelocity.magnitude;

        this.angVelocity = angVelocity;
        shift = 0;
    }
}
