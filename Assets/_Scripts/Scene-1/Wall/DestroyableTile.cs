using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DestroyableTile : MonoBehaviour
{
    [field: SerializeField] public int hp { get; protected set; }
    public abstract int maxHp { get; protected set; }
    public Vector3Int cellPos { get; protected set; }
    public bool isDestroyed { get; protected set; }
    public event Action<DestroyableTile> OnDestroyed;
    public event Action<DestroyableTile> OnRebuilt;
    public int spriteVariantId = 0;

    public void ModifyHp(float amount)
    {
        hp += Mathf.FloorToInt(amount);
        //Debug.Log($"amount modified {amount}, current hp {hp}");
        if (hp > 0)
        {
            if (hp > maxHp) hp = maxHp;
            InvokeRebuiltEvent();
        }
        else if (hp <= 0)
        {
            hp = 0;
            InvokeDestroyedEvent();
        }

        AfterModifyHp();
    }

    protected virtual void AfterModifyHp()
    {
        
    }

    protected virtual void InvokeRebuiltEvent()
    {
        if (!isDestroyed) return;
        OnRebuilt?.Invoke(this);
    }

    protected virtual void InvokeDestroyedEvent()
    {
        if (isDestroyed) return;
        OnDestroyed?.Invoke(this);
    }
}
