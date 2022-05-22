using System;
using UnityEngine;

[RequireComponent(typeof(PlayerStats),typeof(PlayerWeaponManager))]
public class Player : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    [field: SerializeField] public bool isLocal { get; private set; }
    [SerializeField] private Animator _animator;
    public string id { get; set; }

    // For player movement -------------------------------------------------------------
    private bool w_IsDown, a_IsDown, s_IsDown, d_IsDown;
    public enum Button { w, a, s, d }

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
    private Transform _statuePos;
    [SerializeField] private float _minStatueDist = 3.0f;
    [SerializeField] private bool _isNearStatue;

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

        _mousePosSendCoolDown = 1 / _mousePosSendRate;
        _mousePosNextTime = 0;

        stats.OnPlayerDead += HandlePlayerDead;

        _statuePos = UnitManager.Instance.StatuePos;
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
            if(Input.GetKeyDown(KeyCode.T) && _isNearStatue)
            {
                weaponManager.UpgradeEquipedWeapon();
            }

            // Check statue Pos
            var distanceToStatue = Vector2.Distance(transform.position, TilemapManager.instance.statue.transform.position);
            if (distanceToStatue < _minStatueDist)
            {
                _isNearStatue = true;
            }
            else
            {
                _isNearStatue = false;
            }
        }
    }

    public bool isDead => stats.isDead;

    private void HandlePlayerDead()
    {
        OnPlayerDead?.Invoke(id);
    }

    private void FixedUpdate()
    {
        // Move character based on what button is down ---------------------------------------------
        if(!isDead) MoveCharacter();

        // Flip character based on mouse position --------------------------------------------------
        if(syncMousePos.x < transform.position.x && !isFacingLeft)
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
    private static void DetectMovementKeyboard()
    {
        // Detect Keyboard Down -----------------------------------------------------------
        if (Input.GetKeyDown(KeyCode.W))
        {
            NetworkClient.Instance.SetMovementButton(Button.w, true);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            NetworkClient.Instance.SetMovementButton(Button.a, true);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            NetworkClient.Instance.SetMovementButton(Button.s, true);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            NetworkClient.Instance.SetMovementButton(Button.d, true);
        }

        // Detect Keyboard Up -------------------------------------------------------------
        if (Input.GetKeyUp(KeyCode.W))
        {
            NetworkClient.Instance.SetMovementButton(Button.w, false);
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            NetworkClient.Instance.SetMovementButton(Button.a, false);
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            NetworkClient.Instance.SetMovementButton(Button.s, false);
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            NetworkClient.Instance.SetMovementButton(Button.d, false);
        }
    }

    // For detecting input mouse ---------------------------------------------------------
    private void DetectMovementMouse()
    {
        Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        localMousePos = mouseWorldPos;
    }

    // For moving character ------------------------------------------------------------------
    private void MoveCharacter()
    {
        if (w_IsDown || a_IsDown || s_IsDown || d_IsDown)
        {
            _animator.SetBool("isWalk", true);
        }
        else
        {
            _animator.SetBool("isWalk", false);
        }

        float baseSpeed = stats.moveSpeed;
        if (w_IsDown && a_IsDown)
        {
            _rigidbody.velocity = new Vector2(baseSpeed / -2, baseSpeed / 2);
        }
        else if (w_IsDown && d_IsDown)
        {
            _rigidbody.velocity = new Vector2(baseSpeed / 2, baseSpeed / 2);
        }
        else if (s_IsDown && a_IsDown)
        {
            _rigidbody.velocity = new Vector2(baseSpeed / -2, baseSpeed / -2);
        }
        else if (s_IsDown && d_IsDown)
        {
            _rigidbody.velocity = new Vector2(baseSpeed / 2, baseSpeed / -2);
        }
        else if (w_IsDown)
        {
            _rigidbody.velocity = new Vector2(0, baseSpeed);
        }
        else if (a_IsDown)
        {
            _rigidbody.velocity = new Vector2(-baseSpeed, 0);
        }
        else if (s_IsDown)
        {
            _rigidbody.velocity = new Vector2(0, -baseSpeed);
        }
        else if (d_IsDown)
        {
            _rigidbody.velocity = new Vector2(baseSpeed, 0);
        }
        else
        {
            _rigidbody.velocity = new Vector2(0, 0);
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

    // Set Button up/down --------------------------------------------------------------------
    public void SetButton(Button button, bool isDown)
    {
        switch (button)
        {
            case Button.w:
                w_IsDown = isDown;
                break;
            case Button.a:
                a_IsDown = isDown;
                break;
            case Button.s:
                s_IsDown = isDown;
                break;
            case Button.d:
                d_IsDown = isDown;
                break;
        }
    }

    private void OnDestroy()
    {
        stats.OnPlayerDead -= HandlePlayerDead;
    }

    public bool IsNearStatue
    {
        get { return _isNearStatue; }
        private set { _isNearStatue = value; }
    }
}
