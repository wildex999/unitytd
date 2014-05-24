using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonCreateGame : MonoBehaviour
{

    public UILabel gameIdInput;

    private MessageBox createProgressBox;
    private NetManager net;
    private Dictionary<GameObject, bool> oldState; //Old state of buttons
    private MapInfo map;

    private int maxPlayers;
    private string gameName;

    void OnClick()
    {
        net = NetManager.getNetManager(false);
        if (net == null || !net.isLoggedIn())
            return;

        //Listen for disconnects during the process
        net.DisconnectEvent += OnDisconnect;

        //Disable buttons to avoid double creating and other conflicting actions
        oldState = ChildrenChangeColliderState.setNewState(transform.parent.gameObject, false);

        //Ask user to choose map
        MapSelector selector = MapSelector.createMapSelector(transform.parent.gameObject, MapSelectorType.Open);
        selector.SelectMapEvent += onMapSelected;
    }

    void OnDisconnect(System.Net.Sockets.Socket socket, DisconnectCause cause, string message)
    {
        if (createProgressBox != null)
            Destroy(createProgressBox.gameObject);

        ChildrenChangeColliderState.revertState(oldState);
    }

    void onMapSelected(MapInfo item)
    {
        if(item == null)
        {
            ChildrenChangeColliderState.revertState(oldState);
            return;
        }

        map = item;

        //Create CreateGame message
        maxPlayers = item.recommendedPlayers; //TODO: Allow player to set max players
        gameName = gameIdInput.text;
        MessageCreateGame message = new MessageCreateGame(gameName, maxPlayers, "Coop", 0, item.name);
        net.sendMessage(net.mainSocket, message);

        //Show player the progress
        createProgressBox = MessageBox.createMessageBox("Creating game", "Sent request to create new game...");
        createProgressBox.setButtonEnabled(false);

        //net.CreateGameEvent += onCreateGameReply;
        MessageCreateGameReply.messageEvent += onCreateGameReply;
    }

    void onCreateGameReply(MessageCreateGameReply message)
    {
        //net.CreateGameEvent -= onCreateGameReply;
        MessageCreateGameReply.messageEvent -= onCreateGameReply;

        if(message.gameIsCreated() == false)
        {
            createProgressBox.addText("Failed to create game: " + message.getErrorMessage());
            createProgressBox.setButtonEnabled(true);
            gameObject.collider.enabled = true;
            ChildrenChangeColliderState.revertState(oldState);
        }
        else
        {
            createProgressBox.addText("Game created!");
            createProgressBox.addText("Loading...");

            //Create GameManager object that survives a scene change.
            //Set the game data for the Game.
            GameManager game = GameManager.createGameManager(net, true, null, gameName, maxPlayers);
            game.loadMapOnSceneChange(map);
            Application.LoadLevel(1);
        }
    }
}
