using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMenuManager : MonoBehaviour
{
    // Panels ---------------------------------------------------------------------
    [SerializeField] private GameObject preparationPanel;
    [SerializeField] private GameObject inGameMenuPanel;
    [SerializeField] private GameObject mainPanel;

    // Buttons --------------------------------------------------------------------
    [SerializeField] private GameObject startButton;

    // Text -----------------------------------------------------------------------
    [SerializeField] private Text[] playersName;
    [SerializeField] private Text[] playersStatus;

    // Eazy Access ---------------------------------------------------------------
    public static LobbyMenuManager Instance { get; private set; }
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

    void Start()
    {
        // Setup Ui ---------------------------------------------------------------
        preparationPanel.SetActive(true);
        inGameMenuPanel.SetActive(false);
        mainPanel.SetActive(true);

        startButton.SetActive(false);

        string[] defaultName = { NetworkClient.Instance.myName };
        UpdatePlayersInRoom(defaultName);

        StartCoroutine(CountDownStartButton());
    }

    // Count down for start button to appear -------------------------------------
    private IEnumerator CountDownStartButton()
    {
        yield return new WaitForSeconds(GameManager.Instance.RoomWaitTime);

        if (NetworkClient.Instance.isMaster)
        {
            startButton.SetActive(true);
        }
    }

    // Deactivate Panel -----------------------------------------------------------
    public void SetActivePreparationPanel(bool isTrue)
    {
        preparationPanel.SetActive(isTrue);
    } 
    // Update Players in room -----------------------------------------------------
    public void UpdatePlayersInRoom(string[] names)
    {
        for(int i = 0; i < playersName.Length; i++)
        {
            if(i < names.Length)
            {
                playersName[i].text = names[i];
                playersStatus[i].text = "Ready";
            }
            else
            {
                playersName[i].text = "";
                playersStatus[i].text = "";
            }
        }
    }

    // Exit Room ------------------------------------------------------------------
    public void ExitRoom()
    {
        NetworkClient.Instance.ExitRoom();
    }
}
