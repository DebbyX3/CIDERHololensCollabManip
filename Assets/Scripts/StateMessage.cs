using System.Collections;
using System.Collections.Generic;
using System;

/*
 * Send this message to notify the other user in which scene this user is 
 */

public enum UserState : int {
    InGlobalScene,
    WaitingConnection
}

[Serializable]
public struct StateMessageInfo 
{
    public UserState UserState { get; private set; }

    public StateMessageInfo(UserState userState) {
        UserState = userState;
    }
}

[Serializable]
public class StateMessage : Message
{
    public StateMessage(Guid id, StateMessageInfo info) : base(id, info, MessageType.StateMessage) { }
    public StateMessage(StateMessageInfo info) : base(info, MessageType.StateMessage) { }
    public StateMessage(string id, StateMessageInfo info) : base(id, info, MessageType.StateMessage) { }

    public override void ExecuteMessage() {
        throw new System.NotImplementedException();
    }
}
