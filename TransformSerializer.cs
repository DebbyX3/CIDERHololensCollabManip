using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SysDiag = System.Diagnostics;

//https://stackoverflow.com/questions/39345820/easy-way-to-write-and-read-some-transform-to-a-text-file-in-unity3d
[Serializable]
public struct SerializableVector {
    public float X, Y, Z, W;

    public SerializableVector(float x, float y, float z, float w = 0f) {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public static explicit operator SerializableVector(Quaternion a) {
        return new SerializableVector(a.x, a.y, a.z, a.w);
    }

    public static implicit operator SerializableVector(Vector3 a) {
        return new SerializableVector(a.x, a.y, a.z);
    }

    public override string ToString() {
        return "x = " + X + "\ny = " + Y + "\nz = " + Z + "\nw = " + W;
    }
}

[Serializable]
public struct SerializableTransform {

    public SerializableVector Position { get; set; }
    public SerializableVector Rotation { get; set; }
    public SerializableVector Scale { get; set; }

    public SerializableTransform(Transform tr) {
        Position = tr.position;
        Rotation = (SerializableVector) tr.rotation;
        Scale = tr.lossyScale;
    }

    public SerializableTransform(Vector3 position, Quaternion rotation, Vector3 scale) {
        Position = position;
        Rotation = (SerializableVector) rotation;
        Scale = scale;
    }

    public static implicit operator SerializableTransform(Transform t)
    {
        return new SerializableTransform(t);
    }

    public override string ToString() {
        return $"Position {Position}, Rotation {Rotation}, Scale {Scale}";
    }
}


[Serializable]
public static class TransformSerializer {

    // tutta la roba commentata probabilmente non serve a niente perchè la serializzazione la fa lui. sigh quanto tempo perso
    /*
    public static byte[] Serialize(IEnumerable<Transform> transforms) {
        byte[] data = null;

        using (MemoryStream stream = new MemoryStream()) {
            using (BinaryWriter writer = new BinaryWriter(stream)) {
                foreach (Transform transform in transforms) {
                    WriteTransform(writer, transform);
                }

                stream.Position = 0;
                data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
            }
        }

        return data;
    }

    public static byte[] Serialize(IEnumerable<SerializableTransform> transforms) {
        byte[] data = null;

        using (MemoryStream stream = new MemoryStream()) {
            using (BinaryWriter writer = new BinaryWriter(stream)) {
                foreach (SerializableTransform transform in transforms) {
                    WriteTransform(writer, transform);
                }

                stream.Position = 0;
                data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
            }
        }

        return data;
    }

    public static byte[] Serialize(Transform transform) {
        return Serialize(new List<Transform>() { transform });
    }

    public static byte[] Serialize(SerializableTransform transform) {
        return Serialize(new List<SerializableTransform>() { transform });
    }

    private static void WriteTransform(BinaryWriter writer, Transform transform) {
        SysDiag.Debug.Assert(writer != null);

        // Write the transform data.
        WritePosition(writer, transform.position);
        WriteRotation(writer, (SerializableVector) transform.rotation);
        WriteScale(writer, transform.lossyScale);
    }

    private static void WriteTransform(BinaryWriter writer, SerializableTransform transform) {
        SysDiag.Debug.Assert(writer != null);

        // Write the transform data.
        WritePosition(writer, transform.Position);
        WriteRotation(writer, transform.Rotation);
        WriteScale(writer, transform.Scale);
    }

    private static void WritePosition(BinaryWriter writer, SerializableVector position) {
        SysDiag.Debug.Assert(writer != null);

        writer.Write(position.x);
        writer.Write(position.y);
        writer.Write(position.z);
    }

    private static void WriteRotation(BinaryWriter writer, SerializableVector rotation) {
        SysDiag.Debug.Assert(writer != null);

        writer.Write(rotation.x);
        writer.Write(rotation.y);
        writer.Write(rotation.z);
        writer.Write(rotation.w);
    }

    private static void WriteScale(BinaryWriter writer, SerializableVector scale) {
        SysDiag.Debug.Assert(writer != null);

        writer.Write(scale.x);
        writer.Write(scale.y);
        writer.Write(scale.z);
    }

    // can't create a transform object deatached from a gameobject, so i'll return a struct
    public static IEnumerable<SerializableTransform> DeserializeMultiple(byte[] data) {
        var transforms = new List<SerializableTransform>();

        using (MemoryStream stream = new MemoryStream(data)) {
            using (BinaryReader reader = new BinaryReader(stream)) {
                //while (reader.BaseStream.Length - reader.BaseStream.Position >= HeaderSize) {
                transforms.Add(ReadTransform(reader));
                //}
            }
        }

        return transforms;
    }

    public static SerializableTransform Deserialize(byte[] data) {
        return ((List<SerializableTransform>)DeserializeMultiple(data))[0];
    }

    private static SerializableTransform ReadTransform(BinaryReader reader) {
        SysDiag.Debug.Assert(reader != null);

        // Read the transform data.
        Vector3 position = ReadPosition(reader);
        Quaternion rotation = ReadRotation(reader);
        Vector3 scale = ReadScale(reader);

        return new SerializableTransform(position, rotation, scale);
    }

    private static Vector3 ReadPosition(BinaryReader reader) {
        SysDiag.Debug.Assert(reader != null);

        float x = reader.ReadSingle(); //read float
        float y = reader.ReadSingle();
        float z = reader.ReadSingle();

        return new Vector3(x, y, z);
    }

    private static Quaternion ReadRotation(BinaryReader reader) {
        SysDiag.Debug.Assert(reader != null);

        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        float z = reader.ReadSingle();
        float w = reader.ReadSingle();

        return new Quaternion(x, y, z, w);
    }

    private static Vector3 ReadScale(BinaryReader reader) {
        SysDiag.Debug.Assert(reader != null);

        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        float z = reader.ReadSingle();

        return new Vector3(x, y, z);
    }
*/

    //da mettere in un'altra classe probabilmente
    //https://answers.unity.com/questions/1296012/best-way-to-set-game-object-transforms.html
    // Assign deserializedTransform to originalTransform
    public static void AssignDeserTransformToOriginalTransform(this Transform oldTransform, SerializableTransform newTransform) 
    {
        oldTransform.position = new Vector3(newTransform.Position.X, newTransform.Position.Y, newTransform.Position.Z);
        oldTransform.rotation = new Quaternion(newTransform.Rotation.X, newTransform.Rotation.Y, newTransform.Rotation.Z, newTransform.Rotation.W); 
        oldTransform.localScale = new Vector3(newTransform.Scale.X, newTransform.Scale.Y, newTransform.Scale.Z);
    }
}