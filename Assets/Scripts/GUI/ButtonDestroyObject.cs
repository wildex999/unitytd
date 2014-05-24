using UnityEngine;
using System.Collections;

//Destroy a given object on click

public class ButtonDestroyObject : MonoBehaviour
{

    public GameObject target;

    public void OnClick()
    {
        if(target != null)
        {
            Destroy(target.gameObject);
        }
    }
}
