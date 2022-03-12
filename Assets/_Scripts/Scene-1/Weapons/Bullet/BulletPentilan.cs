using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPentilan : BulletBase
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Animation

        if (weapon != null && isLocal)
        {
            Monster monster = GetMonster(collision);
            Monster.Origin ori = monster.origin;
            int Id = monster.ID;

            if (!weapon.IsCrit())
            {
                // Damage here
                NetworkClient.Instance.DamageMonster(Id, ori, weapon.baseAttack);
            }
            else
            {
                // More damage
                NetworkClient.Instance.DamageMonster(Id, ori, weapon.baseAttack * 1.5f);
            }
        }

        Destroy(gameObject);
    }
}
