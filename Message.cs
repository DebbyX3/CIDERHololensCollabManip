using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Message
{
    protected KeyValuePair<int, object> message;

    public Message(int id, Object obj)
    {
        message = new KeyValuePair<int, object>(id, obj);
    }

    public abstract void ExecuteMessage();

    public object getObj() {
        return message.Value;
    }

    public int getId() {
        return message.Key;
    }
}
