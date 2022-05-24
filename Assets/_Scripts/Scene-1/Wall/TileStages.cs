using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileStages : ScriptableObject
{
    [SerializeField] private List<TileBase> _tileStages;
    public List<TileBase> getTileStages => _tileStages;
    public TileBase GetTile(int stage) => _tileStages[stage];
    public int GetIndex(TileBase tile)
    {
        if (_tileStages.Contains(tile))
        {
            for (int index = 0; index < _tileStages.Count; index++)
            {
                if (_tileStages[index] == tile) return index;
            }
        }
        return -1;
    }
    public bool Contains(TileBase tile) => _tileStages.Contains(tile);
}
