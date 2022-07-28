using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectLocation : int {
    OnlyLocalLayer,
    OnlyGlobalLayer,
    BothLocalGlobal // non so se ha senso, forse basta mettere due enum:
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
    private Guid guid;
    private string prefabName;
    private SerializableTransform transform;

    // memento info
    private DateTime date;
    private ObjectLocation objectLocation;

    public Memento(Guid guid, string prefabName, SerializableTransform transform) {
        this.guid = guid;
        this.prefabName = prefabName;
        this.transform = transform;
        this.date = DateTime.Now;
    }

    //call the less demanding constructor + add assignment to the field
    public Memento(Guid guid, string prefabName, SerializableTransform transform, ObjectLocation objectLocation) : this(guid, prefabName, transform)
    {
        this.objectLocation = objectLocation;
    }

    // The Originator (gobjcontroller) uses this method when restoring its state.
    // forse ne mancano altri??
    public Guid GetGuid() {
        return this.guid;
    }

    public string GetPrefabName() {
        return prefabName;
    }

    public SerializableTransform GetTransform() {
        return transform;
    }

    // The rest of the methods are used by the Caretaker to display metadata.
    public string GetName() 
    {
        return $"{date} / Object: {prefabName} - GUID: {guid} - Transform: {transform}";        
    }

    public DateTime GetDate() {
        return date;
    }
    public ObjectLocation GetObjectLocation() {
        return objectLocation;
    }
}
