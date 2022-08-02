using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// mi posso permettere di usare un dizionario e non una lista perchè non mi server uno storico dell'oggetto,

public class CaretakerScene : MonoBehaviour
{
    // keeps mementos of global scene
    private static Dictionary<Guid, Memento> globalListMementos { get; } = new Dictionary<Guid, Memento>();

    // keeps mementos of local scene
    private static Dictionary<Guid, Memento> localListMementos { get; } = new Dictionary<Guid, Memento>();

    // don't really know if i should keep this variable here
    private static Location sceneState = Location.LocalLayer;

    // need 4 events, because the save and restore are done in different moments:
    // save is done always before the restore, and some objs may not be in both scenes
    public static UnityEvent saveGlobalState = new UnityEvent();
    public static UnityEvent saveLocalState = new UnityEvent();

    public static UnityEvent restoreGlobalState = new UnityEvent();
    public static UnityEvent restoreLocalState = new UnityEvent();

    void Start() {
        UIManager.Instance.ChangeSceneStateText(sceneState);
        saveLocalState.Invoke();
    }

    public static void ChangeScene() 
    {
        //salva lo stato degli oggetti nelle loro liste corrette chiamando gli invoke

        //la funzione di salvataggio legge in che scena l'utente è e salva nella lista corrispondente (tipo se sono in local, salvo in local)

        if (sceneState.Equals(Location.GlobalLayer))
            SaveGlobalRestoreLocal();
        else if (sceneState.Equals(Location.LocalLayer))
            SaveLocalRestoreGlobal();

        //la funzione di flip fa il cambio da global a local e viceversa
        FlipSceneState();
    }

    private static void SaveGlobalRestoreLocal() {
        // before changing to local, save the global one
        saveGlobalState.Invoke();
        restoreLocalState.Invoke();
    }

    private static void SaveLocalRestoreGlobal() {
        //before changing to global, save the local one
        saveLocalState.Invoke();
        restoreGlobalState.Invoke();
    }

    public static void SaveGlobalState(GameObjController gObj) {
        globalListMementos[gObj.Guid] = gObj.Save(); // Add or update! No need to check if item already exists in list
    }

    public static void SaveLocalState(GameObjController gObj) {
        localListMementos[gObj.Guid] = gObj.Save(); // Add or update! No need to check if item already exists in list
    }

    public static void RestoreGlobalState(GameObjController gObj) {
        Memento value;

        if (globalListMementos.TryGetValue(gObj.Guid, out value))
            gObj.Restore(value);
        else 
        {
            Debug.Log("Key " + gObj.Guid + " not found in dictionary GlobalListMementos");
            UIManager.Instance.PrintMessages("Key " + gObj.Guid + " not found in dictionary GlobalListMementos");
        }
    }

    public static void RestoreLocalState(GameObjController gObj) {
        Memento value;

        if (localListMementos.TryGetValue(gObj.Guid, out value))
            gObj.Restore(value);
        else 
        {
            Debug.Log("Key " + gObj.Guid + " not found in dictionary LocalListMementos");
            UIManager.Instance.PrintMessages("Key " + gObj.Guid + " not found in dictionary LocalListMementos");
        }
    }

    /*
    private static void SaveState(GameObjController gObj) {
        if (sceneState.Equals(Location.LocalLayer)) {
            SaveLocalState(gObj);
        }

        if (sceneState.Equals(Location.GlobalLayer)) {
            SaveGlobalState(gObj);
        }
    }*/

    private static void FlipSceneState() 
    {
        if (sceneState.Equals(Location.LocalLayer)) {
            ChangeSceneState(Location.GlobalLayer);
        }
        else if (sceneState.Equals(Location.GlobalLayer)) {
            ChangeSceneState(Location.LocalLayer);
        }

        UIManager.Instance.ChangeSceneStateText(sceneState);
    }

    private static void ChangeSceneState(Location sceneState) {
        CaretakerScene.sceneState = sceneState;
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
