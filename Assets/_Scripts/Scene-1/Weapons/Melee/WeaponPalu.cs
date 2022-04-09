using UnityEngine;

public class WeaponPalu : WeaponMelee
{
    [SerializeField] private LayerMask wallMask;

    protected override void OnNormalAttack(Collider2D[] targets)
    {
        foreach (Collider2D x in targets)
        {
            Debug.Log("We hit " + x.name);
            if (x.gameObject.TryGetComponent(out Wall wall))
            {
                wall.ModifyWallHp(10);
                Debug.Log("We repair " + x.name);
            }
        }
    }

    protected override void OnCritical(Collider2D[] targets)
    {
        // Damage them more
        foreach (Collider2D x in targets)
        {
            Debug.Log("We Crit hit " + x.name);
        }
    }
}
