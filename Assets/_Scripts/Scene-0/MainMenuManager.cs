using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Panels
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject createRoomPanel;
    [SerializeField] private GameObject joinRoomPanel;
    [SerializeField] private GameObject waitingPanel;
    [SerializeField] private GameObject selectNamePanel;
    [SerializeField] private GameObject connectPanel;

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
        if(PlayerDataLoader.Instance.TheData.UserName == "")
        {
            selectNamePanel.SetActive(true);
            connectPanel.SetActive(false);
        }
        else
        {
            selectNamePanel.SetActive(false);
            connectPanel.SetActive(true);
            NetworkClient.Instance.BeginConnecting();
        }
    }

    // Universal Load Scene ---------------------------------------------------
    public void LoadScene(int SceneId)
    {
        SceneManager.LoadScene(SceneId);
    }

    // Quick Match Button ------------------------------------------------------
    public void Matchmaking()
    {
        waitingPanel.SetActive(true);
        NetworkClient.Instance.StartMatchmaking();
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
}
