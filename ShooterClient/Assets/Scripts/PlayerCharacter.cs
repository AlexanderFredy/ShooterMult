
using Colyseus.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Character
{
    [SerializeField] private Health _health;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Transform _head;
    [SerializeField] private Transform _cameraPoint;
    [SerializeField] private float _minHeadAngle = -90;
    [SerializeField] private float _maxHeadAngle = 90;
    [SerializeField] private float _jumpForce = 50;
    [SerializeField] private CheckFly _checkFly;
    [SerializeField] private float _jumpDelay = .2f;

    private float _inputV;
    private float _inputH;
    private float _rotateY;
    private float _currentRotateX;
    private float _jumpTime = 0;

    private void Start()
    {
        Transform camera = Camera.main.transform;
        camera.parent = _cameraPoint;
        camera.localPosition = Vector3.zero;
        camera.localRotation = Quaternion.identity;

        _health.SetMax(maxHP);
        _health.SetCurrent(maxHP);
    }

    public void SetInput(float h, float v, float rotateY)
    {
        _inputH = h;
        _inputV = v;
        _rotateY += rotateY;
    }

    void FixedUpdate()
    {
        Move();
        RotateY();
    }

    private void Move()
    {
        Vector3 velocity = (transform.forward * _inputV + transform.right * _inputH).normalized*speed;
        velocity.y = _rigidbody.velocity.y;
        base.velocity = velocity;
        _rigidbody.velocity = velocity;
    }

    public void RotateX(float value)
    {
        _currentRotateX = Mathf.Clamp(_currentRotateX + value, _minHeadAngle, _maxHeadAngle);
        _head.localEulerAngles = new Vector3(_currentRotateX,0,0);
    }

    private void RotateY()
    {
        _rigidbody.angularVelocity = new Vector3(0, _rotateY, 0);
        _rotateY = 0;
    }

    public void GetMoveInfo(out Vector3 position, out Vector3 velocity, out float rotateX, out float rotateY, out float angVelocityY)
    {
        position = transform.position;
        velocity = _rigidbody.velocity;

        rotateX = _head.transform.localEulerAngles.x;
        rotateY = transform.localEulerAngles.y;

        //сглаживание поворота
        angVelocityY = _rigidbody.angularVelocity.y;//body
    }

    public void Jump()
    {
        if (_checkFly.IsFly) return;
        if (Time.time - _jumpTime < _jumpDelay) return;

        _jumpTime = Time.time;
        _rigidbody.AddForce(0, _jumpForce, 0, ForceMode.VelocityChange);
    }

    internal void SitDown() => Sit?.Invoke();
    internal void StandUp() => Stand?.Invoke();

    internal void OnChange(List<DataChange> changes)
    {
        foreach (DataChange change in changes)
        {
            switch (change.Field)
            {
                case "curHp":
                    _health.SetCurrent((sbyte)change.Value);
                    break;
                //default:
                //    Debug.LogWarning("Dont use: " + change.Field);
                //    break;
            }
        }
    }
}
