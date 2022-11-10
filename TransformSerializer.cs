using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SysDiag = System.Diagnostics;

//https://stackoverflow.com/questions/39345820/easy-way-to-write-and-read-some-transform-to-a-text-file-in-unity3d
[Serializable]
public struct SerializableVector : IEquatable<SerializableVector>
{
    public float X, Y, Z, W;

    public SerializableVector(float x, float y, float z, float w = 0f) {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public SerializableVector(Vector3 vector, float w = 0f)
    {
        X = vector.x;
        Y = vector.y;
        Z = vector.z;
        W = w;
    }

    public SerializableVector(Quaternion quaternion)
    {
        X = quaternion.x;
        Y = quaternion.y;
        Z = quaternion.z;
        W = quaternion.w;
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

    public override bool Equals(object obj)
    {
        return obj is SerializableVector vector && Equals(vector);
    }

    public bool Equals(SerializableVector other)
    {
        return X.Equals(other.X) &&
               Y.Equals(other.Y) &&
               Z.Equals(other.Z) &&
               W.Equals(other.W);
    }

    public override int GetHashCode()
    {
        int hashCode = 707706286;
        hashCode = hashCode * -1521134295 + X.GetHashCode();
        hashCode = hashCode * -1521134295 + Y.GetHashCode();
        hashCode = hashCode * -1521134295 + Z.GetHashCode();
        hashCode = hashCode * -1521134295 + W.GetHashCode();
        return hashCode;
    }
}

[Serializable]
public class SerializableTransform : IEquatable<SerializableTransform>
{
    public SerializableVector Position { get; set; } = new SerializableVector(Vector3.zero);
    public SerializableVector Rotation { get; set; } = new SerializableVector(Quaternion.identity);
    public SerializableVector Scale { get; set; } = new SerializableVector(Vector3.zero);

    public SerializableTransform() { }

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

    public static SerializableTransform Default()
    {
        return new SerializableTransform();
    }

    public override bool Equals(object obj)
    {
        return obj is SerializableTransform transform && Equals(transform);
    }

    public bool Equals(SerializableTransform other)
    {
        return Position.Equals(other.Position) &&
               Rotation.Equals(other.Rotation) &&
               Scale.Equals(other.Scale);

        //return EqualityComparer<SerializableVector>.Default.Equals(Position, other.Position) &&
        //       EqualityComparer<SerializableVector>.Default.Equals(Rotation, other.Rotation) &&
        //       EqualityComparer<SerializableVector>.Default.Equals(Scale, other.Scale);
    }

    public override int GetHashCode()
    {
        int hashCode = 1352853554;
        hashCode = hashCode * -1521134295 + Position.GetHashCode();
        hashCode = hashCode * -1521134295 + Rotation.GetHashCode();
        hashCode = hashCode * -1521134295 + Scale.GetHashCode();
        return hashCode;
    }
}


[Serializable]
public static class TransformSerializer 
{
    //da mettere in un'altra classe probabilmente
    //https://answers.unity.com/questions/1296012/best-way-to-set-game-object-transforms.html
    // Assign deserializedTransform to originalTransform
    // Extension method
    public static void AssignDeserTransformToOriginalTransform(this Transform oldTransform, SerializableTransform newTransform) 
    {
        oldTransform.position = new Vector3(newTransform.Position.X, newTransform.Position.Y, newTransform.Position.Z);
        oldTransform.rotation = new Quaternion(newTransform.Rotation.X, newTransform.Rotation.Y, newTransform.Rotation.Z, newTransform.Rotation.W); 
        oldTransform.localScale = new Vector3(newTransform.Scale.X, newTransform.Scale.Y, newTransform.Scale.Z);
    }
}