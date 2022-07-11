using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public abstract class Message
{
    protected KeyValuePair<Guid, object> message;

    public Message(Guid id, System.Object obj)
    {
        message = new KeyValuePair<Guid, object>(id, obj);
    }

    public Message(String id, System.Object obj) {
        message = new KeyValuePair<Guid, object>(Guid.Parse(id), obj);
    }

    public Message(System.Object obj) {
        message = new KeyValuePair<Guid, object>(Guid.NewGuid(), obj);
    }

    public abstract void ExecuteMessage();

    public object getObj() {
        return message.Value;
    }

    public Guid getId() {
        return message.Key;
    }

    public String getIdString() {
        return message.Key.ToString();
    }
}
