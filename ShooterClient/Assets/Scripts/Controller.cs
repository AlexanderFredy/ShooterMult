using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using GameDevWare.Serialization;

public class Controller : MonoBehaviour
{
    [SerializeField] private float _restartDelay = 3f;
    [SerializeField] private Joystick variableJoystick;
    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private PlayerGun _gun;
    [SerializeField] private float _mouseSensetivity = 2f;
    [SerializeField] private float _touchscreenSensetivity = 4f;
    [SerializeField] private GameObject _touchbuttons;
    [SerializeField] private RectTransform _lookRotateZone;
    [SerializeField] private RectTransform _deadRatateZone;
    [SerializeField] private TextMeshProUGUI _debugText;
    [SerializeField] private WeaponController _weaponController;
    [SerializeField] private CharacterAnimation _characterAnimation;
    [SerializeField] private RectTransform _sitStandButton;

    private MultiplayerManager _multiplayerManager;
    private bool _hold = false; 
    private bool _hideCurcor;

    private float pointerSensetivity = 0f;

    private Vector2 _moveInput = Vector2.zero;
    private Vector2 _lookInput = Vector2.zero;
    private bool _shootInput;
    private bool _altShootInput;
    private bool _jumpInput;
    private bool _sitStandInput;
    private bool _standState = true;
    private sbyte _standDirection = 0;
    private int _currentWeaponIndex = 0;

    private void Start()
    {
        _multiplayerManager = MultiplayerManager.Instance;

        SetInputSystem();

        _characterAnimation.IsStand += () => { _standState = true; _standDirection = 0; };
    }

    void Update()
    {
        if (_hold) return;

        if (Application.isMobilePlatform)
        {
            _moveInput = variableJoystick.Direction;
            _lookInput = Vector2.zero;

            foreach (Touch touch in Input.touches)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(_lookRotateZone, touch.position)
                    && !RectTransformUtility.RectangleContainsScreenPoint(_deadRatateZone, touch.position)
                    && touch.phase == UnityEngine.TouchPhase.Moved)
                {
                    _lookInput = touch.deltaPosition.normalized;
                    break;
                }
            }
            //_debugText.text = _lookInput.ToString();
        }
        else
        {
            _moveInput.x = Input.GetAxisRaw("Horizontal");
            _moveInput.y = Input.GetAxisRaw("Vertical");

            _lookInput.x = Input.GetAxis("Mouse X");
            _lookInput.y = Input.GetAxis("Mouse Y");

            _shootInput = Input.GetMouseButton(0);
            _altShootInput = Input.GetMouseButtonDown(1);
            _jumpInput = Input.GetKeyDown(KeyCode.Space);
            _sitStandInput = Input.GetKeyDown(KeyCode.LeftControl);
            //if (Input.GetKeyDown(KeyCode.Escape)) PressEscape();
        }

        _player.SetInput(_moveInput.x, _moveInput.y, _lookInput.x * pointerSensetivity);
        _player.RotateX(-_lookInput.y * pointerSensetivity);

        if (_jumpInput && _standState)
        {
            _player.Jump();
            _jumpInput = false;
        }
        if (_shootInput && _gun.TryShoot(out ShootInfo shootInfo)) SendShoot(ref shootInfo);
        if (_altShootInput)
        {
            if (_gun.EnableAltShoot())
            {
                if (Application.isMobilePlatform)
                    pointerSensetivity /= 8f;
            } else
            {
                if (Application.isMobilePlatform)
                    pointerSensetivity *= 8f;
            }

            _altShootInput = false;
        }

        if (_sitStandInput)
        {
            if (_standState)
            {
                _player.SitDown();
                _standState = false;
                SendSitStart();
            }
            else
            {
                _player.StandUp();
                SendStandStart();
            }
            _sitStandInput = false;
        }

        _sitStandButton.eulerAngles = _standState ? new Vector3(0, 0, -180) : Vector3.zero; 

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
    }

    public void PressJump()
    {
        _jumpInput = true;
    }

    public void PressDownShoot()
    {
        _shootInput = true;
    }

    public void PressUpShoot()
    {
        _shootInput = false;
    }

    public void PressSitStand()
    {
        _sitStandInput = true;
    }

    public void PressAltShoot()
    {
        _altShootInput = true;
    }

    public void WeaponCircleChange()
    {
        _currentWeaponIndex++;
        if (_currentWeaponIndex > 2) _currentWeaponIndex = 0;

        _weaponController.SetWeaponFromInventory(_currentWeaponIndex);
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

    private void SendSitStart()
    {
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"id",_multiplayerManager.GetSessionId()},
        };
        
        _multiplayerManager.SendMessage("sit", data);
    }

    private void SendStandStart()
    {
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"id",_multiplayerManager.GetSessionId()},
        };

        _multiplayerManager.SendMessage("stand", data);
    }

    private void SendMove()
    {
        _player.GetMoveInfo(out Vector3 position, out Vector3 velocity, out float rotateX, out float rotateY, out float angVelocityY);
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"standDir",_standDirection},
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
        _characterAnimation.IsStand -= () => { _standState = true; _standDirection = 0; };
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

