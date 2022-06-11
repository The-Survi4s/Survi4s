using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

public class NetworkClient : MonoBehaviour
{
    // Connection setting ------------------------------------------------------------
    private TcpClient client;
    private NetworkStream networkStream;
    [SerializeField] private int port;
    [SerializeField] private string IpAddress;
    private IPAddress ipAd;
    [SerializeField, Min(0)] private int _maximumRetries = 1;
    [SerializeField] private bool _waitForServer = true;

    // Encryption ---------------------------------------------------------------------
    private RsaEncryption rsaEncryption;
    [SerializeField] private string ServerPublicKey;

    // Player Room and Name -----------------------------------------------------------
    public string myId { get; private set; }
    public string myName { get; private set; }
    public string myRoomName { get; private set; }
    public bool isMaster { get; private set; }
    public int playersCount { get; private set; }
    private int _playerNumber = -1;

    [SerializeField, Min(0.1f)] private float CheckTime = 1f;
    private float _checkTime;

    // Connection status --------------------------------------------------------------
    private bool isVerified;
    
    [Header("Debug")]
    [SerializeField] private int _sendTimes;
    [SerializeField] private float _sendTimesPerSecond;
    [SerializeField] private int _receiveTimes;
    [SerializeField] private float _receiveTimesPerSecond;
    [SerializeField] private float _runTime;
    [SerializeField] private bool useLocalConnection;

    // Eazy access ----------------------------------------------------------------------
    public static NetworkClient Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Components --------------------------------------------------------------------

    void Start()
    {
        // Preparation -------------------------------------------------------------
        client = new TcpClient();
        ipAd = IPAddress.Parse(useLocalConnection ? "127.0.0.1" : IpAddress);
        rsaEncryption = new RsaEncryption(ServerPublicKey);
        isVerified = false;
        isMaster = false;

        _checkTime = CheckTime;
    }

    // Try connecting to server ------------------------------------------------------
    public void BeginConnecting() 
    {
        MainMenuManager.Instance.SetActiveConnectingPanel(true);

        // Get Player Name -----------------------------------------------------------
        myName = PlayerDataLoader.Instance.TheData.UserName;
        myId = GeneratePlayerId();

        // Start try to connect again and again -------------------------------------
        StartCoroutine(TryConnecting());
    }
    private IEnumerator TryConnecting()
    {
        int count = 0;
        while (!client.Connected)
        {
            // Wait 2 second and try again
            yield return new WaitForSeconds(2);

            count++;
            try
            {
                client.Connect(ipAd, port);
                networkStream = client.GetStream();

                // Begin Verification --------------------------------------------------
                BeginVerification();

                Debug.Log("Connected to server");
            }
            catch (Exception e)
            {
                Debug.Log("Try connecting-" + count + " error : " + e.Message);
                if (count > _maximumRetries) break;
            }
        }
    }

    private void BeginVerification()
    {
        // Send ID + name to server encrypted with server public key ------------------------
        BinaryFormatter formatter = new BinaryFormatter();
        string data = myId + myName;
        string encryptedData = rsaEncryption.Encrypt(data, rsaEncryption.serverPublicKey);
        formatter.Serialize(networkStream, encryptedData);

        // Wait for server feedback ----------------------------------------------------------
        data = formatter.Deserialize(networkStream) as string;
        // Check feedbcak is Ok --------------------------------------------------------------
        if(data == "Ok")
        {
            // Begin normal communication ----------------------------------------------------
            isVerified = true;
        }
        else
        {
            // if no, show some error message ------------------------------------------------

        }

        // Set UI
        MainMenuManager.Instance.SetActiveConnectingPanel(false);
    }

    private void Update()
    {
        // Check connection and verification status ---------------------------------
        if (client.Connected && isVerified)
        {
            // If all ok, begin listening -------------------------------------------
            if (networkStream.DataAvailable)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                ReceiveMessage(formatter.Deserialize(networkStream) as string);
            }

            // Always send message to server to tell server that we
            // Still online 
            _checkTime -= Time.deltaTime;
            if (_checkTime <= 0)
            {
                SendMessageClient(Receiver.Server, "A");
            }
        }
        _runTime += Time.deltaTime;
    }

    /// <summary>
    /// Message headers
    /// </summary>
    private enum Header
    {
        /// <summary>Server</summary>
        Svr,
        /// <summary>Room created</summary>
        RCrd,
        /// <summary>Room joined</summary>
        RJnd,
        /// <summary>Room not found</summary>
        RnFd,
        /// <summary>Room is full</summary>
        RsF,
        /// <summary>Exit room</summary>
        REx,
        /// <summary>Sync mouse position</summary>
        MPos,
        /// <summary>Player count</summary>
        PlCt,
        /// <summary>Start game</summary>
        StGm,
        /// <summary>Spawn <see cref="Player"/></summary>
        SpwP,
        /// <summary>Equip <see cref="WeaponBase"/></summary>
        EqWp,
        /// <summary><see cref="Player"/> attack</summary>
        PAtk,
        /// <summary>Spawn <see cref="BulletBase"/></summary>
        SpwB,
        /// <summary>Modify <see cref="Monster"/> hp</summary>
        MdMo,
        /// <summary>Modify <see cref="Player"/> hp</summary>
        MdPl,
        /// <summary>Correct <see cref="Player"/> dead position</summary>
        PlDd,
        /// <summary>Modify <see cref="Wall"/> hp</summary>
        MdWl,
        /// <summary>Spawn <see cref="Monster"/></summary>
        SpwM,
        /// <summary>Add <see cref="StatusEffectBase"/> to a <see cref="Monster"/></summary>
        MoEf,
        /// <summary><see cref="Monster"/> attack</summary>
        MAtk,
        /// <summary>Modify <see cref="Statue"/> hp</summary>
        MdSt,
        /// <summary><see cref="Player"/> leave room / disconnect</summary>
        LRm,
        /// <summary>Destroy <see cref="BulletBase"/></summary>
        DBl,
        /// <summary>Rebuilt <see cref="Wall"/></summary>
        RbWl,
        /// <summary>Upgrade <see cref="WeaponBase"/></summary>
        UpWpn,
        /// <summary>Sync <see cref="Player"/> velocity</summary>
        PlVl,
        /// <summary><see cref="Player"/> jump</summary>
        PJmp,
        /// <summary>Change name</summary>
        ChNm,
        /// <summary>Game over</summary>
        GmOv,
        /// <summary>Sync <see cref="Player"/> position</summary>
        PlPos,
        /// <summary>Start Matchmaking</summary>
        StMtc,
        /// <summary>Request create room</summary>
        CrR,
        /// <summary>Request join room</summary>
        JnR,
        /// <summary>Request exit room</summary>
        ExR,
        /// <summary>Request lock room</summary>
        LcR,
        /// <summary>Set to Master</summary>
        SeMs
    }

    private enum Receiver { None, All, Server, AllExceptSender, SpecificPlayer}

    // Receive and Process incoming message here ----------------------------------
    private void ReceiveMessage(string message)
    {
        _receiveTimes++;
        _receiveTimesPerSecond = _receiveTimes / _runTime;
        // Message format : sender|header|data|data|data... 
        // Svr|RCrd|...
        // ID+NameClient|MPos|...
        var info = message.Split('|');
        info[0] = info[0].Trim('\0');
        if (info[0] == Header.Svr.ToString())
            switch (EnumParse<Header>(info[1]))
            {
                case Header.RCrd:
                    OnCreatedRoom(info[2]);
                    break;
                case Header.RJnd:
                    OnJoinedRoom(info[2], int.Parse(info[3]));
                    break;
                case Header.RnFd:
                    JoinRoomPanel.Instance.RoomNotFound();
                    break;
                case Header.RsF:
                    JoinRoomPanel.Instance.RoomIsFull();
                    break;
                case Header.REx:
                    ScenesManager.Instance.LoadScene(0);
                    break;
                case Header.LRm:
                {
                    UnitManager.Instance.HandlePlayerDisconnect(info[1]);
                    break;
                }
                case Header.ChNm:
                {
                    myId = info[2];
                    myName = info[3];
                    break;
                }
            }
        else
        {
            Debug.Log(EnumParse<Header>(info[1]));
            switch (EnumParse<Header>(info[1]))
            {
                case Header.MPos:
                {
                    UnitManager.Instance.SyncMousePos(info[0], float.Parse(info[2]), float.Parse(info[3]));
                    break;
                }
                case Header.PlCt:
                    Debug.Log(info[2]);
                    playersCount = int.Parse(info[2]);
                    var temp = new string[playersCount];
                    for(int i = 0; i < playersCount; i++)
                    {
                        temp[i] = info[i + 3];
                    }
                    GameUIManager.Instance.UpdatePlayersInRoom(temp);
                    break;
                case Header.StGm:
                    GameManager.Instance.ChangeState(GameManager.GameState.StartGame);
                    break;
                case Header.SpwP:
                    SpawnManager.Instance.ReceiveSpawnPlayer(info[0], ExtractId(info[0]), 
                        new Vector2(float.Parse(info[2]), float.Parse(info[3])),
                        int.Parse(info[4]));
                    break;
                case Header.SpwM:
                    SpawnManager.Instance.ReceiveSpawnMonster(int.Parse(info[2]), int.Parse(info[3]),
                        (Origin)int.Parse(info[4]), float.Parse(info[5]));
                    break;
                case Header.EqWp:
                {
                    UnitManager.Instance.OnEquipWeapon(info[0], info[2]);
                    break;
                }
                case Header.PAtk:
                {
                    UnitManager.Instance.PlayAttackAnimation(info[0]);
                    break;
                }
                case Header.SpwB:
                {
                    var monsterId = int.Parse(info[6]);
                    var a = new Vector2(float.Parse(info[2]), float.Parse(info[3]));
                    var b = new Vector2(float.Parse(info[4]), float.Parse(info[5]));
                    if (monsterId != -1)
                    {
                        UnitManager.Instance.SpawnBullet(monsterId, a, b);
                    }
                    else
                    {
                        UnitManager.Instance.SpawnBullet(info[0], a, b);
                    }
                    break;
                }
                case Header.MdMo:
                {
                    UnitManager.Instance.ModifyMonsterHp(int.Parse(info[2]), float.Parse(info[3]), info[0]);
                    break;
                }
                case Header.MoEf:
                {
                    UnitManager.Instance.ApplyStatusEffectToMonster(int.Parse(info[2]),
                        (StatusEffect)int.Parse(info[3]), float.Parse(info[4]), int.Parse(info[5]));
                    break;
                }
                case Header.MdPl:
                {
                    //Debug.Log("Receive: ModifyPlayerHp " + info[2] + " " + info[3]);
                    UnitManager.Instance.ModifyPlayerHp(info[2], float.Parse(info[3]));
                    break;
                }
                case Header.PlDd:
                {
                    UnitManager.Instance.CorrectDeadPosition(info[0], new Vector2(float.Parse(info[2]), float.Parse(info[3])));
                    break;
                }
                case Header.MdWl:
                {
                    //Debug.Log($"Wall {int.Parse(info[2])} hp modified by {float.Parse(info[3])}");
                    TilemapManager.Instance.ModifyWallHp(int.Parse(info[2]), float.Parse(info[3]));
                    break;
                }
                case Header.MAtk:
                {
                    UnitManager.Instance.PlayMonsterAttackAnimation(int.Parse(info[2]));
                    break;
                }
                case Header.MdSt:
                {
                    //Debug.Log($"Statue hp modified by {float.Parse(info[2])}");
                    TilemapManager.Instance.ModifyStatueHp(float.Parse(info[2]));
                    break;
                }
                case Header.DBl:
                {
                    UnitManager.Instance.DestroyBullet(int.Parse(info[2]));
                    break;
                }
                case Header.RbWl:
                {
                    TilemapManager.Instance.RebuiltWall(int.Parse(info[2]), int.Parse(info[3]));
                    break;
                }
                case Header.UpWpn:
                {
                    UnitManager.Instance.UpgradeWeapon(info[2]);
                    break;
                }
                case Header.PlVl:
                {
                    UnitManager.Instance.SetPlayerVelocity(
                        info[0], 
                        new Vector2(float.Parse(info[2]), float.Parse(info[3])), 
                        (PlayerMovement.Axis)int.Parse(info[4]));
                    break;
                }
                case Header.PJmp:
                {
                    var player = UnitManager.Instance.GetPlayer(info[0]);
                    if(player) player.movement.Jump();
                    break;
                }
                case Header.GmOv:
                {
                    GameManager.Instance.ChangeState(GameManager.GameState.GameOver);
                    break;
                }
                case Header.PlPos:
                    {

                        break;
                    }
                case Header.SeMs:
                    {
                        break;
                    }
            }
        }
    }

    // Process message that is about to be sent ---------------------------------------
    private void SendMessageClient(string target, params string[] message)
    {
        // Message format : target|header|data|data|data...
        // Target code : 1.All  2.Server  3.All except Sender   others:Specific player name

        string data = message.Aggregate(target, (current, x) => current + ("|" + x));

        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(networkStream, data);

        _checkTime = CheckTime;
        _sendTimes++;
        _sendTimesPerSecond = _sendTimes / _runTime;
    }

    // If want to check connection status -------------------------------------------
    public bool IsConnected() => client.Connected;

    // Generate id of 6 random number ------------------------------------------------
    private const int IdLength = 6;
    public string GeneratePlayerId()
    {
        string genId = "";
        for(int i = 0; i < IdLength; i++)
        {
            genId += UnityEngine.Random.Range(0, 10).ToString();
        }
        return genId;
    }

    // Private Method ---------------------------------------------------------------
    private void OnCreatedRoom(string roomName)
    {
        // Set to master, because we are the one who created the room ---------------------------
        isMaster = true;
        playersCount = 1;

        // Load Scene 
        ScenesManager.Instance.LoadScene(1);

        // Debugging
        Debug.Log($"Room '{roomName}' Created");
    }

    private void OnJoinedRoom(string roomName, int count)
    {
        // Load Scene 
        ScenesManager.Instance.LoadScene(1);

        // Do someting with count
        _playerNumber = count;

        // Debugging
        Debug.Log("Joined room " + roomName);
    }

    #region Send Message To Server

    // Public method that can be called to send message to server -------------------
    public void ChangeName(string newId, string newName)
    {
        var msg = MessageBuilder(Header.ChNm, newId, newName );
        SendMessageClient(Receiver.Server, msg);
    }
    public void StartMatchmaking()
    {
        SendMessageClient(Receiver.Server, Header.StMtc);
    }

    public void CreateRoom(string roomName, int maxPlayer, bool isPublic)
    {
        var message = MessageBuilder(Header.CrR, roomName, maxPlayer.ToString(), isPublic.ToString());
        SendMessageClient(Receiver.Server, message);
    }

    public void JoinRoom(string roomName)
    {
        var message = MessageBuilder(Header.JnR, roomName);
        SendMessageClient(Receiver.Server, message);
    }
    public void ExitRoom()
    {
        SendMessageClient(Receiver.Server, Header.ExR);
    }

    public void StartGame()
    {
        SendMessageClient(Receiver.All, Header.StGm);
    }
    public void LockTheRoom()
    {
        SendMessageClient(Receiver.Server, Header.LcR);
    }

    public void SpawnPlayer(Vector2 spawnPos)
    {
        var msg = MessageBuilder(Header.SpwP, spawnPos.x, spawnPos.y, _playerNumber);
        SendMessageClient(Receiver.All, msg);
    }

    public void SpawnMonster(int id, int type, Origin origin, float spawnOffset)
    {
        //Debug.Log($"Send spawn monster: id {id}, type {type}");
        var msg = MessageBuilder(Header.SpwM, id, type, (int)origin, spawnOffset);
        SendMessageClient(Receiver.All, msg);
    }

    public void SetPlayerVelocity(Vector2 velocity, PlayerMovement.Axis axis)
    {
        SendMessageClient(MessageBuilder(Header.PlVl, velocity.x, velocity.y, (int)axis));
    }

    public void SyncPlayerPos(Vector2 pos)
    {
        SendMessageClient(MessageBuilder(Header.PlPos, pos.x, pos.y));
    }

    public void SyncMonsterPos(Vector2 pos)
    {
        SendMessageClient(MessageBuilder(Header.PlPos, pos.x, pos.y));
    }

    public void SendMousePos(Vector2 mousePos)
    {
        SendMessageClient(MessageBuilder(Header.MPos, mousePos.x, mousePos.y));
    }

    public void EquipWeapon(string weaponName)
    {
        SendMessageClient(MessageBuilder(Header.EqWp, weaponName));
    }

    public void StartAttackAnimation() 
    {
        SendMessageClient(MessageBuilder(Header.PAtk));
    }

    public void Jump()
    {
        SendMessageClient(MessageBuilder(Header.PJmp));
    }

    public void StartMonsterAttackAnimation(int targetId) 
    {
        SendMessageClient(MessageBuilder(Header.MAtk, targetId));
    }

    public void SpawnBullet(Vector2 spawnPos, Vector2 targetPos, int spawnedByMonsterId = -1)
    {
        SendMessageClient(MessageBuilder(Header.SpwB, spawnPos.x, spawnPos.y, targetPos.x, targetPos.y, spawnedByMonsterId));
    }

    public void DestroyBullet(int id)
    {
        SendMessageClient(MessageBuilder(Header.DBl, id));
    }

    public void ApplyStatusEffectToMonster(int targetId, StatusEffect effect, float duration, int strength)
    {
        SendMessageClient(MessageBuilder(Header.MoEf, targetId, (int)effect, duration, strength));
    }

    public void CorrectPlayerDeadPosition(float xPos, float yPos)
    {
        SendMessageClient(MessageBuilder(Header.PlDd, xPos, yPos));
    }

    public void RebuildWall(int brokenWallId, int amount)
    {
        SendMessageClient(MessageBuilder(Header.RbWl, brokenWallId, amount));
    }

    public void UpgradeWeapon(string weaponName)
    {
        SendMessageClient(MessageBuilder(Header.UpWpn, weaponName));
    }

    /// <summary>
    /// Sends a message to server to modify hp of any object that has an hp. <br/>
    /// Currently supports:
    /// <br/>- <see cref="Statue"/>
    /// <br/>- <see cref="Player"/>
    /// <br/>- <see cref="Wall"/>
    /// <br/>- <see cref="Monster"/>
    /// </summary>
    /// <param name="obj">The object with hp</param>
    /// <param name="amount">The amount to modify hp</param>
    /// <returns>
    /// <see langword="true"/> on success. 
    /// </returns>
    public bool ModifyHp(Component obj, float amount)
    {
        string[] msg = { };
        switch (obj)
        {
            case Wall wall:
                msg = MessageBuilder(Header.MdWl, wall.id, amount);
                break;
            case Monster monster:
                msg = MessageBuilder(Header.MdMo, monster.id, amount);
                break;
            case Player player:
                msg = MessageBuilder(Header.MdPl, player.name, amount.ToString("f2"));
                break;
            case Statue _:
                msg = MessageBuilder(Header.MdSt, amount);
                break;
        }
        return SendMessageClient(msg, errorMessage: $"Failed to modify HP of {obj}");
    }

    /// <summary>
    /// Sends a message to server to modify hp of anything that has an id and hp. <br/>
    /// Currently supports:
    /// <br/>- <see cref="Wall"/>
    /// <br/>- <see cref="Monster"/>
    /// </summary>
    /// <remarks>
    /// Will try to get a name from the object with <paramref name="id"/> when <paramref name="target"/> is set to
    /// <see cref="Target.Player"/>. <br/>
    /// Ignores <paramref name="id"/> when <paramref name="target"/> is set to <see cref="Target.Statue"/>
    /// </remarks>
    /// <param name="target">Which type is the target</param>
    /// <param name="id">The id of the target</param>
    /// <param name="amount">The amount to modify hp</param>
    public bool ModifyHp(Target target, int id, float amount)
    {
        string[] msg = { };
        switch (target)
        {
            case Target.Player:
                Debug.LogError($"Must use string for Player name.");
                var player = UnitManager.Instance.GetPlayer(id); if (!player) break;
                var name = player.name;
                ModifyHp(target, name, amount);
                break;
            case Target.Statue:
                msg = MessageBuilder(Header.MdSt, amount);
                break;
            case Target.Wall:
                msg = MessageBuilder(Header.MdWl, id, amount);
                break;
            case Target.Monster:
                msg = MessageBuilder(Header.MdMo, id, amount);
                break;
        }
        return SendMessageClient(msg, errorMessage: $"Failed to modify HP of {target}({id})");
    }

    /// <summary>
    /// Sends a message to server to modify hp of anything that has a name and hp. <br/>
    /// Currently supports:
    /// <br/>- <see cref="Player"/>
    /// </summary>
    /// <remarks>
    /// Will try to extract id from <paramref name="name"/> when <paramref name="target"/> is set to
    /// <see cref="Target.Wall"/> or <see cref="Target.Monster"/>. <br/>
    /// Ignores <paramref name="name"/> when <paramref name="target"/> is set to <see cref="Target.Statue"/>
    /// </remarks>
    /// <param name="target">Which type is the target</param>
    /// <param name="name">The name of the target</param>
    /// <param name="amount">The amount to modify hp</param>
    public bool ModifyHp(Target target, string name, float amount)
    {
        string[] msg = { };
        int id;
        switch (target)
        {
            case Target.Player:
                msg = MessageBuilder(Header.MdPl, name, amount.ToString("f2"));
                break;
            case Target.Statue:
                msg = MessageBuilder(Header.MdSt, amount);
                break;
            default:
                id = IntParse(name);
                if (id == int.MinValue) break;
                ModifyHp(target, id, amount);
                break;
        }
        return SendMessageClient(msg, errorMessage:$"Failed to modify HP of {target}({name})");
    }

    /// <summary>
    /// Sends a message to server to modify hp of any object that has an hp but without identifier. <br/>
    /// Currently supports:
    /// <br/>- <see cref="Statue"/>
    /// </summary>
    /// <remarks>
    /// Prints an error when the <paramref name="target"/> specified needs an identifier
    /// </remarks>
    /// <param name="target">Which type is the target</param>
    /// <param name="amount">The amount to modify hp</param>
    public bool ModifyHp(Target target, float amount)
    {
        string[] msg = { };
        switch (target)
        {
            case Target.Player:
                Debug.LogError("Player name not specified!");
                break;
            case Target.Statue:
                msg = MessageBuilder(Header.MdSt, amount);
                break;
            default:
                Debug.LogError("Id not specified!");
                break;
        }
        return SendMessageClient(msg, errorMessage: $"Failed to modify HP of {target}");
    }

    #endregion

    #region Utilities
    /// <summary>
    /// Parses a string to an enum of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="stringToEnum">the string to parse</param>
    /// <returns>The enum <typeparamref name="T"/></returns>
    private static T EnumParse<T>(string stringToEnum)
    {
        return (T) Enum.Parse(typeof(T), stringToEnum, true);
    }

    /// <summary>
    /// Extracts <see cref="IdLength"/> digit numbers from <paramref name="idAndName"/>
    /// </summary>
    /// <param name="idAndName"></param>
    /// <returns><see cref="IdLength"/> digit numbers of id</returns>
    private static int ExtractId(string idAndName)
    {
        return int.Parse(idAndName.Substring(0, IdLength));
    }

    /// <summary>
    /// Emulates data sent from server, then changes <see cref="Receiver"/> type to AllExceptSender
    /// </summary>
    /// <param name="msg"></param>
    /// <returns><see cref="Receiver.AllExceptSender"/></returns>
    private Receiver SelfRun(string[] msg)
    {
        string data = msg.Aggregate(myId + myName, (current, x) => current + ("|" + x));
        ReceiveMessage(data);
        return Receiver.AllExceptSender;
    }

    /// <remarks>
    /// Sends a message to <see cref="Receiver.All"/> if <see cref="_waitForServer"/> is set to <see langword="true"/>. <br/>
    /// Else send it to <see cref="Receiver.AllExceptSender"/>
    /// </remarks>
    /// <param name="msg">The message</param>
    /// <param name="errorMessage">Show this error when fail</param>
    /// <returns></returns>
    private bool SendMessageClient(string[] msg, Receiver receiverIfWaitForServer = Receiver.All, string errorMessage = "Message is empty")
    {
        if (msg.Length > 0)
        {
            SendMessageClient(_waitForServer ? Receiver.All : SelfRun(msg), msg);
            return true;
        }
        Debug.LogError(errorMessage);
        return false;
    }

    /// <summary>
    /// Sends a message to server. 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="header"></param>
    /// <param name="data"></param>
    private void SendMessageClient(Receiver target, Header header, params string[] data)
    {
        SendMessageClient(((int)target).ToString(), MessageBuilder(header, data));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="message"></param>
    private void SendMessageClient(Receiver target, params string[] message)
    {
        SendMessageClient(((int)target).ToString(), message);
    }

    private void SendMessageClient(Receiver target, Header header)
    {
        SendMessageClient(((int)target).ToString(), header.ToString());
    }

    private string[] MessageBuilder(Header header, params float[] data)
    {
        var msg = new List<string>() { header.ToString() };
        foreach (var item in data)
        {
            var isInt = (int)item == item;
            msg.Add(item.ToString(isInt ? "f0" : "f2"));
        }
        return msg.ToArray();
    }

    private string[] MessageBuilder(Header header, params string[] data)
    {
        var msg = new List<string>() { header.ToString() };
        msg.AddRange(data);
        return msg.ToArray();
    }

    private string[] MessageBuilder(Header header)
    {
        return new string[] { header.ToString() };
    }

    private int IntParse(string str)
    {
        var numericString = "";
        int index = 0;
        foreach (char c in str)
        {
            if (c == '-' && index == 0) numericString = string.Concat(numericString, c);
            else if ((c >= '0' && c <= '9'))
            {
                numericString = string.Concat(numericString, c);
            }
            else
            {
                index++;
                continue;
            }
            index++;
        }

        if (int.TryParse(numericString, out int j))
        {
            return j;
        }
        else return int.MinValue;
    }

    #endregion
}