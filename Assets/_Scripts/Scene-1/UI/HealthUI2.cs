using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthUI2 : MonoBehaviour
{
    Text text;
    [SerializeField] public PlayerStats Stats;
    [SerializeField] public float hp_sem, jumlah;
    private Player localPlayer;

    public void Start()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        if (Stats == null)
        {
            Stats = UnitManager.Instance.GetPlayer()?.stats;
            return;
        }
        text.text = Stats.hitPoint.ToString();
    }
}
