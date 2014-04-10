using UnityEngine;
using System.Collections;

//Check if connected to main server, and enable button if so

public class ButtonMultiplayer : MonoBehaviour {

    private NetManager manager;

    public UILabel username;
    public GameObject enableObject;
    public GameObject disableObject;
	
	// Update is called once per frame
	void Update () {
        if (manager == null)
            manager = NetManager.getNetManager(true);
        if (manager == null)
            return;

        if (manager.mainState == MainServerState.Connected)
            collider.enabled = true;
        else
            collider.enabled = false;
	}

    void OnClick()
    {
        //Login with username
        string name = "Default";
        if (username == null)
            Debug.LogError("Could not find label with username. Using default.");
        else
            name = username.text;
        if (name.Length < 3)
        {
            username.text = "Error: Must be 3 or more characters!";
            return;
        }

        if (enableObject != null)
            enableObject.SetActive(true);
        if (disableObject != null)
            disableObject.SetActive(false);

        manager.login(name);
    }
}
