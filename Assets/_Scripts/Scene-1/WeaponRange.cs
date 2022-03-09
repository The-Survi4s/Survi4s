using UnityEngine;

public abstract class WeaponRange : WeaponBase
{
    [SerializeField] private float DefaultShootRange;
    [SerializeField] private float DefaultAmmo;
    public float shootRange { get; private set; }
    public float ammo { get; private set; }

    // TipeX
    // Stun

    // Pentilan
    // Fast attack

    // Minuman Es
    // Splash damage

    // Pelontar Pensil
    // Piercing damage

    public override abstract void OnAttack(Vector2 mousePos);
    public override abstract void OnCritical(Vector2 mousePos);
}
