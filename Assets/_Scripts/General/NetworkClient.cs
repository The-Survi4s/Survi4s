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
    RsaEncryption rsaEncryption;
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
            // Wait 2 second and try agaian
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

    // Receive and Process incoming message here ----------------------------------
    private void ReceiveMessage(string message)
    {
        // Message format : sender|header|data|data|data... 
        // Svr|RCrd|...
        // ID+NameClient|MPos|...
        string[] info = message.Split('|');

        if (info[0] == "Svr")
        {
            switch (info[1])
            {
                case "RCrd":
                    OnCreatedRoom(info[2]);
                    break;
                case "RJnd":
                    OnJoinedRoom(info[2], int.Parse(info[3]));
                    break;
                case "RnFd":
                    JoinRoomPanel.Instance.RoomNotFound();
                    break;
                case "RsF":
                    JoinRoomPanel.Instance.RoomIsFull();
                    break;
                case "REx":
                    ScenesManager.Instance.LoadScene(0);
                    break;
            }
        }
        else
        {
            switch (info[1])
            {
                case "MPos":
                {
                    foreach (var obj in UnitManager.Instance.players.Where(obj => obj.name == info[0].Substring(0, obj.name.Length)))
                    {
                        obj.GetComponent<PlayerController>().SyncMousePos(float.Parse(info[2]), float.Parse(info[3]));
                    }
                    break;
                }
                case "BtDw":
                {
                    foreach (var obj in UnitManager.Instance.players.Where(obj => obj.name == info[0].Substring(0, obj.name.Length)))
                    {
                        obj.GetComponent<PlayerController>().SetButtonDown((PlayerController.Button) Enum.Parse(typeof(PlayerController.Button), info[2], true));
                    }
                    break;
                }
                case "BtUp":
                {
                    foreach (var obj in UnitManager.Instance.players.Where(obj => obj.name == info[0].Substring(0, obj.name.Length)))
                    {
                        obj.GetComponent<PlayerController>().SetButtonUp((PlayerController.Button)Enum.Parse(typeof(PlayerController.Button), info[2], true));
                    }
                    break;
                }
                case "PlCt":
                    playersCount = int.Parse(info[2]);
                    GameMenuManager.Instance.UpdatePlayersInRoom(playersCount);
                    break;
                case "StGm":
                    GameManager.Instance.GameStarted();
                    break;
                case "SwPy":
                    UnitManager.Instance.SpawnPlayer(info[0] ,float.Parse(info[2]), float.Parse(info[3]), int.Parse(info[4]));
                    break;
                case "SwM":
                    SpawnManager.Instance.OnReceiveSpawnMonster(int.Parse(info[2]), (Monster.Type)Enum.Parse(typeof(Monster.Type), info[3], true), (Monster.Origin)Enum.Parse(typeof(Monster.Origin), info[4], true));
                    break;
                case "EqWp":
                {
                    foreach (var obj in UnitManager.Instance.players.Where(obj => obj.name == info[0].Substring(0, obj.name.Length)))
                    {
                        obj.GetComponent<PlayerWeaponManager>().OnEquipWeapon(info[2]);
                    }
                    break;
                }
                case "PAtk":
                {
                    foreach (var obj in UnitManager.Instance.players.Where(obj => obj.name == info[0].Substring(0, obj.name.Length)))
                    {
                        obj.GetComponent<PlayerWeaponManager>().OnAttack();
                    }
                    break;
                }
                case "SwBl":
                {
                    foreach (var obj in UnitManager.Instance.players.Where(obj => obj.name == info[0].Substring(0, obj.name.Length)))
                    {
                        obj.GetComponent<PlayerWeaponManager>().SpawnBullet(float.Parse(info[2]), float.Parse(info[3]), float.Parse(info[4]), float.Parse(info[5]));
                    }
                    break;
                }
                case "DmgM":
                {
                    foreach (var monster in from monster in UnitManager.Instance.monsters let ori = (Monster.Origin)Enum.Parse(typeof(Monster.Origin), info[3], true) where monster.ID == int.Parse(info[2]) && monster.origin == ori select monster)
                    {
                        monster.ReduceHitPoint(float.Parse(info[4]));
                    }
                    break;
                }
                case "StM":
                {
                    foreach (var monster in from monster in UnitManager.Instance.monsters let ori = (Monster.Origin)Enum.Parse(typeof(Monster.Origin), info[3], true) where monster.ID == int.Parse(info[2]) && monster.origin == ori select monster)
                    {
                        monster.Stun(float.Parse(info[4]));
                    }
                    break;
                }
                case "HePl":
                {
                    foreach (var obj in UnitManager.Instance.players.Where(obj => obj.name == info[2].Substring(0, obj.name.Length)))
                    {
                        obj.GetComponent<CharacterStats>().hitPointAdd=int.Parse(info[3]);
                    }
                    break;
                }
                case "DmgPl":
                {
                    foreach (var obj in UnitManager.Instance.players.Where(obj => obj.name == info[2].Substring(0, obj.name.Length)))
                    {
                        obj.GetComponent<CharacterStats>().hitPointAdd=int.Parse(info[3]);
                    }
                    break;
                }
                case "PlDd":
                {
                    foreach (var obj in UnitManager.Instance.players.Where(obj =>
                                 obj.name == info[0].Substring(0, obj.name.Length)))
                    {
                        obj.GetComponent<CharacterStats>().PlayerDead(float.Parse(info[2]), float.Parse(info[3]));
                    }
                    break;
                }
                case "HeWl":
                {
                    WallManager.Instance.ReceiveRepairWall(int.Parse(info[2]), float.Parse(info[3]));
                    break;
                }
                case "DmgWl":
                {
                    WallManager.Instance.ReceiveDamageWall(int.Parse(info[2]), float.Parse(info[3]));
                    break;
                }
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
    public string GeneratePlayerId()
    {
        string genId = "";
        for(int i = 0; i < 6; i++)
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
        string[] message = new string[] { "CrR", roomName, maxPlayer.ToString(), isPublic.ToString() } ;
        SendMessageClient("2", message);
    }
    public void JoinRoom(string roomName)
    {
        string[] message = new string[] { "JnR", roomName, roomName.ToString() };
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
        string[] msg = new string[] { "SwPy", x.ToString("f2"), y.ToString("f2"), skin.ToString() };
        SendMessageClient("1", msg);
    }
    public void SpawnMonster(int Id, Monster.Type type, Monster.Origin origin)
    {
        string[] msg = new string[] { "SwM", Id.ToString(), type.ToString(), origin.ToString() };
        SendMessageClient("1", msg);
    }

    public void MovementButtonDown(PlayerController.Button button)
    {
        string[] msg = new string[] { "BtDw", button.ToString() };
        SendMessageClient("1", msg);
    }
    public void MovementButtonUp(PlayerController.Button button)
    {
        string[] msg = new string[] { "BtUp", button.ToString() };
        SendMessageClient("1", msg);
    }
    public void SendMousePos(float x, float y)
    {
        string[] msg = new string[] { "MPos", x.ToString("f2"), y.ToString("f2") };
        SendMessageClient("1", msg);
    }

    public void EquipWeapon(string weapon)
    {
        string[] msg = new string[] { "EqWp", weapon };
        SendMessageClient("1", msg);
    }

    public void Attack() // Wtf is dis?
    {
        string[] msg = new string[] { "PAtk" };
        SendMessageClient("1", msg);
    }
    public void SpawnBullet(float xSpawnPos, float ySpawnPos, float xMousePos, float yMousePos)
    {
        string[] msg = new string[] { "SwBl", xSpawnPos.ToString("f2"), ySpawnPos.ToString("f2"), xMousePos.ToString("f2"), yMousePos.ToString("f2") };
        SendMessageClient("1", msg);
    }

    public void DamageMonster(int Id, Monster.Origin origin, float damage)
    {
        string[] msg = new string[] { "DmgM", Id.ToString(), origin.ToString(), damage.ToString("f2") };
        SendMessageClient("1", msg);
    }
    public void StunMonster(int Id, Monster.Origin origin, float time)
    {
        string[] msg = new string[] { "StM", Id.ToString(), origin.ToString(), time.ToString("f2") };
        SendMessageClient("1", msg);
    }
    public void HealPlayer(string Id, string name, float healPoint)
    {
        string[] msg = new string[] { "HePl", Id+name, healPoint.ToString("f2") };
        SendMessageClient( "1", msg);
    }
    public void DamagePlayer(string Id, string name, float damage)
    {
        string[] msg = new string[] { "DmgPl", Id + name, damage.ToString("f2") };
        SendMessageClient("1", msg);
    }
    public void PlayerDead(float xPos, float yPos)
    {
        string[] msg = new string[] { "PlDd", xPos.ToString("f2"), yPos.ToString("f2") };
        SendMessageClient("1", msg);
    }
    public void HealWall(int Id, float healPoint)
    {
        string[] msg = new string[] { "HeWl", Id.ToString(), healPoint.ToString("f2") };
        SendMessageClient("1", msg);
    }
    public void DamageWall(int Id, float damage)
    {
        string[] msg = new string[] { "DmgWl", Id.ToString(), damage.ToString("f2") };
        SendMessageClient("1", msg);
    }
}
