using System.Collections;
using System.Collections.Generic;
using System;

public enum UserState : int
{
    InLocalScene,
    InGlobalScene,
    WaitingConnection
}

[Serializable]
public struct DeletionMessageInfo
{
    public UserState UserState { get; private set; }

    public DeletionMessageInfo(UserState userState)
    {
        this.UserState = userState;
    }
}

[Serializable]
public class DeletionMessage : Message
{
    public DeletionMessage(Guid id, StateMessageInfo info) : base(id, info, MessageType.StateMessage) { }
    public DeletionMessage(StateMessageInfo info) : base(info, MessageType.StateMessage) { }
    public DeletionMessage(string id, StateMessageInfo info) : base(id, info, MessageType.StateMessage) { }

    public override void ExecuteMessage()
    {
        throw new System.NotImplementedException();
    }
}
