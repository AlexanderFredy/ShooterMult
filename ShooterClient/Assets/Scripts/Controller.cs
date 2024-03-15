
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Controller : MonoBehaviour
{
    [SerializeField] private float _restartDelay = 3f;
    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private PlayerGun _gun;
    [SerializeField] private float _mouseSensetivity = 2f;
    private MultiplayerManager _multiplayerManager;
    private bool _hold = false; 

    private void Start()
    {
        _multiplayerManager = MultiplayerManager.Instance;
    }

    void Update()
    {
        if (_hold) return;
        
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        bool isShoot = Input.GetMouseButton(0);
        bool jump = Input.GetKeyDown(KeyCode.Space);
        bool sitdown = Input.GetKeyDown(KeyCode.LeftControl);
        bool standUp = Input.GetKeyUp(KeyCode.LeftControl);

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

    public void Restart(string jsonRestartInfo)
    {
        RestartInfo info = JsonUtility.FromJson<RestartInfo>(jsonRestartInfo);
        StartCoroutine(Hold());

        _player.transform.position = new Vector3(info.x, 0, info.z);
        _player.SetInput(0, 0, 0);

        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"pX",info.x},
            {"pY",0},
            {"pZ",info.z},
            {"vX",0},
            {"vY",0},
            {"vZ",0},
            {"rX",0},
            {"rY",0},
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

[System.Serializable]
public struct RestartInfo
{
    public float x;
    public float z;
}
