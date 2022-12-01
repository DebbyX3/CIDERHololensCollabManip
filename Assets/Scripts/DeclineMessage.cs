using System.Collections;
using System.Collections.Generic;
using System;

/*
 * Send this message to signal the other user to decline a voting action, specified by enum DeclineType (in MessagesManager)
*/

[Serializable]
public struct DeclineMessageInfo
{
    public Guid GameObjectGuid { get; private set; }
    public DeclineType DeclineType { get; private set; }

    public DeclineMessageInfo(Guid guid, DeclineType declineType)
    {
        GameObjectGuid = guid;
        DeclineType = declineType;
    }
}

[Serializable]
public class DeclineMessage : Message
{
    public DeclineMessage(Guid id, DeclineMessageInfo info) : base(id, info, MessageType.DeclineMessage) { }
    public DeclineMessage(DeclineMessageInfo info) : base(info, MessageType.DeclineMessage) { }
    public DeclineMessage(string id, DeclineMessageInfo info) : base(id, info, MessageType.DeclineMessage) { }

    public override void ExecuteMessage()
    {
        MessagesManager.Instance.OnDeclineReceived(this);
    }

    public DeclineMessageInfo GetMsgInfo()
    {
        return (DeclineMessageInfo)Info;
    }
}
