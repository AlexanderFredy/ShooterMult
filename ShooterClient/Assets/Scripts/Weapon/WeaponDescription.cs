using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName= "WeaponDescription", menuName = "Weapons/WeaponDescription")]
public class WeaponDescription : ScriptableObject
{
    public int damage;
    public Transform bulletPoint;
    public float bulletSpeed;
    public float shootDelay;

    public GameObject gunPrefab;
    public Bullet bulletPrefab;
}
