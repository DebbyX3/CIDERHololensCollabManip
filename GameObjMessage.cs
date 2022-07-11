using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using SysDiag = System.Diagnostics;

[Serializable]
public struct MessageInfo {
    public Guid GameObjectGuid { get; private set; }
    public SerializableTransform Transform { get; private set; }

    public MessageInfo(Guid gameObjectGuid, SerializableTransform transform) {
        this.GameObjectGuid = gameObjectGuid;
        this.Transform = transform;
    }

    public MessageInfo(Guid gameObjectGuid, Transform transform) {
        this.GameObjectGuid = gameObjectGuid;
        this.Transform = new SerializableTransform(transform.position, transform.rotation, transform.lossyScale);
    }

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
}

//---------------

[Serializable]
public class GameObjMessage : Message {

    public GameObjMessage(Guid id, MessageInfo info) : base(id, info) { }
    public GameObjMessage(MessageInfo info) : base(info) { }
    public GameObjMessage(String id, MessageInfo info) : base(id, info) { }

    public override void ExecuteMessage() 
    {    

    }

    public byte[] Serialize() {
        byte[] data = null;

        using (MemoryStream stream = new MemoryStream()) {
            using (BinaryWriter writer = new BinaryWriter(stream)) {
                SysDiag.Debug.Assert(writer != null);

                writer.Write(getIdString());
                writer.Write(getObj().Serialize());

                stream.Position = 0;
                data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
            }
        }
        
        return data;
    }

    public static GameObjMessage Deserialize(byte[] data) 
    {
        Guid guid;
        MessageInfo info;

        using (MemoryStream stream = new MemoryStream(data)) 
        {
            using (BinaryReader reader = new BinaryReader(stream)) 
            {
                guid = Guid.Parse(reader.ReadString());

                byte[] left = new byte[stream.Length];
                int readBytes = stream.Read(left, (int)stream.Position, (int)(stream.Length - stream.Position));
                byte[] leftCropped = new byte[readBytes];

                Debug.Log(Array.LastIndexOf(left, (byte)0));

                Array.Copy(left, stream.Length - readBytes, leftCropped, 0, readBytes);

                Debug.Log(leftCropped);

                info = MessageInfo.Deserialize(left);

            }
        }

        return new GameObjMessage(guid, info);
    }

    public new MessageInfo getObj() {
        return (MessageInfo)message.Value;
    }
}
