using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPartitur : MonsterBulletBase
{
    protected override void BulletUpdate()
    {
        moveSpeed -= moveSpeed / (maxTravelRange * 100);
    }
}
