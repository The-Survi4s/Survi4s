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
    private Vector3 _historyMousePos;
    public Vector3 syncMousePos { get; private set; }

    // Frame rate sending mouse pos
    [SerializeField] private float _mousePosSendRate = 10;
    private float _mousePosSendCoolDown, _mousePosNextTime;

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

    private void Awake()
    {
        player = GetComponent<Player>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _historyMousePos = Vector3.zero;
        localMousePos = Vector3.zero;
        syncMousePos = Vector3.zero;
        _moveDir = Vector2.zero;
        _lastMoveDir = Vector2.zero;

        _mousePosSendCoolDown = 1 / _mousePosSendRate;
        _mousePosNextTime = 0;
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
        }

        if(_rigidbody.velocity == Vector2.zero)
        {
            player.animator.SetBool("isWalk", false);

            GetComponent<AudioManager>().Stop("Walk");
        }
        else
        {
            player.animator.SetBool("isWalk", true);

            GetComponent<AudioManager>().Play("Walk");            
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
            NetworkClient.Instance.SendMousePos(localMousePos);

            _mousePosNextTime = Time.time + _mousePosSendCoolDown;
        }
    }

    // For Sync mouse pos --------------------------------------------------------------------
    public void SyncMousePos(float x, float y)
    {
        syncMousePos = new Vector3(x, y, 0);
    }
}
