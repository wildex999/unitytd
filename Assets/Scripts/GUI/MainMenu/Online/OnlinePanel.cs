using System.Net.Sockets;
using UnityEngine;

//Watches for disconnects and resets the progress if that happens.
public class OnlinePanel : MonoBehaviour
{
    public UIPanel firstPanel;

    void Start()
    {
        NetManager net = NetManager.getNetManager(true);
        net.DisconnectEvent += OnDisconnect;
    }

    void OnDestroy()
    {
        Debug.Log("Destroyed");
        NetManager net = NetManager.getNetManager(false);
        if (net == null)
            return;
        net.DisconnectEvent -= OnDisconnect;
    }

    private void OnDisconnect(Socket socket, DisconnectCause cause, string message)
    {
        //Mark all panels except the starting one as disabled
        UIPanel[] panels = GetComponentsInChildren<UIPanel>();

        foreach (UIPanel panel in panels)
            panel.gameObject.SetActive(false);

        if (firstPanel == null)
            panels[0].gameObject.SetActive(true);
        else
            firstPanel.gameObject.SetActive(true);

    }
}