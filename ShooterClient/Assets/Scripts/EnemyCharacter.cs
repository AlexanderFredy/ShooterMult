using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacter : MonoBehaviour
{
    private float _lastTimeNetworkUpdatePosition = 0.02f;
    private PositionTime[] _positionsInTime = new PositionTime[2];

    private void Awake()
    {
        _positionsInTime[0].Position = transform.position.Round(2);
        _positionsInTime[0].Time = Time.time;

        _positionsInTime[1].Position = transform.position.Round(2);
        _positionsInTime[1].Time = Time.time + 1f;
    }

    private void Update()
    {
        //print(Time.time - _lastTimeNetworkUpdatePosition);

        if (Time.time - _lastTimeNetworkUpdatePosition > 0.02f)
        {
            var currrentSpeed = (_positionsInTime[1].Position - _positionsInTime[0].Position)/(_positionsInTime[1].Time - _positionsInTime[0].Time);
            transform.position += currrentSpeed * Time.deltaTime;
        }
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;      
    }

    public void UpdateNetworkPositions()
    {
        _positionsInTime[0] = _positionsInTime[1];
        _positionsInTime[1].Position = transform.position.Round(2);
        _positionsInTime[1].Time = Time.time;

        _lastTimeNetworkUpdatePosition = Time.time;
    }

    private struct PositionTime
    {
        public Vector3 Position;
        public float Time;
    }

}
