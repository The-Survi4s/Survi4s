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
            // Damage here

            if (weapon.IsCrit())
            {
                // More Damage

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
