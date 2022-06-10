using System;
using System.Collections;
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
                SendMessageClient("2", "A");
            }
        }
        _runTime += Time.deltaTime;
    }

    // Header enums
    private enum Header
    {
        Svr,    // Server
        RCrd,   // Room Created
        RJnd,   // Room Joined
        RnFd,   // Room not found
        RsF,    // Room is full
        REx,    // Room exit
        MPos,   // Sync Mouse Pos
        PlCt,   // Player Count
        StGm,   // Start game
        SpwP,
        EqWp,
        PAtk,
        SwBl,
        MdMo,
        MdPl,
        PlDd,
        MdWl,
        SpwM,
        MoEf,
        MAtk,
        MdSt,
        LRm,
        DBl,
        RbWl,
        UpWpn,
        PlVl,
        PJmp,
        ChNm,
        GmOv
    }

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
                    OnJoinedRoom(info[2]);
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
            //Debug.Log(EnumParse<Header>(info[1]));
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
                    string[] temp = new string[playersCount];
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
                        EnumParse<Origin>(info[4]), float.Parse(info[5]));
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
                case Header.SwBl:
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
                        EnumParse<StatusEffect>(info[3]), float.Parse(info[4]), int.Parse(info[5]));
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
                    Debug.Log($"Wall {int.Parse(info[2])} hp modified by {float.Parse(info[3])}");
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
                    Debug.Log($"Statue hp modified by {float.Parse(info[2])}");
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
                        EnumParse<PlayerMovement.Axis>(info[4]));
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
            }
        }
    }

    // Process message that is about to be sent ---------------------------------------
    private void SendMessageClient(string target, string message)
    {
        // Message format : target|header|data|data|data...
        // Target code : 1.All  2.Server  3.All except Sender   others:Specific player name
        string[] temp = new string[1];
        temp[0] = message;

        SendMessageClient(target, temp);
    }
    private void SendMessageClient(string target, string[] message)
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
        // Set to master, becuse we the one created room ---------------------------
        isMaster = true;
        playersCount = 1;

        // Load Scene 
        ScenesManager.Instance.LoadScene(1);

        // Debugging
        Debug.Log("Room Created");
    }

    private void OnJoinedRoom(string roomName)
    {
        // Load Scene 
        ScenesManager.Instance.LoadScene(1);

        // Debugging
        Debug.Log("Room Joined");
    }

    #region Send Message To Server

    // Public method that can be called to send message to server -------------------
    public void ChangeName(string newId, string newName)
    {
        string[] msg = { "ChNm", newId, newName };
        SendMessageClient("2", msg);
    }
    public void StartMatchmaking()
    {
        SendMessageClient("2", "StMtc");
    }

    public void CreateRoom(string roomName, int maxPlayer, bool isPublic)
    {
        string[] message = {"CrR", roomName, maxPlayer.ToString(), isPublic.ToString()};
        SendMessageClient("2", message);
    }

    public void JoinRoom(string roomName)
    {
        string[] message = {"JnR", roomName, roomName};
        SendMessageClient("2", message);
    }
    public void ExitRoom()
    {
        SendMessageClient("2", "ExR");
    }

    public void StartGame()
    {
        SendMessageClient("1", "StGm");
    }
    public void LockTheRoom()
    {
        SendMessageClient("2", "LcR");
    }

    public void SpawnPlayer(Vector2 spawnPos, int skin)
    {
        string[] msg = {Header.SpwP.ToString(), spawnPos.x.ToString("f2"), spawnPos.y.ToString("f2"), skin.ToString()};
        SendMessageClient("1", msg);
    }

    public void SpawnMonster(int id, int type, Origin origin, float spawnOffset)
    {
        //Debug.Log($"Send spawn monster: id {id}, type {type}");
        string[] msg = {Header.SpwM.ToString(), id.ToString(), type.ToString(), origin.ToString(), spawnOffset.ToString("F2")};
        SendMessageClient("1", msg);
    }

    public void SetPlayerVelocity(Vector2 velocity, PlayerMovement.Axis axis)
    {
        string[] msg = { Header.PlVl.ToString(), velocity.x.ToString("f2"), velocity.y.ToString("f2"), axis.ToString() };
        SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
    }

    public void SendMousePos(float x, float y)
    {
        string[] msg = {Header.MPos.ToString(), x.ToString("f2"), y.ToString("f2")};
        SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
    }

    public void EquipWeapon(string weapon)
    {
        string[] msg = {Header.EqWp.ToString(), weapon};
        SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
    }

    public void StartAttackAnimation() 
    {
        string[] msg = {Header.PAtk.ToString()};
        SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
    }

    public void Jump()
    {
        string[] msg = { Header.PJmp.ToString() };
        SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
    }

    public void StartMonsterAttackAnimation(int targetId) 
    {
        //Debug.Log("Monster Attack Animation");
        string[] msg = {Header.MAtk.ToString(), targetId.ToString()};
        SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
    }

    public void SpawnBullet(Vector2 spawnPos, Vector2 mousePos, int spawnedByMonsterId = -1)
    {
        string[] msg =
        {
            Header.SwBl.ToString(), spawnPos.x.ToString("f2"), spawnPos.y.ToString("f2"), mousePos.x.ToString("f2"),
            mousePos.y.ToString("f2"), spawnedByMonsterId.ToString()
        };
        SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
    }

    public void DestroyBullet(int id)
    {
        string[] msg = {Header.DBl.ToString(), id.ToString()};
        SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
    }

    public void ModifyMonsterHp(int id, float amount)
    {
        string[] msg = {Header.MdMo.ToString(), id.ToString(), amount.ToString("f2")};
        SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
    }

    public void ApplyStatusEffectToMonster(int targetId, StatusEffect effect, float duration, int strength)
    {
        string[] msg = {Header.MoEf.ToString(), targetId.ToString(), effect.ToString(), duration.ToString("f2"), strength.ToString() };
        SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
    }

    public void ModifyPlayerHp(string playerName, float amount)
    {
        //Debug.Log("Send: Monster deals"+amount+" damage to "+playerName);
        string[] msg = {Header.MdPl.ToString(), playerName, amount.ToString("f2")};
        SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
    }

    public void CorrectPlayerDeadPosition(float xPos, float yPos)
    {
        string[] msg = {Header.PlDd.ToString(), xPos.ToString("f2"), yPos.ToString("f2")};
        SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
    }

    public void ModifyWallHp(int id, float amount)
    {
        string[] msg = {Header.MdWl.ToString(), id.ToString(), amount.ToString("f2")};
        SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
    }

    public void ModifyStatueHp(float amount)
    {
        string[] msg = {Header.MdSt.ToString(), amount.ToString("f2")};
        SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
    }

    public void RebuildWall(int brokenWallId, int amount)
    {
        string[] msg = { Header.RbWl.ToString(), brokenWallId.ToString(), amount.ToString() };
        SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
    }

    public void UpgradeWeapon(string weaponName)
    {
        string[] msg = { Header.UpWpn.ToString(), weaponName };
        SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
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
                msg = new string[] { Header.MdWl.ToString(), wall.id.ToString(), amount.ToString("f2") };
                break;
            case Monster monster:
                msg = new string[] { Header.MdMo.ToString(), monster.id.ToString(), amount.ToString("f2") };
                break;
            case Player player:
                msg = new string[] { Header.MdPl.ToString(), player.name, amount.ToString("f2") };
                break;
            case Statue _:
                msg = new string[] { Header.MdSt.ToString(), amount.ToString("f2") };
                break;
        }
        if (msg.Length > 0)
        {
            Debug.Log($"{msg[0]}, {msg[1]}, {amount}");
            SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
            return true;
        }
        Debug.LogError($"Failed to modify hp of {obj}");
        return false;
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
                var player = UnitManager.Instance.GetPlayer(id);
                if (!player) break;
                var name = player.name;
                ModifyHp(target, name, amount);
                break;
            case Target.Statue:
                msg = new string[] { Header.MdSt.ToString(), amount.ToString("f2") };
                break;
            case Target.Wall:
                msg = new string[] { Header.MdWl.ToString(), id.ToString(), amount.ToString("f2") };
                break;
            case Target.Monster:
                msg = new string[] { Header.MdMo.ToString(), id.ToString(), amount.ToString("f2") };
                break;
        }
        if (msg.Length > 0)
        {
            SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
            return true;
        }
        Debug.LogError($"Failed to modify hp of {target}({id})");
        return false;
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
                msg = new string[] { Header.MdPl.ToString(), name, amount.ToString("f2") };
                break;
            case Target.Statue:
                msg = new string[] { Header.MdSt.ToString(), amount.ToString("f2") };
                break;
            case Target.Wall:
                id = IntParse(name);
                if (id == int.MinValue) break;
                ModifyHp(target, id, amount);
                break;
            case Target.Monster:
                id = IntParse(name);
                if (id == int.MinValue) break;
                ModifyHp(target, id, amount);
                break;
        }
        if (msg.Length > 0)
        {
            SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
            return true;
        }
        Debug.LogError($"Failed to modify hp of {target}({name})");
        return false;
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
                msg = new string[] { Header.MdSt.ToString(), amount.ToString("f2") };
                break;
            case Target.Wall:
                Debug.LogError("Id not specified!");
                break;
            case Target.Monster:
                Debug.LogError("Id not specified!");
                break;
        }
        if (msg.Length > 0)
        {
            SendMessageClient(_waitForServer ? "1" : SelfRun(msg), msg);
            return true;
        }
        Debug.LogError($"Failed to modify hp of {target}");
        return false;
    }

    #endregion

    #region Utilities
    // Utilities
    private static T EnumParse<T>(string stringToEnum)
    {
        return (T) Enum.Parse(typeof(T), stringToEnum, true);
    }

    private static int ExtractId(string idAndName)
    {
        return int.Parse(idAndName.Substring(0, IdLength));
    }

    private string SelfRun(string[] msg)
    {
        string data = msg.Aggregate(myId + myName, (current, x) => current + ("|" + x));
        ReceiveMessage(data);
        return "3";
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