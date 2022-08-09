using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// mi posso permettere di usare un dizionario e non una lista perchè non mi server uno storico dell'oggetto,

public class CaretakerScene : MonoBehaviour
{
    public static CaretakerScene Instance { get; private set; }

    // keeps mementos of global scene
    private Dictionary<Guid, Memento> globalListMementos { get; } = new Dictionary<Guid, Memento>();

    // keeps mementos of local scene
    private Dictionary<Guid, Memento> localListMementos { get; } = new Dictionary<Guid, Memento>();

    // not sure about the use of a Memento object here
    private Dictionary<Guid, Memento> pendingListRequests { get; } = new Dictionary<Guid, Memento>();

    // don't really know if i should keep this variable here
    private Location sceneState = Location.LocalLayer;

    // need 4 events, because the save and restore are done in different moments:
    // save is done always before the restore, and some objs may not be in both scenes
    public UnityEvent saveGlobalState = new UnityEvent();
    public UnityEvent saveLocalState = new UnityEvent();

    public UnityEvent restoreGlobalState = new UnityEvent();
    public UnityEvent restoreLocalState = new UnityEvent();

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start() {
        UIManager.Instance.ChangeSceneStateText(sceneState);

        saveLocalState.Invoke();
        saveGlobalState.Invoke();
    }

    public void ChangeScene()
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

    public void ChangeSceneToGlobal()
    {
        if (sceneState.Equals(Location.LocalLayer))
        {
            SaveLocalRestoreGlobal();
            ChangeSceneState(Location.GlobalLayer);
            UIManager.Instance.ChangeSceneStateText(sceneState);
        }
    }

    private void SaveGlobalRestoreLocal() {
        // before changing to local, save the global one
        saveGlobalState.Invoke();
        restoreLocalState.Invoke();
    }

    private void SaveLocalRestoreGlobal() {
        //before changing to global, save the local one
        saveLocalState.Invoke();
        restoreGlobalState.Invoke();
    }

    public void SaveGlobalState(GameObjController gObj) {
        globalListMementos[gObj.Guid] = gObj.Save(); // Add or update! No need to check if item already exists in list
    }

    public void SaveLocalState(GameObjController gObj) {
        localListMementos[gObj.Guid] = gObj.Save(); // Add or update! No need to check if item already exists in list
    }

    public void RestoreGlobalState(GameObjController gObj) {
        Memento value;

        if (globalListMementos.TryGetValue(gObj.Guid, out value))
            gObj.Restore(value);
        else
        {
            Debug.Log("Key " + gObj.Guid + " not found in dictionary GlobalListMementos");
            UIManager.Instance.PrintMessages("Key " + gObj.Guid + " not found in dictionary GlobalListMementos");
        }
    }

    public void RestoreLocalState(GameObjController gObj) {
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

    private void FlipSceneState()
    {
        if (sceneState.Equals(Location.LocalLayer)) {
            ChangeSceneState(Location.GlobalLayer);
        }
        else if (sceneState.Equals(Location.GlobalLayer)) {
            ChangeSceneState(Location.LocalLayer);
        }

        UIManager.Instance.ChangeSceneStateText(sceneState);
    }

    private void ChangeSceneState(Location sceneState) {
        Instance.sceneState = sceneState;
    }

    public void ExecuteForcedCommit(GameObjController gObj)
    {
        Instance.SaveGlobalState(gObj);
        Instance.ChangeSceneToGlobal();
    }
}
