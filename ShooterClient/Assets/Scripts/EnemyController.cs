using Colyseus.Schema;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] EnemyCharacter _enemy;

    internal void OnChange(List<DataChange> changes)
    {
        Vector3 position = transform.position;
        
        foreach (DataChange change in changes) 
        {
            switch (change.Field)
            {
                case "x":
                    position.x = (float)change.Value;
                    break;
                case "y":
                    position.z = (float)change.Value;
                    break;
                default:
                    Debug.Log("Dont use: " + change.Field);
                    break;
            }
        }

        transform.position = position;

        _enemy.UpdateNetworkPositions();
    }
}
