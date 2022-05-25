using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileStages : ScriptableObject
{
    [SerializeField] private List<TileBase> _tiles;
    public List<TileBase> getTileStages => _tiles;
    public TileBase GetTile(int stage) => _tiles[stage];
    public int GetIndex(TileBase tile)
    {
        if (_tiles.Contains(tile))
        {
            for (int index = 0; index < _tiles.Count; index++)
            {
                if (_tiles[index] == tile) return index;
            }
        }
        return -1;
    }
    public bool Contains(TileBase tile) => _tiles.Contains(tile);
}
