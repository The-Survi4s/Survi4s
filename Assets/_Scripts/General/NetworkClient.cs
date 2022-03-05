using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

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

    // Connection status --------------------------------------------------------------
    private bool isVerified;

    // Singleton ----------------------------------------------------------------------
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
        // Chech connection and verification status ---------------------------------
        if(client.Connected && isVerified)
        {
            // If all ok, begin listening -------------------------------------------
            if (networkStream.DataAvailable)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                ReceiveMassage(formatter.Deserialize(networkStream) as string);
            }

            // Also check connection with server -----------------------------------

        }
    }

    // Receive and Preccess incoming massage here ----------------------------------
    private void ReceiveMassage(string massage)
    {
        // Massage format : sender|header|data|data|data... 
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
            }
            else if (info[1] == "StGm")
            {
                GameManager.Instance.GameStarted();
            }
            else if (info[1] == "SwPy")
            {
                UnitManager.Instance.SpawnPlayer(info[0] ,float.Parse(info[2]), float.Parse(info[3]), int.Parse(info[4]));
            }

        }
    }

    // Proccess massage that want to be send ---------------------------------------
    private void SendMassageClient(string target, string massage)
    {
        // Massage format : target|header|data|data|data...
        // Target code : 1.All  2.Server  3.All except Sender   others:Specific player name
        string[] temp = new string[1];
        temp[0] = massage;

        SendMassageClient(target, temp);
    }
    private void SendMassageClient(string target, string[] massage)
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
        MainMenuManager.Instance.LoadScene(1);
    }
    private void OnJoinedRoom(string roomName, int playerCount)
    {
        playersCount = playerCount;
        MainMenuManager.Instance.LoadScene(1);
    }
    

    // Public method that can be called to send massage to server -------------------
    public void StartMatchmaking()
    {
        SendMassageClient("2", "StMtc");
    }
    public void CreateRoom(string roomName, int maxPlayer, bool isPublic)
    {
        string[] massage = new string[] { "CrR", roomName, maxPlayer.ToString(), isPublic.ToString() } ;
        SendMassageClient("2", massage);
    }
    public void JoinRoom(string roomName)
    {
        string[] massage = new string[] { "JnR", roomName, roomName.ToString() };
        SendMassageClient("2", massage);
    }

    public void StartGame()
    {
        SendMassageClient("1", "StGm");
    }

    public void SpawnPlayer(float x, float y, int skin)
    {
        string[] msg = new string[] { "SwPy", x.ToString("f2"), y.ToString("f2"), skin.ToString() };
        SendMassageClient("1", msg);
    }

    public void MovementButtonDown(CharacterController.Button button)
    {
        string[] msg = new string[] { "BtDw", button.ToString() };
        SendMassageClient("1", msg);
    }
    public void MovementButtonUp(CharacterController.Button button)
    {
        string[] msg = new string[] { "BtUp", button.ToString() };
        SendMassageClient("1", msg);
    }
    public void SendMousePos(float x, float y)
    {
        string[] msg = new string[] { "MPos", x.ToString("f2"), y.ToString("f2") };
        SendMassageClient("1", msg);
    }

    public void Attack(Vector2 mousePos)
    {

    }
}
