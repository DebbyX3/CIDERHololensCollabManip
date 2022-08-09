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

// TODO: forse aggiungere un campo enum per indicare il tipo di commit? forse?
[Serializable]
public struct GameObjMessageInfo {
    public Guid GameObjectGuid { get; private set; }
    public SerializableTransform Transform { get; private set; }
    public string PrefabName { get; private set; }
    public CommitType CommitType { get; private set; }

    // ma al posto di fare un costruttore così astruso perchè non passo direttamente gameobjcontroller?
    // ma se invece GameObjMessageInfo contenesse direttamente il controller? non è + facile? mhm MI SA DI SI!! MA è SERIALIZZABILE??? PENSO DI SI?
    // NO!!!!!!!!! NON METTERE IL CONTROLLER DIRETTAMENTE!!! CHE QUELLO SI TIENE UNA MIRIADE DI ROBE! METTI SOLO QUELLO CHE SERVE NEL MSG!!!

    public GameObjMessageInfo(Guid gameObjectGuid, SerializableTransform transform, string prefabName, CommitType commitType) {
        this.GameObjectGuid = gameObjectGuid;
        this.Transform = transform;
        this.PrefabName = prefabName;
        this.CommitType = commitType;
    }
    public GameObjMessageInfo(GameObjController gObj, CommitType commitType) {
        GameObjectGuid = gObj.Guid;
        Transform = gObj.Transform;
        PrefabName = gObj.PrefabName;
        this.CommitType = commitType;
    }

    public GameObjMessageInfo(Guid gameObjectGuid, Transform transform, string prefabName, CommitType commitType) {
        this.GameObjectGuid = gameObjectGuid;
        this.Transform = new SerializableTransform(transform.position, transform.rotation, transform.lossyScale);
        this.PrefabName = prefabName;
        this.CommitType = commitType;
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
        //if the scene contains the object
        if (GUIDKeeper.ContainsGuid(getMsgInfo().GameObjectGuid)) 
        {
            PrefabHandler.UpdateObject(getMsgInfo().GameObjectGuid, getMsgInfo().Transform);
        } 
        else //if the scene does NOT contain the object
        {
            //Create it
            PrefabHandler.CreateNewObject(getMsgInfo().GameObjectGuid, getMsgInfo().PrefabName, getMsgInfo().Transform);
        }
    }

    public GameObjMessageInfo getMsgInfo() {
        return (GameObjMessageInfo) Info;
    }
}
