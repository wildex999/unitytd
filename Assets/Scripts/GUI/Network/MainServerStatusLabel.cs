using UnityEngine;
using System.Collections;

//Updates the label this component if bound to, with the current main server status

public class MainServerStatusLabel : MonoBehaviour {
    private NetManager net;
    private UILabel label;

    private Color colorError = Color.red;
    private Color colorWaiting = Color.yellow;
    private Color colorOk = Color.green;

    private void GetMainServer()
    {
        //Try to find object with NetBase
        if (net == null)
            net = NetManager.getNetManager(false);
    }

    void Start()
    {
        //Get instance of label
        label = GetComponent<UILabel>();
        if (label == null)
            Debug.LogError("Failed to get Label");
    }
	
	// Update is called once per frame
	void Update () {
        GetMainServer();
        if (net == null)
        {
            label.text = "No Network object found.";
            label.color = colorError;
            return;
        }


        //Update label with status
        switch(net.mainState)
        {
            case MainServerState.NotConnected:
                label.text = "Not Connected";
                label.color = colorWaiting;
                break;
            case MainServerState.Disconnected:
                label.text = "Disconnected";
                label.color = colorWaiting;
                break;
            case MainServerState.ConnectionFailed:
                label.text = "Disconnected: " + net.mainServerError;
                label.color = colorError;
                break;
            case MainServerState.Connecting:
                label.text = "Connecting...";
                label.color = colorWaiting;
                break;
            case MainServerState.Connected:
                label.text = "Connected";
                label.color = colorOk;
                break;
            case MainServerState.LoggedIn:
                label.text = "Logged in! (" + net.getUsername() + ")";
                label.color = colorOk;
                break;
        }
	}
}
