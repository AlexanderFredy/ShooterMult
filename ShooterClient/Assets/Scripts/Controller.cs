using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    [SerializeField] private float _restartDelay = 3f;
    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private PlayerGun _gun;
    [SerializeField] private float _mouseSensetivity = 2f;
    [SerializeField] private RectTransform _inputRotationZone;
    private MultiplayerManager _multiplayerManager;
    private bool _hold = false; 
    private bool _hideCurcor;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _shootAction;
    private InputAction _jumpAction;

    private Vector2 _moveInput = Vector2.zero;
    private Vector2 _lookInput = Vector2.zero;
    private bool _isShooting;

    private void Start()
    {
        _multiplayerManager = MultiplayerManager.Instance;
        _hideCurcor = true;
        //Cursor.lockState = CursorLockMode.Locked;

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
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (!RectTransformUtility.RectangleContainsScreenPoint(_inputRotationZone, touch.position))
                    return;
            }
            
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
    }

    void Update()
    {
        if (_hold) return;

        _player.SetInput(_moveInput.x, _moveInput.y, _lookInput.x * _mouseSensetivity);
        _player.RotateX(-_lookInput.y * _mouseSensetivity);

        if (_isShooting && _gun.TryShoot(out ShootInfo shootInfo)) SendShoot(ref shootInfo);

        //    //if (sitdown) _player.SitDown();
        //    //if (standUp) _player.StandUp();

        SendMove();
    }

    private void PressEscape(InputAction.CallbackContext context)
    {
        _hideCurcor = !_hideCurcor;
        Cursor.lockState = _hideCurcor ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private void SendShoot(ref ShootInfo shootInfo)
    {
        shootInfo.key = _multiplayerManager.GetSessionId();
        string json = JsonUtility.ToJson(shootInfo);

        _multiplayerManager.SendMessage("shoot",json);
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

