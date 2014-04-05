using UnityEngine;
using System.Collections;

//Handles the user selecting a tile for placement

[ExecuteInEditMode]
public class TileItem : MonoBehaviour {

    public MapTile tile;

    void Start()
    {
        setTileSprite();
    }

#if UNITY_EDITOR
    //Set the sprite of the button
    private MapTile oldTile;
    void Update()
    {
        if (tile != oldTile)
        {
            setTileSprite();
            oldTile = tile;
        }
    }
#endif

    void setTileSprite()
    {
        if (tile == null)
            return;
        UITexture texture = GetComponentInChildren<UITexture>();
        if (texture == null)
        {
            Debug.LogWarning("No Texture found for TileItem.");
            return;
        }

        SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("Tile not using sprite renderer!");
            return;
        }

        texture.mainTexture = spriteRenderer.sprite.texture;
    }

    void OnClick()
    {
        EditorMap.instance.startPlaceTile(tile);
    }
}
