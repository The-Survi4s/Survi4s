using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WallTileManager : MonoBehaviour
{
    [SerializeField] private Tilemap _wallTilemap;
    [SerializeField] private TileStages _wallTileStages;
    private Dictionary<Vector2Int, Wall> _wallTiles = new Dictionary<Vector2Int, Wall>();

    public static WallTileManager instance { get; private set; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void InitializeDictionary()
    {
        
    }
}
