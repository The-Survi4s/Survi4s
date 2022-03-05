using UnityEngine;
using UnityEngine.UI;

public class CreateRoomPanel : MonoBehaviour
{
    // Ui obeject
    [SerializeField] private InputField inputRoomName;
    [SerializeField] private Text maxPlayerText;
    [SerializeField] private Text visibilityText;
    [SerializeField] private GameObject confirmButton;

    // Variable
    [SerializeField] private int maxRoomNameLength;
    [SerializeField] private int minRoomNameLength;
    [SerializeField] private int maxPlayer;
    [SerializeField] private int minPlayer;
    private bool isPublic;

    private void Start()
    {
        confirmButton.SetActive(false);

        maxPlayerText.text = maxPlayer.ToString();
        visibilityText.text = "Private";
        isPublic = false;
    }

    // Check room name length
    public void MaxMinRoomName()
    {
        if (inputRoomName.text.Length < minRoomNameLength || inputRoomName.text.Length > maxRoomNameLength)
        {
            confirmButton.SetActive(false);
        }
        else
        {
            confirmButton.SetActive(true);
        }
    }

    // Changing value of max player in room
    public void IncreaseMaxPlayer()
    {
        int temp = int.Parse(maxPlayerText.text);
        if (temp < maxPlayer)
        {
            temp++;
            maxPlayerText.text = temp.ToString();
        }
    }
    public void DecreaseMaxPlayer()
    {
        int temp = int.Parse(maxPlayerText.text);
        if (temp > minPlayer)
        {
            temp--;
            maxPlayerText.text = temp.ToString();
        }
    }

    // Change room visibility
    public void ChangeVisibility()
    {
        if (isPublic)
        {
            visibilityText.text = "Private";
            isPublic = false;
        }
        else
        {
            visibilityText.text = "Public";
            isPublic = true;
        }
    }

    // Method for creating room based on user input
    public void CreateRoom()
    {
        MainMenuManager.Instance.SetActiveWaitingPanel(true);
        // Room name, Max player in room, visibility
        NetworkClient.Instance.CreateRoom(inputRoomName.text, int.Parse(maxPlayerText.text), isPublic);
    }
}
