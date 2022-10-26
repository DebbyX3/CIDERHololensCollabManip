using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using SysDiag = System.Diagnostics;
using System.Runtime.Serialization;

public enum CommitType : int {
    ForcedCommit,
    RequestCommit
}

[Serializable]
public struct GameObjMessageInfo {
    public Guid GameObjectGuid { get; private set; }
    public SerializableTransform Transform { get; private set; }
    public string PrefabName { get; private set; }
    public CommitType CommitType { get; private set; }

    public GameObjMessageInfo(Guid gameObjectGuid, SerializableTransform transform, string prefabName, CommitType commitType) {
        GameObjectGuid = gameObjectGuid;
        Transform = transform;
        PrefabName = prefabName;
        CommitType = commitType;
    }
    public GameObjMessageInfo(GameObjController gObj, CommitType commitType) {
        GameObjectGuid = gObj.Guid;
        Transform = gObj.Transform;
        PrefabName = gObj.PrefabName;
        CommitType = commitType;
    }

    public GameObjMessageInfo(Guid gameObjectGuid, Transform transform, string prefabName, CommitType commitType) {
        GameObjectGuid = gameObjectGuid;
        Transform = new SerializableTransform(transform.position, transform.rotation, transform.lossyScale);
        PrefabName = prefabName;
        CommitType = commitType;
    }
}

//---------------

[Serializable]
public class GameObjMessage : Message {

    public GameObjMessage(Guid id, GameObjMessageInfo info) : base(id, info, MessageType.GameObjMessage) { }
    public GameObjMessage(GameObjMessageInfo info) : base(info, MessageType.GameObjMessage) { }
    public GameObjMessage(string id, GameObjMessageInfo info) : base(id, info, MessageType.GameObjMessage) { }

    public override void ExecuteMessage() 
    {
        CommitManager.Instance.OnCommitReceived(this);
    }

    public GameObjMessageInfo GetMsgInfo() {
        return (GameObjMessageInfo) Info;
    }
}
