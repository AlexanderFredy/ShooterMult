using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private const string Grounded = "Grounded";
    private const string Speed = "Speed";
   
    [SerializeField] private CheckFly _checkFly;
    [SerializeField] private Animator _animator;
    [SerializeField] private Character _character;
    [SerializeField] private float _seatOffset = 1f;
    [SerializeField] private float _seatSpeed = 1f;

    private bool _sitDirection;
    private float _curSitDepth = 0f;
    private bool _sitStandCoroutineIsWorking = false;
    private Vector3 startBodyPosition;

    private void Start()
    {
        _character.Sit += Sit;
        _character.Stand += StandUp;
        startBodyPosition = _character._body.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 localVelocity = _character.transform.InverseTransformVector(_character.velocity);
        float speed = localVelocity.magnitude / _character.speed;
        float sign = Mathf.Sign(localVelocity.z);

        _animator.SetFloat(Speed, sign*speed);
        _animator.SetBool(Grounded, _checkFly.IsFly == false);
    }

    private void Sit()
    {
        _sitDirection = false;
        if (!_sitStandCoroutineIsWorking)
            StartCoroutine(SitStandCoroutine());
    }
    private void StandUp()
    {
        _sitDirection = true;
        if (!_sitStandCoroutineIsWorking)
            StartCoroutine(SitStandCoroutine());
    }

    private IEnumerator SitStandCoroutine()
    {
        _sitStandCoroutineIsWorking = true;

        while (_sitStandCoroutineIsWorking)
        {
            if (_curSitDepth > 0 || _curSitDepth < -_seatOffset)
            {
                _sitStandCoroutineIsWorking = false;
            }

            if (_sitDirection)
                _curSitDepth += _seatSpeed * Time.deltaTime;
            else
                _curSitDepth -= _seatSpeed * Time.deltaTime;

            _curSitDepth = Mathf.Clamp(_curSitDepth, -_seatOffset, 0);

            var locPos = startBodyPosition;
            locPos.z += _curSitDepth;
            _character._body.localPosition = locPos;           

            yield return null;
        }
    }

    private void OnDestroy()
    {
        _character.Sit -= Sit;
        _character.Stand -= StandUp;
    }
}
