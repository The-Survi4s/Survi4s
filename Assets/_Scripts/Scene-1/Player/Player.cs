using System;
using UnityEngine;
using System.Threading.Tasks;

[RequireComponent(typeof(PlayerStat), typeof(PlayerWeaponManager), typeof(PlayerMovement))]
public class Player : MonoBehaviour
{
    [field: SerializeField] public bool isLocal { get; private set; }
    public int id { get; set; }

    // Character Stats -----------------------------------------------------------------
    public PlayerStat stats { get; private set; }
    public PlayerWeaponManager weaponManager { get; private set; }
    public PlayerMovement movement { get; private set; }
    private SpriteRenderer _renderer;
    public bool isFacingLeft { get => _renderer.flipX; }

    [SerializeField] private int _killCount;
    public int KillCount
    {
        get => _killCount;
        private set => _killCount = value;
    }

    private void Awake()
    {
        stats = GetComponent<PlayerStat>();
        weaponManager = GetComponent<PlayerWeaponManager>();
        movement = GetComponent<PlayerMovement>();
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Tell camera to follow this object --------------------------------------------
        if (name == NetworkClient.Instance.myId + NetworkClient.Instance.myName)
        {
            CameraController.Instance.SetTargetFollow(transform);
            isLocal = true;
        }
    }

    private void Update()
    {
        if (isLocal && !isDead)
        {
            // For equip weapon ----------------------------------------------------
            if (Input.GetKeyDown(KeyCode.F))
            {
                weaponManager.EquipWeapon();
            }

            // For SendAttackMessage ----------------------------------------------------------
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                weaponManager.Attack();
            }

            // Temporary upgrade weapon
            if (Input.GetKeyDown(KeyCode.T) && movement.isNearStatue)
            {
                GameUIManager.Instance.ShowUpgradePanel(true);
            }
            if(!movement.isNearStatue) GameUIManager.Instance.ShowUpgradePanel(false);

            _renderer.flipX = movement.syncMousePos.x < transform.position.x;
        }
    }

    public bool isDead => stats.isDead;

    public void AddKillCount()
    {
        KillCount++;
    }
}
