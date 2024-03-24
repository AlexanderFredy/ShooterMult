using Sirenix.OdinInspector;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerGun : Gun
{
    [SerializeField] private int _damage;
    [SerializeField] private Transform _bulletPoint;
    [SerializeField] private float _bulletSpeed;
    [SerializeField] private float _shootDelay;
    [SerializeField] private HeadShotLabel _headshotLablePrefab;
    [SerializeField] private RectTransform _canvas;

    [Header("Aim")]
    [SerializeField] private Transform _aimIndicator;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float maxDistance;
    [SerializeField] private float _pointSize;

    private float _lastShootTime;
    private Camera _camera;
    private Vector3 _aimPoint;

    public bool TryShoot(out ShootInfo info)
    {
        info = new ShootInfo();
        
        if (Time.time - _lastShootTime < _shootDelay) return false;

        Vector3 position = _bulletPoint.position;
        Vector3 direction = (_aimPoint - _bulletPoint.position).normalized;
        Vector3 velocity = direction * _bulletSpeed;    
        
        _lastShootTime = Time.time;
        Bullet bul = Instantiate(_bulletPrefab, position, _bulletPoint.rotation);
        bul.Init(velocity, _damage);
        bul.headShot += ShowHeadShot; //не забьёт ли память???
        shoot?.Invoke();

        info.pX = position.x;
        info.pY = position.y;
        info.pZ = position.z;
        info.dX = velocity.x;
        info.dY = velocity.y;
        info.dZ = velocity.z;

        return true;
    }

    private void ShowHeadShot(Vector3 position)
    {
        var pos = Camera.main.WorldToScreenPoint(position) + new Vector3(0f, 40f);
        Instantiate(_headshotLablePrefab, pos, Quaternion.identity, _canvas);
    }

    public void SetCurrentWeapon(int damage, float bulletSpeed, float shootDelay, Bullet bulletPrefab, GameObject weapon)
    {
        _damage = damage;
        _bulletSpeed = bulletSpeed;
        _shootDelay = shootDelay;
        _bulletPrefab = bulletPrefab;
        _bulletPoint = weapon.transform.Find("BulletPoint");
    }

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        Vector3 point = new Vector3(_camera.pixelWidth / 2, _camera.pixelHeight / 2, 0);
        Ray ray = _camera.ScreenPointToRay(point);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, ~_layerMask, QueryTriggerInteraction.Ignore))
        {
            _aimPoint = hit.point;
            //_aimIndicator.position = hit.point;
            //float distance = Vector3.Distance(point, hit.point);
            //_aimIndicator.localScale = Vector3.one * _pointSize * distance;
        }
        else
        {
            _aimPoint = ray.GetPoint(maxDistance);
            //_aimIndicator.position = hit.point;
            //_aimIndicator.localScale = Vector3.one * _pointSize * maxDistance;
        }      
    }

    void OnGUI()
    {
        int size = 12;
        float posX = _camera.pixelWidth / 2 - size / 4;
        float posY = _camera.pixelHeight / 2 - size / 2;
        GUI.Label(new Rect(posX, posY, size, size), "X");
}
}
