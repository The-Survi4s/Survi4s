using UnityEngine;

public class WeaponRange : WeaponBase
{
    [SerializeField] private float DefaultShootRange;
    [SerializeField] private int DefaultAmmo;
    public GameObject bullet;
    public int ammo { get; private set; }

    private void Start()
    {
        ammo = DefaultAmmo;
    }

    public Vector2 GetOwnerMousePos()
    {
        if (owner == null)
        {
            return new Vector2(0, 0);
        }

        return owner.GetComponent<CharacterController>().syncMousePos;
    }
    public void ReduceAmmo(int ammo)
    {
        this.ammo -= ammo;
    }
    public void ReloadAmmo(int ammo)
    {
        this.ammo += ammo;
    }
    public void ReloadAmmo()
    {
        this.ammo = DefaultAmmo;
    }

    public override void OnAttack()
    {
        // Play animation


        // Only do this if local
        if (IsLocal() && ammo > 0)
        {
            // Send massage to spawn bullet
            Vector2 attackPoint = GetOwnerAttackPoint();
            Vector2 mousePos = GetOwnerMousePos();
            NetworkClient.Instance.SpawnBullet(attackPoint.x, attackPoint.y, mousePos.x, mousePos.y);
        }
    }

    public override void SpawnBullet(Vector2 spawnPos, Vector2 mousePos)
    {
        // Make a bullet
        GameObject temp = Instantiate(bullet, spawnPos, Quaternion.identity);
        BulletBase bulTemp = temp.GetComponent<BulletBase>();

        // Roatate bullet
        bulTemp.SetRotation(mousePos);
        // Set Bullet weapon
        bulTemp.SetWeapon(this);
        // Set Range
        bulTemp.SetFireRange(DefaultShootRange);
        // Set local
        if (IsLocal())
        {
            bulTemp.SetToLocal();
        }
    }
}
