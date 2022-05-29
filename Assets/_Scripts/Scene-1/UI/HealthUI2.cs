using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthUI2 : MonoBehaviour
{
    [SerializeField] private Text text;
    private PlayerStat _stat;
    private Player _localPlayer;

    void Update()
    {
        if (!_stat)
        {
            if (_localPlayer)
            {
                _stat = _localPlayer.stats;
            }
            else
            {
                _localPlayer = UnitManager.Instance.GetPlayer();
            }
            return;
        }
        text.text = _stat.hitPoint.ToString();
    }
}
