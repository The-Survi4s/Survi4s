using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BulletBase : MonoBehaviour
{
    [SerializeField] protected float DefaultMoveSpeed;
    [SerializeField] protected float DefaultTravelRange;
    [SerializeField] protected float CriticalMultiplier = 1.5f;
    private Vector2 startPos;
    public float moveSpeed { get; private set; }
    private bool rotationIsSet;

    protected WeaponBase weapon { get; private set; }
    public bool isLocal { get; private set; }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (rotationIsSet)
        {
            // Move bullet
            transform.position += moveSpeed * transform.right * Time.deltaTime;

            if(Vector2.Distance(transform.position, startPos) > DefaultTravelRange)
            {
                Destroy(gameObject);
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        Monster monster = collision.GetComponent<Monster>();
        if (monster == null) return;
        // Animation
        PlayAnimation();
        if (weapon != null && isLocal)
        {
            if (!weapon.IsCritical())
            {
                OnNormalShot(monster);
            }
            else
            {
                OnCriticalShot(monster);
            }
        }
        OnEndOfTrigger();
    }

    protected virtual void PlayAnimation()
    {

    }
    protected virtual void SpawnParticle()
    {

    }
    protected virtual void OnNormalShot(Monster monster)
    {
        // Damage here
        NetworkClient.Instance.ModifyMonsterHp(monster.id, -weapon.baseAttack);
    }
    protected virtual void OnCriticalShot(Monster monster)
    {
        // More damage
        NetworkClient.Instance.ModifyMonsterHp(monster.id, -weapon.baseAttack * CriticalMultiplier);
    }
    protected virtual void OnEndOfTrigger()
    {
        Destroy(gameObject);
    }

    private void SetRotation(Vector2 mousePos)
    {
        // Rotate weapon based on owner mouse pos
        float AngleRad = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x);
        float AngleDeg = (180 / Mathf.PI) * AngleRad;
        transform.rotation = Quaternion.Euler(0, 0, AngleDeg);
        rotationIsSet = true;
    }

    public void Init(WeaponBase weaponOrigin, Vector2 mousePos, bool isLocal = false)
    {
        weapon = weaponOrigin;
        SetRotation(mousePos);
        this.isLocal = isLocal;
        moveSpeed = DefaultMoveSpeed;
        startPos = transform.position;
    }
}
