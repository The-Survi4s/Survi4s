using UnityEngine;

public class WeaponRange : WeaponBase
{
    [SerializeField] private int _maxAmmo;
    [SerializeField] private GameObject _bullet;
    [field: SerializeField] public float inAccuracy { get; private set; }
    private int _ammo;
    public int ammo
    {
        get => _ammo;
        set => _ammo = Mathf.Clamp(value, 0, _maxAmmo);
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

        return ownerPlayer.syncMousePos;
    }

    public void ReloadAmmo() => ammo = _maxAmmo;

    public override void ReceiveAttackMessage()
    {
        base.ReceiveAttackMessage();
        //Debug.Log(ammo);
        // Only do this if local
        if (isLocal)
        {
            if(ammo > 0)
            {
                // Send message to spawn bullet
                Vector2 attackPoint = GetOwnerAttackPoint();
                Vector2 mousePos = GetOwnerMousePos();
                NetworkClient.Instance.SpawnBullet(attackPoint, mousePos);
            }
            else
            {
                if(ownerPlayer.IsNearStatue)
                {
                    ReloadAmmo();
                }
            }
        }
    }

    public void SpawnBullet(Vector2 spawnPos, Vector2 mousePos)
    {
        // Make a bullet
        GameObject temp = Instantiate(_bullet, spawnPos, Quaternion.identity);
        PlayerBulletBase bulTemp = temp.GetComponent<PlayerBulletBase>();

        // init bullet
        bulTemp.Init(this, mousePos, UnitManager.Instance.GetIdThenAddBullet(bulTemp), isLocal);
    }
}
