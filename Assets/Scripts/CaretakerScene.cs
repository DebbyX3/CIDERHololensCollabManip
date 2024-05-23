using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    // binary file of an existing global scene to load
    public TextAsset globalSceneToLoad;

    /*
    Note: I've considered keeping the 2 Global and Local lists as just one list because having 2 lists that 
    specify the behavior and meaning of the object location is not the best, also because I have the ObjectLocation
    enum inside every gobjcontroller, and a mishmatch of flag and list belonging could be a disaster! 
    So I must be VERY careful

    But I can't keep the 2 lists together, because when I want to retrive the global (or local) state, I don't want to
    overwrite the previous state (and that happens for sure since the key is the guid!)
    */

    // keeps mementos of global scene
    [HideInInspector]
    public Dictionary<Guid, Memento> GlobalListMementos { get; } = new Dictionary<Guid, Memento>();

    // keeps mementos of local scene
    private Dictionary<Guid, Memento> LocalListMementos { get; } = new Dictionary<Guid, Memento>();

    private Dictionary<Guid, Memento> CommitPendingListRequests = new Dictionary<Guid, Memento>();

    private Dictionary<Guid, Memento> DeletionPendingListRequests { get; } = new Dictionary<Guid, Memento>();

    public Location SceneState { get; private set; } = Location.LocalLayer;

    // Need different events for save and restore, because the save and restore are done in different moments:
    // Save is done always before the restore, and some objs may not be in both scenes
    [HideInInspector]
    public UnityEvent SaveGlobalStateEvent = new UnityEvent();

    [HideInInspector]
    public UnityEvent SaveLocalStateEvent = new UnityEvent();

    [HideInInspector]
    public UnityEvent SaveCommitPendingListEvent = new UnityEvent();

    [HideInInspector]
    public UnityEvent SaveDeletionPendingListEvent = new UnityEvent();


    [HideInInspector]
    public UnityEvent RestoreGlobalStateEvent = new UnityEvent();

    [HideInInspector]
    public UnityEvent RestoreLocalStateEvent = new UnityEvent();

    [HideInInspector]
    public UnityEvent RestoreCommitPendingListEvent = new UnityEvent();

    [HideInInspector]
    public UnityEvent RestoreDeletionPendingListEvent = new UnityEvent();


    [HideInInspector]
    public UnityEvent HideObjectEvent = new UnityEvent();

    // Stores the current active gameobject - where 'active' means the last object touched/manipolated
    public GameObjController CurrentActiveGObj { get; set; } = null;

    // Stores the current manipulated object - if no obj is manipulated, the field is null
    public GameObjController CurrentManipolatedGObj { get; set; } = null;

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

        // Decomment this only if you want to load a specific saved scene
        //LoadGlobalScene();
    }

    private void LoadGlobalScene()
    {
        Dictionary<Guid, Memento> loadedGlobalList = ObjectsFiles.ReadData(globalSceneToLoad);

        foreach (KeyValuePair<Guid, Memento> entry in loadedGlobalList)
        {
            PrefabManager.Instance.CreateNewObjectInGlobal(entry.Key, entry.Value.GetPrefabName(), entry.Value.GetMaterialName(), entry.Value.GetTransform());
        }
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
        // Save object states in their correct lists by calling the invokes

        // The save function reads in which scene the user is and saves in the corresponding list (e.g. if I'm in local, I save in local)

        if (SceneState.Equals(Location.GlobalLayer))
            SaveGlobalAndPendingRestoreLocal();
        else if (SceneState.Equals(Location.LocalLayer))
            SaveLocalRestoreGlobalAndPending();

        // The flip function changes from global to local and vice versa
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

    public void SaveGlobalState(GameObjController gObjContr)
    {
        if (!CommitPendingListRequests.ContainsKey(gObjContr.Guid) && !DeletionPendingListRequests.ContainsKey(gObjContr.Guid))
            GlobalListMementos[gObjContr.Guid] = gObjContr.Save(); // Add or update! No need to check if item already exists in list
    }

    public void SaveLocalState(GameObjController gObjContr)
    {
        LocalListMementos[gObjContr.Guid] = gObjContr.Save(); // Add or update! No need to check if item already exists in list
    }

    public void SaveCommitPendingState(GameObjController gObjContr)
    {
        CommitPendingListRequests[gObjContr.Guid] = gObjContr.Save(); // Add or update! No need to check if item already exists in list
    }

    public void SaveDeletionPendingState(GameObjController gObjContr)
    {
        DeletionPendingListRequests[gObjContr.Guid] = gObjContr.Save(); // Add or update! No need to check if item already exists in list
    }

    public void RestoreGlobalState(GameObjController gObjContr)
    {
        Memento value;

        if (gObjContr.ObjectLocation.HasFlag(ObjectLocation.Global))
        {
            if (GlobalListMementos.TryGetValue(gObjContr.Guid, out value)) // if the obj is in the global list, restore it
            {
                // show obj since every obj is hidden because of previous HideObject(GameObjController gObj) call  
                gObjContr.gameObject.SetActive(true);
                gObjContr.Restore(value);
            }
            else
            {
                Debug.Log("Key " + gObjContr.Guid + " not found in dictionary GlobalListMementos");
                UIManager.Instance.PrintMessages("Key " + gObjContr.Guid + " not found in dictionary GlobalListMementos");
            }
        }
    }

    public void RestoreLocalState(GameObjController gObjContr)
    {
        Memento value;

        if (gObjContr.ObjectLocation.HasFlag(ObjectLocation.Local))
        {
            if (LocalListMementos.TryGetValue(gObjContr.Guid, out value)) // if the obj is in the local list, restore it
            {
                // show obj since every obj is hidden because of previous HideObject(GameObjController gObj) call           
                gObjContr.gameObject.SetActive(true);
                gObjContr.Restore(value);
            }
            else
            {
                Debug.Log("Key " + gObjContr.Guid + " not found in dictionary LocalListMementos");
                UIManager.Instance.PrintMessages("Key " + gObjContr.Guid + " not found in dictionary LocalListMementos");
            }
        }
    }

    public void RestoreCommitPendingState(GameObjController gObjContr)
    {
        Memento value;

        if (gObjContr.ObjectLocation.HasFlag(ObjectLocation.CommitPending))
        {
            if (CommitPendingListRequests.TryGetValue(gObjContr.Guid, out value)) // if the obj is in the commit pending list, restore it
            {
                // show obj since every obj is hidden because of previous HideObject(GameObjController gObj) call           
                gObjContr.gameObject.SetActive(true);
                gObjContr.Restore(value);

                gObjContr.EnableMeshOutlineCommitPending(true);
            }
            else
            {
                Debug.Log("Key " + gObjContr.Guid + " not found in dictionary CommitPendingListRequests");
                UIManager.Instance.PrintMessages("Key " + gObjContr.Guid + " not found in dictionary CommitPendingListRequests");
            }
        }
    }

    public void RestoreDeletionPendingState(GameObjController gObjContr)
    {
        Memento value;

        if (gObjContr.ObjectLocation.HasFlag(ObjectLocation.DeletionPending))
        {
            if (DeletionPendingListRequests.TryGetValue(gObjContr.Guid, out value)) // if the obj is in the Deletion pending list, restore it
            {
                // show obj since every obj is hidden because of previous HideObject(GameObjController gObj) call           
                gObjContr.gameObject.SetActive(true);
                gObjContr.Restore(value);

                gObjContr.EnableMeshOutlineDeletionPending(true);
            }
            else
            {
                Debug.Log("Key " + gObjContr.Guid + " not found in dictionary DeletionPendingListRequests");
                UIManager.Instance.PrintMessages("Key " + gObjContr.Guid + " not found in dictionary DeletionPendingListRequests");
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

            if (gobjController.ObjectLocation.HasFlag(ObjectLocation.Global))
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

    // Return a read only reference to the commit pending list - I just want to read it
    public ref readonly Dictionary<Guid, Memento> GetCommitPendingListRequests()
    {
        return ref CommitPendingListRequests;
    }
}