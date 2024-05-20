using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Memento
{
    // originator (gobjcontroller) info
    private Guid Guid;
    private string PrefabName;
    private string MaterialName;
    private SerializableTransform Transform;
    private ObjectLocation ObjectLocation;

    public Memento(Guid guid, string prefabName, string materialName, SerializableTransform transform, ObjectLocation objectLocation) {
        Guid = guid;
        PrefabName = prefabName;
        MaterialName = materialName;
        Transform = new SerializableTransform(transform);
        ObjectLocation = objectLocation;
    }

    // only used to read pending commit list
    public Guid GetGuid() {
        return Guid;
    }

    // Not used because the originator does not have to modify the GUID, the prefabname or the location
    /*
    public ObjectLocation GetObjectLocation()
    {
        return ObjectLocation;
    }*/

    // only used to read existing file
    public string GetPrefabName() {
        return PrefabName;
    }
    

    // The Originator (gobjcontroller) uses this method when restoring its transform
    public SerializableTransform GetTransform() {
        return Transform;
    }

    // The Originator (gobjcontroller) uses this method when restoring its material
    public string GetMaterialName()
    {
        return MaterialName;
    }

    public override string ToString()
    {
        return "Hash: " + GetHashCode() +
            "\nPrefabName: " + PrefabName +
            "\nMaterialName: " + MaterialName +
            "\nTransform: " + Transform +
            "\nTransformHash: " + Transform.GetHashCode();
    }
}
