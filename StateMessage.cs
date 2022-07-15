using System.Collections;
using System.Collections.Generic;
using System;

public enum UserState : int {
    EditingInLocalScene,
    Waiting
}

[Serializable]
public struct StateMessageInfo {
    public UserState UserState { get; private set; }

    public StateMessageInfo(UserState userState) {
        this.UserState = userState;
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
