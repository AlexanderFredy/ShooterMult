using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGun : Gun
{
    [SerializeField] private WeaponDescription[] _weaponArray = new WeaponDescription[3];
    [SerializeField] private Transform _weaponHandler;
    private GameObject _weaponModel;
    private Transform _bulletPoint;

    private void Start()
    {
        ChangeWeapon(0);
    }

    public void Shoot(Vector3 position, Vector3 velocity)
    {
        Instantiate(_bulletPrefab, position, _bulletPoint.rotation).Init(velocity);
        shoot?.Invoke();
    }

    public void ChangeWeapon(int num)
    {
        if (_weaponModel != null) Destroy(_weaponModel);

        _weaponModel = Instantiate(_weaponArray[num].gunPrefab, _weaponHandler);
        _bulletPoint = _weaponModel.transform.Find("BulletPoint");
        _bulletPrefab = _weaponArray[num].bulletPrefab;

        if (_weaponModel.TryGetComponent(out GunAnimations anim))
            anim.SetGun(this);
    }
}
