using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private int MaxIdCount;
    private int IdCount = 0;

    private void Start()
    {
        SpawnMonster(Monster.Origin.Bottom);
    }

    public void SpawnMonster(Monster.Origin origin)
    {
        IdCount = (IdCount + 1) % MaxIdCount;
        NetworkClient.Instance.SpawnMonster(IdCount, origin);
    }
}
