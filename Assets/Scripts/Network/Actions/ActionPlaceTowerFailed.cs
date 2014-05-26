using System.IO;
using UnityEngine;


//Notifies player that their attempt to place a tower failed(Remove dummy)

public class ActionPlaceTowerFailed : Action
{
    private MapTile tile;

    //Create action
    public ActionPlaceTowerFailed(GameManager game, MapTile tile)
        : base(Actions.PlaceTowerFailed, game.thisPlayer, game)
    {
        this.tile = tile;
    }

    //Parse action
    public ActionPlaceTowerFailed(BinaryReader stream, Actions action, Player player, GameManager game, out bool valid)
        : base(action, player, game)
    {
        //Permission check
        if (!game.isAuthorative)
        {
            if (player != game.gameHost) //Only host is allowed to send this to players
            {
                valid = false;
                return;
            }
        }

        //Read the stream
        int tileX = stream.ReadInt32();
        int tileY = stream.ReadInt32();

        tile = game.getMap().getTile(tileX, tileY);
        if(tile == null)
        {
            Debug.LogError("Unable to get tile for Tower place failure(" + tileX + " | " + tileY);
            valid = false;
            return;
        }

        valid = true;
    }

    public override void run()
    {
        MapManager map = game.getMap();

        //Check if there is still a dummy there(Might have been replaced with something else)
        ITileObject obj = tile.getMapObject();
        DummyTower tower = obj as DummyTower;
        if (tower != null)
            tower.removeTower(true);
    }

    //Serialized format:
    //tileX (int)
    //tileY (int)
    public override byte[] getBytes()
    {
        MemoryStream memory = new MemoryStream(8);
        BinaryWriter stream = new BinaryWriter(memory);

        stream.Write(tile.tileX);
        stream.Write(tile.tileY);
        return memory.GetBuffer();
    }
}