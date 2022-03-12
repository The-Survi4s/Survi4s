using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
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

    public enum Origin { Top, Right, Bottom, Left }

    public Origin origin { get; private set; }
    public int ID { get; private set; }

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

    private void OnEnable()
    {
        switch (origin)
        {
            case Origin.Top:
                UnitManager.OnWallFallenTop += SetTargetWall;
                break;
            case Origin.Right:
                UnitManager.OnWallFallenRight += SetTargetWall;
                break;
            case Origin.Bottom:
                UnitManager.OnWallFallenBottom += SetTargetWall;
                break;
            case Origin.Left:
                UnitManager.OnWallFallenLeft += SetTargetWall;
                break;
        }
    }
    private void OnDisable()
    {
        switch (origin)
        {
            case Origin.Top:
                UnitManager.OnWallFallenTop -= SetTargetWall;
                break;
            case Origin.Right:
                UnitManager.OnWallFallenRight -= SetTargetWall;
                break;
            case Origin.Bottom:
                UnitManager.OnWallFallenBottom -= SetTargetWall;
                break;
            case Origin.Left:
                UnitManager.OnWallFallenLeft -= SetTargetWall;
                break;
        }
    }
    public void SetTargetWall(Wall wall)
    {
        
    }
}
