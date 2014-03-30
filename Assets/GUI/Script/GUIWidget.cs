using UnityEngine;
using System.Collections;

//All GUI elements must use this as a base to correctly interact with a Panel
//This is used for ordering and resizing/fitting.

public class GUIWidget : MonoBehaviour {

    protected GUIPanel panel; //The panel that this widget belongs to
    public bool fitCollisionToSprite = true; //Fit Collision box to sprite bounds

    [SerializeField]
    protected int order; //Order on the panel(Used when interacting with widgets that overlap)

    public virtual void doFitCollisionToSprite()
    {
        if(fitCollisionToSprite == false)
            return;

        SpriteRenderer spr;
        BoxCollider2D col = collider2D as BoxCollider2D;
        if(col == null)
            return;
        spr = renderer as SpriteRenderer;
        if(spr == null)
            return;


        col.size = new Vector2(spr.bounds.size.x / transform.lossyScale.x, spr.bounds.size.y / transform.lossyScale.y);
    }

    //Get the Panel that this widget belongs to
    public virtual GUIPanel getPanel()
    {
        return panel;
    }

    //Set the panel this widget belongs to
    public virtual void setPanel(GUIPanel setPanel)
    {
        panel = setPanel;
    }

    //Returns the interaction order of this Widget on the panel
    public virtual int getOrder()
    {
        return order;
    }

    public virtual void setOrder(int newOrder)
    {
        order = newOrder;
    }

}
