using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameInfoLabel : MonoBehaviour {
    public UILabel label;

    void Start()
    {
        label.text = "Press \"Get Info\" to get info about game with the given name.";
    }

	// Update is called once per frame
	void Update () {
        NetManager net = NetManager.getNetManager(false);
        if (net == null)
            return;
        Debug.Log("Update");
        //net.GetGameReplyEvent += getGameReplyHandler;
        MessageGetGameReply.messageEvent += getGameReplyHandler;

        //Stop calling update on script
        GetComponent<GameInfoLabel>().enabled = false;
	}

    private void getGameReplyHandler(MessageGetGameReply message)
    {
        if(!message.gotGame())
        {
            label.text = "No game with that name was found.";
            return;
        }

        List<Player> players = message.getPlayers();
        label.text = "Owner: " + message.getOwner().getName() + "\n" +
            "Map: " + message.getMapName() + "\n" +
            "Players(" + (players.Count+1) + "/" + message.getMaxPlayers() + ")";
        label.text += "\n    -" + message.getOwner().getName();
        foreach(Player player in players)
            label.text += "\n    -" + player.getName();
    }
}
