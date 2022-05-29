using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDropItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerWeaponManager>().PlayerWeaponXp += Random.Range(4, 20);
            Destroy(gameObject);
        }
    }
}
