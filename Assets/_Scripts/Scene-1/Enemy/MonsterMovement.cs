using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
    /*
    public static float speed = 1;
   // public float distance;
    private bool movingRight = true;
    //public Transform groundDetection;
    // Start is called before the first frame update
    /*   void Start()
       {

       }*/

    // Update is called once per frame
    /* void Update()
     {
         transform.Translate(Vector2.left * speed * Time.deltaTime);
       //  RaycastHit2D groundInfo = Physics2D.Raycast(groundDetection.position, Vector2.down, distance);

             if (movingRight == true)
             {
                 transform.eulerAngles = new Vector3(0, 180, 0);
                // movingRight = false;
             }


         }
 */
    public float speed;
    private Transform player;
    public float lineOfSite;
    public float shootingRange;
    public GameObject bullet;
    public GameObject bulletParent;
    public float fireRate = 0.1f;
    private float nextFireTime;
    ///[SerizalizeField]
   // public GameObject Kroco, NonKroco;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Tower").transform;
    }

    void Update()
    {
        float distanceFromPlayer = Vector2.Distance(player.position, transform.position);
        if (distanceFromPlayer < lineOfSite && distanceFromPlayer > shootingRange)
        {
            transform.position = Vector2.MoveTowards(this.transform.position, player.position, speed * Time.deltaTime);
        }
        else if (distanceFromPlayer <= shootingRange && nextFireTime < Time.time)
        {
            Instantiate(bullet, bulletParent.transform.position, Quaternion.identity);
            nextFireTime = Time.time + fireRate;
        }
        transform.position = Vector2.MoveTowards(this.transform.position, player.position, speed * Time.deltaTime);
    }
    
    /*public void Kroco(){
        transform.position = Vector2.MoveTowards(this.GameObject.position, player.position, speed * Time.deltaTime);

    }*/
        public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, lineOfSite);
    }
}


