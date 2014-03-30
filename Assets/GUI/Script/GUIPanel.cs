using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * A panel contains multiple other GUI elements(Buttons, panels etc.)
 * Clips and hides anything outside the panel, allowing for scrolling of elements.
 * Note: When doClip = true, forces every child to use a shared material with custom clipping shader(Diffuse)
 * */

[ExecuteInEditMode]
public class GUIPanel : GUIWidget {

    [SerializeField]
    private bool _doClip = true;

    [SerializeField]
    private bool _disableHidden = false;

    [SerializeField]
    private bool _clipSizeSprite = true; //Make clip size cover sprite

    [SerializeField]
    private Vector2 _clipSize = new Vector2(1f, 1);

    private struct ChildState
    {
        Material material;
    }
    private Dictionary<int, ChildState> previousChildState = new Dictionary<int, ChildState>(); //Keeps the previous state of a child object, allowing us to revert it

    private BoxCollider2D panelCollider; 

	// Use this for initialization
	void Start () {
        panelCollider = GetComponent<BoxCollider2D>();
        renderer.material = renderer.material;
        refresh();
	}

    void Update()
    {
        #if UNITY_EDITOR
        refresh(); //Move clipping when object moves
        #endif
    }

    public override int getOrder()
    {
        //Panels can have other panels as parents, if so thir order are the parents order + order
        if (panel == null)
            return order;
        return panel.getOrder() + order;
    }

    //Force it to recheck values and children
    public void refresh()
    {
        if (_clipSizeSprite)
            _clipSize = new Vector2(renderer.bounds.size.x, renderer.bounds.size.y);

        //Make sure the collision box covers the clip
        panelCollider.size = new Vector2(_clipSize.x / transform.lossyScale.x, _clipSize.y / transform.lossyScale.y);

        //Go through every child
        recursiveChildUpdate(transform);
        recursiveChildMaterial(transform, renderer.sharedMaterial);
        refreshMove();
    }

   /* void OnMouseDown()
    {
        //Detect click on itself and children
        //If click is outside clip, ignore!
        //TODO: list of in-clip objects, thus avoiding checking everything thats hidden
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 clickPoint = Camera.allCameras[0].ScreenToWorldPoint(Input.mousePosition);
            if (clickPoint.x > transform.position.x - _clipSize.x / 2f && clickPoint.y > transform.position.y - _clipSize.y / 2f &&
                    clickPoint.x < transform.position.x + _clipSize.x / 2f && clickPoint.y < transform.position.y + _clipSize.y / 2f)
            {
                recursiveChildClick(gameObject.transform, new Vector2(clickPoint.x, clickPoint.y));
            }
        }
    }*/

    //Add any children to the panel
    private void recursiveChildUpdate(Transform currentParent)
    {
        foreach(Transform childTransform in currentParent)
        {
            GUIWidget widget = childTransform.GetComponent<GUIWidget>();
            if(widget != null)
                widget.setPanel(this);
            recursiveChildUpdate(childTransform);
        }
    }

    private void recursiveChildMaterial(Transform currentParent, Material setMaterial)
    {
        foreach(Transform childTransform in currentParent)
        {
            //Set material and shader used by child to be the same as the top parent
            Renderer childRenderer = childTransform.renderer;
            if (childRenderer != null)
            {
                if (_doClip)
                {
                    if (childRenderer is SpriteRenderer)
                    {
                        //Move over the texture(Avoid leaking)
                        Texture tex = childRenderer.sharedMaterial.mainTexture;
                        childRenderer.material = setMaterial;
                        childRenderer.sharedMaterial.mainTexture = tex;
                    }
                    else
                    {
                        //Give the shader the values if it accepts them
                        childRenderer.sharedMaterial.SetVector("_clipRect", new Vector4(transform.position.x - _clipSize.x / 2f, transform.position.y - _clipSize.y / 2f, _clipSize.x, _clipSize.y));
                    }
                }
                //TODO: Revert to default sprite material
                /*else
                    childRenderer.material = new Material(Shader.Find("Sprites-Default"));*/

                //Set any child fully outside clipping rect to be invisible
                if (childRenderer.bounds.max.x < transform.position.x - _clipSize.x / 2f || childRenderer.bounds.max.y < transform.position.y - _clipSize.y / 2f ||
                    childRenderer.bounds.min.x > transform.position.x + _clipSize.x / 2f || childRenderer.bounds.min.y > transform.position.y + _clipSize.y / 2f)
                {
                    //Disable/Hide
                    if (_disableHidden)
                        childTransform.gameObject.SetActive(false);
                    else
                        childTransform.renderer.enabled = false;
                }
                else
                {
                    //TODO: We can't know if it was deactivated independent of this panel, so re-enabling it is a bit wrong
                    //Enable/show
                    childTransform.gameObject.SetActive(true);
                    childTransform.renderer.enabled = true;
                }
            }

            //Move on to children of child
            recursiveChildMaterial(childTransform, setMaterial);
        }
    }
    /*
    //Go through every child of panel and check if they accept input and pass it on
    private bool recursiveChildClick(Transform currentParent, Vector2 mousePos)
    {
        foreach(Transform child in currentParent)
        {
            IClickHandler handler = child.GetComponent<MonoBehaviour>() as IClickHandler;
            if (handler != null && child.collider2D != null)
            {
                //Check if click is within childs collision box
                if (child.collider2D.OverlapPoint(mousePos))
                {
                    //TODO: Deal with overlapping IClickHandlers, for now we just use the first one
                    //A solution would be to get a list of all of them that overlaps, then get the top one(GUI specific depth, or SpriteRenderer layer?) and work downwards until one of them handles it.
                    if(handler.onClick(mousePos))
                        return true;
                }
            }
            if (recursiveChildClick(child, mousePos) == true)
                return true;
        }

        return false; //No handler found
    }*/

    //A ligther refresh that assumes only the parent panel has moved(Update rect offset in shader)
    public void refreshMove()
    {
        if (_doClip)
            renderer.sharedMaterial.SetVector("_clipRect", new Vector4(renderer.bounds.min.x, renderer.bounds.min.y, _clipSize.x, _clipSize.y));
        else
            renderer.sharedMaterial.SetVector("_clipRect", new Vector4(renderer.bounds.min.x, renderer.bounds.min.y, renderer.bounds.max.x, renderer.bounds.max.y));
    }

    //Whether to not draw anything outside the clip rectangle
    public bool DoClip
    {
        get { return _doClip; }
        set
        {
            _doClip = value;
            refresh();
        }
    }

    //Whether or not to fully disable children that are outside the clipping rect when clipping is enabled
    public bool DisableHidden
    {
        get { return _disableHidden; }
        set
        {
            _disableHidden = value;
            refresh();
        }
    }

    public Vector2 ClipSize
    {
        get { return _clipSize; }
        set
        {
            _clipSize = value;
            refresh();
        }
    }
}
