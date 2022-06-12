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
    public bool IsConnected => client.Connected;
    public string myId { get; private set; }
    public string myName { get; private set; }
    public string myRoomName { get; private set; }
    public bool isMaster { get; private set; }
    public int playersCount { get; private set; }
    private int _playerNumber = 0;

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
                SendMessageClient(Recipient.Server, "A");
            }
        }
        _runTime += Time.deltaTime;
    }

    // Generate id of 6 random number ------------------------------------------------
    private const int IdLength = 6;
    public string GeneratePlayerId()
    {
        string genId = "";
        for (int i = 0; i < IdLength; i++)
        {
            genId += UnityEngine.Random.Range(0, 10).ToString();
        }
        //Debug.Log("Id generated:" + genId);
        return genId;
    }

    #region Type declarations
    /// <summary>
    /// Message subject
    /// </summary>
    private enum Subject
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

    private enum Recipient { None, All, Server, AllExceptSender, SpecificPlayer}
    #endregion

    #region Receive Message
    // Receive and Process incoming message here ----------------------------------
    private void ReceiveMessage(string message)
    {
        _receiveTimes++;
        _receiveTimesPerSecond = _receiveTimes / _runTime;
        // Message format : sender|header|data|data|data... 
        // Svr|RCrd|...
        // ID+NameClient|MPos|...
        message.Trim('\0');
        var info = message.Split('|');
        info[0] = info[0].Trim('\0');
        message = "";
        foreach (var item in info)
        {
            message += item + "|";
        }
        Debug.Log(message);
        if (info[0] == Subject.Svr.ToString())
            switch (EnumParse<Subject>(info[1]))
            {
                case Subject.RCrd:
                    OnCreatedRoom(info[2]);
                    break;
                case Subject.RJnd:
                    OnJoinedRoom(info[2], int.Parse(info[3]));
                    break;
                case Subject.RnFd:
                    JoinRoomPanel.Instance.RoomNotFound();
                    break;
                case Subject.RsF:
                    JoinRoomPanel.Instance.RoomIsFull();
                    break;
                case Subject.REx:
                    ScenesManager.Instance.LoadScene(0);
                    break;
                case Subject.LRm:
                {
                    UnitManager.Instance.HandlePlayerDisconnect(info[1]);
                    break;
                }
                case Subject.ChNm:
                {
                    myId = info[2];
                    myName = info[3];
                    break;
                }
            }
        else
        {
            //Debug.Log(EnumParse<Subject>(info[1]));
            switch (EnumParse<Subject>(info[1]))
            {
                case Subject.MPos:
                {
                    UnitManager.Instance.SyncMousePos(info[0], float.Parse(info[2]), float.Parse(info[3]));
                    break;
                }
                case Subject.PlCt:
                    Debug.Log("Player count:" + info[2]);
                    playersCount = int.Parse(info[2]);
                    var temp = new string[playersCount];
                    for(int i = 0; i < playersCount; i++)
                    {
                        temp[i] = info[i + 3];
                    }
                    GameUIManager.Instance.UpdatePlayersInRoom(temp);
                    break;
                case Subject.StGm:
                    GameManager.Instance.ChangeState(GameManager.GameState.StartGame);
                    break;
                case Subject.SpwP:
                    SpawnManager.Instance.ReceiveSpawnPlayer(info[0], ExtractId(info[0]), 
                        new Vector2(float.Parse(info[2]), float.Parse(info[3])),
                        int.Parse(info[4]));
                    break;
                case Subject.SpwM:
                    SpawnManager.Instance.ReceiveSpawnMonster(int.Parse(info[2]), int.Parse(info[3]),
                        (Origin)int.Parse(info[4]), float.Parse(info[5]));
                    break;
                case Subject.EqWp:
                {
                    UnitManager.Instance.OnEquipWeapon(info[0], info[2]);
                    break;
                }
                case Subject.PAtk:
                {
                    UnitManager.Instance.PlayAttackAnimation(info[0]);
                    break;
                }
                case Subject.SpwB:
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
                case Subject.MdMo:
                {
                    UnitManager.Instance.ModifyMonsterHp(int.Parse(info[2]), float.Parse(info[3]), info[0]);
                    break;
                }
                case Subject.MoEf:
                {
                    UnitManager.Instance.ApplyStatusEffectToMonster(int.Parse(info[2]),
                        (StatusEffect)int.Parse(info[3]), float.Parse(info[4]), int.Parse(info[5]));
                    break;
                }
                case Subject.MdPl:
                {
                    //Debug.Log("Receive: ModifyPlayerHp " + info[2] + " " + info[3]);
                    UnitManager.Instance.ModifyPlayerHp(info[2], float.Parse(info[3]));
                    break;
                }
                case Subject.PlDd:
                {
                    UnitManager.Instance.CorrectDeadPosition(info[0], new Vector2(float.Parse(info[2]), float.Parse(info[3])));
                    break;
                }
                case Subject.MdWl:
                {
                    //Debug.Log($"Wall {int.Parse(info[2])} hp modified by {float.Parse(info[3])}");
                    TilemapManager.Instance.ModifyWallHp(int.Parse(info[2]), float.Parse(info[3]));
                    break;
                }
                case Subject.MAtk:
                {
                    UnitManager.Instance.PlayMonsterAttackAnimation(int.Parse(info[2]));
                    break;
                }
                case Subject.MdSt:
                {
                    //Debug.Log($"Statue hp modified by {float.Parse(info[2])}");
                    TilemapManager.Instance.ModifyStatueHp(float.Parse(info[2]));
                    break;
                }
                case Subject.DBl:
                {
                    UnitManager.Instance.DestroyBullet(int.Parse(info[2]));
                    break;
                }
                case Subject.RbWl:
                {
                    TilemapManager.Instance.RebuiltWall(int.Parse(info[2]), int.Parse(info[3]));
                    break;
                }
                case Subject.UpWpn:
                {
                    UnitManager.Instance.UpgradeWeapon(info[2]);
                    break;
                }
                case Subject.PlVl:
                {
                    UnitManager.Instance.SetPlayerVelocity(
                        info[0], 
                        new Vector2(float.Parse(info[2]), float.Parse(info[3])), 
                        (PlayerMovement.Axis)int.Parse(info[4]));
                    break;
                }
                case Subject.PJmp:
                {
                    var player = UnitManager.Instance.GetPlayer(info[0]);
                    if(player) player.movement.Jump();
                    break;
                }
                case Subject.GmOv:
                {
                    GameManager.Instance.ChangeState(GameManager.GameState.GameOver);
                    break;
                }
                case Subject.PlPos:
                    {

                        break;
                    }
                case Subject.SeMs:
                    {
                        break;
                    }
            }
        }
    }
    #endregion

    #region Send Message Wrapper
    /// <summary>
    /// Emulates data sent from server, then changes <see cref="Recipient"/> type to AllExceptSender
    /// </summary>
    /// <param name="message"></param>
    /// <returns><see cref="Recipient.AllExceptSender"/></returns>
    private Recipient SelfRun(string[] message)
    {
        string data = message.Aggregate(myId + myName, (current, x) => current + ("|" + x));
        ReceiveMessage(data);
        return Recipient.AllExceptSender;
    }

    /// <summary>
    /// Sends a message to server. 
    /// </summary>
    /// <param name="receiver">The recipient of the message</param>
    /// <param name="subject">The message subject</param>
    /// <param name="body">The message body</param>
    private void SendMessageClient(Recipient receiver, Subject subject, params string[] body)
    {
        SendMessageClient(((int)receiver).ToString(), MessageBuilder(subject, body));
    }

    /// <summary>
    /// Sends a message to server. 
    /// </summary>
    /// <param name="receiver">The recipient of the message</param>
    /// <param name="subject">The message subject</param>
    /// <param name="body">The message body</param>
    private void SendMessageClient(Recipient receiver, Subject subject, params float[] body)
    {
        SendMessageClient(((int)receiver).ToString(), MessageBuilder(subject, body));
    }

    /// <summary>
    /// Sends a message to server with auto <see cref="Recipient"/>
    /// <br/><paramref name="message"/> must already includes <see cref="Subject"/>
    /// </summary>
    /// <remarks>
    /// Receiver set to <see cref="Recipient.All"/> if <see cref="_waitForServer"/> is set to <see langword="true"/>. <br/>
    /// Else set it to <see cref="Recipient.AllExceptSender"/>
    /// </remarks>
    /// <param name="message">The message</param>
    /// <param name="errorMessage">Show this error when fail</param>
    /// <returns></returns>
    private bool SendMessageClient(string[] message, string errorMessage = "Message is empty")
    {
        if (message.Length > 0)
        {
            SendMessageClient(_waitForServer ? Recipient.All : SelfRun(message), message);
            return true;
        }
        Debug.LogError(errorMessage);
        return false;
    }

    /// <summary>
    /// Sends a message to server with auto <see cref="Recipient"/>
    /// </summary>
    /// <remarks>
    /// Receiver set to <see cref="Recipient.All"/> if <see cref="_waitForServer"/> is set to <see langword="true"/>. <br/>
    /// Else set it to <see cref="Recipient.AllExceptSender"/>
    /// </remarks>
    /// <param name="subject"></param>
    /// <param name="body">The message</param>
    /// <param name="errorMessage">Show this error when fail</param>
    /// <returns></returns>
    private bool SendMessageClient(Subject subject, params string[] body)
    {
        return SendMessageClient(MessageBuilder(subject, body));
    }

    /// <summary>
    /// Sends a message to server with auto <see cref="Recipient"/>
    /// </summary>
    /// <remarks>
    /// Receiver set to <see cref="Recipient.All"/> if <see cref="_waitForServer"/> is set to <see langword="true"/>. <br/>
    /// Else set it to <see cref="Recipient.AllExceptSender"/>
    /// </remarks>
    /// <param name="subject"></param>
    /// <param name="body">The message</param>
    /// <param name="errorMessage">Show this error when fail</param>
    /// <returns></returns>
    private bool SendMessageClient(Subject subject, params float[] body)
    {
        return SendMessageClient(MessageBuilder(subject, body));
    }

    /// <summary>
    /// Sends a message to server with auto <see cref="Recipient"/>
    /// </summary>
    /// <remarks>
    /// Receiver set to <see cref="Recipient.All"/> if <see cref="_waitForServer"/> is set to <see langword="true"/>. <br/>
    /// Else set it to <see cref="Recipient.AllExceptSender"/>
    /// </remarks>
    /// <param name="subject"></param>
    /// <param name="body">The message</param>
    /// <param name="errorMessage">Show this error when fail</param>
    /// <returns></returns>
    private bool SendMessageClient(Subject subject)
    {
        return SendMessageClient(MessageBuilder(subject));
    }

    /// <summary>
    /// Sends a message to server. 
    /// <br/><paramref name="message"/> must already includes <see cref="Subject"/>
    /// </summary>
    /// <param name="receiver"></param>
    /// <param name="message"></param>
    private void SendMessageClient(Recipient receiver, params string[] message)
    {
        SendMessageClient(((int)receiver).ToString(), message);
    }

    private void SendMessageClient(Recipient receiver, Subject subject)
    {
        SendMessageClient(((int)receiver).ToString(), subject.ToString());
    }

    /// <summary>
    /// Constructs a message. 
    /// </summary>
    /// <param name="subject">Message subject</param>
    /// <param name="body">Message body</param>
    /// <returns></returns>
    private string[] MessageBuilder(Subject subject, params float[] body)
    {
        var msg = new List<string>() { subject.ToString() };
        foreach (var item in body)
        {
            var isInt = (int)item == item;
            msg.Add(item.ToString(isInt ? "f0" : "f2"));
        }
        return msg.ToArray();
    }

    /// <summary>
    /// Constructs a message. 
    /// </summary>
    /// <param name="subject">Message subject</param>
    /// <param name="body">Message body</param>
    /// <returns></returns>
    private string[] MessageBuilder(Subject subject, params string[] body)
    {
        var msg = new List<string>() { subject.ToString() };
        msg.AddRange(body);
        return msg.ToArray();
    }

    /// <summary>
    /// Constructs a message. 
    /// </summary>
    /// <param name="subject">Message subject</param>
    /// <returns></returns>
    private string[] MessageBuilder(Subject subject)
    {
        return new string[] { subject.ToString() };
    }

    // Process message that is about to be sent ---------------------------------------
    private void SendMessageClient(string target, params string[] message)
    {
        // Message format : target|header|data|data|data...
        // Target code : 1.All  2.Server  3.All except Sender   others:Specific player name

        string data = message.Aggregate(target, (current, x) => current + ("|" + x));
        Debug.Log("Send:" + data);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(networkStream, data);

        _checkTime = CheckTime;
        _sendTimes++;
        _sendTimesPerSecond = _sendTimes / _runTime;
    }
    #endregion

    #region Event Handlers
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
    #endregion

    #region Send Message Calls

    // Public method that can be called to send message to server -------------------
    public void ChangeName(string newId, string newName)
    {
        SendMessageClient(Recipient.Server, Subject.ChNm, newId, newName);
    }
    public void StartMatchmaking()
    {
        SendMessageClient(Recipient.Server, Subject.StMtc);
    }

    public void CreateRoom(string roomName, int maxPlayer, bool isPublic)
    {
        SendMessageClient(Recipient.Server, Subject.CrR, roomName, maxPlayer.ToString(), isPublic.ToString());
    }

    public void JoinRoom(string roomName)
    {
        SendMessageClient(Recipient.Server, Subject.JnR, roomName);
    }
    public void ExitRoom()
    {
        SendMessageClient(Recipient.Server, Subject.ExR);
    }

    public void StartGame()
    {
        SendMessageClient(Recipient.All, Subject.StGm);
    }
    public void LockTheRoom()
    {
        SendMessageClient(Recipient.Server, Subject.LcR);
    }

    public void SpawnPlayer(Vector2 spawnPos)
    {
        SendMessageClient(Recipient.All, Subject.SpwP, spawnPos.x, spawnPos.y, _playerNumber);
    }

    public void SpawnMonster(int id, int type, Origin origin, float spawnOffset)
    {
        //Debug.Log($"Send spawn monster: id {id}, type {type}");
        SendMessageClient(Recipient.All, Subject.SpwM, id, type, (int)origin, spawnOffset);
    }

    public void SetPlayerVelocity(Vector2 velocity, PlayerMovement.Axis axis)
    {
        SendMessageClient(Subject.PlVl, velocity.x, velocity.y, (int)axis);
    }

    public void SyncPlayerPos(Vector2 pos)
    {
        SendMessageClient(Subject.PlPos, pos.x, pos.y);
    }

    public void SyncMonsterPos(Vector2 pos)
    {
        SendMessageClient(Subject.PlPos, pos.x, pos.y);
    }

    public void SendMousePos(Vector2 mousePos)
    {
        SendMessageClient(Subject.MPos, mousePos.x, mousePos.y);
    }

    public void EquipWeapon(string weaponName)
    {
        SendMessageClient(Subject.EqWp, weaponName);
    }

    public void StartAttackAnimation() 
    {
        SendMessageClient(Subject.PAtk);
    }

    public void Jump()
    {
        SendMessageClient(MessageBuilder(Subject.PJmp));
    }

    public void StartMonsterAttackAnimation(int targetId) 
    {
        SendMessageClient(Subject.MAtk, targetId);
    }

    public void SpawnBullet(Vector2 spawnPos, Vector2 targetPos, int spawnedByMonsterId = -1)
    {
        SendMessageClient(Subject.SpwB, spawnPos.x, spawnPos.y, targetPos.x, targetPos.y, spawnedByMonsterId);
    }

    public void DestroyBullet(int id)
    {
        SendMessageClient(Subject.DBl, id);
    }

    public void ApplyStatusEffectToMonster(int targetId, StatusEffect effect, float duration, int strength)
    {
        SendMessageClient(Subject.MoEf, targetId, (int)effect, duration, strength);
    }

    public void CorrectPlayerDeadPosition(float xPos, float yPos)
    {
        SendMessageClient(Subject.PlDd, xPos, yPos);
    }

    public void RebuildWall(int brokenWallId, int amount)
    {
        SendMessageClient(Subject.RbWl, brokenWallId, amount);
    }

    public void UpgradeWeapon(string weaponName)
    {
        SendMessageClient(Subject.UpWpn, weaponName);
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
        string[] message = { };
        switch (obj)
        {
            case Wall wall:
                message = MessageBuilder(Subject.MdWl, wall.id, amount);
                break;
            case Monster monster:
                message = MessageBuilder(Subject.MdMo, monster.id, amount);
                break;
            case Player player:
                message = MessageBuilder(Subject.MdPl, player.name, amount.ToString("f2"));
                break;
            case Statue _:
                message = MessageBuilder(Subject.MdSt, amount);
                break;
        }
        return SendMessageClient(message, errorMessage: $"Failed to modify HP of {obj}");
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
        string[] message = { };
        switch (target)
        {
            case Target.Player:
                Debug.LogError($"Must use string for Player name.");
                var player = UnitManager.Instance.GetPlayer(id); if (!player) break;
                var name = player.name;
                ModifyHp(target, name, amount);
                break;
            case Target.Statue:
                message = MessageBuilder(Subject.MdSt, amount);
                break;
            case Target.Wall:
                message = MessageBuilder(Subject.MdWl, id, amount);
                break;
            case Target.Monster:
                message = MessageBuilder(Subject.MdMo, id, amount);
                break;
        }
        return SendMessageClient(message, errorMessage: $"Failed to modify HP of {target}({id})");
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
        string[] message = { };
        int id;
        switch (target)
        {
            case Target.Player:
                message = MessageBuilder(Subject.MdPl, name, amount.ToString("f2"));
                break;
            case Target.Statue:
                message = MessageBuilder(Subject.MdSt, amount);
                break;
            default:
                id = IntParse(name);
                if (id == int.MinValue) break;
                ModifyHp(target, id, amount);
                break;
        }
        return SendMessageClient(message, errorMessage:$"Failed to modify HP of {target}({name})");
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
        string[] message = { };
        switch (target)
        {
            case Target.Player:
                Debug.LogError("Player name not specified!");
                break;
            case Target.Statue:
                message = MessageBuilder(Subject.MdSt, amount);
                break;
            default:
                Debug.LogError("Id not specified!");
                break;
        }
        return SendMessageClient(message, errorMessage: $"Failed to modify HP of {target}");
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