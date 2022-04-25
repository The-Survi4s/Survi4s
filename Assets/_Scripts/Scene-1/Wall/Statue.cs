using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statue : DestroyableTile
{
    public override int maxHp { get; protected set; }

    [SerializeField] private Sprite[] _haloStageSprites;

    void Start()
    {
        maxHp = GameManager.Instance.gameSetting.initialStatueHp;
        cellPos = TilemapManager.instance.GetCellPosition(transform.position);
        hp = maxHp;
        TilemapManager.instance.SetStatue(this);
    }

    protected override void AfterModifyHp()
    {
        TilemapManager.instance.UpdateWallTilemap(this);
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
}
