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
            if (!weapon.IsCrit())
            {
                // Damage here

            }
            else
            {
                // More damage

            }
        }

        Destroy(gameObject);
    }
}
