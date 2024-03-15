using Colyseus.Schema;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] EnemyCharacter _enemy;
    [SerializeField] EnemyGun _gun;
    private List<float> _recievedTimeInterval = new List<float> { 0,0,0,0,0};
    private float _lastResiveTime = 0f;
    private Player _player;

    private float AvarageInterval
    {
        get
        {
            float totalIntervalTime = 0;
            for (int i = 0; i < _recievedTimeInterval.Count; i++)
            {
                totalIntervalTime += _recievedTimeInterval[i];
            }

            return totalIntervalTime / _recievedTimeInterval.Count;
        }
    }

    public void Init(string key, Player player)
    {
        _enemy.Init(key);

        _player = player;
        _enemy.SetSpeed(player.speed);
        _enemy.SetMaxHP(player.maxHp);
        _player.OnChange += OnChange;
    }

    public void Shoot(in ShootInfo info)
    {
        Vector3 position = new Vector3(info.pX, info.pY, info.pZ);
        Vector3 velocity = new Vector3(info.dX, info.dY, info.dZ);

        _gun.Shoot(position,velocity);
    }

    public void Destroy()
    {
        _player.OnChange -= OnChange;
        Destroy(gameObject);
    }

    private void SaveRecievedTime()
    {
        float interval = Time.time - _lastResiveTime;
        _lastResiveTime = Time.time;

        _recievedTimeInterval.Add(interval);
        _recievedTimeInterval.RemoveAt(0);
    }

    internal void OnChange(List<DataChange> changes)
    {
        SaveRecievedTime();
        
        Vector3 position = _enemy.targetPosition;
        Vector3 velocity = _enemy.velocity;

        float rotationY = _enemy.targetRotationY;
        Vector3 angVelocity = _enemy.angVelocity;

        foreach (DataChange change in changes) 
        {
            switch (change.Field)
            {
                case "pX":
                    position.x = (float)change.Value;
                    break;
                case "pY":
                    position.y = (float)change.Value;
                    break;
                case "pZ":
                    position.z = (float)change.Value;
                    break;
                case "vX":
                    velocity.x = (float)change.Value;
                    break;
                case "vY":
                    velocity.y = (float)change.Value;
                    break;
                case "vZ":
                    velocity.z = (float)change.Value;
                    break;
                case "rX":
                    _enemy.SetRotateHeadX((float)change.Value);
                    break;
                case "rY":
                    rotationY = (float)change.Value;
                    break;
                case "avY":
                    angVelocity.y = (float)change.Value;
                    break;
                default:
                    Debug.LogWarning("Dont use: " + change.Field);
                    break;
            }
        }

        _enemy.SetMovement(position,velocity, AvarageInterval);
        _enemy.SetRotateBodyY(rotationY,angVelocity, AvarageInterval);
    }
}
