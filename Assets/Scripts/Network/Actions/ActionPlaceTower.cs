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
        //Permission check
        if(!game.isAuthorative)
        {
            if(player != game.gameHost) //Only host is allowed to send this to players
            {
                valid = false;
                return;
            }
        }

        //Read the stream
        //Tile
        int tileX = stream.ReadInt32();
        int tileY = stream.ReadInt32();
        tile = game.getMap().getTile(tileX, tileY);
        if(tile == null)
        {
            Debug.LogError("Could not get tile for " + tileX + " | " + tileY);
            valid = false;
            return;
        }

        //Player
        int placingPlayerId = stream.ReadInt32();
        placingPlayer = game.getPlayer(placingPlayerId);
        if(placingPlayer == null)
        {
            Debug.LogError("Could not get player: " + placingPlayerId);
            valid = false;
            return;
        }

        //Tower
        int prefabId = stream.ReadInt32();
        GameObject prefab = Library.instance.getPrefab(prefabId);
        if(prefab == null)
        {
            Debug.LogError("Could not get prefab: " + prefabId);
            valid = false;
            return;
        }

        tower = prefab.GetComponent<TowerBase>();
        if(tower == null)
        {
            Debug.LogError("Could not get TowerBase from prefab: " + prefab);
            valid = false;
            return;
        }

        valid = true;
    }

    public override void run()
    {
        MapManager map = game.getMap();

        //Debug.Log("TOWER PLACE ON STEP: " + game.getCurrentFixedStep());
        Monster lastMonster = Monster.monsters.Last.Value;
        //Debug.Log("Monster count: " + Monster.monsters.Count + " Last Monster: " + lastMonster + " X: " + lastMonster.getFixedPosition().x + " Y: " + lastMonster.getFixedPosition().y);

        //If we are server
        if(game.isAuthorative) 
        {
            if (map.authorativePlaceTower(tile, tower, placingPlayer))
            {
                //Broadcast to players
                game.outgoingActions.Add(new ActionPlaceTower(game, tile, tower, placingPlayer)); //Can we just reuse the current action(this)?
            }
            else
            {
                if(player != game.thisPlayer)
                {
                    //Send failure message to client
                    Action failureAction = new ActionPlaceTowerFailed(game, tile);
                    game.getNetManager().sendAction(failureAction, placingPlayer);
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
    //userid(int) - Original placing player
    //prefab id(int) - Tower prefab
    //Tower data?(byte[])
    public override byte[] getBytes()
    {
        MemoryStream memory = new MemoryStream(16);
        BinaryWriter stream = new BinaryWriter(memory);

        stream.Write(tile.tileX);
        stream.Write(tile.tileY);
        stream.Write(player.getId());
        stream.Write(tower.prefabId);
        //stream.Write(tower.getBytes());
        return memory.GetBuffer();
    }
}