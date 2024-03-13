using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [field: SerializeField] public float speed { get; protected set; } = 2f;

    public Vector3 velocity { get; protected set; }
    public Vector3 angVelocity { get; protected set; }
}
