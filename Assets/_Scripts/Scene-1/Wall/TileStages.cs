using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileStages : ScriptableObject
{
    [SerializeField] private TileBase[] _tileStages;
}
