using UnityEngine;

public abstract class WeaponMelee : WeaponBase
{
    [SerializeField] private float DefaultAttackRad;
    public float attackRad { get; private set; }

    // Palu 
    // Can repair wall

    // Pentungan
    // Knockback enemy

    // Jajan
    // Area Heal + buff damage

    public override abstract void Attack();
    public override abstract void OnCritical();       
}
