using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState {StartGame, WavePreparation, WaveSpawn, WaveOver, GameOver}
    private GameState _gameState;
    public float preparationDoneTime { get; private set; }

    [Serializable]
    public struct GameSettings
    {
        public float preparationTime;
        public int initialStatueHp;
        public int maxWave;

        public GameSettings(float preparationTime, int initialStatueHp, int maxWave)
        {
            this.preparationTime = preparationTime;
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
        if (_gameState == GameState.StartGame || 
            _gameState == GameState.GameOver || 
            _gameState == GameState.WavePreparation || 
            UnitManager.Instance.playerCount <= 0) return;
        if (UnitManager.Instance.playerAliveCount <= 0 || TilemapManager.instance.statue.hp <= 0)
        {
            TilemapManager.instance.statue.PlayDestroyedAnimation();
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
        // disable player movement
        // Show game over screen with score and disconnect button
        // Ke scene 0
    }

    private async void HandleWaveOver()
    {
        //Wait enemy habis
        while (UnitManager.Instance.monsterAliveCount > 0)
        {
            await Task.Yield();
        }

        ChangeState(SpawnManager.instance.currentWave < gameSetting.maxWave
            ? GameState.WavePreparation
            : GameState.GameOver);
    }

    private async void HandleWaveSpawn()
    {
        await SpawnManager.instance.StartWave();
        ChangeState(GameState.WaveOver);
    }

    private async void HandleWavePreparation()
    {
        // Start countdown timer
        await CountDown(gameSetting.preparationTime);
        // On countdown done, or on some button pressed, wave spawn
        ChangeState(GameState.WaveSpawn);
    }

    private async Task CountDown(float time)
    {
        preparationDoneTime = Time.time + time;
        while (Time.time < preparationDoneTime)
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

        // Spawn Player ---------------------------------------------------------------------
        SpawnManager.instance.SendSpawnPlayer();

        // Deactivate Panels ----------------------------------------------------------------
        GameMenuManager.Instance.SetActivePreparationPanel(false);

        Physics2D.IgnoreLayerCollision(3, 6); //Supaya player tidak collision dengan monster
        Physics2D.IgnoreLayerCollision(8, 6); 
        Physics2D.IgnoreLayerCollision(8, 9); 
        ChangeState(GameState.WavePreparation); 
    }

    // Button hooks -------------------------------------------------------------------
    public void StartGame()
    {
        NetworkClient.Instance.StartGame();
        NetworkClient.Instance.LockTheRoom();
    }

    public async void ForceStartNextWave()
    {
        if(_gameState != GameState.WavePreparation) return;
        await SpawnManager.instance.StartWave();
    }
}
