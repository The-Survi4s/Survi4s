using UnityEngine;
using UnityEngine.UI;

public class JoinRoomPanel : MonoBehaviour
{
    // Ui Object -----------------------------------------------------------
    [SerializeField] private InputField inputRoomName;
    [SerializeField] private GameObject roomNotFoundText;

    // Eazy access -----------------------------------------------------------
    public static JoinRoomPanel Instance { get; private set; }
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
    }

    private void Start()
    {
        roomNotFoundText.SetActive(false);
    }

    // Method for joining room ----------------------------------------------------
    public void JoinRoom()
    {
        MainMenuManager.Instance.SetActiveWaitingPanel(true);

        // Send message to server ---------------------------------------------------
        NetworkClient.Instance.JoinRoom(inputRoomName.text);
    }

    // If cannot join --------------------------------------------------------------
    public void RoomNotFound()
    {
        MainMenuManager.Instance.SetActiveWaitingPanel(false);

        roomNotFoundText.GetComponent<Text>().text = "Room Not Found!";
        roomNotFoundText.SetActive(true);
    }
    public void RoomIsFull()
    {
        MainMenuManager.Instance.SetActiveWaitingPanel(false);

        roomNotFoundText.GetComponent<Text>().text = "Room is Full";
        roomNotFoundText.SetActive(true);
    }
}
