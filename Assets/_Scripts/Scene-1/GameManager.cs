using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SpawnManager spawnManager;

    public enum GameState {StartGame, WavePreparation, WaveSpawn, WaveOver, GameOver}

    private GameState gameState;

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
        ChangeState(GameState.StartGame);
    }

    public void ChangeState(GameState newState)
    {
        switch (newState)
        {
            case GameState.StartGame:
                HandleStartGame();
                break;
        }
    }

    private void HandleStartGame()
    {
        //Lakukan sesuatu
        ChangeState(GameState.WavePreparation); //Cuma contoh
    }

    // For starting games -------------------------------------------------------------------
    public void StartGame()
    {
        NetworkClient.Instance.StartGame();
        NetworkClient.Instance.LockTheRoom();
    }
    public void GameStarted()
    {
        // Spawn Player ---------------------------------------------------------------------
        UnitManager.Instance.SendSpawnPlayer(0, 0, 0);

        // Deactivate Panels ----------------------------------------------------------------
        GameMenuManager.Instance.SetActivePreparationPanel(false);

        // Spawn Monster
        SpawnManager.instance.StartWave();
    }

}
