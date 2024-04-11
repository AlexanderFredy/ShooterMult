using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Colyseus;
using System;

public class MultiplayerManager : ColyseusManager<MultiplayerManager>
{
    [SerializeField] Skins _skins;
    [field: SerializeField] public LossCounter _lossCounter { get; private set; }
    [field: SerializeField] public SpawnPoints _spawnPoints {get; private set;}

    [SerializeField] PlayerCharacter _player;
    [SerializeField] EnemyController _enemy;

    private ColyseusRoom<State> _room;
    private Dictionary<string,EnemyController> _enemies = new Dictionary<string, EnemyController>(); 

    protected override void Awake()
    {
        base.Awake();
        Instance.InitializeClient();
        Connect();
       
    }

    private async void Connect()
    {
        _spawnPoints.GetPoint(UnityEngine.Random.Range(0, _spawnPoints.length), out Vector3 spawnPosition, out Vector3 spawnRotation);
        
        Dictionary<string,object> data = new Dictionary<string, object>
        {
            { "skins", _skins.length },
            { "points", _spawnPoints.length },
            { "speed", _player.speed },
            { "hp", _player.maxHP },
            { "pX", spawnPosition.x },
            { "pY", spawnPosition.y },
            { "pZ", spawnPosition.z },
            { "rY", spawnRotation.y }
        };
        
        _room = await Instance.client.JoinOrCreate<State>("state_handler",data);

        _room.OnStateChange += OnChange;

        _room.OnMessage<string>("Shoot",ApplyShoot);
        _room.OnMessage<string>("Change_Weapon", ChangeWeapon);
        _room.OnMessage<string>("Sit", SitEnemy);
        _room.OnMessage<string>("Stand", StandEnemy);
    }

    private void ApplyShoot(string jsonShootInfo)
    {
        ShootInfo shootInfo = JsonUtility.FromJson<ShootInfo>(jsonShootInfo);
        if (_enemies.ContainsKey(shootInfo.key) == false)
        {
            Debug.LogError("There is not an enemy, but he tried to shoot");
            return;
        }

        _enemies[shootInfo.key].Shoot(shootInfo);
    }

    private void ChangeWeapon(string jsonWeponInfo)
    {
        WeaponInfo wpInfo = JsonUtility.FromJson<WeaponInfo>(jsonWeponInfo);
        if (_enemies.ContainsKey(wpInfo.key) == false)
        {
            Debug.LogError("There is not an enemy, but he tried to change a weapon");
            return;
        }

        _enemies[wpInfo.key].ChangeWeapon(wpInfo);
    }

    private void SitEnemy(string playerID)
    {
        if (_enemies.ContainsKey(playerID))
        {
            _enemies[playerID].SitDown();
        }
    }

    private void StandEnemy(string playerID)
    {
        if (_enemies.ContainsKey(playerID))
        {
            _enemies[playerID].StandUp();
        }
    }

    private void OnChange(State state, bool isFirstState)
    {
        if (!isFirstState) return;

        state.players.ForEach((key,player) =>
        {
            if (key == _room.SessionId)
                CreatePlayer(player);
            else
                CreateEnemy(key, player);
        });

        _room.State.players.OnAdd += CreateEnemy;
        _room.State.players.OnRemove += RemoveEnemy;
    }

    private void CreatePlayer(Player player)
    {
        var position = new Vector3(player.pX, player.pY, player.pZ);

        Quaternion rotation = Quaternion.Euler(0,player.rY,0);
        var PlayerCharacter = Instantiate(_player, position, rotation);
        player.OnChange += PlayerCharacter.OnChange;

        _room.OnMessage<int>("Restart", PlayerCharacter.GetComponent<Controller>().Restart);

        PlayerCharacter.GetComponent<SetSkin>().Set(_skins.GetMaterial(player.skin));
    }

    private void CreateEnemy(string key, Player player)
    {    
        var position = new Vector3(player.pX, player.pY, player.pZ);

        var enemy = Instantiate(_enemy, position, Quaternion.identity);
        enemy.Init(key, player);
        enemy.GetComponent<SetSkin>().Set(_skins.GetMaterial(player.skin));

        _enemies.Add(key, enemy);
    }

    private void RemoveEnemy(string key, Player player)
    { 
        if (_enemies.ContainsKey(key) == false) return;

        var enemy = _enemies[key];
        enemy.Destroy();

        _enemies.Remove(key);
    }

    protected override void OnDestroy()
    { 
        base.OnDestroy();
        _room.Leave();
    }

    public void SendMessage(string key, Dictionary<string,object> data)
    {
        _room.Send(key,data);
    }

    public void SendMessage(string key, string data)
    {
        _room.Send(key, data);
    }

    public string GetSessionId() => _room.SessionId;
}
