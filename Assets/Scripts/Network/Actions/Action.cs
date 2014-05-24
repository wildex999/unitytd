using System.IO;

//An action that has been performed, or is to be performed.
//For the local authorative player this is used to delay the local action until the right moment(GameManager FixedUpdate)
//When clients send action request, or the server informs the clients of actions, this is used as well.

//Adding actions procedure:
//1. Create class inheriting from Action
//2. Add to Actions enum(below)
//3. Add to parseAction switch(below)

public enum Actions
{
    PlaceTower
}

public abstract class Action
{
    public Actions action;

    //Player who sent this action(Note: Not necessarily then one who initiated it. I.e, client->server(player = client), server->clients(player = server) )
    //Thus if player = server, can be used to check if the action is authorative or now. If you require initial player, store it as a custom value.
    public Player player; 
    public GameManager game;

    public Action(Actions action, Player player, GameManager game = null)
    {
        this.action = action;
        this.player = player;
        this.game = game;
    }

    //Valid: whether or not the action is valid. If false, ignore action
    public static Action parseAction(BinaryReader stream, Actions action, Player player, GameManager game, out bool valid)
    {
        switch(action)
        {
            case Actions.PlaceTower:
                return new ActionPlaceTower(stream, action, player, game, out valid);

        }
        valid = false;
        return null;
    }

    //Perform the described action
    public abstract void run();

    //Write to Binary Stream(Network/Recording)
    public abstract byte[] writeAction(BinaryWriter stream);
}