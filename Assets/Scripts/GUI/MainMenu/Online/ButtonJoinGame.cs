using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ButtonJoinGame : MonoBehaviour
{

    public UILabel gameIdInput;

    private MessageBox joinProgressBox;
    private NetManager net;
    private Dictionary<GameObject, bool> oldState; //Old state of buttons

    private bool downloadingMap = false;
    private int mapDownloadTimeout = 60; //How many seconds to wait for map download to finish(Map is sent as one message for now)
    private DateTime mapDownloadStart; //Map download timeout check

    private MessageJoinGameReply replyMessage; //The reply message from joining, containing all the game data we need

    void OnClick()
    {
        net = NetManager.getNetManager(false);
        if (net == null || !net.isLoggedIn())
            return;

        Debug.Log("ClickJoin");

        //Listen for disconnects during the process
        net.DisconnectEvent += OnDisconnect;

        //Disable buttons to avoid double joining and other conflicts
        oldState = ChildrenChangeColliderState.setNewState(transform.parent.gameObject, false);

        //Send Join request
        MessageJoinGame message = new MessageJoinGame(gameIdInput.text);
        net.sendMessage(net.mainSocket, message);

        //Show player the progress
        joinProgressBox = MessageBox.createMessageBox("Joining game", "Sent request to join: " + gameIdInput.text + ".");
        joinProgressBox.setButtonEnabled(false);

        MessageJoinGameReply.messageEvent += onJoinGameReply;

    }

    void OnDestroy()
    {
        //Clean up events
        MessageJoinGameReply.messageEvent -= onJoinGameReply;
        MessageMapDownload.messageEvent -= onMapDownloaded;

        if (net != null)
            net.DisconnectEvent -= OnDisconnect;
    }

    void Update()
    {
        //Check for timeout
        if(downloadingMap)
        {
            if((DateTime.Now - mapDownloadStart).TotalSeconds > mapDownloadTimeout)
            {
                downloadingMap = false;
                MessageMapDownload.messageEvent -= onMapDownloaded;
                //TODO: Leave game with error
                joinProgressBox.addText("Error: Map download timed out!");
                onError();
            }
        }
    }

    //Something went wrong, allow closing window etc.
    void onError()
    {
        if(oldState != null)
        {
            ChildrenChangeColliderState.revertState(oldState);
            oldState = null;
        }
        joinProgressBox.setButtonEnabled(true);
        gameObject.collider.enabled = true;
    }

    void OnDisconnect(System.Net.Sockets.Socket socket, DisconnectCause cause, string message)
    {
        onError();

        if (joinProgressBox != null)
            Destroy(joinProgressBox.gameObject);
    }


    void onJoinGameReply(MessageJoinGameReply message)
    {
        MessageJoinGameReply.messageEvent -= onJoinGameReply;

        //TODO: Move map loading/downloading/waiting into the GameManager, so that we can receive and handle
        //Other messages before map download(Like map change, player leave/join etc.) which is currently handled by
        //the game manager.
        //TODO: What happens if the authorative player sends a map change message while we are waiting on this map?
        //Should do: Abort current map download, start next one.

        if (message.joinedGame() == false)
        {
            joinProgressBox.addText("Failed to join game: " + message.getErrorMessage());
            onError();
        }
        else
        {
            joinProgressBox.addText("Game Joined!");

            replyMessage = message;

            if(message.getMapId() == 0)
            {
                //If MapId is 0, this is a custom, unpublished map. 
                //The authorative player should automaticly send it to us as we join.
                joinProgressBox.addText("Downloading custom map from host...");
                mapDownloadStart = DateTime.Now;
                downloadingMap = true;

                MessageMapDownload.messageEvent += onMapDownloaded;
            }
            else
            {
                //Check if we have map cached, if not then download it
                //TODO:
            }


        }

    }

    //Called once the client has the map
    void onMapDownloaded(MessageMapDownload message)
    {
        //Verify it's from the host
        if(message.senderPlayerId != -1 && message.senderPlayerId != replyMessage.getOwner().getId())
        {
            Debug.LogError("Got Map Download from player who is neither Main Server nor Host: " + message.senderPlayerId);
            return;
        }

        Debug.Log("Download complete");
        MessageMapDownload.messageEvent -= onMapDownloaded;
        joinProgressBox.addText("Download complete! Loading...");
        downloadingMap = false;
        //Get map info
        MapInfo map = null;
        try
        {
            map = MapBase.readMapHeader(message.getMapData());
        }
        catch(Exception ex)
        {
            joinProgressBox.addText("Error loading map: " + ex.Message);
            onError();
            return;
        }

        map.mapData = message.getMapData();

        //Create GameManager object that survives a scene change.
        //Set the game data for the Game.
        GameManager game = GameManager.createGameManager(net, false, null, replyMessage.getGameName(), replyMessage.getMaxPlayers());
        game.loadMapOnSceneChange(map);

        //Set-up game
        game.onJoin(replyMessage);

        Application.LoadLevel(1);
    }
}
