using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTipeX : BulletBase
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Animation

        if(weapon!= null && isLocal)
        {
            Monster monster = GetMonster(collision);
            Monster.Origin ori = monster.origin;
            int Id = monster.ID;

            NetworkClient.Instance.DamageMonster(Id, ori, weapon.baseAttack);

            if (weapon.IsCrit())
            {
                NetworkClient.Instance.StunMonster(Id, ori, weapon.baseAttack);
            }
        }

        Destroy(gameObject);
    }
}
