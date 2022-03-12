using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEsSegar : BulletBase
{
    [SerializeField] private float SplashRad;

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

            }
            else
            {
                // More damage + area bigger

            }
        }

        Destroy(gameObject);
    }


}