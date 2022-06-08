using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameUIManager : MonoBehaviour
{
    private Player _localPlayer;

    public static GameUIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        SetupGameOver();
        SetupLobby();
        SetupHealthUI();
        SetupAmmoUI();
        _countdownPanel.SetActive(false);
        _gameOverPanel.SetActive(false);
        _UWPanel.SetActive(false);
        _damageOverlay.SetActive(false);
        _weaponSelectPanel.SetActive(true);
    }

    private void Update()
    {
        // UI not dependent on LocalPlayer
        UpdateCountdownUI();
        UpdateStatueUI();
        UpdateWaveUI();

        // UI dependent on LocalPlayer
        if (!_localPlayer)
        {
            _localPlayer = UnitManager.Instance.GetPlayer();
            return;
        }
        UpdateHealthUI();
        UpdateAmmoUI();
        UpdateExpUI();
        ShowUpgradePrompt();
    }

    [Header("Health UI")]
    [SerializeField] private RectTransform _healthBar;
    private float _healthBarWidthMax;
    private PlayerStat _stat;

    private void SetupHealthUI()
    {
        _healthBarWidthMax = _healthBar.sizeDelta.x;
    }

    private void UpdateHealthUI()
    {
        if (!_stat)
        {
            _stat = _localPlayer.stats;
            return;
        }
        _healthBar.sizeDelta = new Vector2(_healthBarWidthMax * _stat.hitPoint / _stat.MaxHitPoint, _healthBar.sizeDelta.y);
    }

    [Header("Ammo UI")]
    [SerializeField] private GameObject _ammoUIPrefab;
    [SerializeField] private float _ammoGap;
    [SerializeField] private Vector2 _ammoOffset = new Vector2(-2, 2);
    private List<GameObject> _ammoUIList;
    private WeaponRange _weaponRange;

    private void UpdateAmmoUI()
    {
        var weapon = _localPlayer.weaponManager.weapon;
        if (weapon is WeaponRange wr)
        {
            _weaponRange = wr;
            SetAmmoActive(wr.Ammo);
        }
        else
        {
            SetAmmoActive(0);
        }
    }

    private void SetAmmoActive(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (_ammoUIList.Count > i) _ammoUIList[i].SetActive(true);
            else
            {
                _ammoUIList.Add(Instantiate(_ammoUIPrefab, _weaponSelectPanel.transform));
                var rect = _ammoUIList[_ammoUIList.Count - 1].transform as RectTransform;
                rect.anchorMax = new Vector2(1, 0);
                rect.anchorMin = new Vector2(1, 0);
                rect.pivot = new Vector2(1, 0);
                rect.anchoredPosition = new Vector2(-_ammoGap * i + _ammoOffset.x, _ammoOffset.y);
            }
        }
        if(_ammoUIList.Count > count)
        {
            for (int i = count; i < _ammoUIList.Count; i++)
            {
                _ammoUIList[i].SetActive(false);
            }
        }
    }

    private void SetupAmmoUI()
    {
        _ammoUIList = new List<GameObject>();
    }

    [Header("XP Counter UI")]
    [SerializeField] private Text _currentXpText;
    [SerializeField] private Text _killCountText;
    [SerializeField] private Text _monsterLeftText;

    private void UpdateExpUI()
    {
        _currentXpText.text = _localPlayer.weaponManager.PlayerWeaponExp.ToString();
        _killCountText.text = _localPlayer.KillCount.ToString();
        _monsterLeftText.text = UnitManager.Instance.monsterAliveCount.ToString() + " monsters left";
    }

    [Header("Statue HP UI")]
    [SerializeField] private Text _statueHpText;
    [SerializeField] private Color _fullStatueHpColor;
    [SerializeField] private Color _halfStatueHpColor;
    [SerializeField] private Color _dangerStatueHpColor;

    private void UpdateStatueUI()
    {
        var hp = TilemapManager.instance.statue.hp;
        var maxHp = TilemapManager.instance.statue.maxHp;
        _statueHpText.text = hp.ToString();
        _statueHpText.color = 
            hp > maxHp * 2 / 3.0f ? _fullStatueHpColor : 
            hp > maxHp / 4.0f ? _halfStatueHpColor : 
            _dangerStatueHpColor;
    }

    [Header("Wave Counter UI")]
    [SerializeField] private Text _waveNumberText;

    private void UpdateWaveUI()
    {
        var waveNumber = SpawnManager.instance.currentWave - 1;
        _waveNumberText.text = waveNumber > 0 ? waveNumber.ToString() : "";
    }

    [Header("Wave Countdown UI")]
    [SerializeField] private GameObject _countdownPanel;
    [SerializeField] private Text _countdownText;
    [SerializeField] private float _inactiveDelay;
    [SerializeField] private Color _color1;
    [SerializeField] private Color _color2;
    private float _countdownDuration;
    private float _doneTime;

    private void UpdateCountdownUI()
    {
        if (_countdownPanel.activeInHierarchy)
        {
            var secondsLeft = Mathf.Max(_doneTime - Time.time, 0);
            var hours = TimeSpan.FromSeconds(secondsLeft).Hours;
            var minutes = TimeSpan.FromSeconds(secondsLeft).Minutes;
            var seconds = TimeSpan.FromSeconds(secondsLeft).Seconds;
            _countdownText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
            _countdownText.color = Color.Lerp(_color2, _color1, secondsLeft / _countdownDuration);
            //Debug.Log($"secondsLeft {secondsLeft}, h:{hours}, m:{minutes}, s:{seconds}");
        }
        if (_doneTime + _inactiveDelay < Time.time) _countdownPanel.SetActive(false);
    }

    public void StartWaveCountdown(float duration)
    {
        _countdownDuration = duration;
        _doneTime = _countdownDuration + Time.time;
        _countdownPanel.SetActive(true);
    }

    [Header("Game Over UI")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private Text _nameResultText;
    [SerializeField] private Text _killCountResultText;

    private void SetupGameOver()
    {
        GameManager.GameOver += GameOverEventHandler;
        _nameResultText.text = "";
        _killCountResultText.text = "";
    }

    private void GameOverEventHandler()
    {
        _inGameMenuPanel.SetActive(false);
        _UWPanel.SetActive(false);
        _gameOverPanel.SetActive(true);

        // Tampilkan nama semua player dan score masing2 player
        foreach (var player in UnitManager.Instance.players)
        {
            _nameResultText.text += player.name.Substring(6) + "\n";
            _killCountResultText.text += player.KillCount.ToString() + "\n";
        }
    }

    [Header("Upgrade Weapon UI")]
    [SerializeField] private GameObject _UWPanel;
    [Serializable]
    private struct StatText
    {
        public Text atkText;
        public Text critText;
        public Text cooldownText;
    }
    [Serializable]
    private struct DoubleStatText
    {
        public StatText first;
        public StatText second;
    }
    [SerializeField] private DoubleStatText _statTexts;
    [SerializeField] private Text _costText;
    [SerializeField] private Color _costColorEnough;
    [SerializeField] private Color _costColorNotEnough;
    [SerializeField] private GameObject _weaponSelectPanel;
    [SerializeField] private GameObject _UWButton;
    [SerializeField] private GameObject _UWPrompt;

    private void UpdateUpgradeUI()
    {
        if (_UWPanel.activeInHierarchy)
        {
            var weapon = _localPlayer.weaponManager.weapon;
            _statTexts.first.atkText.text = weapon.baseAttack.ToString();
            _statTexts.first.critText.text = weapon.critRate.ToString();
            _statTexts.first.cooldownText.text = weapon.cooldownTime.ToString();
            _statTexts.second.atkText.text = "+" + (weapon.LevelUpPreview_Atk - weapon.baseAttack).ToString("f2");
            _statTexts.second.critText.text = "+" + (weapon.LevelUpPreview_Crit - weapon.critRate).ToString("f2");
            _statTexts.second.cooldownText.text = (weapon.cooldownTime - weapon.LevelUpPreview_Cooldown).ToString("f2");
            _costText.text = weapon.UpgradeCost.ToString();

            if (_localPlayer.weaponManager.PlayerWeaponExp < weapon.UpgradeCost)
            {
                _costText.color = _costColorNotEnough;
                _UWButton.SetActive(false);
            }
            else
            {
                _costText.color = _costColorEnough;
                _UWButton.SetActive(true);
            }
        }
    }

    public void ShowUpgradePanel(bool isActive)
    {
        if (GameManager.Instance.currentState == GameManager.GameState.GameOver && isActive) return;
        _UWPanel.SetActive(isActive);
        _weaponSelectPanel.SetActive(!isActive);
        if(isActive) UpdateUpgradeUI();
    }

    public void ShowUpgradePrompt()
    {
        _UWPrompt.SetActive(_localPlayer.movement.isNearStatue);
    }

    public void OnClickUpgradeButton()
    {
        _localPlayer.weaponManager.UpgradeEquipedWeapon();
        ShowUpgradePanel(false);
    }

    [Header("Damage Overlay")]
    [SerializeField] private GameObject _damageOverlay;

    public async void ShowDamageOverlay(int milisecondDuration)
    {
        _damageOverlay.SetActive(true);
        await System.Threading.Tasks.Task.Delay(milisecondDuration);
        _damageOverlay.SetActive(false);
    }

    [Header("Weapon UI")]
    [SerializeField] private Image _selectedWeapon;

    public void ChangeWeaponImage(Sprite newSprite)
    {
        _selectedWeapon.sprite = newSprite;
        if (!newSprite) _selectedWeapon.color = new Color(0, 0, 0, 0);
        else _selectedWeapon.color = new Color(255, 255, 255, 255);
    }

    [Header("In Game UI")]
    [SerializeField] private GameObject _inGameMenuPanel;
    [SerializeField] private GameObject _mainPanel;

    public void ExitRoom()
    {
        NetworkClient.Instance.ExitRoom();
    }

    [Header("Lobby UI")]
    [SerializeField] private GameObject _preparationPanel;
    [SerializeField] private GameObject _startButton;
    [SerializeField] private Text[] _playersName;
    [SerializeField] private Text[] _playersStatus;

    void SetupLobby()
    {
        // Setup Ui ---------------------------------------------------------------
        _preparationPanel.SetActive(true);
        _inGameMenuPanel.SetActive(false);
        _mainPanel.SetActive(true);

        _startButton.SetActive(false);

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
            _startButton.SetActive(true);
        }
    }

    // Deactivate Panel -----------------------------------------------------------
    public void SetActivePreparationPanel(bool isTrue)
    {
        _preparationPanel.SetActive(isTrue);
    }
    // Update Players in room -----------------------------------------------------
    public void UpdatePlayersInRoom(string[] names)
    {
        for (int i = 0; i < _playersName.Length; i++)
        {
            if (i < names.Length)
            {
                _playersName[i].text = names[i];
                _playersStatus[i].text = "Ready";
            }
            else
            {
                _playersName[i].text = "";
                _playersStatus[i].text = "";
            }
        }
    }
}
