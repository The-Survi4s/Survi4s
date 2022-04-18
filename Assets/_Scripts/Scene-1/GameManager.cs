using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Statue _statue;

    public enum GameState {StartGame, WavePreparation, WaveSpawn, WaveOver, GameOver}
    private GameState _gameState;

    [SerializeField] private float _preparationTime = 0f;

    [Serializable]
    public struct GameSettings
    {
        public int maxPlayer;
        public int initialStatueHp;
        public int maxWave;

        public GameSettings(int maxPlayer = 4, int initialStatueHp = 20, int maxWave = 100)
        {
            this.maxPlayer = maxPlayer;
            this.initialStatueHp = initialStatueHp;
            this.maxWave = maxWave;
        }
    }

    [field: SerializeField] public GameSettings gameSetting { get; private set; }

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

    private void Update()
    {
        CheckGameOver();
    }

    private void CheckGameOver()
    {
        if (_gameState == GameState.StartGame || _gameState == GameState.GameOver) return;
        if (UnitManager.Instance.PlayerAliveCount <= 0 || _statue.Hp <= 0)
        {
            _statue.PlayDestroyedAnimation();
            ChangeState(GameState.GameOver);
        }
    }

    public void ChangeState(GameState newState)
    {
        Debug.Log(newState);
        _gameState = newState;
        switch (newState)
        {
            case GameState.StartGame:
                HandleStartGame();
                break;
            case GameState.WavePreparation:
                HandleWavePreparation();
                break;
            case GameState.WaveSpawn:
                HandleWaveSpawn();
                break;
            case GameState.WaveOver:
                HandleWaveOver();
                break;
            case GameState.GameOver:
                HandleGameOver();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    private void HandleGameOver()
    {
        // Show game over screen with score and disconnect button
        // Ke scene 0
    }

    private async void HandleWaveOver()
    {
        //Wait enemy habis
        while (UnitManager.Instance.MonsterAliveCount > 0)
        {
            await Task.Yield();
        }
        ChangeState(GameState.WavePreparation);
    }

    private async void HandleWaveSpawn()
    {
        await SpawnManager.instance.StartWave();
        ChangeState(GameState.WaveOver);
    }

    private async void HandleWavePreparation()
    {
        // Start countdown timer
        await CountDown(_preparationTime);
        // On countdown done, or on some button pressed, wave spawn
        ChangeState(GameState.WaveSpawn);
    }

    private async Task CountDown(float time)
    {
        var end = Time.time + time;
        while (Time.time < end)
        {
            DoStuffWhileCountDown();
            await Task.Yield();
        }
    }

    private void DoStuffWhileCountDown()
    {
        // Tampilkan timer, update timer
        //Debug.Log("Countdown");
    }

    private void HandleStartGame()
    {
        //Lakukan sesuatu

        // Display statue HP dan UI lain

        GameStarted();
        ChangeState(GameState.WavePreparation); 
    }

    // For starting games -------------------------------------------------------------------
    public void StartGame()
    {
        NetworkClient.Instance.StartGame();
        NetworkClient.Instance.LockTheRoom();
    }
    private void GameStarted()
    {
        // Spawn Player ---------------------------------------------------------------------
        UnitManager.Instance.SendSpawnPlayer(0, 0, 0);

        // Deactivate Panels ----------------------------------------------------------------
        GameMenuManager.Instance.SetActivePreparationPanel(false);
    }

}
