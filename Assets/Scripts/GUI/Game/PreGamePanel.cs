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

        return panel;
    }

    void OnDestroy()
    {
        if (game != null && game.getCurrentChatOutput() == chatOutput)
            game.setCurrentChatOutput(null);
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
            if (player == game.gameHost)
                title = "[Host]";
            playersString += "- " + title + player.getName() + "\n";
        }
        playersList.text = playersString;
        
    }
}