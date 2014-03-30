using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * This will handle the input on any GUI element on the current scene.
 * All GUI Panels should be a child of the GUIHandler GameObject.
 * */

public class GUIHandler : MonoBehaviour {
    public Camera guiCamera;

    void OnMouseDown()
    {
        //Use RayCasting to figure out which object that was clicked.
        //First figure out the top Panel(Highest order)(Clicks can not go through panels)
        //Then sort the widgets
        Vector3 mousePos = guiCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
        SortedList<GUIWidget, IClickHandler> handlers = new SortedList<GUIWidget, IClickHandler>(new SortedListOrderCompare());
        GUIPanel topPanel = null;

        //Get top Panel
        foreach(RaycastHit2D hit in hits)
        {
            GUIWidget widget = hit.collider.GetComponent<GUIWidget>();
            if (widget == null)
                continue;
            GUIPanel panel = widget.getPanel();
            if (panel == null)
            {
                panel = widget as GUIPanel;
                if(panel == null)
                    continue;
            }

            if (topPanel == null || panel.getOrder() >= topPanel.getOrder()) //If they are of equal order, the last one in the list will be top
                topPanel = panel;
        }

        //Debug.Log("Top Panel:" + topPanel.getOrder());

        //Get widgets in top Panel
        foreach(RaycastHit2D hit in hits)
        {
            GUIWidget widget = hit.collider.GetComponent<GUIWidget>();
            if (widget == null)
                continue;

            if (widget.getPanel() != topPanel)
                continue;

            IClickHandler handler = widget as IClickHandler;
            if (handler == null)
                continue;

            handlers.Add(widget, handler);
        }

        //Debug.Log("Handlers: " + handlers.Count);

        //Call them as ordered until one handles the click
        foreach(KeyValuePair<GUIWidget, IClickHandler> handler in handlers)
        {
            if (handler.Value.onClick())
            {
                //Debug.Log("Handled by: " + handler.Key);
                break;
            }
        }
    }
}
