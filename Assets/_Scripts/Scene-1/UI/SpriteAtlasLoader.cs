using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class SpriteAtlasLoader : MonoBehaviour
{
    [SerializeField] private SpriteAtlas atlas;
    [SerializeField] private string spriteName;
    void Start()
    {
        var image = GetComponent<Image>();
        if (image) image.sprite = atlas.GetSprite(spriteName);
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer) renderer.sprite = atlas.GetSprite(spriteName);
    }
}
