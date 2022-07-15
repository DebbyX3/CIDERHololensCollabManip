using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using SysDiag = System.Diagnostics;
using System.Runtime.Serialization;

[Serializable]
public struct GameObjMessageInfo {
    public Guid GameObjectGuid { get; private set; }
    public SerializableTransform Transform { get; private set; }
    public string PrefabName { get; private set; }

    public GameObjMessageInfo(Guid gameObjectGuid, SerializableTransform transform, string prefabName) {
        this.GameObjectGuid = gameObjectGuid;
        this.Transform = transform;
        this.PrefabName = prefabName;
    }

    public GameObjMessageInfo(Guid gameObjectGuid, Transform transform, string prefabName) {
        this.GameObjectGuid = gameObjectGuid;
        this.Transform = new SerializableTransform(transform.position, transform.rotation, transform.lossyScale);
        this.PrefabName = prefabName;
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
        if (GUIDList.ContainsGuid(getMsgInfo().GameObjectGuid)) 
        {
            PrefabHandler.UpdateObject(getMsgInfo().GameObjectGuid, getMsgInfo().Transform);
        } 
        else //if the scene does NOT contain the object
        {
            //Create it
            PrefabHandler.CreateNewObject(getMsgInfo().PrefabName, getMsgInfo().Transform);
        }

        Debug.Log("hei, sto eseguendo il messaggio che ha inviato gObjMsg!!!");
    }

    public GameObjMessageInfo getMsgInfo() {
        return (GameObjMessageInfo) Info;
    }
}
