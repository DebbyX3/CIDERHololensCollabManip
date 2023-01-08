using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
 PLEASE NOTE!
    CareTakerScene MUST BE executed BEFORE the GameObjController script! 
    So, in the Unity execution order list, CareTakerScene has a lower number than GameObjController 
    (that means GameObjController has an higher number than CareTakerScene)

    This is because GameObjController needs an instance of CareTakerScene in the Awake, 
    and the operations that use this instance of CareTaker cannot be moved in the Start method of GameObjController   
    (it needs to attach listeners to events, so the Awake is necessary before the Start of the placed object is called)
 */

public enum Location : int
{
    LocalLayer,
    GlobalLayer
}

public class CaretakerScene : MonoBehaviour
{
    public static CaretakerScene Instance { get; private set; }

    /*
    Note: I've considered keeping the 2 Global and Local lists as just one list because having 2 lists that 
    specify the behavior and meaning of the object location is not the best, also because I have the ObjectLocation
    enum inside every gobjcontroller, and a mishmatch of flag and list belonging could be a disaster! 
    So I must be VERY careful

    But I can't keep the 2 lists together, because when I want to retrive the global (or local) state, I don't want to
    overwrite the previous state (and that happens for sure since the key is the guid!)
    */

    // keeps mementos of global scene
    public Dictionary<Guid, Memento> GlobalListMementos { get; set; } = new Dictionary<Guid, Memento>();

    // keeps mementos of local scene
    public Dictionary<Guid, Memento> LocalListMementos { get; } = new Dictionary<Guid, Memento>();

    public Dictionary<Guid, Memento> CommitPendingListRequests { get; } = new Dictionary<Guid, Memento>();

    public Dictionary<Guid, Memento> DeletionPendingListRequests { get; } = new Dictionary<Guid, Memento>();

    private Location SceneState = Location.LocalLayer;

    // Need different events for save and restore, because the save and restore are done in different moments:
    // Save is done always before the restore, and some objs may not be in both scenes
    public UnityEvent SaveGlobalStateEvent = new UnityEvent();
    public UnityEvent SaveLocalStateEvent = new UnityEvent();
    public UnityEvent SaveCommitPendingListEvent = new UnityEvent();
    public UnityEvent SaveDeletionPendingListEvent = new UnityEvent();

    public UnityEvent RestoreGlobalStateEvent = new UnityEvent();
    public UnityEvent RestoreLocalStateEvent = new UnityEvent();
    public UnityEvent RestoreCommitPendingListEvent = new UnityEvent();
    public UnityEvent RestoreDeletionPendingListEvent = new UnityEvent();

    public UnityEvent HideObjectEvent = new UnityEvent();

    // Stores the current active gameobject - where 'active' means the last object touched/manipolated
    public GameObjController CurrentActiveGObj { get; set; }

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

    private void Start()
    {
        UIManager.Instance.ChangeSceneStateText(SceneState);

        SaveLocalStateEvent.Invoke();
        SaveGlobalStateEvent.Invoke();
    }

    private void SaveGlobalAndPendingRestoreLocal()
    {
        // before changing to local, save the global and pending scenes
        SaveDeletionPendingListEvent.Invoke();
        SaveCommitPendingListEvent.Invoke();
        SaveGlobalStateEvent.Invoke();

        // hide all objects (each obj is subscribed to this event)
        HideObjectEvent.Invoke();

        // change to local
        RestoreLocalStateEvent.Invoke();
    }

    private void SaveLocalRestoreGlobalAndPending()
    {
        //before changing to global, save the local one
        SaveLocalStateEvent.Invoke();

        // hide all objects (each obj is subscribed to this event)
        HideObjectEvent.Invoke();

        // change to global and pending
        RestoreGlobalStateEvent.Invoke();
        RestoreCommitPendingListEvent.Invoke();
        RestoreDeletionPendingListEvent.Invoke();
    }

    private void FlipSceneState()
    {
        if (SceneState.Equals(Location.LocalLayer))
        {
            ChangeSceneState(Location.GlobalLayer);
        }
        else if (SceneState.Equals(Location.GlobalLayer))
        {
            ChangeSceneState(Location.LocalLayer);
        }

        UIManager.Instance.ChangeSceneStateText(SceneState);
    }

    private void ChangeSceneState(Location sceneState)
    {
        Instance.SceneState = sceneState;
    }

    //------------ PUBLIC
    public bool IsGlobalScene()
    {
        return SceneState.Equals(Location.GlobalLayer);
    }

    public bool IsLocalScene()
    {
        return SceneState.Equals(Location.LocalLayer);
    }

    public void ChangeScene()
    {
        //salva lo stato degli oggetti nelle loro liste corrette chiamando gli invoke

        //la funzione di salvataggio legge in che scena l'utente è e salva nella lista corrispondente (tipo se sono in local, salvo in local)

        if (SceneState.Equals(Location.GlobalLayer))
            SaveGlobalAndPendingRestoreLocal();
        else if (SceneState.Equals(Location.LocalLayer))
            SaveLocalRestoreGlobalAndPending();

        //la funzione di flip fa il cambio da global a local e viceversa
        FlipSceneState();
    }

    public void ChangeSceneToGlobal()
    {
        if (SceneState.Equals(Location.LocalLayer))
        {
            SaveLocalRestoreGlobalAndPending();
            ChangeSceneState(Location.GlobalLayer);
            UIManager.Instance.ChangeSceneStateText(SceneState);
        }
    }

    public void ChangeSceneToLocal()
    {
        if (SceneState.Equals(Location.GlobalLayer))
        {
            SaveGlobalAndPendingRestoreLocal();
            ChangeSceneState(Location.LocalLayer);
            UIManager.Instance.ChangeSceneStateText(SceneState);
        }
    }

    //*****************

    public void SaveGlobalState(GameObjController gObj)
    {
        if (!CommitPendingListRequests.ContainsKey(gObj.Guid) && !DeletionPendingListRequests.ContainsKey(gObj.Guid))
            GlobalListMementos[gObj.Guid] = gObj.Save(); // Add or update! No need to check if item already exists in list
    }

    public void SaveLocalState(GameObjController gObj)
    {
        LocalListMementos[gObj.Guid] = gObj.Save(); // Add or update! No need to check if item already exists in list
    }

    public void SaveCommitPendingState(GameObjController gObj)
    {
        CommitPendingListRequests[gObj.Guid] = gObj.Save(); // Add or update! No need to check if item already exists in list
    }

    public void SaveDeletionPendingState(GameObjController gObj)
    {
        DeletionPendingListRequests[gObj.Guid] = gObj.Save(); // Add or update! No need to check if item already exists in list
    }

    public void RestoreGlobalState(GameObjController gObj)
    {
        Memento value;

        if (gObj.ObjectLocation.HasFlag(ObjectLocation.Global))
        {
            if (GlobalListMementos.TryGetValue(gObj.Guid, out value)) // if the obj is in the global list, restore it
            {
                // show obj since every obj is hidden because of previous HideObject(GameObjController gObj) call  
                gObj.gameObject.SetActive(true);
                gObj.Restore(value);
            }
            else
            {
                Debug.Log("Key " + gObj.Guid + " not found in dictionary GlobalListMementos");
                UIManager.Instance.PrintMessages("Key " + gObj.Guid + " not found in dictionary GlobalListMementos");
            }
        }
    }

    public void RestoreLocalState(GameObjController gObj)
    {
        Memento value;

        if (gObj.ObjectLocation.HasFlag(ObjectLocation.Local))
        {
            if (LocalListMementos.TryGetValue(gObj.Guid, out value)) // if the obj is in the local list, restore it
            {
                // show obj since every obj is hidden because of previous HideObject(GameObjController gObj) call           
                gObj.gameObject.SetActive(true);
                gObj.Restore(value);
            }
            else
            {
                Debug.Log("Key " + gObj.Guid + " not found in dictionary LocalListMementos");
                UIManager.Instance.PrintMessages("Key " + gObj.Guid + " not found in dictionary LocalListMementos");
            }
        }
    }

    public void RestoreCommitPendingState(GameObjController gObj)
    {
        Memento value;

        if (gObj.ObjectLocation.HasFlag(ObjectLocation.CommitPending))
        {
            if (CommitPendingListRequests.TryGetValue(gObj.Guid, out value)) // if the obj is in the commit pending list, restore it
            {
                // show obj since every obj is hidden because of previous HideObject(GameObjController gObj) call           
                gObj.gameObject.SetActive(true);
                gObj.Restore(value);

                PrefabManager.Instance.ChangeMaterialCommitPendingState(gObj.gameObject);
            }
            else
            {
                Debug.Log("Key " + gObj.Guid + " not found in dictionary CommitPendingListRequests");
                UIManager.Instance.PrintMessages("Key " + gObj.Guid + " not found in dictionary CommitPendingListRequests");
            }
        }
    }
    
    public void RestoreDeletionPendingState(GameObjController gObj)
    {
        Memento value;

        if (gObj.ObjectLocation.HasFlag(ObjectLocation.DeletionPending))
        {
            if (DeletionPendingListRequests.TryGetValue(gObj.Guid, out value)) // if the obj is in the Deletion pending list, restore it
            {
                // show obj since every obj is hidden because of previous HideObject(GameObjController gObj) call           
                gObj.gameObject.SetActive(true);
                gObj.Restore(value);

                PrefabManager.Instance.ChangeMaterialDeletionPendingState(gObj.gameObject);
            }
            else
            {
                Debug.Log("Key " + gObj.Guid + " not found in dictionary DeletionPendingListRequests");
                UIManager.Instance.PrintMessages("Key " + gObj.Guid + " not found in dictionary DeletionPendingListRequests");
            }
        }
    }

    public void RemoveFromLocalList(Guid guid)
    {
        LocalListMementos.Remove(guid);                 // If the dict does not contain an element with the
                                                        // specified key, the dict remains unchanged.
                                                        // No exception is thrown.
    }

    public void RemoveFromGlobalList(Guid guid)
    {
        GlobalListMementos.Remove(guid);                // If the dict does not contain an element with the
                                                        // specified key, the dict remains unchanged.
                                                        // No exception is thrown.
    }

    public void RemoveFromCommitPendingList(Guid guid)
    {
        CommitPendingListRequests.Remove(guid);         // If the dict does not contain an element with the
                                                        // specified key, the dict remains unchanged.
                                                        // No exception is thrown.
    }
    
    public void RemoveFromDeletionPendingList(Guid guid)
    {
        DeletionPendingListRequests.Remove(guid);       // If the dict does not contain an element with the
                                                        // specified key, the dict remains unchanged.
                                                        // No exception is thrown.
    }

    public void EmptyAllCommitPendingStates()
    {
        CommitPendingListRequests.Clear();
    }

    public void ResubscribeRemainingObjsToGlobalEvents()
    {
        GameObjController gobjController;

        foreach (GameObject gobj in GUIDKeeper.List.Values)
        {
            gobjController = gobj.GetComponent<GameObjController>();

            if(gobjController.ObjectLocation.HasFlag(ObjectLocation.Global))
                gobjController.SubscribeToGlobalScene();
        }
    }

    public void ResubscribeRemainingObjsToLocalEvents()
    {
        GameObjController gobjController;

        foreach (GameObject gobj in GUIDKeeper.List.Values)
        {
            gobjController = gobj.GetComponent<GameObjController>();

            if (gobjController.ObjectLocation.HasFlag(ObjectLocation.Local))
                gobjController.SubscribeToLocalScene();
        }
    }

    public void ResubscribeRemainingObjsToCommitPendingEvents()
    {
        GameObjController gobjController;

        foreach (GameObject gobj in GUIDKeeper.List.Values)
        {
            gobjController = gobj.GetComponent<GameObjController>();

            if (gobjController.ObjectLocation.HasFlag(ObjectLocation.CommitPending))
                gobjController.SubscribeToCommitPendingList();
        }
    }    
    
    public void ResubscribeRemainingObjsToDeletionPendingEvents()
    {
        GameObjController gobjController;

        foreach (GameObject gobj in GUIDKeeper.List.Values)
        {
            gobjController = gobj.GetComponent<GameObjController>();

            if (gobjController.ObjectLocation.HasFlag(ObjectLocation.DeletionPending))
                gobjController.SubscribeToDeletionPendingList();
        }
    }
}
