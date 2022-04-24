using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPencil : PlayerBulletBase
{
    [SerializeField] private int _piercingPower;
    private int _piercesLeft;

    private void Awake()
    {
        _piercesLeft = _piercingPower;
    }

    protected override void OnEndOfTrigger()
    {
        if (_piercesLeft > 0)
        {
            _piercesLeft--;
        }
        else
        {
            base.OnEndOfTrigger();
        }
    }
}
