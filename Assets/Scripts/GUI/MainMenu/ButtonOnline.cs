using UnityEngine;
using System.Collections;

public class ButtonOnline : MonoBehaviour {

    void OnClick()
    {
        //Create NetManager if it doesn't exist
        NetManager manager = NetManager.getNetManager();
    }
}
