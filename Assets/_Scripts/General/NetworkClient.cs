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

    // Encryption ---------------------------------------------------------------------
    private RsaEncryption rsaEncryption;
    [SerializeField] private string ServerPublicKey;

    // Player Room and Name -----------------------------------------------------------
    public string myId { get; private set; }
    public string myName { get; private set; }
    public string myRoomName { get; private set; }
    public bool isMaster { get; private set; }
    public int playersCount { get; private set; }

    private float DefaultCheckTime;
    private float checkTime;

    // Connection status --------------------------------------------------------------
    private bool isVerified;

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
        ipAd = IPAddress.Parse(IpAddress);
        rsaEncryption = new RsaEncryption(ServerPublicKey);
        isVerified = false;
        isMaster = false;

        DefaultCheckTime = .8f;
        checkTime = DefaultCheckTime;
    }

    // Try connecting to server ------------------------------------------------------
    public void BeginConnecting() 
    {
        // Get Player Name -----------------------------------------------------------
        myName = PlayerDataLoader.Instance.TheData.UserName;
        //myId = PlayerDataLoader.Instance.TheData.UserId;
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
        if(client.Connected && isVerified)
        {
            // If all ok, begin listening -------------------------------------------
            if (networkStream.DataAvailable)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                ReceiveMessage(formatter.Deserialize(networkStream) as string);
            }

            // Always send message to server to tell server that we
            // Still online 
            checkTime -= Time.deltaTime;
            if(checkTime <= 0)
            {
                SendMessageClient("2", "A");
            }
        }
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
        BtDw,   // Button Down
        PlCt,   // Player Count
        StGm,   // Start game
        SwPy,
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
        RbWl
    }

    // Receive and Process incoming message here ----------------------------------
    private void ReceiveMessage(string message)
    {
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
                case Header.BtDw:
                {
                    UnitManager.Instance.SetButton(info[0], EnumParse<Player.Button>(info[2]),
                        bool.Parse(info[3]));
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
                    GameMenuManager.Instance.UpdatePlayersInRoom(temp);
                    break;
                case Header.StGm:
                    GameManager.Instance.ChangeState(GameManager.GameState.StartGame);
                    break;
                case Header.SwPy:
                    SpawnManager.instance.ReceiveSpawnPlayer(info[0], ExtractId(info[0]), 
                        new Vector2(float.Parse(info[2]), float.Parse(info[3])),
                        int.Parse(info[4]));
                    break;
                case Header.SpwM:
                    SpawnManager.instance.ReceiveSpawnMonster(int.Parse(info[2]), int.Parse(info[3]),
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
                    UnitManager.Instance.ModifyMonsterHp(int.Parse(info[2]), float.Parse(info[3]));
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
                    Debug.Log("Receive: ModifyPlayerHp " + info[2] + " " + info[3]);
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
                    TilemapManager.instance.ModifyWallHp(int.Parse(info[2]), float.Parse(info[3]));
                    break;
                }
                case Header.MAtk:
                {
                    UnitManager.Instance.PlayMonsterAttackAnimation(int.Parse(info[2]));
                    break;
                }
                case Header.MdSt:
                {
                    TilemapManager.instance.ModifyStatueHp(float.Parse(info[2]));
                    break;
                }
                case Header.DBl:
                {
                    UnitManager.Instance.DestroyBullet(int.Parse(info[2]));
                    break;
                }
                case Header.RbWl:
                {
                    TilemapManager.instance.RebuiltWall(int.Parse(info[2]), int.Parse(info[3]));
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

        checkTime = DefaultCheckTime;
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

    #region Send Message To Server

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

    // Public method that can be called to send message to server -------------------
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
        string[] msg = {Header.SwPy.ToString(), spawnPos.x.ToString("f2"), spawnPos.y.ToString("f2"), skin.ToString()};
        SendMessageClient("1", msg);
    }

    public void SpawnMonster(int id, int type, Origin origin, float spawnOffset)
    {
        string[] msg = {Header.SpwM.ToString(), id.ToString(), type.ToString(), origin.ToString(), spawnOffset.ToString("F2")};
        SendMessageClient("1", msg);
    }

    public void SetMovementButton(Player.Button button, bool isDown)
    {
        string[] msg = {Header.BtDw.ToString(), button.ToString(), isDown.ToString()};
        SendMessageClient("1", msg);
    }

    public void SendMousePos(float x, float y)
    {
        string[] msg = {Header.MPos.ToString(), x.ToString("f2"), y.ToString("f2")};
        SendMessageClient("1", msg);
    }

    public void EquipWeapon(string weapon)
    {
        string[] msg = {Header.EqWp.ToString(), weapon};
        SendMessageClient("1", msg);
    }

    public void StartAttackAnimation() 
    {
        string[] msg = {Header.PAtk.ToString()};
        SendMessageClient("1", msg);
    }

    public void StartMonsterAttackAnimation(int targetId) 
    {
        string[] msg = {Header.MAtk.ToString(), targetId.ToString()};
        SendMessageClient("1", msg);
    }

    public void SpawnBullet(Vector2 spawnPos, Vector2 mousePos, int spawnedByMonsterId = -1)
    {
        string[] msg =
        {
            Header.SwBl.ToString(), spawnPos.x.ToString("f2"), spawnPos.y.ToString("f2"), mousePos.x.ToString("f2"),
            mousePos.y.ToString("f2"), spawnedByMonsterId.ToString()
        };
        SendMessageClient("1", msg);
    }

    public void DestroyBullet(int id)
    {
        string[] msg = {Header.DBl.ToString(), id.ToString()};
        SendMessageClient("1", msg);
    }

    public void ModifyMonsterHp(int id, float amount)
    {
        string[] msg = {Header.MdMo.ToString(), id.ToString(), amount.ToString("f2")};
        SendMessageClient("1", msg);
    }

    public void ApplyStatusEffectToMonster(int targetId, StatusEffect effect, float duration, int strength)
    {
        string[] msg = {Header.MoEf.ToString(), targetId.ToString(), effect.ToString(), duration.ToString("f2"), strength.ToString() };
        SendMessageClient("1", msg);
    }

    public void ModifyPlayerHp(string playerName, float amount)
    {
        Debug.Log("Send: Monster deals"+amount+" damage to "+playerName);
        string[] msg = {Header.MdPl.ToString(), playerName, amount.ToString("f2")};
        SendMessageClient("1", msg);
    }

    public void CorrectPlayerDeadPosition(float xPos, float yPos)
    {
        string[] msg = {Header.PlDd.ToString(), xPos.ToString("f2"), yPos.ToString("f2")};
        SendMessageClient("1", msg);
    }

    public void ModifyWallHp(int id, float amount)
    {
        string[] msg = {Header.MdWl.ToString(), id.ToString(), amount.ToString("f2")};
        SendMessageClient("1", msg);
    }

    public void ModifyStatueHp(float amount)
    {
        string[] msg = {Header.MdSt.ToString(), amount.ToString("f2")};
        SendMessageClient("1", msg);
    }

    public void RebuildWall(int brokenWallId, int amount)
    {
        string[] msg = { Header.RbWl.ToString(), brokenWallId.ToString(), amount.ToString() };
        SendMessageClient("1", msg);
    }

    #endregion

    #region Utilities
    // Utilities
    private static T EnumParse<T>(string stringToEnum)
    {
        return (T) Enum.Parse(typeof(T), stringToEnum, true);
    }

    private static string ExtractId(string idAndName)
    {
        return idAndName.Substring(0, IdLength);
    }

    #endregion
}