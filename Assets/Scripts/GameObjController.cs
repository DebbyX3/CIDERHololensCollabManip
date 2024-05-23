using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
using UnityEngine;
using UnityEngine.Events;
using MSUtilities = Microsoft.MixedReality.Toolkit.Utilities;

/*
 PLEASE NOTE!
    GameObjController MUST BE executed AFTER the CareTakerScene script! 
    So, in the Unity execution order list, CareTakerScene has a lower number than GameObjController 
    (that means GameObjController has an higher number than CareTakerScene)

    This is because GameObjController needs an instance of CareTakerScene in the Awake, 
    and the operations that use this instance of CareTaker cannot be moved in the Start method of GameObjController   
    (it needs to attach listeners to events, so the Awake is necessary before the Start of the placed object is called)


 PLEASE NOTE!
    UIManager needs to be run BEFORE GameObjController, because GameObjController needs references to the object that UIManager has
 */

/*
 A Flag enum is useful because I can combine the flags together to get a combination.
 In this case I can have both global and local as the object location (Global + Local = 3)
 */
[Flags]
public enum ObjectLocation
{
    None = 0,
    Local = 1,
    Global = 2,
    CommitPending = 4,
    DeletionPending = 8
}

public class GameObjController : MonoBehaviour
{
    public Guid Guid { get; private set; }
    public string PrefabName { get; private set; }
    public string MaterialName { get; private set; }

    // Use this just because it is easier to access rather than always doing gameObject.transform
    // Would set this as readonly, but I can't since therefore it needs to be initializated in the constructor
    // (and Unity does not allow constructors)
    public Transform Transform;

    public MSUtilities.MeshOutline meshOutlineCommitPending;
    public MSUtilities.MeshOutline meshOutlineDeletionPending;

    public ObjectLocation ObjectLocation { get; private set; } = ObjectLocation.None;

    // Indicates if the pending obj was received or sent
    public UserType CommitPendingObjectUserType { get; private set; } = UserType.None;

    public UserType DeletionPendingObjectUserType { get; private set; } = UserType.None;

    // Need to keep the references to the unity actions in order to disable them
    private UnityAction SaveGlobalStateAction;
    private UnityAction RestoreGlobalStateAction;

    private UnityAction SaveLocalStateAction;
    private UnityAction RestoreLocalStateAction;

    private UnityAction SaveCommitPendingListAction;
    private UnityAction RestoreCommitPendingListAction;

    private UnityAction SaveDeletionPendingListAction;
    private UnityAction RestoreDeletionPendingListAction;

    private GameObject NearLocalFollowingMenu;
    private GameObject NearGlobalFollowingMenu;
    private GameObject NearCommitPendingFollowingMenu;
    private GameObject NearDeletionPendingFollowingMenu;

    private GameObject LocalForcedCommitButton;
    private GameObject LocalRequestCommitButton;

    private void Awake()
    {
        // Use this just because it is easier to access rather than always doing gameObject.transform
        Transform = gameObject.transform;

        // Generate new guid
        Guid = Guid.NewGuid();

        // Add guid and attached gameobject in list
        GUIDKeeper.AddToList(Guid, gameObject);

        // Set UnityActions
        SetGlobalUnityActions();
        SetLocalUnityActions();
        SetCommitPendingUnityActions();
        SetDeletionPendingUnityActions();

        // Subscribe to event to hide the object at each scene change
        // no need to use an unityaction (I think) because i don't need to unsubscribe from this event! it's fixed
        CaretakerScene.Instance.HideObjectEvent.AddListener(() => HideObject());

        // Add manipulation event/s
        ObjectManipulator objManip = gameObject.GetComponent<ObjectManipulator>();
        objManip.OnManipulationStarted.AddListener(OnSelect);
        objManip.OnManipulationEnded.AddListener(OnUnselect);

        // I can't attach it from the inspector, because the controlled is created at runtime! So I need to reference it using UIManager
        // NOTE: set SlateColor BEFORE calling SetNearLocalFollowingMenu!
        // Alternatively, I can just call UIManager.Instance.SlateColor when I need it, and not reference it using a field (but whatever)
        // SlateColor = UIManager.Instance.SlateColor;

        /* Create the menus
           Note: The menus need to stay in Awake because they are referred even when the gameobject is not active,
           for example in SetActiveLocalMenu. The said function will throw an error if the creations of the menu 
           is done in the Start method, because Start is called only when the gameobject is active, but not before
       
           Long story short: keep these methods here
        */
        CreateNearLocalFollowingMenu();
        CreateNearGlobalFollowingMenu();
        CreateNearCommitPendingFollowingMenu();
        CreateNearDeletionPendingFollowingMenu();
    }

    public void SetPrefabName(string prefabName)
    {
        PrefabName = prefabName;
    }

    public void SetMaterialName(string materialName)
    {
        MaterialName = materialName;
    }

    public void SetCommitPendingObjectUserType(UserType commitPendingObjectUserType)
    {
        CommitPendingObjectUserType = commitPendingObjectUserType;
    }

    public void SetDeletionPendingObjectUserType(UserType deletionPendingObjectUserType)
    {
        DeletionPendingObjectUserType = deletionPendingObjectUserType;
    }

    public void SetGuid(Guid guid)
    {
        Guid oldGuid = Guid;
        Guid = guid;

        if (GUIDKeeper.ContainsGuid(oldGuid))
        {
            GUIDKeeper.RemoveFromList(oldGuid);
        }

        GUIDKeeper.AddToList(guid, gameObject);
    }

    public void CopyObjectInLocal()
    {
        // I have the obj, so it means it is already in the existing obj GUIDKeeper List
        PrefabManager.Instance.PutExistingObjectInLocal(Guid, Transform, MaterialName);
    }

    /// <summary>
    /// Removes the object from the scene indicated by "fromScene" based on the type of user "userType" (Sender/Receiver)
    /// </summary>
    /// <param name="fromScene">The scene in which we want to delete the object</param>
    /// <param name="userType">The type of user: sender of the deletion or receiver of the deletion. Default value: Sender</param>
    public void DeleteObject(ObjectLocation fromScene, UserType userType = UserType.Sender)
    {
        bool hide = true;

        switch (fromScene)
        {
            case ObjectLocation.Local: // Delete it from the Local scene
            {
                // Always run it 
                UnsubscribeAndRemoveFromLocalScene();

                // If there are no flags, it means that the previous call deleted the only flag, but the method only removes
                // the local Flag! So, the object has no flags (thus it only had the local one)
                // the object is/was only in the local scene: completely delete it!
                if (ContainsOnlyFlag(ObjectLocation.None))
                {
                    GUIDKeeper.RemoveFromList(Guid);
                    Destroy(gameObject); //also destroy its children, e.g.: menus/buttons                    
                }

                break;
            }

            case ObjectLocation.Global: // Delete it from the Global scene
            {
                // Always run these 
                UnsubscribeAndRemoveFromGlobalScene();

                // If there are no flags, it means that the previous call deleted the only flag, but the method only removes
                // the global Flag! So, the object has no flags (thus it only had the global one)
                // the object is/was only in the global scene: completely delete it!
                if (ContainsOnlyFlag(ObjectLocation.None))
                {
                    if (userType.Equals(UserType.Receiver))
                    {
                        CopyObjectInLocal();
                        SetActiveCommitButtons(true);
                        hide = false;

                        break;
                    }
                    else if (userType.Equals(UserType.Sender))
                    {
                        GUIDKeeper.RemoveFromList(Guid);
                        Destroy(gameObject); //also destroy its children, e.g.: menus/buttons                            
                    }
                }

                if (userType.Equals(UserType.Sender))
                    MessagesManager.Instance.SendGlobalForcedDeleteMessage(this); // Send deletion message

                break;
            }

            case ObjectLocation.None:
                break;

            default: break;
        }

        if (hide)
            HideObject();

        EnableMeshOutlineDeletionPending(false);
    }

    // Duplicate obj with a slight movement of 0.1f on axis X and Y
    public void DuplicateObj()
    {
        PrefabManager.Instance.CreateNewObjectInLocal(PrefabName, MaterialName);
    }

    // ---------------------- BEGIN COMMIT METHODS ----------------------

    public void PrepareForcedCommit()
    {
        MessagesManager.Instance.SendForcedCommit(this);
    }

    public void PrepareRequestCommit()
    {
        SetCommitPendingObjectUserType(UserType.Sender);
        MessagesManager.Instance.SendRequestCommit(this);
    }  

    public void OnForcedCommitReceived()
    {
        bool wasLocalScene = false;

        // Want to modify object in the global scene, so check and switch to it.
        // Then re-switch to the local one if that's the case
        if (CaretakerScene.Instance.IsLocalScene())
        {
            CaretakerScene.Instance.ChangeSceneToGlobal();
            wasLocalScene = true;
        }

        SetActiveManipulation(false);
        EnableMeshOutlineCommitPending(false);

        if (wasLocalScene)
            CaretakerScene.Instance.ChangeSceneToLocal();
    }

    public void OnRequestCommitReceived()
    {
        bool wasLocalScene = false;

        // Want to modify object in the global scene, so check and switch to it.
        // Then re-switch to the local one if that's the case
        if (CaretakerScene.Instance.IsLocalScene())
        {
            CaretakerScene.Instance.ChangeSceneToGlobal();
            wasLocalScene = true;
        }

        SetCommitPendingObjectUserType(UserType.Receiver);
        SetActiveGlobalMenu(false);
        SetActiveManipulation(false);

        if (wasLocalScene)
            CaretakerScene.Instance.ChangeSceneToLocal();
    }

    public void AcceptCommit()
    {
        MessagesManager.Instance.AcceptCommit(this);
        EnableMeshOutlineCommitPending(false);
    }

    // todo: maybe move this function in PrefabManager? nah?
    public void DeclineCommit(UserType userType = UserType.Sender)
    {
        RemoveCommitPending();

        bool wasLocalScene = false;

        // Want to edit object in the global scene, so check and switch to it.
        // Then re-switch to the local one if that's the case
        if (CaretakerScene.Instance.IsLocalScene())
        {
            CaretakerScene.Instance.ChangeSceneToGlobal();
            wasLocalScene = true;
        }

        // If the obj exists in the global layer, revert to that state
        if (ObjectLocation.HasFlag(ObjectLocation.Global))
        {
            CaretakerScene.Instance.RestoreGlobalState(this);

            //operazione molto pesante, ma dovrebbe funzionare lo stesso: CaretakerScene.Instance.ChangeSceneToGlobal();
        }
        else // if it is not in global
        {
            if (ObjectLocation.HasFlag(ObjectLocation.Local)) // Then check if it is local
            {
                HideObject(); // if that's the case, just hide it from the global layer
            }
            else // if it's not in local layer, then delete it
            {
                GUIDKeeper.RemoveFromList(Guid);
                Destroy(gameObject); //also destroy its children, e.g.: menus/buttons                    
            }
        }

        // If the previous scene was the global one, reswitch to the global
        if (wasLocalScene)
            CaretakerScene.Instance.ChangeSceneToLocal();

        if (userType.Equals(UserType.Sender))
            MessagesManager.Instance.SendDeclineCommit(this); // Send decline commit message
    }

    public void RemoveCommitPending()
    {
        UnsubscribeAndRemoveFromCommitPendingList();
        CommitPendingObjectUserType = UserType.None;

        SetActiveCommitPendingMenu(false);
    }

    // ---------------------- END COMMIT METHODS ----------------------

    // ---------------------- BEGIN PENDING DELETION METHODS ----------------------

    public void PrepareGlobalForcedDeletion()
    {
        DeleteObject(ObjectLocation.Global);
    }

    public void PrepareGlobalRequestDeletion()
    {
        //SetCommitPendingObjectUserType(UserType.Sender); // wrong line??
        SetDeletionPendingObjectUserType(UserType.Sender);
        MessagesManager.Instance.SendGlobalRequestDeletionMessage(this);

        SetActiveCommitButtons(false);
        SetActiveGlobalMenu(false);
    }

    public void OnRequestDeletionReceived()
    {
        bool wasLocalScene = false;

        // Want to modify object in the global scene, so check and switch to it.
        // Then re-switch to the local one if that's the case
        if (CaretakerScene.Instance.IsLocalScene())
        {
            CaretakerScene.Instance.ChangeSceneToGlobal();
            wasLocalScene = true;
        }

        SetDeletionPendingObjectUserType(UserType.Receiver);
        SetActiveGlobalMenu(false);
        SetActiveManipulation(false);

        SetActiveCommitButtons(false);

        if (wasLocalScene)
            CaretakerScene.Instance.ChangeSceneToLocal();
    }

    public void AcceptDeletion()
    {
        MessagesManager.Instance.AcceptDeletion(this);
        SetActiveCommitButtons(true);
        EnableMeshOutlineDeletionPending(false);
    }

    // todo: maybe move this function in PrefabManager? nah?
    public void DeclineDeletion(UserType userType = UserType.Sender)
    {
        RemoveDeletionPending();
        SetActiveCommitButtons(true);

        bool wasLocalScene = false;

        // Want to edit object in the global scene, so check and switch to it.
        // Then re-switch to the local one if that's the case
        if (CaretakerScene.Instance.IsLocalScene())
        {
            CaretakerScene.Instance.ChangeSceneToGlobal();
            wasLocalScene = true;
        }

        CaretakerScene.Instance.RestoreGlobalState(this);

        // If the previous scene was the global one, reswitch to the global
        if (wasLocalScene)
            CaretakerScene.Instance.ChangeSceneToLocal();

        if (userType.Equals(UserType.Sender))
            MessagesManager.Instance.SendDeclineDeletion(this); // Send decline deletion message
    }

    public void RemoveDeletionPending()
    {
        UnsubscribeAndRemoveFromDeletionPendingList();
        DeletionPendingObjectUserType = UserType.None;

        SetActiveDeletionPendingMenu(false);
    }

    // ---------------------- END PENDING DELETION METHODS ----------------------

    // ---------------------- BEGIN METHODS FOR 'MEMENTO' PATTERN ----------------------

    public Memento Save()
    {
        return new Memento(Guid, PrefabName, MaterialName, Transform, ObjectLocation);
    }

    public void Restore(Memento memento)
    {
        // A memento should not touch or restore the object guid, its prefab name and location
        // It just needs to change the propreties, like position/rot/scale, or colors etc...

        // Assign transform
        Transform.AssignDeserTransformToOriginalTransform(memento.GetTransform());

        // Assign material
        MaterialName = memento.GetMaterialName();
        PrefabManager.Instance.ChangeMaterial(gameObject, MaterialName);

        // Disable possible outlines
        EnableMeshOutlineCommitPending(false);
        EnableMeshOutlineDeletionPending(false);
    }

    // ---------------------- END MEMENTO METHODS ----------------------

    // ---------------------- BEGIN UN/SUB METHODS ----------------------

    private void SetGlobalUnityActions()
    {
        // Just set them, not attach to a listener yet
        SaveGlobalStateAction += () => CaretakerScene.Instance.SaveGlobalState(this);

        RestoreGlobalStateAction += () => CaretakerScene.Instance.RestoreGlobalState(this);
        RestoreGlobalStateAction += () => SetActiveManipulation(false);

        RestoreGlobalStateAction += () => SetActiveLocalMenu(false); // Untoggle local menu
        RestoreGlobalStateAction += () => SetActiveCommitPendingMenu(false); // untoggle commit pending menu
        RestoreGlobalStateAction += () => SetActiveDeletionPendingMenu(false); // untoggle deletion pending menu
    }

    private void SetLocalUnityActions()
    {
        // Just set them, not attach to a listener yet
        SaveLocalStateAction += () => CaretakerScene.Instance.SaveLocalState(this);

        RestoreLocalStateAction += () => CaretakerScene.Instance.RestoreLocalState(this);
        RestoreLocalStateAction += () => SetActiveManipulation(true);

        RestoreLocalStateAction += () => SetActiveGlobalMenu(false); // untoggle global menu
        RestoreLocalStateAction += () => SetActiveCommitPendingMenu(false); // untoggle commit pending menu
        RestoreLocalStateAction += () => SetActiveDeletionPendingMenu(false); // untoggle deletion pending menu
    }

    private void SetCommitPendingUnityActions()
    {
        // Just set them, not attach to a listener yet
        SaveCommitPendingListAction += () => CaretakerScene.Instance.SaveCommitPendingState(this);

        RestoreCommitPendingListAction += () => CaretakerScene.Instance.RestoreCommitPendingState(this);
        RestoreCommitPendingListAction += () => SetActiveManipulation(false);

        RestoreCommitPendingListAction += () => SetActiveLocalMenu(false); // Untoggle local menu
        RestoreCommitPendingListAction += () => SetActiveGlobalMenu(false); // untoggle global menu
    }

    private void SetDeletionPendingUnityActions()
    {
        // Just set them, not attach to a listener yet
        SaveDeletionPendingListAction += () => CaretakerScene.Instance.SaveDeletionPendingState(this);

        RestoreDeletionPendingListAction += () => CaretakerScene.Instance.RestoreDeletionPendingState(this);
        RestoreDeletionPendingListAction += () => SetActiveManipulation(false);

        RestoreDeletionPendingListAction += () => SetActiveLocalMenu(false); // Untoggle local menu
        RestoreDeletionPendingListAction += () => SetActiveGlobalMenu(false); // untoggle global menu
    }

    // Adding multiple identical listeners results in only a single call being made.
    public void SubscribeToGlobalScene()
    {
        CaretakerScene.Instance.SaveGlobalStateEvent.AddListener(SaveGlobalStateAction);
        CaretakerScene.Instance.RestoreGlobalStateEvent.AddListener(RestoreGlobalStateAction);


        // Add global location to object location
        AddFlagToObjectLocation(ObjectLocation.Global);
    }

    // Adding multiple identical listeners results in only a single call being made.
    public void SubscribeToLocalScene()
    {
        CaretakerScene.Instance.SaveLocalStateEvent.AddListener(SaveLocalStateAction);
        CaretakerScene.Instance.RestoreLocalStateEvent.AddListener(RestoreLocalStateAction);

        // Add local location to object location
        AddFlagToObjectLocation(ObjectLocation.Local);
    }

    // Adding multiple identical listeners results in only a single call being made.
    public void SubscribeToCommitPendingList()
    {
        CaretakerScene.Instance.SaveCommitPendingListEvent.AddListener(SaveCommitPendingListAction);
        CaretakerScene.Instance.RestoreCommitPendingListEvent.AddListener(RestoreCommitPendingListAction);

        AddFlagToObjectLocation(ObjectLocation.CommitPending);
    }
    
    public void SubscribeToDeletionPendingList()
    {
        CaretakerScene.Instance.SaveDeletionPendingListEvent.AddListener(SaveDeletionPendingListAction);
        CaretakerScene.Instance.RestoreDeletionPendingListEvent.AddListener(RestoreDeletionPendingListAction);

        AddFlagToObjectLocation(ObjectLocation.DeletionPending);
    }

    /* PLEASE NOTE:
         It is necessary to call the specific "Resubscribe" method when a GObj is unsubscribing!
         This is because the Action passed in the RemoveListener call are reference types and also multicast delegates.
         That means that when a GObj unsubscribes from an event, ALL the GObjs attached to that events are unsubsctibed!
         (And that sucks a lot, but I discovered that only at the end... yeah)

         TLDR: Call the "Resubscribe" method to make all the previously subscribed objects subscribe again
         (I think the most well fitted class to do this is the CaretakerScene one)
         */

    public void UnsubscribeAndRemoveFromGlobalScene()
    {
        CaretakerScene.Instance.SaveGlobalStateEvent.RemoveListener(SaveGlobalStateAction);
        CaretakerScene.Instance.RestoreGlobalStateEvent.RemoveListener(RestoreGlobalStateAction);

        CaretakerScene.Instance.RemoveFromGlobalList(Guid);

        // Remove global location from object location
        RemoveFlagFromObjectLocation(ObjectLocation.Global);

        // Note: do this AFTER removing the flag location (previous call)
        CaretakerScene.Instance.ResubscribeRemainingObjsToGlobalEvents();
    }

    public void UnsubscribeAndRemoveFromLocalScene()
    {
        CaretakerScene.Instance.SaveLocalStateEvent.RemoveListener(SaveLocalStateAction);
        CaretakerScene.Instance.RestoreLocalStateEvent.RemoveListener(RestoreLocalStateAction);

        CaretakerScene.Instance.RemoveFromLocalList(Guid);

        // Remove local location from object location
        RemoveFlagFromObjectLocation(ObjectLocation.Local);

        // Note: do this AFTER removing the flag location (previous call)
        CaretakerScene.Instance.ResubscribeRemainingObjsToLocalEvents();
    }

    public void UnsubscribeAndRemoveFromCommitPendingList()
    {
        CaretakerScene.Instance.SaveCommitPendingListEvent.RemoveListener(SaveCommitPendingListAction);
        CaretakerScene.Instance.RestoreCommitPendingListEvent.RemoveListener(RestoreCommitPendingListAction);

        CaretakerScene.Instance.RemoveFromCommitPendingList(Guid);

        RemoveFlagFromObjectLocation(ObjectLocation.CommitPending);

        // Note: do this AFTER removing the flag location (previous call)
        CaretakerScene.Instance.ResubscribeRemainingObjsToCommitPendingEvents();
    }

    public void UnsubscribeAndRemoveFromDeletionPendingList()
    {
        CaretakerScene.Instance.SaveDeletionPendingListEvent.RemoveListener(SaveDeletionPendingListAction);
        CaretakerScene.Instance.RestoreDeletionPendingListEvent.RemoveListener(RestoreDeletionPendingListAction);

        CaretakerScene.Instance.RemoveFromDeletionPendingList(Guid);

        RemoveFlagFromObjectLocation(ObjectLocation.DeletionPending);

        // Note: do this AFTER removing the flag location (previous call)
        CaretakerScene.Instance.ResubscribeRemainingObjsToDeletionPendingEvents();
    }

    // Always hide the object on scene change!
    public void HideObject(bool hide = true)
    {
        gameObject.SetActive(!hide);
    }

    // ---------------------- END UN/SUB METHODS ----------------------

    // ---------------------- PRIVATE ----------------------

    private void OnSelect(ManipulationEventData data)
    {
        // Toggle global or local menu on object selection

        if (CaretakerScene.Instance.IsGlobalScene())
        {
            // If the object is ONLY in the global scene and NOT in the Commit pending list and NOT in the Deletion pending list
            if (ObjectLocation.HasFlag(ObjectLocation.Global) &&
                !ObjectLocation.HasFlag(ObjectLocation.CommitPending) &&
                !ObjectLocation.HasFlag(ObjectLocation.DeletionPending))
            {
                SetActiveGlobalMenu(true);
            }
            // else if the object is ALSO in the Commit pending list
            else if (ObjectLocation.HasFlag(ObjectLocation.CommitPending))
            {
                SetActiveGlobalMenu(false);

                // Show commit pending menu only if the Commit pending obj was RECEIVED
                if (CommitPendingObjectUserType.Equals(UserType.Receiver))
                    SetActiveCommitPendingMenu(true);
            }
            // else if the object is ALSO in the deletion pending list
            else if (ObjectLocation.HasFlag(ObjectLocation.DeletionPending))
            {
                SetActiveGlobalMenu(false);

                // Show deletion pending menu only if the deletion pending obj was RECEIVED
                if (DeletionPendingObjectUserType.Equals(UserType.Receiver))
                    SetActiveDeletionPendingMenu(true);
            }
        }
        else if (CaretakerScene.Instance.IsLocalScene())
        {
            SetActiveLocalMenu(true);
        }

        // Make all the previous active object menus inactive
        if (CaretakerScene.Instance.CurrentActiveGObj != null && CaretakerScene.Instance.CurrentActiveGObj != this)
        {
            CaretakerScene.Instance.CurrentActiveGObj.SetActiveLocalMenu(false);
            CaretakerScene.Instance.CurrentActiveGObj.SetActiveGlobalMenu(false);
            CaretakerScene.Instance.CurrentActiveGObj.SetActiveCommitPendingMenu(false);
            CaretakerScene.Instance.CurrentActiveGObj.SetActiveDeletionPendingMenu(false);
        }

        // Make this object the current active object
        if (CaretakerScene.Instance.CurrentActiveGObj != this)
            CaretakerScene.Instance.CurrentActiveGObj = this;

        // Make this object the current manipulated one
        if (CaretakerScene.Instance.CurrentManipolatedGObj != this)
            CaretakerScene.Instance.CurrentManipolatedGObj = this;
    }

    private void OnUnselect(ManipulationEventData data)
    {
        CaretakerScene.Instance.CurrentManipolatedGObj = null;
    }

    private void CreateNearLocalFollowingMenu()
    {
        NearLocalFollowingMenu = UIManager.Instance.SetNearLocalFollowingMenu(this, 
            out LocalForcedCommitButton, out LocalRequestCommitButton);
    }

    private void CreateNearGlobalFollowingMenu()
    {
        NearGlobalFollowingMenu = UIManager.Instance.SetNearGlobalFollowingMenu(this);
    }

    private void CreateNearCommitPendingFollowingMenu()
    {
        NearCommitPendingFollowingMenu = UIManager.Instance.SetNearCommitPendingFollowingMenu(this);
    }

    private void CreateNearDeletionPendingFollowingMenu()
    {
        NearDeletionPendingFollowingMenu = UIManager.Instance.SetNearDeletionPendingFollowingMenu(this);
    }

    private void SetActiveManipulation(bool active)
    {
        // Want to just spawn the object menu on manipulation, so lock rotations and movements

        // If manip is active (true), then do not lock movements (make 'enable' false)
        gameObject.GetComponent<MoveAxisConstraint>().enabled = !active; // not - because i want to lock on false

        // If manip is not active (false), then also lock y axis (so lock every axis)
        if (!active)
            gameObject.GetComponent<RotationAxisConstraint>().ConstraintOnRotation =
                MSUtilities.AxisFlags.YAxis | MSUtilities.AxisFlags.XAxis | MSUtilities.AxisFlags.ZAxis;
        else // If manip is active (true), then do not lock y axis (only lock axis x and z) 
            gameObject.GetComponent<RotationAxisConstraint>().ConstraintOnRotation =
                MSUtilities.AxisFlags.XAxis | MSUtilities.AxisFlags.ZAxis;
    }

    private void SetActiveLocalMenu(bool active)
    {
        NearLocalFollowingMenu.SetActive(active);
        NearLocalFollowingMenu.GetComponent<RadialView>().enabled = active;
    }

    private void SetActiveGlobalMenu(bool active)
    {
        NearGlobalFollowingMenu.SetActive(active);
        NearGlobalFollowingMenu.GetComponent<RadialView>().enabled = active;
    }

    private void SetActiveCommitPendingMenu(bool active)
    {
        NearCommitPendingFollowingMenu.SetActive(active);
        NearCommitPendingFollowingMenu.GetComponent<RadialView>().enabled = active;
    }

    private void SetActiveDeletionPendingMenu(bool active)
    {
        NearDeletionPendingFollowingMenu.SetActive(active);
        NearDeletionPendingFollowingMenu.GetComponent<RadialView>().enabled = active;
    }

    private void SetActiveCommitButtons(bool active)
    {
        LocalForcedCommitButton.SetActive(active);
        LocalRequestCommitButton.SetActive(active);
    }

    public void EnableMeshOutlineCommitPending(bool enable)
    {
        meshOutlineCommitPending.enabled = enable;
    }

    public void EnableMeshOutlineDeletionPending(bool enable)
    {
        meshOutlineDeletionPending.enabled = enable;
    }

    // ------------------ FLAGS ------------------

    private void AddFlagToObjectLocation(ObjectLocation flagToAdd)
    {
        ObjectLocation |= flagToAdd;
    }

    private bool RemoveFlagFromObjectLocation(ObjectLocation flagToRemove)
    {
        bool hasFlag = ObjectLocation.HasFlag(flagToRemove);
        ObjectLocation &= ~flagToRemove;

        return hasFlag;
    }

    public bool ContainsOnlyFlag(ObjectLocation flag)
    {
        return ObjectLocation == flag;
    }

    public override string ToString()
    {
        if (PrefabName != null && MaterialName != null)
        {        
            // Set material name by trimming everything before the dash
            return MaterialName.Substring(MaterialName.IndexOf('-') + 1)  // +1 to exclude the dash
                   + " "
                   + PrefabName;
        }
        else
            return name.Remove(name.LastIndexOf("("));
    }
}