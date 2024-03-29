using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class Controller : MonoBehaviour
{
    [SerializeField] private float _restartDelay = 3f;
    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private PlayerGun _gun;
    [SerializeField] private float _mouseSensetivity = 2f;
    [SerializeField] private float _touchscreenSensetivity = 4f;
    [SerializeField] private GameObject _touchbuttons;
    [SerializeField] private RectTransform _lookRotateZone;
    [SerializeField] private RectTransform _deadRatateZone;
    [SerializeField] private TextMeshProUGUI _debugText;
    [SerializeField] private WeaponController _weaponController;

    private MultiplayerManager _multiplayerManager;
    private bool _hold = false; 
    private bool _hideCurcor;

    private float pointerSensetivity = 0f;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _shootAction;
    private InputAction _jumpAction;
    private InputAction _escapeAction;
    private InputAction _weaponChangeAction;

    private Vector2 _moveInput = Vector2.zero;
    private Vector2 _lookInput = Vector2.zero;
    private bool _isShooting;
    private int _currentWeaponIndex = 0;

    private void Start()
    {
        _multiplayerManager = MultiplayerManager.Instance;

        SetInputSystem();
    }

    void Update()
    {
        if (_hold) return;

        _player.SetInput(_moveInput.x, _moveInput.y, _lookInput.x * pointerSensetivity);
        _player.RotateX(-_lookInput.y * pointerSensetivity);

        if (_isShooting && _gun.TryShoot(out ShootInfo shootInfo)) SendShoot(ref shootInfo);

        //    //if (sitdown) _player.SitDown();
        //    //if (standUp) _player.StandUp();

        SendMove();
    }

    private void SetInputSystem()
    {
        if (Application.isMobilePlatform)
        {
            pointerSensetivity = _touchscreenSensetivity;
            _touchbuttons.SetActive(true);
        }
        else
        {
            pointerSensetivity = _mouseSensetivity;
            _hideCurcor = true;
            Cursor.lockState = CursorLockMode.Locked;
            _touchbuttons.SetActive(false);
        }

        _moveAction = new InputAction("move", binding: "Gamepad/rightStick");
        _moveAction.AddCompositeBinding("Dpad")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        _moveAction.performed += context => { _moveInput = context.ReadValue<Vector2>().normalized; };
        _moveAction.canceled += context => { _moveInput = Vector2.zero; };
        _moveAction.Enable();

        _lookAction = new InputAction("look", binding: "<Mouse>/Delta");
        _lookAction.AddBinding("<Touchscreen>/Delta");
        _lookAction.performed += context => {
            if (Application.isMobilePlatform)
            {
                foreach (Touch touch in Input.touches)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(_lookRotateZone, touch.position)
                        && touch.phase == UnityEngine.TouchPhase.Moved
                        && !RectTransformUtility.RectangleContainsScreenPoint(_deadRatateZone, touch.position))
                    {
                        _lookInput = touch.deltaPosition.normalized;
                        break;
                    }
                }
                //_debugText.text = _lookInput.ToString();
            }
            else
                _lookInput = context.ReadValue<Vector2>().normalized;
        };
        _lookAction.canceled += context => { _lookInput = Vector2.zero; };
        _lookAction.Enable();

        _shootAction = new InputAction("shoot", binding: "<Mouse>/leftButton");
        _shootAction.AddBinding("<Keyboard>/m");

        _shootAction.performed += context => { _isShooting = true; };
        _shootAction.canceled += context => { _isShooting = false; };
        _shootAction.Enable();

        _jumpAction = new InputAction("jump", binding: "<Keyboard>/Space");

        _jumpAction.performed += context => { _player.Jump(); };
        _jumpAction.Enable();

        _weaponChangeAction = new InputAction("weapon_change", binding: "<Keyboard>/b");
        _weaponChangeAction.performed += context => { WeaponCircleChange(); };
        _weaponChangeAction.Enable();

        _escapeAction = new InputAction("escape", binding: "<Keyboard>/Escape");

        _escapeAction.started += context => { PressEscape(); };
        _escapeAction.Enable();
    }

    private void PressEscape()
    {
        _hold = !_hold;
        _hideCurcor = !_hideCurcor;
        Cursor.lockState = _hideCurcor ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private void SendShoot(ref ShootInfo shootInfo)
    {
        shootInfo.key = _multiplayerManager.GetSessionId();
        string json = JsonUtility.ToJson(shootInfo);

        _multiplayerManager.SendMessage("shoot",json);
    }

    private void WeaponCircleChange()
    {
        _currentWeaponIndex++;
        if (_currentWeaponIndex > 2) _currentWeaponIndex = 0;

        _weaponController.SetWeaponFromInventory(_currentWeaponIndex);
    }

    private void SendMove()
    {
        _player.GetMoveInfo(out Vector3 position, out Vector3 velocity, out float rotateX, out float rotateY, out float angVelocityY);
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"pX",position.x},
            {"pY",position.y},
            {"pZ",position.z},
            {"vX",velocity.x},
            {"vY",velocity.y},
            {"vZ",velocity.z},
            {"rX",rotateX},
            {"rY",rotateY},
            {"avY",angVelocityY},
        };
        MultiplayerManager.Instance.SendMessage("move",data);
    }

    public void Restart(int SpawnIndex)
    {
        _multiplayerManager._spawnPoints.GetPoint(SpawnIndex, out Vector3 position, out Vector3 rotation);
        StartCoroutine(Hold());

        _player.transform.position = position;
        rotation.x = 0;
        rotation.z = 0;
        _player.transform.eulerAngles = rotation;
        _player.SetInput(0, 0, 0);

        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"pX",position.x},
            {"pY",position.y},
            {"pZ",position.z},
            {"vX",0},
            {"vY",0},
            {"vZ",0},
            {"rX",0},
            {"rY",rotation.y},
            {"avY",0},
        };
        MultiplayerManager.Instance.SendMessage("move", data);
    }

    private IEnumerator Hold()
    {
        _hold = true;
        yield return new WaitForSecondsRealtime(_restartDelay);
        _hold = false;
    }

    private void OnDestroy()
    {
        _moveAction.Disable();
        _lookAction.Disable();
        _shootAction.Disable(); 
        _jumpAction.Disable();
    }
}

[System.Serializable]
public struct ShootInfo
{
    public string key;
    public float pX;
    public float pY;
    public float pZ;
    public float dX;
    public float dY;
    public float dZ;
}

