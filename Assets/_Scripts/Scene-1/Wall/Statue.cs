using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statue : DestroyableTile
{
    public override int maxHp { get; protected set; }
    public bool IsInitialized { get; private set; }

    [SerializeField] private Sprite[] _haloStageSprites;

    void Start()
    {
        maxHp = GameManager.Instance.Settings.initialStatueHp;
        cellPos = TilemapManager.Instance.WorldToCell(transform.position);
        hp = maxHp;
        TilemapManager.Instance.SetStatue(this);
        IsInitialized = true;
    }

    public void PlayDestroyedAnimation()
    {

    }

    // Dipanggil ketika collision
    public void ShowUpgrades(WeaponBase weapon)
    {
        // Upgrade statue, or

        // Upgrade weapon
    }

    private void UpdateHaloSprite()
    {

    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}
