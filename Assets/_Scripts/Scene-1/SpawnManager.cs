using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    //[SerializeField] private int MaxIdCount;
   // private int IdCount = 0;
   /* public GameObject[] enemies;
    public Transform[] spawnPoint;
    private int rand;
    private int randPosition;
    public float startTimeBtwspawns;
    private float timeBtwSpawns;*/
    /* private void Start()
     {
         // SpawnMonster(Monster.Origin.Bottom);
         timeBtwSpawns = startTimeBtwspawns;
     }

     public void SpawnMonster()
     {
         //IdCount = (IdCount + 1) % MaxIdCount;
         //NetworkClient.Instance.SpawnMonster(IdCount, origin);

         if (timeBtwSpawns <= 0)
         {
             rand = Random.Range(0, enemies.Length);
             randPosition = Random.Range(0, spawnPoint.Length);
             NetworkClient.Instantiate(enemies[rand], spawnPoint[rand].transform.position, Quaternion.identity);
             timeBtwSpawns = startTimeBtwspawns;
         }
         else
         {
             timeBtwSpawns -= Time.deltaTime;
         }

     }
     void Update()
     {
         SpawnMonster();
     }*/
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
