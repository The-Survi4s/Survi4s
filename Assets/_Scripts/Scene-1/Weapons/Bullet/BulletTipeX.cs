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
            // Damage here

            if (weapon.IsCrit())
            {
                // Stun here

            }
        }

        Destroy(gameObject);
    }
}
