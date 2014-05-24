using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MessageBox : MonoBehaviour {

    public UILabel title;
    public UITextList message;
    public UIButton button;

    private Dictionary<GameObject, bool> disabledStates = null;

    public static MessageBox createMessageBox(string title, string message, GameObject disableInput = null)
    {
        Dictionary<GameObject, bool> oldState = null;
        if(disableInput != null)
        {
            //Disable input before creating messagebox(Allow input on messagebox in case parent is disabled)
             oldState = ChildrenChangeColliderState.setNewState(disableInput, false);
        }

        //Find a panel to hold the MessageBox
        GameObject panel = GameObject.FindGameObjectWithTag("MessagePanel");
        if (panel == null)
        {
            Debug.LogError("Unable to find a panel to place MessageBox on!");
            return null;
        }

        GameObject newObj = NGUITools.AddChild(panel, GUIPrefabs.getGUIPrefabs().MessageBox);
        MessageBox messageBox = newObj.GetComponent<MessageBox>();

        //Move it towards the camera so that hitbox is always on top
        messageBox.transform.localPosition = new Vector3(messageBox.transform.localPosition.x, messageBox.transform.localPosition.y, -40);

        messageBox.setTitle(title);
        messageBox.setText(message);

        if (disableInput != null)
            messageBox.disabledStates = oldState;

        return messageBox;
    }

    public void setTitle(string newTitle)
    {
        title.text = newTitle;
    }

    public void setText(string newMessage)
    {
        message.Clear();
        message.Add(newMessage);
    }

    public void addText(string newMessage)
    {
        message.Add(newMessage);
    }

    public void setButtonText(string text)
    {
        UILabel label = button.GetComponentInChildren<UILabel>();
        if (label == null)
            return;
        label.text = text;
    }

    public void setButtonEnabled(bool enabled)
    {
        button.collider.enabled = enabled;
    }

    public void setButtonVisible(bool visible)
    {
        button.enabled = visible;
    }

    public void OnClose()
    {
        if (disabledStates != null)
            ChildrenChangeColliderState.revertState(disabledStates);
        Destroy(gameObject);
    }
}
