using System;
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
    public enum Type {TypeA, B}

    public Origin origin { get; private set; }
    public int ID { get; private set; }
    public Wall targetWall { get; private set; }

    private void Awake()
    {
        hitPoint = DefaultHitPoint;
        attack = DefaultAttack;
        cooldown = DefaultCooldownAttack;
        moveSpeed = DefaultMoveSpeed;
        rotationSpeed = DefaultRotationSpeed;

        ID = -1;
    }

    public static event Action<int> OnMonsterDeath; 

    public void SetOrigin(Origin ori) => origin = ori;
    public void SetID(int Id) => ID = Id == -1 ? Id : ID;

    public void ReduceHitPoint(float damage)
    {
        hitPoint -= damage;

        if(hitPoint <= 0)
        {
            OnMonsterDeath?.Invoke(ID);
            SpawnManager.Instance.ClearIDIndex(ID);
            Destroy(gameObject);
        }
    }

    public void SendAttackMessage()
    {
        //Hapus pemanggilan fungsi di bawah ini. Cuma contoh (yg salah)
        NetworkClient.Instance.DamagePlayer("","",0);
        NetworkClient.Instance.DamageWall(0,1);
    }

    public void OnReceiveAttackMessage()
    {

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
                WallManager.OnWallFallenTop += SetTargetWall;
                break;
            case Origin.Right:
                WallManager.OnWallFallenRight += SetTargetWall;
                break;
            case Origin.Bottom:
                WallManager.OnWallFallenBottom += SetTargetWall;
                break;
            case Origin.Left:
                WallManager.OnWallFallenLeft += SetTargetWall;
                break;
        }
    }
    private void OnDisable()
    {
        switch (origin)
        {
            case Origin.Top:
                WallManager.OnWallFallenTop -= SetTargetWall;
                break;
            case Origin.Right:
                WallManager.OnWallFallenRight -= SetTargetWall;
                break;
            case Origin.Bottom:
                WallManager.OnWallFallenBottom -= SetTargetWall;
                break;
            case Origin.Left:
                WallManager.OnWallFallenLeft -= SetTargetWall;
                break;
        }
    }
    public void SetTargetWall(Wall wall)
    {
        targetWall = wall;
    }
}
