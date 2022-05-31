using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    // Panels
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject createRoomPanel;
    [SerializeField] private GameObject joinRoomPanel;
    [SerializeField] private GameObject waitingPanel;
    [SerializeField] private GameObject selectNamePanel;
    [SerializeField] private GameObject connectPanel;
    [SerializeField] private GameObject exitPanel;

    public Text nameText;

    // Eazy Access -------------------------------------------------------------------
    public static MainMenuManager Instance { get; private set; }
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
        // Set all panels -------------------------------------------------------------
        mainPanel.SetActive(true);
        waitingPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        joinRoomPanel.SetActive(false);
        exitPanel.SetActive(false);
        if(PlayerDataLoader.Instance.TheData.UserName == "")
        {
            selectNamePanel.SetActive(true);
            connectPanel.SetActive(false);
        }
        else if (!NetworkClient.Instance.IsConnected())
        {
            selectNamePanel.SetActive(false);
            connectPanel.SetActive(true);
            NetworkClient.Instance.BeginConnecting();
        }
        else
        {
            selectNamePanel.SetActive(false);
            connectPanel.SetActive(false);
        }

        nameText.text = PlayerDataLoader.Instance.TheData.UserName;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(!createRoomPanel.activeSelf && !joinRoomPanel.activeSelf && 
                !waitingPanel.activeSelf && !selectNamePanel.activeSelf)
            {
                exitPanel.SetActive(true);
            }
        }
    }

    // Quick Match Button ------------------------------------------------------
    public void Matchmaking()
    {
        waitingPanel.SetActive(true);
        NetworkClient.Instance.StartMatchmaking();
    }

    // Select Name -------------------------------------------------------------
    public void SelectName()
    {
        selectNamePanel.SetActive(true);
    }

    // Setting Panels -----------------------------------------------------------
    public void SetActiveWaitingPanel(bool isTrue)
    {
        waitingPanel.SetActive(isTrue);
    }
    public void SetActiveConnectingPanel(bool isTrue)
    {
        connectPanel.SetActive(isTrue);
    }

    public void ExitGames()
    {
        Application.Quit();
    }
}
