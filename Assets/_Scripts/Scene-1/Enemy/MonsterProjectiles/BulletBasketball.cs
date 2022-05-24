using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBasketball : MonsterBulletBase
{
    [SerializeField, Range(0, 360)] private float _rotationRandomAddMax;

    protected override void OnEndOfTrigger()
    {
        AddRotation(Random.Range(-_rotationRandomAddMax, _rotationRandomAddMax));
        base.OnEndOfTrigger();
    }
}
