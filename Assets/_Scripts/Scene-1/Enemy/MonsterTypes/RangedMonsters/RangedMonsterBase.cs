using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RangedMonsterBase : Monster
{
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Vector3 _bulletSpawnOffset;
    private Vector3 _attackPoint => 
        transform.position + 
        transform.right * _bulletSpawnOffset.x * (_renderer.flipX ? -1 : 1 ) + 
        transform.up * _bulletSpawnOffset.y;

    protected override void Attack(Component nearestObj)
    {
        RangedAttack(nearestObj);
    }

    private void RangedAttack(Component nearestObj)
    {
        NetworkClient.Instance.SpawnBullet(_attackPoint, nearestObj.transform.position, id);
    }

    public void SpawnBullet(Vector2 spawnPos, Vector2 targetPos)
    {
        // Make a bullet
        GameObject temp = Instantiate(_bulletPrefab, spawnPos, Quaternion.identity);
        MonsterBulletBase bulTemp = temp.GetComponent<MonsterBulletBase>();

        // init bullet
        bulTemp.Initialize(this, targetPos, UnitManager.Instance.GetIdThenAddBullet(bulTemp));
    }

    private void OnDrawGizmosSelected()
    {
        if (!_renderer) _renderer = GetComponent<SpriteRenderer>();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_attackPoint, 0.2f);
    }
}
