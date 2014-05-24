using UnityEngine;
using System.Collections;

//Handles the user selecting a tower for placement

[ExecuteInEditMode]
public class TowerItem : MonoBehaviour
{

    public TowerBase tower;
    private MapManager map;

    void Start()
    {
        setTowerSprite();
    }

#if UNITY_EDITOR
    //Set the sprite of the button
    private TowerBase oldTower;
    void Update()
    {
        if (tower != oldTower)
        {
            setTowerSprite();
            oldTower = tower;
        }
    }
#endif

    void setTowerSprite()
    {
        if (tower == null)
            return;
        UITexture texture = GetComponentInChildren<UITexture>();
        if (texture == null)
        {
            Debug.LogWarning("No Texture found for TileItem.");
            return;
        }

        SpriteRenderer spriteRenderer = tower.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("Tile not using sprite renderer!");
            return;
        }

        texture.mainTexture = spriteRenderer.sprite.texture;
    }

    void OnClick()
    {
        MapManager map = MapManager.getMapManager();
        if (map == null)
            return;

        map.startPlacingTower(tower);
    }
}
