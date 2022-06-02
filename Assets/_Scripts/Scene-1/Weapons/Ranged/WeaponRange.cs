using UnityEngine;

public class WeaponRange : WeaponBase
{
    [field: SerializeField] public int MaxAmmo { get; private set; }
    [SerializeField] private GameObject _bullet;
    [field: SerializeField] public float inAccuracy { get; private set; }
    [SerializeField] private int _ammo;
    public int Ammo
    {
        get => _ammo;
        private set => _ammo = Mathf.Clamp(value, 0, MaxAmmo);
    }

    private void Start()
    {
        ReloadAmmo();
    }

    public Vector2 GetOwnerMousePos()
    {
        if (ownerPlayer == null)
        {
            return new Vector2(0, 0);
        }

        return ownerPlayer.movement.syncMousePos;
    }

    public void ReloadAmmo() => Ammo = MaxAmmo;

    public override void ReceiveAttackMessage()
    {
        base.ReceiveAttackMessage();
        // Only do this if local
        if (isLocal)
        {
            if(Ammo > 0)
            {
                // Send message to spawn bullet
                Vector2 attackPoint = GetOwnerAttackPoint();
                Vector2 mousePos = GetOwnerMousePos();
                NetworkClient.Instance.SpawnBullet(attackPoint, mousePos);
            }
            else if(ownerPlayer.movement.isNearStatue)
            {
                ReloadAmmo();
            }
        }
    }

    public void SpawnBullet(Vector2 spawnPos, Vector2 mousePos)
    {
        // Make a bullet
        GameObject temp = Instantiate(_bullet, spawnPos, Quaternion.identity);
        PlayerBulletBase bulTemp = temp.GetComponent<PlayerBulletBase>();

        // init bullet
        bulTemp.Initialize(this, mousePos, UnitManager.Instance.GetIdThenAddBullet(bulTemp), isLocal);

        // Reduce ammo
        Ammo--;
    }
}
