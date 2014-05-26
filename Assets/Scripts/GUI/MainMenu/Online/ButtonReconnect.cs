using UnityEngine;
using System.Collections;

//Button enabling the user to reconnect when the connection to the main server is lost

public class ButtonReconnect : MonoBehaviour
{
    private NetManager manager;
    private UIWidget[] renderers;
    private bool prevState = true;

    void Start()
    {
        renderers = GetComponentsInChildren<UIWidget>();
    }

    // Update is called once per frame
    void Update()
    {
        if (manager == null)
            manager = NetManager.getNetManager(true);
        if (manager == null)
            return;

        if (manager.mainState == MainServerState.Connecting || manager.mainState == MainServerState.Connected || manager.isLoggedIn())
        {
            if (prevState == false)
                return;

            collider.enabled = false;
            foreach (UIWidget renderer in renderers)
                renderer.enabled = false;
            prevState = false;
        }
        else
        {
            if (prevState == true)
                return;

            collider.enabled = true;
            foreach (UIWidget renderer in renderers)
                renderer.enabled = true;
            prevState = true;
        }
    }

    void OnClick()
    {
        if (manager == null)
            return;

        manager.connectToMainServer(NetManager.mainIp, NetManager.mainPort);
    }
}
