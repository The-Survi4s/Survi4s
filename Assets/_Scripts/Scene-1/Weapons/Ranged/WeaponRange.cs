using UnityEngine;

public class WeaponRange : WeaponBase
{
    [SerializeField] private int MaxAmmo;
    public GameObject bullet;

    private int _ammo;

    public int ammo
    {
        get => _ammo;
        set
        {
            _ammo += value;
            if (_ammo > MaxAmmo)
            {
                _ammo = MaxAmmo;
            }
        }
    }

    private void Start()
    {
        ReloadAmmo();
    }

    public Vector2 GetOwnerMousePos()
    {
        if (owner == null)
        {
            return new Vector2(0, 0);
        }

        return _ownerPlayerController.syncMousePos;
    }

    public void ReloadAmmo() => ammo = MaxAmmo;

    public override void OnAttack()
    {
        base.OnAttack();

        // Only do this if local
        if (IsLocal() && ammo > 0)
        {
            // Send message to spawn bullet
            Vector2 attackPoint = GetOwnerAttackPoint();
            Vector2 mousePos = GetOwnerMousePos();
            NetworkClient.Instance.SpawnBullet(attackPoint.x, attackPoint.y, mousePos.x, mousePos.y);
        }
    }

    public void SpawnBullet(Vector2 spawnPos, Vector2 mousePos)
    {
        // Make a bullet
        GameObject temp = Instantiate(bullet, spawnPos, Quaternion.identity);
        BulletBase bulTemp = temp.GetComponent<BulletBase>();

        // init bullet
        bulTemp.Init(this,mousePos,IsLocal());
    }
}
