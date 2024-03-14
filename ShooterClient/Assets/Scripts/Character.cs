using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [field: SerializeField] public int maxHP { get; protected set; } = 10;
    [field: SerializeField] public float speed { get; protected set; } = 2f;
    [SerializeField] public Transform _body;

    public Vector3 velocity { get; protected set; }
    public Vector3 angVelocity { get; protected set; }

    public float sitFactor { get; protected set; }
    public Action Sit;
    public Action Stand;
}
