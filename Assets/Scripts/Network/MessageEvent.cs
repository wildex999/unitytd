using System;
using UnityEngine;

public abstract class MessageEventBase
{
    public abstract void Dispatch(Message message);
}


public class MessageEvent<T> : MessageEventBase where T : Message
{
    public delegate void EventHandler(T message);
    protected event EventHandler eventDelegate;

    public override void Dispatch(Message message)
    {
        if(message == null)
        {
            Debug.LogError("Trying to dispatch events on null message");
            return;
        }

        if(eventDelegate != null)
        {
            T gotMessage = message as T;
            if (gotMessage == null)
            {
                Debug.LogError("Unable to cast message on Event dispatch: " + message.getCommand());
                return;
            }

            eventDelegate(gotMessage);
        }
    }

    public static MessageEvent<T> operator +(MessageEvent<T> self, EventHandler handler)
    {
        self.eventDelegate += handler;
        return self;
    }

    public static MessageEvent<T> operator -(MessageEvent<T> self, EventHandler handler)
    {
        self.eventDelegate -= handler;
        return self;
    }
}