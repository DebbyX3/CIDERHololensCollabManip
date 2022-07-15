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

    public GameObjMessageInfo(Guid gameObjectGuid, SerializableTransform transform) {
        this.GameObjectGuid = gameObjectGuid;
        this.Transform = transform;
    }

    public GameObjMessageInfo(Guid gameObjectGuid, Transform transform) {
        this.GameObjectGuid = gameObjectGuid;
        this.Transform = new SerializableTransform(transform.position, transform.rotation, transform.lossyScale);
    }

    /*
    public byte[] Serialize() {
        byte[] data = null;

        using (MemoryStream stream = new MemoryStream()) {
            using (BinaryWriter writer = new BinaryWriter(stream)) {
                SysDiag.Debug.Assert(writer != null);

                writer.Write(GameObjectGuid.ToString());
                writer.Write(TransformSerializer.Serialize(Transform));

                stream.Position = 0;
                data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
            }
        }

        return data;
    }
    */

    /*
    public static MessageInfo Deserialize(byte[] data) {
        Guid guid;
        SerializableTransform transform;

        using (MemoryStream stream = new MemoryStream(data)) {
            using (BinaryReader reader = new BinaryReader(stream)) {
                guid = Guid.Parse(reader.ReadString());

                byte[] left = new byte[stream.Length - stream.Position]; //???
                stream.Read(left, (int)stream.Position, (int)stream.Length);

                transform = TransformSerializer.Deserialize(left);       
            }
        }

        return new MessageInfo(guid, transform);
    }
    */
}

//---------------

[Serializable]
public class GameObjMessage : Message{

    public GameObjMessage(Guid id, GameObjMessageInfo info, MessageType messageType) : base(id, info, messageType) { }
    public GameObjMessage(GameObjMessageInfo info, MessageType messageType) : base(info, messageType) { }
    public GameObjMessage(String id, GameObjMessageInfo info, MessageType messageType) : base(id, info, messageType) { }

    public override void ExecuteMessage() 
    {
        //if the scene contains the object
        if (GUIDList.List.ContainsKey(getMsgInfo().GameObjectGuid) ) {
            //provaPrefab.groda();
        } 
        else //if the scene does NOT contain the object
        {
            //then create it - TODO
        }

        Debug.Log("hei, sto eseguendo il messaggio che ha inviato gObjMsg!!!");
    }

    public GameObjMessageInfo getMsgInfo() {
        return (GameObjMessageInfo) Info;
    }
}
