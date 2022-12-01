using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using SysDiag = System.Diagnostics;
using System.Runtime.Serialization;

[Serializable]
public struct GameObjMessageInfo
{
    public Guid GameObjectGuid { get; private set; }
    public SerializableTransform Transform { get; private set; }
    public string PrefabName { get; private set; }
    public string MaterialName { get; private set; }
    public CommitType CommitType { get; private set; }

    public GameObjMessageInfo(Guid gameObjectGuid, SerializableTransform transform, string prefabName, string materialName, CommitType commitType)
    {
        GameObjectGuid = gameObjectGuid;
        Transform = transform;
        PrefabName = prefabName;
        MaterialName = materialName;
        CommitType = commitType;
    }

    public GameObjMessageInfo(GameObjController gObj, CommitType commitType)
        : this(gObj.Guid, gObj.Transform, gObj.PrefabName, gObj.MaterialName, commitType) { }

    public GameObjMessageInfo(Guid gameObjectGuid, Transform transform, string prefabName, string materialName, CommitType commitType)
       : this(gameObjectGuid, (SerializableTransform)transform, prefabName, materialName, commitType) { }
}

//---------------

[Serializable]
public class GameObjMessage : Message {

    public GameObjMessage(Guid id, GameObjMessageInfo info) : base(id, info, MessageType.GameObjMessage) { }
    public GameObjMessage(GameObjMessageInfo info) : base(info, MessageType.GameObjMessage) { }
    public GameObjMessage(string id, GameObjMessageInfo info) : base(id, info, MessageType.GameObjMessage) { }

    public override void ExecuteMessage() 
    {
        MessagesManager.Instance.OnCommitReceived(this);
    }

    public GameObjMessageInfo GetMsgInfo() {
        return (GameObjMessageInfo) Info;
    }
}