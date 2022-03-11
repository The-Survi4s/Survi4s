using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rigidbody;
    public bool isLocal { get; private set; }

    // For player movement -------------------------------------------------------------
    private bool wIsDown, aIsDown, sIsDown, dIsDown;
    public enum Button { w, a, s, d }

    // For player facing ---------------------------------------------------------------
    private Camera mainCamera;
    public Vector3 localMousePos { get; private set; }
    private Vector3 historyMousePos;
    public Vector3 syncMousePos { get; private set; }
    public bool isFacingLeft { get; private set; }

    // Character Stats -----------------------------------------------------------------
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private CharacterWeapon characterWeapon;

    // Frame rate sending mouse pos
    [SerializeField] private float mousePosSendRate;
    private float mousePosSendCoolDown, mousePosNextTime;

    private void Start()
    {
        // Tell camera to follow this object --------------------------------------------
        if(name == NetworkClient.Instance.myId + NetworkClient.Instance.myName)
        {
            // Set the camera ----------------------------------------------------------
            mainCamera = Camera.main;
            CameraController.Instance.SetTargetFollow(this.gameObject.transform);
            isLocal = true;
        }

        historyMousePos = new Vector3(0, 0, 0);
        localMousePos = new Vector3(0, 0, 0);
        syncMousePos = new Vector3(0, 0, 0);

        mousePosSendCoolDown = 1 / mousePosSendRate;
        mousePosNextTime = 0;
    }

    private void Update()
    {
        if (isLocal)
        {
            // For Movement --------------------------------------------------------
            DetectMovementKeyboard();
            DetectMovementMouse();

            // For equip weapon ----------------------------------------------------
            if(Input.GetKeyDown(KeyCode.F))
            {
                characterWeapon.EquipWeapon();
            }

            // For Attack ----------------------------------------------------------
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                characterWeapon.Attack();
            }
        }
    }

    private void FixedUpdate()
    {
        // Move character based on what button is down ---------------------------------------------
        MoveCharacter();

        // Flip character based on mouse position --------------------------------------------------
        if(syncMousePos.x < transform.position.x && !isFacingLeft)
        {
            gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
            isFacingLeft = true;
        }
        else if (syncMousePos.x > transform.position.x && isFacingLeft)
        {
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
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
        // Detect Keyboard Down -----------------------------------------------------------
        if (Input.GetKeyDown(KeyCode.W))
        {
            NetworkClient.Instance.MovementButtonDown(Button.w);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            NetworkClient.Instance.MovementButtonDown(Button.a);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            NetworkClient.Instance.MovementButtonDown(Button.s);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            NetworkClient.Instance.MovementButtonDown(Button.d);
        }

        // Detect Keyboard Up -------------------------------------------------------------
        if (Input.GetKeyUp(KeyCode.W))
        {
            NetworkClient.Instance.MovementButtonUp(Button.w);
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            NetworkClient.Instance.MovementButtonUp(Button.a);
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            NetworkClient.Instance.MovementButtonUp(Button.s);
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            NetworkClient.Instance.MovementButtonUp(Button.d);
        }
    }

    // For detecting input mouse ---------------------------------------------------------
    private void DetectMovementMouse()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        localMousePos = mouseWorldPos;
    }

    // For moving character ------------------------------------------------------------------
    private void MoveCharacter()
    {
        float baseSpeed = characterStats.moveSpeed;
        if (wIsDown && aIsDown)
        {
            rigidbody.velocity = new Vector2(baseSpeed / -2, baseSpeed / 2);
        }
        else if (wIsDown && dIsDown)
        {
            rigidbody.velocity = new Vector2(baseSpeed / 2, baseSpeed / 2);
        }
        else if (sIsDown && aIsDown)
        {
            rigidbody.velocity = new Vector2(baseSpeed / -2, baseSpeed / -2);
        }
        else if (sIsDown && dIsDown)
        {
            rigidbody.velocity = new Vector2(baseSpeed / 2, baseSpeed / -2);
        }
        else if (wIsDown)
        {
            rigidbody.velocity = new Vector2(0, baseSpeed);
        }
        else if (aIsDown)
        {
            rigidbody.velocity = new Vector2(-baseSpeed, 0);
        }
        else if (sIsDown)
        {
            rigidbody.velocity = new Vector2(0, -baseSpeed);
        }
        else if (dIsDown)
        {
            rigidbody.velocity = new Vector2(baseSpeed, 0);
        }
        else
        {
            rigidbody.velocity = new Vector2(0, 0);
        }
    }

    // For sending mouse position ------------------------------------------------------------
    private void SendMousePos()
    {
        // Need update for better connection
        if (localMousePos != historyMousePos && Time.time >= mousePosNextTime)
        {
            historyMousePos = localMousePos;
            NetworkClient.Instance.SendMousePos(localMousePos.x, localMousePos.y);

            mousePosNextTime = Time.time + mousePosSendCoolDown;
        }
    }

    // For Sync mouse pos --------------------------------------------------------------------
    public void SyncMousePos(float x, float y)
    {
        syncMousePos = new Vector3(x, y, 0);
    }

    // Set Button up/down --------------------------------------------------------------------
    public void SetButtonDown(Button button)
    {
        if(button == Button.w)
        {
            wIsDown = true;
        }
        else if (button == Button.a)
        {
            aIsDown = true;
        }
        else if (button == Button.s)
        {
            sIsDown = true;
        }
        else if (button == Button.d)
        {
            dIsDown = true;
        }
    }
    public void SetButtonUp(Button button)
    {
        if (button == Button.w)
        {
            wIsDown = false;
        }
        else if (button == Button.a)
        {
            aIsDown = false;
        }
        else if (button == Button.s)
        {
            sIsDown = false;
        }
        else if (button == Button.d)
        {
            dIsDown = false;
        }
    }
}
