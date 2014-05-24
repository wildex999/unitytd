using UnityEngine;
using System.Collections;

public class ButtonGetGameInfo : MonoBehaviour {

    public UILabel gameIdInput;

    void OnClick()
    {
        NetManager net = NetManager.getNetManager(false);
        if (net == null)
            return;

        //Create GetGame message
        MessageGetGame message = new MessageGetGame(gameIdInput.text);
        net.sendMessage(net.mainSocket, message);
        Debug.Log("Sent message");
    }
}
