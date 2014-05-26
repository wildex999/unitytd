using System.Collections.Generic;
using UnityEngine;

public class PreGamePanel : MonoBehaviour
{
    private GameManager game;

    //GUI
    public UILabel gameName;
    public UILabel mapName;
    public UILabel mapDescription;
    public UILabel playersList;
    public UITextList chatOutput;
    public UILabel okButtonLabel;
    public UISprite okButtonBackground;

    private Dictionary<int, bool> playerReadyState = new Dictionary<int, bool>();
    private bool ourReadyState = false;

    private static Color readyColor = Color.green;
    private static Color notReadyColor = Color.yellow;
    private static string readyText = "Ready";
    private static string notReadyText = "Not Ready";

    //Events
    //LaunchGame
    
    public static PreGamePanel createPreGamePanel(GameObject parent, GameManager game)
    {
        GameObject inst = NGUITools.AddChild(parent, GUIPrefabs.getGUIPrefabs().PreGamePanel);
        PreGamePanel panel = inst.GetComponent<PreGamePanel>();

        if(panel == null)
            Debug.LogError("Failed to create PreGamePanel!");

        //Show the current info
        panel.game = game;
        game.setCurrentChatOutput(panel.chatOutput);
        panel.infoUpdated();

        if (game.isAuthorative)
            panel.okButtonLabel.text = "Launch";
        else
        {
            panel.okButtonBackground.color = notReadyColor; //Yellow when not ready(default)
            panel.okButtonLabel.text = notReadyText;
        }

        //Set-up events
        MessageSetReady.messageEvent += panel.onReadyStateChange;
        MessagePlayerLeft.messageEvent += panel.onPlayerLeave;

        return panel;
    }

    void Update()
    {
        //Make sure the button color stays, as hovering resets it, this disables the hover coloring.
        if (ourReadyState == true)
            okButtonBackground.color = readyColor;
        else
            okButtonBackground.color = notReadyColor;
    }

    void OnDestroy()
    {
        if (game != null && game.getCurrentChatOutput() == chatOutput)
            game.setCurrentChatOutput(null);

        //Clean-up events
        MessageSetReady.messageEvent -= onReadyStateChange;
        MessagePlayerLeft.messageEvent -= onPlayerLeave;
    }

    //Ready/Launch button pressed
    void OnReadyButtonPressed()
    {
        if(game.isAuthorative)
        {
            game.setGameState(GameState.Running);
            return;
        }

        if (ourReadyState == false) //Not ready
        {
            ourReadyState = true;
            okButtonLabel.text = readyText;
        }
        else
        {
            ourReadyState = false;
            okButtonLabel.text = notReadyText;
        }

        //Broadcast state change
        MessageSetReady readyMessage = new MessageSetReady(ourReadyState);
        MessageBroadcast broadcast = new MessageBroadcast(readyMessage, false);
        NetManager net = game.getNetManager();
        if (net == null)
            return;
        net.sendMessage(net.mainSocket, broadcast);

        //Update local view
        playerReadyState[game.thisPlayer.getId()] = ourReadyState;
        infoUpdated();
    }

    void onReadyStateChange(MessageSetReady message)
    {
        playerReadyState[message.senderPlayerId] = message.getReadyState();
        infoUpdated();
    }

    void onPlayerLeave(MessagePlayerLeft message)
    {
        //Reset the ready state of player who leaves
        playerReadyState.Remove(message.senderPlayerId);
        infoUpdated();
    }

    //Refresh the info if there has been a change to players, map or game.
    public void infoUpdated()
    {
        if (gameName == null || game == null)
            return;

        gameName.text = game.getGameName();

        if(game.getMap() == null)
        {
            mapName.text = "Map: None";
            mapDescription.text = "No map has been loaded!";
        }

        MapInfo mapInfo = game.getMap().getMapInfo();
        mapName.text =  "Map: " + mapInfo.name;
        mapDescription.text = mapInfo.description;

        List<Player> players = game.players;
        string playersString = "";
        foreach(Player player in players)
        {
            //TODO: Color if ready etc.
            string title = "";
            string ready = "[FF0000]NR[-]";
            if (player == game.gameHost)
            {
                title = "[Host]";
                ready = ""; //No Ready state on Owner
            }
            else
            {
                bool readyState;
                if (!playerReadyState.TryGetValue(player.getId(), out readyState))
                {
                    readyState = false;
                    playerReadyState[player.getId()] = false; //Default to false
                }
                if (readyState == true)
                    ready = "[00FF00]R[-]";
            }

            playersString += "- " + title + player.getName() + "(" + ready + ")\n";
        }
        playersList.text = playersString;
        
    }
}