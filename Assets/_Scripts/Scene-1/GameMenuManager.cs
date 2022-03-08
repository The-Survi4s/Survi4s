using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuManager : MonoBehaviour
{
    // Panels ---------------------------------------------------------------------
    [SerializeField] private GameObject preparationPanel;
    [SerializeField] private GameObject inGameMenuPanel;
    [SerializeField] private GameObject mainPanel;

    // Buttons --------------------------------------------------------------------
    [SerializeField] private GameObject startButton;

    // Text -----------------------------------------------------------------------
    [SerializeField] private Text playersInRoom;

    // Eazy Access ---------------------------------------------------------------
    public static GameMenuManager Instance { get; private set; }
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

        playersInRoom.text = NetworkClient.Instance.playersCount.ToString();

        StartCoroutine(CountDownStartButton());
    }

    // Count down for start button to appear -------------------------------------
    private IEnumerator CountDownStartButton()
    {
        yield return new WaitForSeconds(2);

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

    // Exit Room ------------------------------------------------------------------
    public void ExitRoom()
    {
        NetworkClient.Instance.ExitRoom();
    }
}
