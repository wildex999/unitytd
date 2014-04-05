using UnityEngine;
using System.Collections;

//Handles the user selecting a tile for placement

[ExecuteInEditMode]
public class TileItem : MonoBehaviour {

    public MapTile tile;

#if UNITY_EDITOR
    //Set the sprite of the button
    private MapTile oldTile;
    void Update()
    {
        if (tile == null || tile == oldTile)
            return;
        UITexture texture = GetComponentInChildren<UITexture>();
        if(texture == null)
        {
            Debug.LogWarning("No Texture found for TileItem.");
            return;
        }

        SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
        if(spriteRenderer == null)
        {
            Debug.LogWarning("Tile not using sprite renderer!");
            return;
        }

        texture.mainTexture = spriteRenderer.sprite.texture;
        oldTile = tile;
    }
#endif

    void OnClick()
    {
        EditorMap.instance.startPlaceTile(tile);
    }
}
