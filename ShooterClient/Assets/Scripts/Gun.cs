using System;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] public Bullet _bulletPrefab;
    public Action shoot;
}
