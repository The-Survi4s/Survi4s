using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDropItem : MonoBehaviour
{
    private AudioManager audioManager;

    private void Awake()
    {
        audioManager = GetComponent<AudioManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        audioManager.Play("PickUp");

        if (collision.gameObject.TryGetComponent(out PlayerWeaponManager playerWeaponManager))
        {


            playerWeaponManager.AddExp(Random.Range(4, 20));
            Destroy(gameObject);
        }
    }
}
