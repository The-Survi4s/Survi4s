using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{



    // Eazy access --------------------------------------------------------------------------
    public static GameManager Instance { get; private set; }
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

    // For starting games -------------------------------------------------------------------
    public void StartGame()
    {
        NetworkClient.Instance.StartGame();
    }
    public void GameStarted()
    {
        // Spawn Player ---------------------------------------------------------------------
        NetworkClient.Instance.SpawnPlayer(0, 0, 0);

        // Deactivate Panels ----------------------------------------------------------------
        GameMenuManager.Instance.SetActivePreparationPanel(false);
    }

}
