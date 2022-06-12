using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private AudioManager audioManager;

    public enum GameState {InLobby, StartGame, WavePreparation, WaveSpawn, WaveOver, GameOver}
    private GameState _gameState;
    public GameState currentState => _gameState;
    private int _playerLayer;
    private int _monsterLayer;
    private int _monsterBulletLayer;
    private int _playerBulletLayer;
    private int _groundLayer;
    private int _wallLayer;
    public float preparationDoneTime { get; private set; }
    public float RoomWaitTime => _roomWaitTime;
    [SerializeField] private float _roomWaitTime = 2;

    public static event Action GameOver;

    [Serializable]
    public struct GameSetting
    {
        public float preparationTime;
        public int initialStatueHp;
        public int maxWave;

        public GameSetting(float preparationTime, int initialStatueHp, int maxWave)
        {
            this.preparationTime = preparationTime;
            this.initialStatueHp = initialStatueHp;
            this.maxWave = maxWave;
        }
    }

    [field: SerializeField] public GameSetting Settings { get; private set; }

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

        _playerLayer = LayerMask.GetMask("Player");
        _monsterLayer = LayerMask.GetMask("Enemy");
        _monsterBulletLayer = LayerMask.GetMask("EnemyBullet");
        _playerBulletLayer = LayerMask.GetMask("PlayerBullet");
        _groundLayer = LayerMask.GetMask("Ground");
        _wallLayer = LayerMask.GetMask("Wall");

        audioManager = GetComponent<AudioManager>();
    }

    private void Update()
    {
        CheckGameOver();
    }

    private void CheckGameOver()
    {
        //If uninitialized return
        if (!Ready() || 
            !(_gameState == GameState.WaveSpawn || _gameState == GameState.WaveOver)) return;
        if (UnitManager.Instance.playerAliveCount <= 0 || TilemapManager.Instance.statue.hp <= 0)
        {
            ChangeState(GameState.GameOver);
        }
    }

    private bool Ready()
    {
        return (
            UnitManager.Instance.PlayerInitializedCount == UnitManager.Instance.playerCount &&
            UnitManager.Instance.playerCount > 0 &&
            TilemapManager.Instance.statue.IsInitialized);
    }

    public void ChangeState(GameState newState)
    {
        Debug.Log(_gameState + " -> " + newState);
        if (_gameState == newState) return;
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
        audioManager.Stop("Relax");
        audioManager.Stop("Intense");
        audioManager.Play("GameOver");

        TilemapManager.Instance.statue.PlayDestroyedAnimation();
        // Broadcast game over
        GameOver?.Invoke();
    }

    private async void HandleWaveOver()
    {
        //Wait enemy habis
        while (UnitManager.Instance.monsterAliveCount > 0)
        {
            await Task.Yield();
        }

        ChangeState(SpawnManager.Instance.currentWave < Settings.maxWave
            ? GameState.WavePreparation
            : GameState.GameOver);
    }

    private async void HandleWaveSpawn()
    {
        audioManager.Play("Intense");
        audioManager.Stop("Relax");

        await SpawnManager.Instance.StartWave();
        ChangeState(GameState.WaveOver);
    }

    private async void HandleWavePreparation()
    {
        // Start countdown timer
        await CountDown(Settings.preparationTime);
        // On countdown done, or on some button pressed, wave spawn
        ChangeState(GameState.WaveSpawn);
    }

    private async Task CountDown(float duration)
    {
        preparationDoneTime = Time.time + duration;
        GameUIManager.Instance.StartWaveCountdown(duration);
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
        // Display statue HP dan UI lain

        // Spawn Player ---------------------------------------------------------------------
        SpawnManager.Instance.SendSpawnPlayer();

        // Deactivate Panels ----------------------------------------------------------------
        GameUIManager.Instance.SetActivePreparationPanel(false);
        
        SetIgnoreCollisions();

        GameUIManager.Instance.ChangeWeaponImage(null);
        ChangeState(GameState.WavePreparation);

        audioManager.Play("Relax");
    }

    private void SetIgnoreCollisions()
    {
        Physics2D.IgnoreLayerCollision(Log2(_playerLayer), Log2(_monsterLayer)); 
        Physics2D.IgnoreLayerCollision(Log2(_groundLayer), Log2(_monsterLayer)); 
        Physics2D.IgnoreLayerCollision(Log2(_groundLayer), Log2(_playerBulletLayer)); 
        Physics2D.IgnoreLayerCollision(Log2(_playerLayer), Log2(_playerBulletLayer)); 
        Physics2D.IgnoreLayerCollision(Log2(_monsterLayer), Log2(_monsterBulletLayer)); 
        Physics2D.IgnoreLayerCollision(Log2(_playerBulletLayer), Log2(_wallLayer));
        Physics2D.IgnoreLayerCollision(Log2(_playerBulletLayer), Log2(_monsterBulletLayer));
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
        await SpawnManager.Instance.StartWave();
    }

    private int Log2(int a) => (int)Mathf.Log(a, 2);
}
