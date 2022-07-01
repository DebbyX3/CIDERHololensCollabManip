using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using SysDiag = System.Diagnostics;

//https://stackoverflow.com/questions/39345820/easy-way-to-write-and-read-some-transform-to-a-text-file-in-unity3d
[Serializable]
public struct SerializableTransform
{
    public Vector3 Position { get; private set; }
    public Quaternion Rotation { get; private set; }
    public Vector3 Scale { get; private set; }

    public SerializableTransform(Transform tr)
    {
        Position = tr.position;
        Rotation = tr.rotation;
        Scale = tr.lossyScale;
    }

    public SerializableTransform(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        this.Position = position;
        this.Rotation = rotation;
        this.Scale = scale;
    }
}

public static class TransformSerializer
{
        public static byte[] Serialize(IEnumerable<Transform> transforms)
    {        
        byte[] data = null;

        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                foreach (Transform transform in transforms)
                {
                    WriteTransform(writer, transform);
                }

                stream.Position = 0;
                data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
            }
        }

        return data;
    }

    public static byte[] Serialize(Transform transform)
    {
        return Serialize(new List<Transform>() { transform });
    }

    private static void WriteTransform(BinaryWriter writer, Transform transform)
    {
        SysDiag.Debug.Assert(writer != null);

        // Write the transform data.
        WritePosition(writer, transform.position);
        WriteRotation(writer, transform.rotation);
        WriteScale(writer, transform.lossyScale);
    }

    private static void WritePosition(BinaryWriter writer, Vector3 position)
    {
        SysDiag.Debug.Assert(writer != null);

        writer.Write(position.x);
        writer.Write(position.y);
        writer.Write(position.z);
    }

    private static void WriteRotation(BinaryWriter writer, Quaternion rotation)
    {
        SysDiag.Debug.Assert(writer != null);

        writer.Write(rotation.x);
        writer.Write(rotation.y);
        writer.Write(rotation.z);
        writer.Write(rotation.w);
    }

    private static void WriteScale(BinaryWriter writer, Vector3 scale)
    {
        SysDiag.Debug.Assert(writer != null);

        writer.Write(scale.x);
        writer.Write(scale.y);
        writer.Write(scale.z);
    }

    // can't create a transform object deatached from a gameobject, so i'll return a struct
    public static IEnumerable<SerializableTransform> DeserializeMultiple(byte[] data)
    {
        var transforms = new List<SerializableTransform>();

        using (MemoryStream stream = new MemoryStream(data))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                //while (reader.BaseStream.Length - reader.BaseStream.Position >= HeaderSize) {
                transforms.Add(ReadTransform(reader));
                //}
            }
        }

        return transforms;
    }

    public static SerializableTransform Deserialize(byte[] data)
    {
        return ((List<SerializableTransform>)DeserializeMultiple(data))[0];
    }

    private static SerializableTransform ReadTransform(BinaryReader reader)
    {
        SysDiag.Debug.Assert(reader != null);

        // Read the transform data.
        Vector3 position = ReadPosition(reader);
        Quaternion rotation = ReadRotation(reader);
        Vector3 scale = ReadScale(reader);

        return new SerializableTransform(position, rotation, scale);
    }

    private static Vector3 ReadPosition(BinaryReader reader)
    {
        SysDiag.Debug.Assert(reader != null);

        float x = reader.ReadSingle(); //read float
        float y = reader.ReadSingle();
        float z = reader.ReadSingle();

        return new Vector3(x, y, z);
    }

    private static Quaternion ReadRotation(BinaryReader reader)
    {
        SysDiag.Debug.Assert(reader != null);

        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        float z = reader.ReadSingle();
        float w = reader.ReadSingle();

        return new Quaternion(x, y, z, w);
    }

    private static Vector3 ReadScale(BinaryReader reader)
    {
        SysDiag.Debug.Assert(reader != null);

        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        float z = reader.ReadSingle();

        return new Vector3(x, y, z);
    }

    //da mettere in un'altra classe probabilmente
    //https://answers.unity.com/questions/1296012/best-way-to-set-game-object-transforms.html
    public static void LoadTransform(this Transform originalTransform, SerializableTransform deserializedTransform)
    {
        originalTransform.position = deserializedTransform.Position;
        originalTransform.rotation = deserializedTransform.Rotation;
        originalTransform.localScale = deserializedTransform.Scale;
    }
    
}