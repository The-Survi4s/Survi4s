using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnRandom : MonoBehaviour
{
    public GameObject[] enemies;
    public Transform[] spawnPoint;
    private int rand;
    private int randPosition;
    public float startTimeBtwspawns;
    private float timeBtwSpawns;
    // Start is called before the first frame update
    private void Start()
    {
        timeBtwSpawns = startTimeBtwspawns;
    }

    // Update is called once per frame
    private void Update()
    {
        if (timeBtwSpawns <= 0)
        {
            rand = Random.Range(0, enemies.Length);
            randPosition = Random.Range(0, spawnPoint.Length);
            Instantiate(enemies[rand], spawnPoint[rand].transform.position, Quaternion.identity);
            timeBtwSpawns = startTimeBtwspawns;
        }
        else
        {
            timeBtwSpawns -= Time.deltaTime;
        }
    }
}
