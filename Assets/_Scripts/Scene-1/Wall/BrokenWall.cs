using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenWall : DestroyableTile
{
    void Start()
    {
        hp = 0;
        isDestroyed = true;
        NavMeshController.UpdateNavMesh();
    }

    public int id { get; private set; }

    public Origin origin { get; private set; }

    public override int maxHp { get; protected set; }

    public void Init(int wallId, Vector3Int wallCellPos, Origin origin)
    {
        cellPos = wallCellPos;
        id = wallId;
        this.origin = origin;
    }

    private void OnDestroy()
    {
        NavMeshController.UpdateNavMesh();
        Destroy(gameObject);
    }

    public void RemoveFromMap()
    {
        //Debug.Log($"Broken wall {this} ini akan ter usir");
        TilemapManager.Instance.RemoveBrokenWall(cellPos);
        Destroy(gameObject);
        Destroy(this);
    }
}
