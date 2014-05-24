using System.IO;
using UnityEngine;


public class ActionPlaceTower : Action
{
    private MapTile tile;
    private TowerBase tower;
    private Player placingPlayer; //Player placing the tower(Not always equal to Action player. As the clients will se this action as coming from the server)

    //Create action
    public ActionPlaceTower(GameManager game, MapTile tile, TowerBase tower, Player placingPlayer)
        : base(Actions.PlaceTower, game.thisPlayer, game)
    {
        this.tile = tile;
        this.tower = tower;
        this.placingPlayer = placingPlayer;
    }

    //Parse action
    public ActionPlaceTower(BinaryReader stream, Actions action, Player player, GameManager game, out bool valid)
        : base(action, player, game)
    {
        valid = true;
    }

    public override void run()
    {
        MapManager map = game.getMap();

        //If we are server
        if(game.isAuthorative) 
        {
            if (map.authorativePlaceTower(tile, tower, placingPlayer))
            {
                //Broadcast to players
            }
            else
            {
                if(player != game.thisPlayer)
                {
                    //TODO: Send failure message to client
                }
            }
        }
        else //If we are client
        {
            if(player == game.gameHost)
            {
                game.getMap().serverPlaceTower(tile, tower, placingPlayer);
            }
            else
            {
                Debug.LogError("Got PlaceTower from non-authorative player!");
            }
        }
    }

    //Serialized format:
    //tileX (int)
    //tileY (int)
    //prefab id(int) - Tower prefab
    //userid(int) - Original placing player
    public override byte[] writeAction(System.IO.BinaryWriter stream)
    {
        throw new System.NotImplementedException();
    }
}