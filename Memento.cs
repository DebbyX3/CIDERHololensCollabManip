using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Memento
{
    // originator (gobjcontroller) info
    private Guid Guid;
    private string PrefabName;
    private string MaterialName;
    private SerializableTransform Transform;

    // memento info
    private DateTime Date;
    private ObjectLocation ObjectLocation;

    public Memento(Guid guid, string prefabName, string materialName, SerializableTransform transform) {
        Guid = guid;
        PrefabName = prefabName;
        MaterialName = materialName;
        Transform = transform;
        Date = DateTime.Now;
    }

    // Call the less demanding constructor + add assignment to the field
    public Memento(Guid guid, string prefabName, string materialName, SerializableTransform transform, ObjectLocation objectLocation) 
                   : this(guid, prefabName, materialName, transform)
    {
        ObjectLocation = objectLocation;
    }

    // Not used because the originator does not have to modify the GUID or the prefabname!!!!
    /*
    public Guid GetGuid() {
        return Guid;
    }

    public string GetPrefabName() {
        return PrefabName;
    }
    */

    // The Originator (gobjcontroller) uses this method when restoring its transform
    public SerializableTransform GetTransform() {
        return Transform;
    }

    // The Originator (gobjcontroller) uses this method when restoring its material
    public string GetMaterialName()
    {
        return MaterialName;
    }

    // The rest of the methods are used by the Caretaker to display metadata.
    public string GetName() 
    {
        return $"{Date} / Object: {PrefabName} - GUID: {Guid} - Transform: {Transform}";        
    }

    public DateTime GetDate() {
        return Date;
    }
    public ObjectLocation GetObjectLocation() {
        return ObjectLocation;
    }
}
