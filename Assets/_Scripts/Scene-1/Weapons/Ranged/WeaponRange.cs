using UnityEngine;

public class WeaponRange : WeaponBase
{
    [field: SerializeField] public int MaxAmmo { get; private set; }
    [SerializeField] private GameObject _bullet;
    [field: SerializeField] public float inAccuracy { get; private set; }
    private int _ammo;
    public int Ammo
    {
        get => _ammo;
        private set => _ammo = Mathf.Clamp(value, 0, MaxAmmo);
    }

    private void Start()
    {
        ReloadAmmo();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(AttackPoint, 0.2f);
    }

    public Vector2 GetOwnerMousePos()
    {
        if (ownerPlayer == null)
        {
            return new Vector2(0, 0);
        }

        return ownerPlayer.movement.syncedMousePos;
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
                NetworkClient.Instance.SpawnBullet(AttackPoint, GetOwnerMousePos());
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
