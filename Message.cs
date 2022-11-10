using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public enum MessageType : int {
    BaseMessage,
    GameObjMessage,
    MeshMessage,
    StateMessage,
    DeletionMessage
}

[Serializable]
public abstract class Message
{
    public Guid Id { get; private set; }
    public object Info { get; private set; }
    public MessageType MessageType { get; private set; } = MessageType.BaseMessage;

    public Message(Guid id, System.Object info, MessageType messageType)
    {
        Id = id;
        Info = info;
        MessageType = messageType;
    }

    public Message(string id, System.Object info, MessageType messageType) 
    {
        Id = Guid.Parse(id);
        Info = info;
        MessageType = messageType;
    }

    public Message(object info, MessageType messageType) 
    {
        Id = Guid.NewGuid();
        Info = info;
        MessageType = messageType;
    }

    public string getIdString() {
        return Id.ToString();
    }

    public byte[] Serialize() {
        BinaryFormatter formatter = new BinaryFormatter();

        using (MemoryStream stream = new MemoryStream()) {

            formatter.Serialize(stream, this);
            return stream.ToArray();
        }
    }

    public static Message Deserialize(byte[] data) {
        BinaryFormatter formatter = new BinaryFormatter();
        Message msg;

        using (MemoryStream stream = new MemoryStream(data)) {
            msg = (Message)formatter.Deserialize(stream);        
        }

        return msg;
    }

    public abstract void ExecuteMessage();
}
