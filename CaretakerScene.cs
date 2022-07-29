using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// mi posso permettere di usare un dizionario e non una lista perchè non mi server uno storico dell'oggetto,
//visto che la 'storia' sarebbe 
//forse non serve tutta la classe static? boh
public class CaretakerScene : MonoBehaviour
{
    //keeps mementos of global scene
    private static Dictionary<Guid, Memento> globalListMementos { get; } = new Dictionary<Guid, Memento>();

    //keeps mementos of local scene
    private static Dictionary<Guid, Memento> localListMementos { get; } = new Dictionary<Guid, Memento>();

    // don't really know if i should keep this variable here
    private static Location sceneState = Location.LocalLayer;

    public static UnityEvent saveGlobalState = new UnityEvent();
    public static UnityEvent saveLocalState = new UnityEvent();

    //provo a mettere solo un evento generico?
    public static UnityEvent saveGenericState = new UnityEvent();

    public static void SaveGlobalState(GameObjController gObj) {
        //salva
        if (globalListMementos.ContainsKey(gObj.Guid)) {
            globalListMementos[gObj.Guid] = gObj.Save();
        } 
        else {
            globalListMementos.Add(gObj.Guid, gObj.Save());
        }
    }

    public static void SaveLocalState(GameObjController gObj) {
        //salva
        if (localListMementos.ContainsKey(gObj.Guid)) {
            localListMementos[gObj.Guid] = gObj.Save();
        }
        else {
            localListMementos.Add(gObj.Guid, gObj.Save());
        }
    }

    public static void ChangeSceneToLocal()        
    {
        //before changing to local, save the global one
        saveGlobalState.Invoke();

        ChangeSceneState(Location.LocalLayer);
    }

    public static void ChangeSceneToGlobal() 
    {
        //before changing to global, save the local one
        saveLocalState.Invoke();

        ChangeSceneState(Location.GlobalLayer);
    }

    // nota: questa funzione dovrebbe fare tutto da sola il cambio scena, senza passar in che scena si vuole cambiare e assumendo
    // di averne solo una. se invece voglio specificare il tipo di scena da cambiare, devo usare le funzioni sopra
    public static void ChangeScene() 
    {        
        //salva lo stato degli oggetti nelle loro liste corrette chiamando un invoke
        //la funzione di salvataggio legge in che scena l'utente è e salva nella lista corrispondente (tipo se sono in local, salvo in local)
        saveGenericState.Invoke();

        //la funzione di flip fa il cambio da global a local e viceversa
        FlipSceneState();
    }

    public static void ChangeSceneState(Location sceneState) {
        CaretakerScene.sceneState = sceneState;
    }

    // don't know if that's useful
    public static void FlipSceneState() 
    {
        if (sceneState.Equals(Location.LocalLayer)) {
            sceneState = Location.GlobalLayer;
        }

        if (sceneState.Equals(Location.GlobalLayer)) {
            sceneState = Location.LocalLayer;
        }
    }
    
    public static void SaveState(GameObjController gObj)
    {        
        if (sceneState.Equals(Location.LocalLayer)) 
        {
            if (localListMementos.ContainsKey(gObj.Guid)) {
                localListMementos[gObj.Guid] = gObj.Save();
            } else {
                localListMementos.Add(gObj.Guid, gObj.Save());
            }
        }

        if (sceneState.Equals(Location.GlobalLayer)) {
            if (globalListMementos.ContainsKey(gObj.Guid)) {
                globalListMementos[gObj.Guid] = gObj.Save();
            } else {
                globalListMementos.Add(gObj.Guid, gObj.Save());
            }
        }
    }

    /*
    public void Backup() {
        Debug.Log("\nCaretaker: Saving Originator (GObj)'s state...");
        this.globalListMementos.Add(this.Originator.Save());
    }

    public void Undo() {
        if (this.globalListMementos.Count == 0) {
            return;
        }

        var memento = globalListMementos[globalListMementos.Count-1]; // get last element!
        
        // ma non lo devo rimuovere! mi sa che devo ripensare al salvataggio mhm
        //this.mementos.Remove(memento);

        Debug.Log("Caretaker: Restoring state to: " + memento.GetName());

        try {
            this.Originator.Restore(memento);
        } catch (Exception) {
            this.Undo();
        }
    }

    public void ShowHistory() {
        Console.WriteLine("Caretaker: Here's the list of mementos:");

        foreach (var memento in this.globalListMementos) {
            Debug.Log(memento.GetName());
        }
    }*/
}
