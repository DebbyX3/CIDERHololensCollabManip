using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Location : int {
    LocalLayer,
    GlobalLayer
    //BothLocalGlobal // non so se ha senso, forse basta mettere due enum:
                    // uno per local e uno per global, poi se esiste local
                    // allora metto local e le sue cose, se esiste global
                    // metto global e le sue cose. quando faccio switch scena
                    // cerco se esiste lo stato dell'oggetto in quella scena,
                    // se c'è bene, altrimenti niente, non lo metto! non so se funziona?
}

/*
 oppure posso provare a fare una lista diversa per ogni scena, quindi è la lista che si tiene chi c'è in ogni scena, e non l'oggetto stesso? 
 o entrambi? boh??? quindi è tipo il caretaker che dice chi c'è in quale scena!! potrebbe funzionarex
 */

public class Memento
{
    // originator (gobjcontroller) info
    private Guid Guid;
    private string PrefabName;
    private string MaterialName;
    private SerializableTransform Transform;

    // memento info
    private DateTime Date;
    private Location ObjectLocation;

    public Memento(Guid guid, string prefabName, string materialName, SerializableTransform transform) {
        Guid = guid;
        PrefabName = prefabName;
        MaterialName = materialName;
        Transform = transform;
        Date = DateTime.Now;
    }

    // Call the less demanding constructor + add assignment to the field
    public Memento(Guid guid, string prefabName, string materialName, SerializableTransform transform, Location objectLocation) 
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
    public Location GetObjectLocation() {
        return ObjectLocation;
    }
}
