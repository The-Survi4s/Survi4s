using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] private float DefaultHitPoint;
    [SerializeField] private float DefaultAttack;
    [SerializeField] private float DefaultCooldownAttack;
    [SerializeField] private float DefaultMoveSpeed;
    [SerializeField] private float DefaultRotationSpeed;

    public float hitPoint { get; private set; }
    public float attack { get; private set; }
    public float cooldown { get; private set; }
    public float moveSpeed { get; private set; }
    public float rotationSpeed { get; private set; }

    public enum Origin { Top, Right, Buttom, Left }
    
    private Origin origin;
    private int ID;

    private void Start()
    {
        hitPoint = DefaultHitPoint;
        attack = DefaultAttack;
        cooldown = DefaultCooldownAttack;
        moveSpeed = DefaultMoveSpeed;
        rotationSpeed = DefaultRotationSpeed;

        ID = -1;
    }

    public void SetOrigin(Origin ori)
    {
        origin = ori;
    }
    public void SetID(int Id)
    {
        if(Id == -1)
            ID = Id;
    }

    public void ReduceHitPoint(float damage)
    {
        hitPoint -= damage;
    }
    public void Stun(float second)
    {
        moveSpeed = 0;
        StartCoroutine(StunEffect(second));
    }
    private IEnumerator StunEffect(float second)
    {
        yield return new WaitForSeconds(second);
        moveSpeed = DefaultMoveSpeed;
    }
}
