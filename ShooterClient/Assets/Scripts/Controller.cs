using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] private float _restartDelay = 3f;
    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private PlayerGun _gun;
    [SerializeField] private float _mouseSensetivity = 2f;
    private MultiplayerManager _multiplayerManager;
    private bool _hold = false; 
    private bool _hideCurcor;

    private void Start()
    {
        _multiplayerManager = MultiplayerManager.Instance;
        _hideCurcor = true;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _hideCurcor = !_hideCurcor;
            Cursor.lockState = _hideCurcor ? CursorLockMode.Locked : CursorLockMode.None;
        }
        
        if (_hold) return;

        float h = 0;
        float v = 0;

        float mouseX = 0;
        float mouseY = 0;
        bool isShoot = false;

        bool jump = false;
        bool sitdown = false;
        bool standUp = false;

        if (_hideCurcor)
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");

            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");
            isShoot = Input.GetMouseButton(0);

            jump = Input.GetKeyDown(KeyCode.Space);
            sitdown = Input.GetKeyDown(KeyCode.LeftControl);
            standUp = Input.GetKeyUp(KeyCode.LeftControl);
        }     

        _player.SetInput(h, v, mouseX * _mouseSensetivity);
        _player.RotateX(-mouseY * _mouseSensetivity);

        if (jump && !sitdown) _player.Jump();
        if (isShoot && _gun.TryShoot(out ShootInfo shootInfo)) SendShoot(ref shootInfo);
        
        if (sitdown) _player.SitDown();
        if (standUp) _player.StandUp();

        SendMove();
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

