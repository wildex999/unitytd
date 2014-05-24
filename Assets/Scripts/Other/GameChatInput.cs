using UnityEngine;

//Based on NGUI ChatInput

[RequireComponent(typeof(UIInput))]
public class GameChatInput : MonoBehaviour
{
    private UIInput mInput;
    private bool mIgnoreNextEnter = false;

    public delegate void ChatEventHandler(string message);
    public static event ChatEventHandler ChatEvent;

    void Start()
    {
        mInput = GetComponent<UIInput>();
    }

    void Update()
    {
        //Press enter to get focus
        if (Input.GetKeyUp(KeyCode.Return))
        {
            if (!mIgnoreNextEnter && !mInput.selected)
                mInput.selected = true;

            mIgnoreNextEnter = false;
        }
    }

    void OnSubmit()
    {

        if(ChatEvent != null)
        {
            //string text = NGUITools.StripSymbols(mInput.text);
            string text = mInput.text; //TODO: Filter for bad stuff


            if(!string.IsNullOrEmpty(text))
            {
                ChatEvent(text);
                mInput.text = "";
                mInput.selected = false;
            }
        }

        mIgnoreNextEnter = true;
    }
}