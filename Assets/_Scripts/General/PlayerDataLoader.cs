using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class PlayerDataLoader : MonoBehaviour
{
    // Player data ---------------------------------------------------------------------------------------
    public PlayerData TheData { get; set; }

    // Singleton -----------------------------------------------------------------------------------------
    public static PlayerDataLoader Instance { get; private set; }
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        // Load the data
        TheData = LoadPlayerData();
    }

    // For saving game ------------------------------------------------------------------------------------
    public void SavePlayerData()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/survi4s.txt";
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, TheData);
        stream.Close();
    }

    // For load game --------------------------------------------------------------------------------------
    private PlayerData LoadPlayerData()
    {
        string path = Application.persistentDataPath + "/survi4s.txt";

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            PlayerData data = formatter.Deserialize(stream) as PlayerData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.Log("File Not Found");
            PlayerData temp = new PlayerData();
            return temp;
        }
    }
}
