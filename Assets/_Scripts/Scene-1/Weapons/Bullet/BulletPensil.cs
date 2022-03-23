using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPensil : BulletBase
{
    [SerializeField] private int piercingPower;

    protected override void OnEndOfTrigger()
    {
        if (piercingPower > 0)
        {
            piercingPower--;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
