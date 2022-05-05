using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAnakFutsal : MeleeMonsterBase
{
    [SerializeField] private Vector3 _chaseOffset;
    [SerializeField] private float _chaseOffsetAngle;
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        _monsterMovement.SetOffset(_chaseOffset, transform.rotation.z + _chaseOffsetAngle);
    }
}
