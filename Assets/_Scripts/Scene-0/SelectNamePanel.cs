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
        // Set new Player name --------------------------------------------------------------
        PlayerDataLoader.Instance.TheData.UserName = inputName.text;
        // Change Name Id -------------------------------------------------------------------
        PlayerDataLoader.Instance.TheData.UserId = NetworkClient.Instance.GeneratePlayerId();

        // Change name UI -------------------------------------------------------------------

        // Tell Server ----------------------------------------------------------------------
        if (NetworkClient.Instance.IsConnected())
        {

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
