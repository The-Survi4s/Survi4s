using System;
using UnityEngine;
using System.Threading.Tasks;

[RequireComponent(typeof(PlayerStats),typeof(PlayerWeaponManager))]
public class Player : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    [field: SerializeField] public bool isLocal { get; private set; }
    [SerializeField] private Animator _animator;
    public int id { get; set; }

    // For player movement -------------------------------------------------------------
    public enum Axis { none, all, x, y }

    // For player facing ---------------------------------------------------------------
    private Camera _mainCamera;
    public Vector3 localMousePos { get; private set; }
    private Vector3 _historyMousePos;
    public Vector3 syncMousePos { get; private set; }
    public bool isFacingLeft { get; private set; }

    // Character Stats -----------------------------------------------------------------
    public PlayerStats stats { get; private set; }
    public PlayerWeaponManager weaponManager { get; private set; }

    public event Action<string> OnPlayerDead;

    // Frame rate sending mouse pos
    [SerializeField] private float _mousePosSendRate = 0.2f;
    private float _mousePosSendCoolDown, _mousePosNextTime;

    // Check for near statue
    [SerializeField] private float _minStatueDist = 3.0f;
    [SerializeField] public bool isNearStatue { get; private set; }

    // Store last direction
    /// <summary>
    /// Player's current move direction. Clamps to (-1, -1) and (1, 1). 
    /// </summary>
    private Vector2 _moveDir;
    /// <summary>
    /// Player's last move direction. Will never become <see cref="Vector2.zero"/> except at the start. 
    /// </summary>
    private Vector2 _lastMoveDir;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
        weaponManager = GetComponent<PlayerWeaponManager>();
    }
    
    private void Start()
    {
        // Tell camera to follow this object --------------------------------------------
        if(name == NetworkClient.Instance.myId + NetworkClient.Instance.myName)
        {
            // Set the camera ----------------------------------------------------------
            _mainCamera = Camera.main;
            CameraController.Instance.SetTargetFollow(transform);
            isLocal = true;
        }

        _historyMousePos = Vector3.zero;
        localMousePos = Vector3.zero;
        syncMousePos = Vector3.zero;
        _moveDir = Vector2.zero;
        _lastMoveDir = Vector2.zero;

        _mousePosSendCoolDown = 1 / _mousePosSendRate;
        _mousePosNextTime = 0;

        stats.OnPlayerDead += HandlePlayerDead;
    }

    private void Update()
    {
        if (isLocal && !isDead)
        {
            // For Movement --------------------------------------------------------
            DetectMovementKeyboard();
            DetectMovementMouse();

            // For equip weapon ----------------------------------------------------
            if(Input.GetKeyDown(KeyCode.F))
            {
                weaponManager.EquipWeapon();
            }

            // For SendAttackMessage ----------------------------------------------------------
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                weaponManager.Attack();
            }

            // Temporary upgrade weapon
            if(Input.GetKeyDown(KeyCode.T) && isNearStatue)
            {
                weaponManager.UpgradeEquipedWeapon();
            }

            // Check statue Pos
            var distanceToStatue = Vector2.Distance(transform.position, TilemapManager.instance.statue.transform.position);
            if (distanceToStatue < _minStatueDist)
            {
                isNearStatue = true;
            }
            else
            {
                isNearStatue = false;
            }

            // Jump wall
            if (Input.GetKeyDown(KeyCode.Space))
            {
                NetworkClient.Instance.Jump();
            }
        }
    }

    public async void Jump()
    {
        //Play animasi jump
        await Task.Delay(1);
        //teleport
        transform.position = TilemapManager.instance.GetJumpPos(transform.position, _lastMoveDir);
    }

    public bool isDead => stats.isDead;

    private void HandlePlayerDead()
    {
        OnPlayerDead?.Invoke(name);
    }

    private void FixedUpdate()
    {
        // Move character based on what button is down ---------------------------------------------
        if(!isDead) _rigidbody.velocity = _moveDir * stats.moveSpeed;

        // Flip character based on mouse position --------------------------------------------------
        if (syncMousePos.x < transform.position.x && !isFacingLeft)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            isFacingLeft = true;
        }
        else if (syncMousePos.x > transform.position.x && isFacingLeft)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            isFacingLeft = false;
        }

        // Send Mouse Position ---------------------------------------------------------------------
        if (isLocal)
        {
            SendMousePos();
        }
    }

    // For detecting input keyboard ------------------------------------------------------
    private void DetectMovementKeyboard()
    {
        Axis axis = Axis.none;
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var verticalInput = Input.GetAxisRaw("Vertical");
        if (horizontalInput != _moveDir.x) axis = Axis.x;
        if (verticalInput != _moveDir.y) axis = Axis.y;
        if (horizontalInput != _moveDir.x && verticalInput != _moveDir.y)
        {
            axis = Axis.all;
        }
        if (axis == Axis.none) return;
        NetworkClient.Instance.SetPlayerVelocity(new Vector2(horizontalInput, verticalInput), axis);
    }

    // For detecting input mouse ---------------------------------------------------------
    private void DetectMovementMouse()
    {
        Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        localMousePos = mouseWorldPos;
    }

    // For moving character ------------------------------------------------------------------

    public void SetVelocity(Vector2 velocity, Axis axis)
    {
        var curVelocity = _rigidbody.velocity;
        switch (axis)
        {
            case Axis.all:
                {
                    curVelocity = velocity;
                    break;
                }
            case Axis.x:
                {
                    curVelocity.x = velocity.x;
                    break;
                }
            case Axis.y:
                {
                    curVelocity.y = velocity.y;
                    break;
                }
        }
        _moveDir = new Vector2(Mathf.Clamp(curVelocity.x, -1, 1), Mathf.Clamp(curVelocity.y, -1, 1));
        if (curVelocity != Vector2.zero)
        {
            _lastMoveDir = _moveDir;
        }
    }

    // For sending mouse position ------------------------------------------------------------
    private void SendMousePos()
    {
        // Need update for better connection
        if (localMousePos != _historyMousePos && Time.time >= _mousePosNextTime)
        {
            _historyMousePos = localMousePos;
            NetworkClient.Instance.SendMousePos(localMousePos.x, localMousePos.y);

            _mousePosNextTime = Time.time + _mousePosSendCoolDown;
        }
    }

    // For Sync mouse pos --------------------------------------------------------------------
    public void SyncMousePos(float x, float y)
    {
        syncMousePos = new Vector3(x, y, 0);
    }

    private void OnDestroy()
    {
        stats.OnPlayerDead -= HandlePlayerDead;
    }
}
