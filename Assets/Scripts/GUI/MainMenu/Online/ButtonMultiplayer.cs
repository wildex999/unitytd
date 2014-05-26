using UnityEngine;
using System.Collections;

//Check if connected to main server, and enable button if so

public class ButtonMultiplayer : MonoBehaviour {

    private NetManager manager;

    public UIInput username;
    public GameObject enableObject;
    public GameObject disableObject;
	
	// Update is called once per frame
	void Update () {
        if (manager == null)
            manager = NetManager.getNetManager(true);
        if (manager == null)
            return;

        if(manager.mainState == MainServerState.LoggedIn)
        {
            //Go on to the next step
            if (enableObject != null)
                enableObject.SetActive(true);
            if (disableObject != null)
                disableObject.SetActive(false);
        }

        if (manager.mainState == MainServerState.Connected)
            collider.enabled = true;
        else
            collider.enabled = false;
	}

    void OnClick()
    {
        if (!collider.enabled) //Stop Input field onSubmit from skipping the disabled button
            return;

        //Login with username
        string name = "Default";
        if (username == null)
            Debug.LogError("Could not find username input. Using default.");
        else
            name = username.text;

        if (name.Length < 3)
        {
            MessageBox.createMessageBox("Error", "Username must be at least 3 characters long!");
            return;
        }

        if (enableObject != null)
            enableObject.SetActive(true);
        if (disableObject != null)
            disableObject.SetActive(false);

        manager.login(name);
    }
}
