using UnityEngine;

public class WeaponRange : WeaponBase
{
    [SerializeField] private float DefaultShootRange;
    public float shootRange { get; private set; }


    public override void Attack()
    {
        
    }

}
