using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameOver : MonoBehaviour
{
    [SerializeField] public PlayerStats Stats;
    [SerializeField] public float hp_sem, jumlah;
    //TriggerHp trik;
    //UnitManager unit;
    // Start is called before the first frame update
    public GameObject gameOver;
    public void Start()
    {
     //   text = GetComponent<Text>();
        //Stats = GameObject.FindObjectOfType(typeof(CharacterStats)) as CharacterStats;
        //Stats.hitPoint();

        StartCoroutine(OnFindLocalPlayer());
        gameOver.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Stats == null)
            return;
        //Debug.Log("Stats:" + Stats._hitPoint);
        //hp_sem = Stats._hitPoint;
        // jumlah = hp_sem - Stats.speed;
        //text.text = Stats.hitPoint.ToString();

        //hp_sem = Stats.hitPoint.set('1');
        //      text.text = Stats.hitPoint.ToString();
        //Debug.Log("hp " + hp_sem + "stats: "+ Stats._hitPoint);
        //      Debug.Log("Unit " + unit.playerAliveCount);
        if (Stats.hitPoint == 0)
        {
            gameOver.gameObject.SetActive(true);
        }

    }
    public Player LocalPlayer()
    {
        var players = FindObjectsOfType<Player>();

        foreach (Player p in players)
        {
            Debug.Log(p.gameObject.name);
            if (p.isLocal)
                return p;
        }
        return null;
    }
    IEnumerator OnFindLocalPlayer()
    {
        while (LocalPlayer() == null)
        {
            Debug.Log("Local player null");
            yield return new WaitForSeconds(0.2f);
        }
        Stats = UnitManager.Instance.GetPlayer().stats;

    }
}