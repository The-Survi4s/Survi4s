using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
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
        MAtk
    }

    // Receive and Process incoming message here ----------------------------------
    private void ReceiveMessage(string message)
    {
        // Message format : sender|header|data|data|data... 
        // Svr|RCrd|...
        // ID+NameClient|MPos|...
        var info = message.Split('|');

        if (EnumParse<Header>(info[0]) == Header.Svr)
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
            }
        else
            switch (EnumParse<Header>(info[1]))
            {
                case Header.MPos:
                {
                    UnitManager.Instance.SyncMousePos(ExtractName(info[0]), float.Parse(info[2]), float.Parse(info[3]));
                    break;
                }
                case Header.BtDw:
                {
                    UnitManager.Instance.SetButton(ExtractName(info[0]), EnumParse<PlayerController.Button>(info[2]),
                        bool.Parse(info[3]));
                    break;
                }
                case Header.PlCt:
                    playersCount = int.Parse(info[2]);
                    GameMenuManager.Instance.UpdatePlayersInRoom(playersCount);
                    break;
                case Header.StGm:
                    GameManager.Instance.GameStarted();
                    break;
                case Header.SwPy:
                    UnitManager.Instance.SpawnPlayer(info[0], ExtractId(info[0]), float.Parse(info[2]), float.Parse(info[3]),
                        int.Parse(info[4]));
                    break;
                case Header.SpwM:
                    SpawnManager.instance.OnReceiveSpawnMonster(int.Parse(info[2]), int.Parse(info[3]),
                        EnumParse<Monster.Origin>(info[4]), float.Parse(info[5]));
                    break;
                case Header.EqWp:
                {
                    UnitManager.Instance.OnEquipWeapon(ExtractName(info[0]), info[2]);
                    break;
                }
                case Header.PAtk:
                {
                    UnitManager.Instance.PlayAttackAnimation(ExtractName(info[0]));
                    break;
                }
                case Header.SwBl:
                {
                    UnitManager.Instance.SpawnBullet(ExtractName(info[0]), float.Parse(info[2]), float.Parse(info[3]),
                        float.Parse(info[4]), float.Parse(info[5]));
                    break;
                }
                case Header.MdMo:
                {
                    UnitManager.Instance.ModifyMonsterHp(int.Parse(info[2]),float.Parse(info[3]));
                    break;
                }
                case Header.MoEf:
                {
                    UnitManager.Instance.ApplyStatusEffectToMonster(int.Parse(info[2]),
                        EnumParse<StatusEffect>(info[3]), int.Parse(info[4]), float.Parse(info[5]));
                    break;
                }
                case Header.MdPl:
                {
                    UnitManager.Instance.ModifyPlayerHp(ExtractName(info[2]), int.Parse(info[3]));
                    break;
                }
                case Header.PlDd:
                {
                    UnitManager.Instance.CorrectDeadPosition(ExtractName(info[0]), float.Parse(info[2]), float.Parse(info[3]));
                    break;
                }
                case Header.MdWl:
                {
                    WallManager.Instance.ReceiveModifyWallHp(int.Parse(info[2]), float.Parse(info[3]));
                    break;
                }
                case Header.MAtk:
                {
                    UnitManager.Instance.PlayMonsterAttackAnimation(int.Parse(info[2]));
                    break;
                }
            }
    }

    // Proccess message that want to be send ---------------------------------------
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
    public bool IsConnected()
    {
        return client.Connected;
    }

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
    }
    private void OnJoinedRoom(string roomName, int playerCount)
    {
        playersCount = playerCount;

        // Load Scene 
        ScenesManager.Instance.LoadScene(1);
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

    public void SpawnPlayer(float x, float y, int skin)
    {
        string[] msg = {Header.SwPy.ToString(), x.ToString("f2"), y.ToString("f2"), skin.ToString()};
        SendMessageClient("1", msg);
    }

    public void SpawnMonster(int Id, int type, Monster.Origin origin, float spawnOffset)
    {
        string[] msg = {Header.SpwM.ToString(), Id.ToString(), type.ToString(), origin.ToString(), spawnOffset.ToString("F2")};
        SendMessageClient("1", msg);
    }

    public void SetMovementButton(PlayerController.Button button, bool isDown)
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

    public void SpawnBullet(float xSpawnPos, float ySpawnPos, float xMousePos, float yMousePos)
    {
        string[] msg =
        {
            Header.SwBl.ToString(), xSpawnPos.ToString("f2"), ySpawnPos.ToString("f2"), xMousePos.ToString("f2"),
            yMousePos.ToString("f2")
        };
        SendMessageClient("1", msg);
    }

    public void ModifyMonsterHp(int id, float amount)
    {
        string[] msg = {Header.MdMo.ToString(), id.ToString(), amount.ToString("f2")};
        SendMessageClient("1", msg);
    }

    public void ApplyStatusEffectToMonster(int targetId, StatusEffect effect, int strength, float duration)
    {
        string[] msg = {Header.MoEf.ToString(), targetId.ToString(), effect.ToString(), strength.ToString(), duration.ToString("f2")};
        SendMessageClient("1", msg);
    }

    public void ModifyPlayerHp(string id, string playerName, float amount)
    {
        string[] msg = {Header.MdPl.ToString(), id + playerName, amount.ToString("f2")};
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

    // Utilities
    private static T EnumParse<T>(string stringToEnum)
    {
        return (T) Enum.Parse(typeof(T), stringToEnum, true);
    }

    private static string ExtractName(string nameAndId)
    {
        return nameAndId.Substring(0, nameAndId.Length - IdLength);
    }

    private static string ExtractId(string nameAndId)
    {
        return nameAndId.Substring(nameAndId.Length - IdLength);
    }
}