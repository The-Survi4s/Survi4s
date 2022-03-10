using UnityEngine;

public abstract class WeaponRange : WeaponBase
{
    [SerializeField] private float DefaultShootRange;
    [SerializeField] private float DefaultAmmo;
    public GameObject bullet;
    public float shootRange { get; private set; }
    public float ammo { get; private set; }

    public Vector2 GetOwnerMousePos()
    {
        if (owner == null)
        {
            return new Vector2(0, 0);
        }

        return owner.GetComponent<CharacterController>().syncMousePos;
    }

    // TipeX
    // Stun

    // Pentilan
    // Fast attack

    // Minuman Es
    // Splash damage

    // Pelontar Pensil
    // Piercing damage

    // Vector3 forward
    // Vector3 distance
    // Vector2 Up

    public override abstract void OnAttack();
}
