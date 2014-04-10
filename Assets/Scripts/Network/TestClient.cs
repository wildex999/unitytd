using UnityEngine;
using System.Collections;

public class TestClient : MonoBehaviour {
    private NetworkGame game;
    private NetManager manager;
	
    void Create()
    {

    }

    //Start trying to join a named custom game(Async)
    //Returns false on early error
    public delegate void OnJoinGame(bool success);
    public bool joinCustomGame(string name, OnJoinGame callback)
    {
        //Joining a game involves multiple steps. First ask the main server if the game exists and get the game info(Ip, map, players etc.)
        //Then connect to the game server ip and ask if it's possible to join
        //Call the given callback with the results
        return true;
    }

    //Leave the current game
    public bool leaveGame()
    {
        return true;
    }
}
