using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BulletBase : MonoBehaviour
{
    [SerializeField] private float DefaultMoveSpeed;
    private float DefaultFireRange;
    private Vector2 startPos;
    public float moveSpeed { get; private set; }
    private bool rotationIsSet;

    public WeaponBase weapon { get; private set; }
    public bool isLocal { get; private set; }

    private void Start()
    {
        moveSpeed = DefaultMoveSpeed;
        startPos = transform.position;
    }

    private void Update()
    {
        if (rotationIsSet)
        {
            // Move bullet
            transform.position += moveSpeed * transform.right * Time.deltaTime;

            if(Vector2.Distance(transform.position, startPos) > DefaultFireRange)
            {
                Destroy(gameObject);
            }
        }
    }

    public Monster GetMonster(Collider2D collider)
    {
        return collider.GetComponent<Monster>();
    }

    public void SetRotation(Vector2 mousePos)
    {
        // Rotate weapon based on owner mouse pos
        float AngleRad = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x);
        float AngleDeg = (180 / Mathf.PI) * AngleRad;
        transform.rotation = Quaternion.Euler(0, 0, AngleDeg);
        rotationIsSet = true;
    }
    public void SetWeapon(WeaponBase weapon)
    {
        this.weapon = weapon;
    }
    public void SetToLocal()
    {
        isLocal = true;
    }
    public void SetFireRange(float range)
    {
        DefaultFireRange = range;
    }
}
