using System;
using System.Collections;
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
    [SerializeField] private string customId;
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

    void Start()
    {
        // Prepararation -------------------------------------------------------------
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
        myId = customId;

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
            // if no, show some error massage ------------------------------------------------

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

            // Alwasy send massage to server to tell server that we
            // Still online 
            checkTime -= Time.deltaTime;
            if(checkTime <= 0)
            {
                SendMessageClient("2", "A");
            }
        }
    }

    // Receive and Preccess incoming massage here ----------------------------------
    private void ReceiveMessage(string massage)
    {
        // Message format : sender|header|data|data|data... 
        // Svr|RCrd|...
        // ID+NamaClient|MPos|...
        string[] info = massage.Split('|');

        if (info[0] == "Svr")
        {
            if (info[1] == "RCrd")
            {
                OnCreatedRoom(info[2]);
            }
            else if (info[1] == "RJnd")
            {
                OnJoinedRoom(info[2], int.Parse(info[3]));
            }
            else if (info[1] == "RnFd")
            {
                JoinRoomPanel.Instance.RoomNotFound();
            }
            else if (info[1] == "RsF")
            {
                JoinRoomPanel.Instance.RoomIsFull();
            }
            else if (info[1] == "REx")
            {
                ScenesManager.Instance.LoadScene(0);
            }
        }
        else
        {
            if (info[1] == "MPos")
            {
                foreach (GameObject obj in UnitManager.Instance.players)
                {
                    if (obj.name == info[0].Substring(0, obj.name.Length))
                    {
                        obj.GetComponent<CharacterController>().SyncMousePos(float.Parse(info[2]), float.Parse(info[3]));
                    }
                }
            }
            else if (info[1] == "BtDw")
            {
                foreach (GameObject obj in UnitManager.Instance.players)
                {
                    if (obj.name == info[0].Substring(0, obj.name.Length))
                    {
                        obj.GetComponent<CharacterController>().SetButtonDown((CharacterController.Button) Enum.Parse(typeof(CharacterController.Button), info[2], true));
                    }
                }
            }
            else if (info[1] == "BtUp")
            {
                foreach (GameObject obj in UnitManager.Instance.players)
                {
                    if (obj.name == info[0].Substring(0, obj.name.Length))
                    {
                        obj.GetComponent<CharacterController>().SetButtonUp((CharacterController.Button)Enum.Parse(typeof(CharacterController.Button), info[2], true));
                    }
                }
            }
            else if (info[1] == "PlCt")
            {
                playersCount = int.Parse(info[2]);
                GameMenuManager.Instance.UpdatePlayersInRoom(playersCount);
            }
            else if (info[1] == "StGm")
            {
                GameManager.Instance.GameStarted();
            }
            else if (info[1] == "SwPy")
            {
                UnitManager.Instance.SpawnPlayer(info[0] ,float.Parse(info[2]), float.Parse(info[3]), int.Parse(info[4]));
            }
            else if (info[1] == "SwM")
            {
                UnitManager.Instance.OnSpawnMonster(int.Parse(info[2]), (Monster.Origin)Enum.Parse(typeof(Monster.Origin), info[3], true));
            }
            else if (info[1] == "EqWp")
            {
                foreach (GameObject obj in UnitManager.Instance.players)
                {
                    if (obj.name == info[0].Substring(0, obj.name.Length))
                    {
                        obj.GetComponent<CharacterWeapon>().OnEquipWeapon(info[2]);
                    }
                }
            }
            else if (info[1] == "PAtk")
            {
                foreach (GameObject obj in UnitManager.Instance.players)
                {
                    if (obj.name == info[0].Substring(0, obj.name.Length))
                    {
                        obj.GetComponent<CharacterWeapon>().OnAttack();
                    }
                }
            }
            else if (info[1] == "SwBl")
            {
                foreach (GameObject obj in UnitManager.Instance.players)
                {
                    if (obj.name == info[0].Substring(0, obj.name.Length))
                    {
                        obj.GetComponent<CharacterWeapon>().SpawnBullet(float.Parse(info[2]), float.Parse(info[3]), float.Parse(info[4]), float.Parse(info[5]));
                    }
                }
            }
            else if (info[1] == "DmgM")
            {
                foreach (Monster monster in UnitManager.Instance.monsters)
                {
                    Monster.Origin ori = (Monster.Origin)Enum.Parse(typeof(Monster.Origin), info[3], true);
                    if (monster.ID == int.Parse(info[2]) && monster.origin == ori)
                    {
                        monster.ReduceHitPoint(float.Parse(info[4]));
                    }
                }
            }
            else if (info[1] == "StM")
            {
                foreach (Monster monster in UnitManager.Instance.monsters)
                {
                    Monster.Origin ori = (Monster.Origin)Enum.Parse(typeof(Monster.Origin), info[3], true);
                    if (monster.ID == int.Parse(info[2]) && monster.origin == ori)
                    {
                        monster.Stun(float.Parse(info[4]));
                    }
                }
            }
            else if (info[1] == "HePl")
            {
                foreach (GameObject obj in UnitManager.Instance.players)
                {
                    if (obj.name == info[2].Substring(0, obj.name.Length))
                    {
                        obj.GetComponent<CharacterStats>().HealHitPoint(int.Parse(info[3]));
                    }
                }
            }
            else if (info[1] == "DmgPl")
            {
                foreach (GameObject obj in UnitManager.Instance.players)
                {
                    if (obj.name == info[2].Substring(0, obj.name.Length))
                    {
                        obj.GetComponent<CharacterStats>().ReduceHitPoint(int.Parse(info[3]));
                    }
                }
            }
            else if (info[1] == "PlDd")
            {
                foreach (GameObject obj in UnitManager.Instance.players)
                {
                    if (obj.name == info[0].Substring(0, obj.name.Length))
                    {
                        obj.GetComponent<CharacterStats>().PlayerDead(float.Parse(info[2]), float.Parse(info[3]));
                    }
                }
            }
            else if (info[1] == "HeWl")
            {
                foreach (Wall wall in UnitManager.Instance.walls)
                {
                    Monster.Origin ori = (Monster.Origin)Enum.Parse(typeof(Monster.Origin), info[3], true);
                    if (wall.ID == int.Parse(info[2]) && wall.origin == ori)
                    {
                        wall.RepairWall(float.Parse(info[4]));
                    }
                }
            }
            else if (info[1] == "DmgWl")
            {
                foreach (Wall wall in UnitManager.Instance.walls)
                {
                    Monster.Origin ori = (Monster.Origin)Enum.Parse(typeof(Monster.Origin), info[3], true);
                    if (wall.ID == int.Parse(info[2]) && wall.origin == ori)
                    {
                        wall.DamageWall(float.Parse(info[4]));
                    }
                }
            }
        }
    }

    // Proccess massage that want to be send ---------------------------------------
    private void SendMessageClient(string target, string massage)
    {
        // Massage format : target|header|data|data|data...
        // Target code : 1.All  2.Server  3.All except Sender   others:Specific player name
        string[] temp = new string[1];
        temp[0] = massage;

        SendMessageClient(target, temp);
    }
    private void SendMessageClient(string target, string[] massage)
    {
        // Massage format : target|header|data|data|data...
        // Target code : 1.All  2.Server  3.All except Sender   others:Specific player name

        string data = target;
        foreach (string x in massage)
        {
            data += "|" + x;
        }

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
        return genId.ToString();
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

    // Public method that can be called to send massage to server -------------------
    public void StartMatchmaking()
    {
        SendMessageClient("2", "StMtc");
    }
    public void CreateRoom(string roomName, int maxPlayer, bool isPublic)
    {
        string[] massage = new string[] { "CrR", roomName, maxPlayer.ToString(), isPublic.ToString() } ;
        SendMessageClient("2", massage);
    }
    public void JoinRoom(string roomName)
    {
        string[] massage = new string[] { "JnR", roomName, roomName.ToString() };
        SendMessageClient("2", massage);
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
    public void SpawnMonster(int Id, Monster.Origin origin)
    {
        string[] msg = new string[] { "SwM", Id.ToString(), origin.ToString() };
        SendMessageClient("1", msg);
    }

    public void MovementButtonDown(CharacterController.Button button)
    {
        string[] msg = new string[] { "BtDw", button.ToString() };
        SendMessageClient("1", msg);
    }
    public void MovementButtonUp(CharacterController.Button button)
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

    public void Attack()
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
    public void HealWall(string Id, Monster.Origin origin, float healPoint)
    {
        string[] msg = new string[] { "HeWl", Id.ToString(), origin.ToString(), healPoint.ToString("f2") };
        SendMessageClient("1", msg);
    }
    public void DamageWall(string Id, Monster.Origin origin, float damage)
    {
        string[] msg = new string[] { "DmgWl", Id.ToString(), origin.ToString(), damage.ToString("f2") };
        SendMessageClient("1", msg);
    }
}
