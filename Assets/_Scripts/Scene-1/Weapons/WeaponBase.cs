using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected float defaultBaseAttack;
    [SerializeField] protected float defaultCritRate;
    [SerializeField] protected float maxCooldownTime;

    public float baseAttack { get; protected set; }
    public float critRate { get; protected set; }
    public float cooldownTime { get; protected set; }
    protected float nextAttackTime;
    public GameObject owner { get; private set; }
    public bool IsUsed() => owner != null;

    public int upgradeLevel { get; private set; }

    // Animation Variables
    [SerializeField] protected Vector3 offset;
    [SerializeField]
    protected struct SwingTo
    {
        public float degree;
        public float t;
    }
    [SerializeField] private List<SwingTo> swingQueues = new List<SwingTo>();
    protected float swingDegree;
    private double animationEndDegree = 5f;
    protected int animationStep;

    // Particles -----------------------------------------
    

    // Cached components --------------------
    protected Player ownerPlayer;


    // -------------------
    protected virtual void Init()
    {
        baseAttack = defaultBaseAttack;
        critRate = defaultCritRate;
        cooldownTime = maxCooldownTime;
        ownerPlayer = owner?.GetComponent<Player>();
    }

    private void Awake() => Init();
    private void Update()
    {
        // Check if this weapon is equipped ----------------------------------------
        if (owner == null) return;
        // Follow owner
        transform.position = owner.transform.position +
                             (ownerPlayer.isFacingLeft ? new Vector3 (-offset.x, offset.y, offset.z) : new Vector3(offset.x, offset.y, offset.z));
        // Swing animation
        if (animationStep < swingQueues.Count)
        {
            LerpAnimation();
            if (Mathf.Abs(swingDegree - Mathf.LerpAngle(swingDegree, swingQueues[animationStep].degree,
                    swingQueues[animationStep].t)) < animationEndDegree) animationStep++;
        }
        // Rotate weapon based on owner mouse pos
        RotateWeapon(isLocal
            ? ownerPlayer.localMousePos
            : ownerPlayer.syncMousePos);
    }
    
    // Network methods -----------------------------------
    public void SendAttackMessage()
    {
        // Check cooldown
        if (!(Time.time >= nextAttackTime)) return;
        // Send attack message
        NetworkClient.Instance.StartAttackAnimation();
        // Cooldown
        nextAttackTime = Time.time + cooldownTime;
    }
    public bool isLocal => ownerPlayer.isLocal;

    // Animation methods ---------------------------------------
    protected virtual void PlayAnimation() => animationStep = 0;
    protected void LerpAnimation() =>
        swingDegree = Mathf.LerpAngle(swingDegree, swingQueues[animationStep].degree, swingQueues[animationStep].t);
    protected virtual void SpawnParticle() { }
    private void RotateWeapon(Vector3 target)
    {
        var angleRad = Mathf.Atan2(target.y - transform.position.y, target.x - transform.position.x);
        var angleDeg = (180 / Mathf.PI) * angleRad +
                       swingDegree * (ownerPlayer.isFacingLeft ? -1 : 1);
        transform.rotation = Quaternion.Euler(0, 0, angleDeg);
    }

    // Attack methods --------------------------------------------
    public bool IsCritical() => Random.Range(0f, 100f) < critRate; 
    public Vector2 GetOwnerAttackPoint() => 
        owner == null 
        ? Vector2.zero 
        : (Vector2)owner.GetComponent<PlayerWeaponManager>().GetAttackPoint().position;

    public virtual void ReceiveAttackMessage() => PlayAnimation();

    // Equip / UnEquip -------------------------------------------------
    public void EquipWeapon(PlayerWeaponManager player)
    {
        if (owner != null) return;
        owner = player.gameObject;
        ownerPlayer = owner.GetComponent<Player>();
    }
    public void UnEquipWeapon(PlayerWeaponManager player, Vector2 dropPos, float zRotation)
    {
        if (player.name != owner.name) return;
        owner = null;
        transform.position = dropPos;
        transform.rotation = Quaternion.Euler(0, 0, zRotation);
        ownerPlayer = null;
    }

    // Upgrade weapon, dipanggil dari statue
    public void UpgradeWeapon()
    {
        baseAttack *= 1.05f;
        critRate *= 1.05f;
        maxCooldownTime *= 0.9f;
    }
}
