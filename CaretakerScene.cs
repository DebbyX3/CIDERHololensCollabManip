using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
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

public class CaretakerScene : MonoBehaviour
{
    public static CaretakerScene Instance { get; private set; }

    // keeps mementos of global scene
    private Dictionary<Guid, Memento> GlobalListMementos { get; } = new Dictionary<Guid, Memento>();

    // keeps mementos of local scene
    private Dictionary<Guid, Memento> LocalListMementos { get; } = new Dictionary<Guid, Memento>();

    // not sure about the use of a Memento object here
    private Dictionary<Guid, Memento> PendingListRequests { get; } = new Dictionary<Guid, Memento>();

    // don't really know if i should keep this variable here
    private Location SceneState = Location.LocalLayer;

    // need 4 events, because the save and restore are done in different moments:
    // save is done always before the restore, and some objs may not be in both scenes
    public UnityEvent SaveGlobalStateEvent = new UnityEvent();
    public UnityEvent SaveLocalStateEvent = new UnityEvent();

    public UnityEvent RestoreGlobalStateEvent = new UnityEvent();
    public UnityEvent RestoreLocalStateEvent = new UnityEvent();

    public UnityEvent HideObjectEvent = new UnityEvent();

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

    private void SaveGlobalRestoreLocal()
    {
        // before changing to local, save the global one
        SaveGlobalStateEvent.Invoke();

        // hide all objects (each obj is subscribed to this event)
        HideObjectEvent.Invoke();

        // change to local
        RestoreLocalStateEvent.Invoke();
    }

    private void SaveLocalRestoreGlobal()
    {
        //before changing to global, save the local one
        SaveLocalStateEvent.Invoke();

        // hide all objects (each obj is subscribed to this event)
        HideObjectEvent.Invoke();

        // change to global
        RestoreGlobalStateEvent.Invoke();
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
            SaveGlobalRestoreLocal();
        else if (SceneState.Equals(Location.LocalLayer))
            SaveLocalRestoreGlobal();

        //la funzione di flip fa il cambio da global a local e viceversa
        FlipSceneState();
    }

    public void ChangeSceneToGlobal()
    {
        if (SceneState.Equals(Location.LocalLayer))
        {
            SaveLocalRestoreGlobal();
            ChangeSceneState(Location.GlobalLayer);
            UIManager.Instance.ChangeSceneStateText(SceneState);
        }
    }

    public void ChangeSceneToLocal()
    {
        if (SceneState.Equals(Location.GlobalLayer))
        {
            SaveGlobalRestoreLocal();
            ChangeSceneState(Location.LocalLayer);
            UIManager.Instance.ChangeSceneStateText(SceneState);
        }
    }

    //*****************

    public void SaveGlobalState(GameObjController gObj)
    {
        GlobalListMementos[gObj.Guid] = gObj.Save(); // Add or update! No need to check if item already exists in list
    }

    public void SaveLocalState(GameObjController gObj)
    {
        LocalListMementos[gObj.Guid] = gObj.Save(); // Add or update! No need to check if item already exists in list
    }

    public void SavePendingState(GameObjController gObj)
    {
        PendingListRequests[gObj.Guid] = gObj.Save(); // Add or update! No need to check if item already exists in list
    }

    public void EmptyAllPendingStates()
    {
        PendingListRequests.Clear();
    }

    public void RemoveFromPendingStates(GameObjController gObj)
    {
        PendingListRequests.Remove(gObj.Guid);
    }

    public void RestoreGlobalState(GameObjController gObj)
    {
        Memento value;

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

    public void RestoreLocalState(GameObjController gObj)
    {
        Memento value;

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

    public void ExecuteForcedCommit(GameObjController gObj)
    {
        //Instance.SaveGlobalState(gObj);
        //Instance.ChangeSceneToGlobal();
    }

    public void ExecuteVotingCommit(GameObjController gObj)
    {
        Instance.SavePendingState(gObj);
    }


    // mi sa che questi 3 li posso spostare in UIManager?
    public void ShowDialogOnGlobalScene(Dialog dialog)
    {
        if (IsGlobalScene())
        {
            dialog.gameObject.SetActive(true);
            dialog.GetComponent<SolverHandler>().enabled = true;
        }
    }

    public void ShowSlateOnLocalScene(GameObject slate)
    {
        if (IsLocalScene())
        {
            slate.gameObject.SetActive(true);
            slate.GetComponent<RadialView>().enabled = true;
        }
    }

    public void HideSlateOnGlobalScene(GameObject slate)
    {
        if (IsGlobalScene())
        {
            slate.gameObject.SetActive(false);
        }
    }

}
