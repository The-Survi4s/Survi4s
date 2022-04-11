using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterMovement : Spawner
{
    [SerializeField] private AIBase AI;
    public float speed;
    private Transform wall,wall2,wall3,wall4;
    public float lineOfSite;
    public float shootingRange;
    //public GameObject bullet;
    //public GameObject bulletParent;
    public float fireRate = 0.1f;
    private float nextFireTime;
    public GameObject spawn1, spawn2, spawn3, spawn4;
    // Spawner spawner;
    
    void Start()
    {
        
       wall = GameObject.FindGameObjectWithTag("Wall").transform;
        wall2 = GameObject.FindGameObjectWithTag("Wall2").transform;
        wall3 = GameObject.FindGameObjectWithTag("Wall3").transform;
        wall4 = GameObject.FindGameObjectWithTag("Wall4").transform;
    }

    void Update()
    {
        //  float distanceFromWall = Vector2.Distance(wall.position, transform.position);
        // float distanceFromWall2 = Vector2.Distance(wall2.position, transform.position);
        //float distanceFromWall3 = Vector2.Distance(wall3.position, transform.position);
        //float distanceFromWall4 = Vector2.Distance(wall4.position, transform.position);
            Wall1();
    //        Wall2();
 
    

        /*if (origin == Monster.Origin.Top)
        {
            transform.position = Vector2.MoveTowards(this.transform.position, wall.position, speed * Time.deltaTime);
            Debug.Log("atas");
        }
        else if (origin == Monster.Origin.Right)
        {
            transform.position = Vector2.MoveTowards(this.transform.position, wall2.position, speed * Time.deltaTime);
            Debug.Log("kanan");
        }
        else if (origin == Monster.Origin.Bottom)
        {
            transform.position = Vector2.MoveTowards(this.transform.position, wall3.position, speed * Time.deltaTime);
        }
        else if(origin == Monster.Origin.Left)
        {
            transform.position = Vector2.MoveTowards(this.transform.position, wall4.position, speed * Time.deltaTime);
        }*/

        //    if()

    }
    public void Wall1() {
    float distanceFromPlayer = Vector2.Distance(wall.position, transform.position);
        if (distanceFromPlayer<lineOfSite && distanceFromPlayer> shootingRange)
        {
            transform.position = Vector2.MoveTowards(this.transform.position, wall.position, speed* Time.deltaTime);
    }
        else if (distanceFromPlayer <= shootingRange && nextFireTime<Time.time)
        {
          //  Instantiate(bullet, bulletParent.transform.position, Quaternion.identity);
            nextFireTime = Time.time + fireRate;
        }
transform.position = Vector2.MoveTowards(this.transform.position, wall.position, speed * Time.deltaTime);
}
    public void Wall2()
    {
        float distanceFromPlayer = Vector2.Distance(wall2.position, transform.position);
        if (distanceFromPlayer < lineOfSite && distanceFromPlayer > shootingRange)
        {
            transform.position = Vector2.MoveTowards(this.transform.position, wall2.position, speed * Time.deltaTime);
        }
        else if (distanceFromPlayer <= shootingRange && nextFireTime < Time.time)
        {
            //  Instantiate(bullet, bulletParent.transform.position, Quaternion.identity);
            nextFireTime = Time.time + fireRate;
        }
        transform.position = Vector2.MoveTowards(this.transform.position, wall2.position, speed * Time.deltaTime);
    }


    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, lineOfSite);
    }
}
