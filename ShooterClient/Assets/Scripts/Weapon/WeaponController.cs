using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private WeaponDescription[] _weaponArray = new WeaponDescription[3];
    [SerializeField] private PlayerGun _currentGun;
    [SerializeField] private Transform _weaponHandler;
    private GameObject _weaponModel;

    private void Start()
    {
        SetWeaponFromInventory(0);
    }

    void Update()
    {
        int weaponNumber = -1;
        
        bool weapon1 = Input.GetKeyDown(KeyCode.Alpha1);
        bool weapon2 = Input.GetKeyDown(KeyCode.Alpha2);
        bool weapon3 = Input.GetKeyDown(KeyCode.Alpha3);

        if (weapon1) weaponNumber = 0;
        if (weapon2) weaponNumber = 1;
        if (weapon3) weaponNumber = 2;

        if (weaponNumber > -1)
            SetWeaponFromInventory(weaponNumber);
    }

    public void SetWeaponFromInventory(int weaponNumber)
    {
        if (_weaponModel != null) Destroy(_weaponModel);

        _weaponModel = Instantiate(_weaponArray[weaponNumber].gunPrefab,_weaponHandler.transform);

        _currentGun.SetCurrentWeapon(
            _weaponArray[weaponNumber].damage,
            _weaponArray[weaponNumber].bulletSpeed,
            _weaponArray[weaponNumber].shootDelay,
            _weaponArray[weaponNumber].bulletPrefab,
            _weaponModel);

        if (_weaponModel.TryGetComponent(out GunAnimations anim))
            anim.SetGun(_currentGun);

        SendChangeWeapon(weaponNumber);
    }

    private void SendChangeWeapon(int num)
    {
        WeaponInfo message = new();
        message.key = MultiplayerManager.Instance.GetSessionId();
        message.num = num;
        string json = JsonUtility.ToJson(message);

        MultiplayerManager.Instance.SendMessage("change_weapon", json);
    }
}

[System.Serializable]
public struct WeaponInfo
{
    public string key;
    public int num;
}
