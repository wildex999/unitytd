using System.Net.Sockets;

//Player in a game
public class Player
{
    //Different states the player can be in before, during and after a game
    public enum PlayerState
    {
        Offline, //Used for single-player
        Online, //Online, but not in any game(Logged in)
        Joining, //The state the player is in when joining the game. The player is at this point determining whether it needs to download the map or not.
        DownloadingMap, //The player is downloading the map(Either from the main server, or from the hosting player if map id = 0)
        NotReady, //The player has not readied during preGame(If joining a running game, this is skipped)
        Ready, //Ready
        Playing, //Currently playing
        Leaving //Leaving the game(Currently not synced online). Might be used in the future if leaving cleanly requires some action before leaving(Saving).
    }

    private string name;
    private int id;
    private PlayerState state;
    private static Player localPlayer;

    public delegate void PlayerStateChangedHandler(Player player, Player.PlayerState newState);
    public event PlayerStateChangedHandler PlayerStateChangedEvent;

    public Player(string name, int id, Player.PlayerState state = Player.PlayerState.Offline)
    {
        this.name = name;
        this.id = id;
    }

    public string getName()
    {
        return name;
    }

    public void setName(string name)
    {
        this.name = name;
    }

    public int getId()
    {
        return id;
    }

    public void setId(int newId)
    {
        id = newId;
    }

    public PlayerState getState()
    {
        return state;
    }

    public void setState(Player.PlayerState state)
    {
        //Call event listeners
        if (PlayerStateChangedEvent != null)
            PlayerStateChangedEvent(this, state);
        this.state = state;
    }

    public static void setLocalPlayer(Player player)
    {
        localPlayer = player;
    }

    //Return the currently signed/logged in player
    public static Player getLocalPlayer()
    {
        if(localPlayer == null)
        {
            //Create default player
            localPlayer = new Player("LocalPlayer", -1, Player.PlayerState.Offline);
        }
        return localPlayer;
    }
}