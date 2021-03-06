using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioManager))]
public abstract class DestroyableTile : MonoBehaviour
{
    [field: SerializeField] public int hp { get; protected set; }
    public abstract int maxHp { get; protected set; }
    public Vector3Int cellPos { get; protected set; }
    public bool isDestroyed { get; protected set; }
    public event Action<DestroyableTile> OnDestroyed;
    public event Action<DestroyableTile> OnRebuilt;
    public int spriteVariantId = 0;

    protected AudioManager audioManager;

    private void Awake()
    {
        audioManager = GetComponent<AudioManager>();
    }

    public void ModifyHp(float amount, int silentLevel = 0)
    {
        hp += Mathf.FloorToInt(amount);
        //Debug.Log($"amount modified {amount}, current hp {hp}");
        if (hp > 0)
        {
            if (amount < 0)
            {
                audioManager.Play("Attacked");
            }

            if (hp > maxHp) hp = maxHp;
            if (silentLevel > 1) return;
            InvokeRebuiltEvent();
        }
        else if (hp <= 0)
        {
            audioManager.Play("Destroyed");

            hp = 0;
            if (silentLevel > 1) return;
            InvokeDestroyedEvent();
        }
        if(silentLevel > 0) return;
        SpawnParticle();
        AfterModifyHp();
    }

    private void SpawnParticle()
    {

    }

    private void AfterModifyHp()
    {
        TilemapManager.Instance.UpdateWallTilemap(this);
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

    [ContextMenu(nameof(DamageTileBy10))]
    private void DamageTileBy10()
    {
        ModifyHp(-10);
    }
    [ContextMenu(nameof(HealTileBy10))]
    private void HealTileBy10()
    {
        ModifyHp(10);
    }
}
