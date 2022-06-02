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

        // UI dependent on LocalPlayer
        if (!_localPlayer)
        {
            _localPlayer = UnitManager.Instance.GetPlayer();
            return;
        }
        UpdateHealthUI();
        UpdateAmmoUI();
        UpdateCounterUI();
    }

    [Header("Health UI")]
    [SerializeField] private Text _currentHpText;
    [SerializeField] private Text _maxHpText;
    private PlayerStat _stat;

    private void UpdateHealthUI()
    {
        if (!_stat)
        {
            _stat = _localPlayer.stats;
            return;
        }
        _currentHpText.text = _stat.hitPoint.ToString();
        _maxHpText.text = _stat.MaxHitPoint.ToString();
    }

    [Header("Ammo UI")]
    [SerializeField] private Text _currentAmmoText;
    [SerializeField] private Text _maxAmmoText;
    private WeaponRange _weaponRange;

    private void UpdateAmmoUI()
    {
        if (!_weaponRange)
        {
            var weapon = _localPlayer.weaponManager.weapon;
            if (weapon is WeaponRange wr) _weaponRange = wr;
            else
            {
                _currentAmmoText.text = "-";
                _maxAmmoText.text = "-";
            }
            return;
        }

        _currentAmmoText.text = _weaponRange.Ammo.ToString();
        _maxAmmoText.text = _weaponRange.MaxAmmo.ToString();
    }

    [Header("Counter UI")]
    [SerializeField] private Text _currentXpText;
    [SerializeField] private Text _killCountText;

    private void UpdateCounterUI()
    {
        _currentXpText.text = _localPlayer.weaponManager.PlayerWeaponExp.ToString();
        _killCountText.text = _localPlayer.KillCount.ToString();
    }

    [Header("Wave Countdown UI")]
    [SerializeField] private GameObject _countdownPanel;
    [SerializeField] private Text _countdownText;
    [SerializeField] private float _inactiveDelay;
    [SerializeField] private Color _color1;
    [SerializeField] private Color _color2;
    private float _countdownDuration;

    private void UpdateCountdownUI()
    {
        var doneTime = _countdownDuration + Time.time;
        if (_countdownPanel.activeInHierarchy)
        {
            var secondsLeft = Mathf.Min(doneTime - Time.time, 0);
            var hours = TimeSpan.FromSeconds(secondsLeft).Hours;
            var minutes = TimeSpan.FromSeconds(secondsLeft).Minutes;
            var seconds = TimeSpan.FromSeconds(secondsLeft).Seconds;
            _countdownText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
            _countdownText.color = Color.Lerp(_color2, _color1, secondsLeft / _countdownDuration);
        }
        if (doneTime + _inactiveDelay < Time.time) _countdownPanel.SetActive(false);
    }

    public void StartWaveCountdown(float duration)
    {
        _countdownDuration = duration;
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
        _gameOverPanel.SetActive(true);

        // Tampilkan nama semua player dan score masing2 player
        foreach (var player in UnitManager.Instance.players)
        {
            _nameResultText.text += player.name + "\n";
            _killCountResultText.text += player.KillCount.ToString() + "\n";
        }
    }

    [Header("Upgrade Weapon UI")]
    [SerializeField] private GameObject _UWPanel;
    [Serializable]
    private struct StatText
    {
        public Text levelText;
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
    [SerializeField] private GameObject _weaponSelectPanel;

    private void UpdateUpgradeUI()
    {
        if (_UWPanel.activeInHierarchy)
        {
            var weapon = _localPlayer.weaponManager.weapon;
            _statTexts.first.atkText.text = weapon.baseAttack.ToString();
            _statTexts.first.critText.text = weapon.critRate.ToString();
            _statTexts.first.cooldownText.text = weapon.cooldownTime.ToString();
            _statTexts.second.atkText.text = "+" + (weapon.LevelUpPreview_Atk - weapon.baseAttack).ToString();
            _statTexts.second.critText.text = "+" + (weapon.LevelUpPreview_Crit - weapon.critRate).ToString();
            _statTexts.second.cooldownText.text = "+" + (weapon.LevelUpPreview_Cooldown - weapon.cooldownTime).ToString();
            _costText.text = weapon.UpgradeCost.ToString();
        }
    }

    public void ShowUpgradePanel(bool isActive)
    {
        _UWPanel.SetActive(isActive);
        _weaponSelectPanel.SetActive(!isActive);
        if(isActive) UpdateUpgradeUI();
    }

    public void OnClickUpgradeButton()
    {
        _localPlayer.weaponManager.UpgradeEquipedWeapon();
        ShowUpgradePanel(false);
    }

    [Header("Damage Overlay")]
    [SerializeField] private GameObject _damageOverlay;

    public async void ShowDamageOverlay(float duration)
    {
        _damageOverlay.SetActive(true);
        await System.Threading.Tasks.Task.Delay(500);
        _damageOverlay.SetActive(false);
    }

    [Header("Weapon UI")]
    [SerializeField] private Image _selectedWeapon;

    public void ChangeWeaponImage(Sprite newSprite) => _selectedWeapon.sprite = newSprite;
}
