using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class PlayerMovement : MonoBehaviour
{
    //Otw refactor Player
    private Player player;
    private Rigidbody2D _rigidbody;
    // For player movement -------------------------------------------------------------
    public enum Axis { none, all, x, y }
    // For player facing ---------------------------------------------------------------
    private Camera _mainCamera;

    public Vector3 localMousePos { get; private set; }
    private Vector3 _lastMousePos;
    public Vector3 syncedMousePos { get; private set; }
    [SerializeField, Min(0)] private float _mousePosSyncRate = 10;
    [SerializeField, Min(0)] private float _maxMouseDistDiff = 0.2f;
    private float _mousePosSendCooldown, _mousePosNextTime;

    [SerializeField, Min(0)] private float _positionSyncRate = 10;
    [SerializeField, Min(0)] private float _maxDistanceDifference = 1;
    private float _posSendCooldown, _posNextTime;
    private Vector3 _lastPosition;

    // Check for near statue
    [SerializeField] private float _minStatueDist = 3.0f;
    [field: SerializeField] public bool isNearStatue { get; private set; }

    // Store last direction
    /// <summary>
    /// Player's current move direction. Clamps to (-1, -1) and (1, 1). 
    /// </summary>
    private Vector2 _moveDir;
    /// <summary>
    /// Player's last move direction. Will never become <see cref="Vector2.zero"/> except at the start. 
    /// </summary>
    private Vector2 _lastMoveDir;

    private AudioManager _audioManager;

    private void Awake()
    {
        player = GetComponent<Player>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _audioManager = GetComponent<AudioManager>();
    }

    private void Start()
    {
        _mousePosSendCooldown = 1 / _mousePosSyncRate;
        _posSendCooldown = 1 / _positionSyncRate;
    }

    private void Update()
    {
        if (player.isLocal && !player.isDead)
        {
            if (!_mainCamera) _mainCamera = Camera.main;

            // For Movement --------------------------------------------------------
            DetectMovementKeyboard();
            DetectMovementMouse();

            // Jump wall
            if (Input.GetKeyDown(KeyCode.Space))
            {
                NetworkClient.Instance.Jump();
            }
        }

        // Check statue Pos
        var distanceToStatue = Vector2.Distance(transform.position, TilemapManager.Instance.statue.transform.position);
        if (distanceToStatue < _minStatueDist) isNearStatue = true;
        else isNearStatue = false;
    }

    public async void Jump()
    {
        //Play animasi jump
        await Task.Delay(1);
        //teleport
        transform.position = TilemapManager.Instance.GetJumpPos(transform.position, _lastMoveDir);

        player.animator.SetTrigger("Jump");
    }

    private void FixedUpdate()
    {
        // Move character based on what button is down ---------------------------------------------
        if (!player.isDead) _rigidbody.velocity = _moveDir * player.stats.moveSpeed;
        else _rigidbody.velocity = _moveDir * 0;

        // Send Mouse Position ---------------------------------------------------------------------
        if (player.isLocal)
        {
            SendMousePos();
            SendPosition();
        }

        SetAnimation(_rigidbody.velocity != Vector2.zero);
    }

    private void SetAnimation(bool isWalking)
    {
        player.animator.SetBool("isWalk", isWalking);
        if (isWalking) _audioManager.Play("Walk");
        else _audioManager.Stop("Walk");
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
        CameraController.Instance.SetTargetDir(new Vector2(horizontalInput, verticalInput));
        NetworkClient.Instance.SetPlayerVelocity(new Vector2(horizontalInput, verticalInput), axis);
    }

    private void SendPosition()
    {
        if (Vector3.Distance(_lastPosition, transform.position) > _maxDistanceDifference && Time.time >= _posNextTime)
        {
            _lastPosition = transform.position;
            NetworkClient.Instance.SyncPlayerPos(transform.position);

            _posNextTime = Time.time + _posSendCooldown;
        }
    }

    public void SetPosition(Vector2 pos) => transform.position = pos;

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
        if (Vector3.Distance(localMousePos, _lastMousePos) > _maxMouseDistDiff && Time.time >= _mousePosNextTime)
        {
            _lastMousePos = localMousePos;
            NetworkClient.Instance.SendMousePos(localMousePos);

            _mousePosNextTime = Time.time + _mousePosSendCooldown;
        }
    }

    // For Sync mouse pos --------------------------------------------------------------------
    public void SyncMousePos(float x, float y)
    {
        syncedMousePos = new Vector3(x, y, 0);
    }
}
