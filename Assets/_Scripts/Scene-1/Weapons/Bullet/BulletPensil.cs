using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPensil : BulletBase
{
    [SerializeField] private int piercingPower;

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

        if(piercingPower > 0)
        {
            piercingPower--;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
