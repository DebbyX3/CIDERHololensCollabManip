using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using SysDiag = System.Diagnostics;

public class TransformSerializer : MonoBehaviour
{
    /*
    [Serializable]
    public struct SerializebleVector {
        public float x, y, z, w;

        public SerializebleVector(float x, float y, float z, float w = 0f) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static explicit operator SerializebleVector(Quaternion a) {
            return new SerializebleVector(a.x, a.y, a.z, a.w);
        }

        public static implicit operator SerializebleVector(Vector3 a) {
            return new SerializebleVector(a.x, a.y, a.z);
        }

    }*/

    //https://stackoverflow.com/questions/39345820/easy-way-to-write-and-read-some-transform-to-a-text-file-in-unity3d
    [Serializable]
    public struct SerializableTransform {
        Vector3 position;
        Quaternion rotation;
        Vector3 scale;

        public SerializableTransform(Transform tr) {
            position = tr.position;
            rotation = tr.rotation;
            scale = tr.lossyScale;
        }

        public SerializableTransform(Vector3 position, Quaternion rotation, Vector3 scale) {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }

    public static byte[] Serialize(IEnumerable<Transform> transforms) 
    {
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

    public static byte[] Serialize(Transform transform) {
        return Serialize(new List<Transform>() { transform });
    }

    private static void WriteTransform(BinaryWriter writer, Transform transform) {
        SysDiag.Debug.Assert(writer != null);

        // Write the transform data.
        WritePosition(writer, transform.position);
        WriteRotation(writer, transform.rotation);
        WriteScale(writer, transform.lossyScale);
    }

    private static void WritePosition(BinaryWriter writer, Vector3 position) {
        SysDiag.Debug.Assert(writer != null);

        writer.Write(position.x);
        writer.Write(position.y);
        writer.Write(position.z);
    }

    private static void WriteRotation(BinaryWriter writer, Quaternion rotation) {
        SysDiag.Debug.Assert(writer != null);

        writer.Write(rotation.x);
        writer.Write(rotation.y);
        writer.Write(rotation.z);
        writer.Write(rotation.w);
    }

    private static void WriteScale(BinaryWriter writer, Vector3 scale) {
        SysDiag.Debug.Assert(writer != null);

        writer.Write(scale.x);
        writer.Write(scale.y);
        writer.Write(scale.z);
    }

    // can't create a transform object deatached from a gameobject, so i'll return a struct
    public static IEnumerable<SerializableTransform> DeserializeMultiple(byte[] data) 
    {
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

        SerializableTransform transform;

        // Read the transform data.
        Vector3 position = ReadPosition(reader);
        Quaternion rotation = ReadRotation(reader);
        Vector3 scale = ReadScale(reader);

        return new SerializableTransform(position, rotation, scale);
    }

    private static Vector3 ReadPosition(BinaryReader reader) {
        SysDiag.Debug.Assert(reader != null);

        int x = reader.ReadInt32();
        int y = reader.ReadInt32();
        int z = reader.ReadInt32();

        return new Vector3(x, y, x);
    }

    private static Quaternion ReadRotation(BinaryReader reader) {
        SysDiag.Debug.Assert(reader != null);

        int x = reader.ReadInt32();
        int y = reader.ReadInt32();
        int z = reader.ReadInt32();
        int w = reader.ReadInt32();

        return new Quaternion(x, y, x, w);
    }

    private static Vector3 ReadScale(BinaryReader reader) {
        SysDiag.Debug.Assert(reader != null);

        int x = reader.ReadInt32();
        int y = reader.ReadInt32();
        int z = reader.ReadInt32();

        return new Vector3(x, y, x);
    }
}
