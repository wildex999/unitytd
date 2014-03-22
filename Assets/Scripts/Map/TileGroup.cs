

using UnityEngine;
public class TileGroup
{
    public MapTile baseTile; //Top left tile
    public int size; //Size of tile group(Square, 2 = 2x2, 3 = 3x3 etc.)
    public ITileObject obj;

    public TileGroup(ITileObject obj)
    {
        this.obj = obj;
    }

    //Remove this object from all tiles in group
    public void removeFromGroup()
    {
        if(baseTile != null)
        {
            MapManager map = baseTile.getMapManager();
            for(int x = 0; x < size; x++)
            {
                for(int y = 0; y < size; y++)
                {
                    MapTile currentTile = map.getTile(baseTile.tileX + x, baseTile.tileY + y);
                    if (currentTile == null)
                        continue;
                    currentTile.setMapObject(null);
                }
            }
        }
        baseTile = null;
        size = 0;
    }

    //Add object to all tiles in group
    private void addToGroup()
    {
        if(baseTile != null)
        {
            MapManager map = baseTile.getMapManager();
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    MapTile currentTile = map.getTile(baseTile.tileX + x, baseTile.tileY + y);
                    if (currentTile == null)
                        continue;
                    currentTile.setMapObject(obj);
                }
            }
        }
    }

    public void setGroup(MapTile baseTile, int size, bool position=true)
    {
        removeFromGroup();

        if(baseTile == null)
            return;

        this.baseTile = baseTile;
        this.size = size;
        addToGroup();

        //Move to middle of tile group
        if(position)
        {
            //Since the tiles are side by side, we don't need to get all their actualy positions(1 tile always = 1 unit)
            float offset = (size / 2f) - 0.5f;
            obj.getGameObject().transform.parent = baseTile.transform;
            obj.getGameObject().transform.localPosition = new Vector3(offset, offset);
        }
    }

}