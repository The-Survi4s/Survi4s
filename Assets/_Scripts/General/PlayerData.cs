
[System.Serializable]
public class PlayerData
{
    // Contain player personal data --------------------------------------------
    public string UserName { get; set; }
    public string UserId { get; set; }

    // Constructor -------------------------------------------------------------
    public PlayerData()
    {
        UserName = "";
        UserId = "";
    }
}
