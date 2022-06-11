using UnityEngine;
using UnityEngine.UI;

public class SelectNamePanel : MonoBehaviour
{
    [SerializeField] private InputField inputName;
    [SerializeField] private GameObject confirmButton;
    [SerializeField] private GameObject backButton;

    private void Start()
    {
        confirmButton.SetActive(false);

        if (PlayerDataLoader.Instance.TheData.UserName == "")
        {
            backButton.SetActive(false);
        }
        else
        {
            backButton.SetActive(true);
        }
    }

    public void SelectName()
    {
        string nameTemp, IdTemp;
        // Set new Player name --------------------------------------------------------------
        PlayerDataLoader.Instance.TheData.UserName = inputName.text;
        nameTemp = inputName.text;
        inputName.text = "";
        // Change Name Id -------------------------------------------------------------------
        PlayerDataLoader.Instance.TheData.UserId = NetworkClient.Instance.GeneratePlayerId();
        IdTemp = PlayerDataLoader.Instance.TheData.UserId;

        // Save new name and id
        PlayerDataLoader.Instance.SavePlayerData();

        // Change name UI -------------------------------------------------------------------
        MainMenuManager.Instance.UpdateName(nameTemp);

        // Tell Server ----------------------------------------------------------------------
        if (NetworkClient.Instance.IsConnected)
        {
            NetworkClient.Instance.ChangeName(IdTemp, nameTemp);
        }
        else
        {
            // If not connected yet, begin connection
            NetworkClient.Instance.BeginConnecting();
        }
        

        // Save Player Data -----------------------------------------------------------------
        PlayerDataLoader.Instance.SavePlayerData();

        gameObject.SetActive(false);
    }

    // Check name character size
    public void MaxMinName()
    {
        if (inputName.text.Length < 3 || inputName.text.Length > 10)
        {
            confirmButton.SetActive(false);
        }
        else
        {
            confirmButton.SetActive(true);
        }
    }
}
