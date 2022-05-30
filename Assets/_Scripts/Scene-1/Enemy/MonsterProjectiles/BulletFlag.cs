using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletFlag : MonsterBulletBase
{
    protected override void BulletUpdate()
    {
        AddRotation(moveSpeed / 100);
    }
}
