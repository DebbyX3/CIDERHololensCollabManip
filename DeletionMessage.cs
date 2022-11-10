using System.Collections;
using System.Collections.Generic;
using System;

/*
 * Send this message to signal the other user to delete the object from the global scene
 * When this message is received, the object has to be removed/deleted from the global scene
*/

[Serializable]
public struct DeletionMessageInfo
{
    public Guid GameObjectGuid { get; private set; }

    public DeletionMessageInfo(Guid guid)
    {
        GameObjectGuid = guid;
    }
}

[Serializable]
public class DeletionMessage : Message
{
    public DeletionMessage(Guid id, DeletionMessageInfo info) : base(id, info, MessageType.DeletionMessage) { }
    public DeletionMessage(DeletionMessageInfo info) : base(info, MessageType.DeletionMessage) { }
    public DeletionMessage(string id, DeletionMessageInfo info) : base(id, info, MessageType.DeletionMessage) { }

    public override void ExecuteMessage()
    {
        MessagesManager.Instance.OnDeletionReceived(this);
    }

    public DeletionMessageInfo GetMsgInfo()
    {
        return (DeletionMessageInfo)Info;
    }
}
