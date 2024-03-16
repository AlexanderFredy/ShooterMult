using UnityEngine;

public class PlayerGun : Gun
{
    [SerializeField] private int _damage;
    [SerializeField] private Transform _bulletPoint;
    [SerializeField] private float _bulletSpeed;
    [SerializeField] private float _shootDelay;
    private float _lastShootTime;

    public bool TryShoot(out ShootInfo info)
    {
        info = new ShootInfo();
        
        if (Time.time - _lastShootTime < _shootDelay) return false;

        Vector3 position = _bulletPoint.position;
        Vector3 velocity = _bulletPoint.forward*_bulletSpeed;       
        
        _lastShootTime = Time.time;
        Instantiate( _bulletPrefab, position, _bulletPoint.rotation).Init(velocity, _damage);
        shoot?.Invoke();

        info.pX = position.x;
        info.pY = position.y;
        info.pZ = position.z;
        info.dX = velocity.x;
        info.dY = velocity.y;
        info.dZ = velocity.z;

        return true;
    }

    public void SetCurrentWeapon(int damage, float bulletSpeed, float shootDelay, Bullet bulletPrefab, GameObject weapon)
    {
        _damage = damage;
        _bulletSpeed = bulletSpeed;
        _shootDelay = shootDelay;
        _bulletPrefab = bulletPrefab;
        _bulletPoint = weapon.transform.Find("BulletPoint");
    }
}
