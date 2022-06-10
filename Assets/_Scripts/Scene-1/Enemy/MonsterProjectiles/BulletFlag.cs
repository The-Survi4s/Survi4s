using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletFlag : MonsterBulletBase
{
    protected override void BulletUpdate()
    {
        moveSpeed += moveSpeed / 1000;
    }
}
