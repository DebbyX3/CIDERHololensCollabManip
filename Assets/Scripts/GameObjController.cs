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
    Pending = 4
}

public class GameObjController : MonoBehaviour
{
    public Guid Guid { get; private set; }
    public string PrefabName { get; private set; }
    public string MaterialName { get; private set; }
    public SerializableTransform Transform;
    public ObjectLocation ObjectLocation { get; private set; } = ObjectLocation.None;

    // Indicates if the pending obj was received or sent
    public UserType PendingObjectUserType { get; private set; } = UserType.None;

    // Need to keep the references to the unity actions in order to disable them
    private UnityAction SaveGlobalStateAction;
    private UnityAction RestoreGlobalStateAction;

    private UnityAction SaveLocalStateAction;
    private UnityAction RestoreLocalStateAction;

    private UnityAction SavePendingListAction;
    private UnityAction RestorePendingListAction;

    private GameObject NearLocalFollowingMenu;
    private GameObject NearGlobalFollowingMenu;
    private GameObject NearPendingFollowingMenu;

    private void Awake() 
    {
        // Generate new guid
        Guid = Guid.NewGuid();

        // Add guid and attached gameobject in list
        GUIDKeeper.AddToList(Guid, gameObject);

        // Set initial positions/rotations/scale
        Transform.Position = gameObject.transform.position;
        Transform.Rotation = (SerializableVector)gameObject.transform.rotation;
        Transform.Scale = gameObject.transform.lossyScale;

        // Set UnityActions
        SetGlobalUnityActions();
        SetLocalUnityActions();
        SetPendingUnityActions();

        // Subscribe to event to hide the object at each scene change
        // no need to use an unityaction (I think) because i don't need to unsubscribe from this event! it's fixed
        CaretakerScene.Instance.HideObjectEvent.AddListener(() => HideObject());

        // Add manipulation event/s
        ObjectManipulator objManip = gameObject.GetComponent<ObjectManipulator>();
        objManip.OnManipulationStarted.AddListener(OnSelect);

        // I can't attach it from the inspector, because the controlled is created at runtime! So I need to reference it using UIManager
        // NOTE: set SlateColor BEFORE calling SetNearLocalFollowingMenu!
        // Alternatively, I can just call UIManager.Instance.SlateColor when I need it, and not reference it using a field (but whatever)
        //SlateColor = UIManager.Instance.SlateColor;

        /* Create the menus
           Note: The menus need to stay in Awake because they are referred even when the gameobject is not active,
           for example in SetActiveLocalMenu. The said function will throw an error if the creations of the menu 
           is done in the Start method, because Start is called only when the gameobject is active, but not before
       
           Long story short: keep these 2 methods here
        */
        CreateNearLocalFollowingMenu();
        CreateNearGlobalFollowingMenu();
        CreateNearPendingFollowingMenu();
    }

    private void Update() 
    {
        // Can be done better but not really urgent
        if (Transform.Position.X != gameObject.transform.position.x ||
            Transform.Position.Y != gameObject.transform.position.y ||
            Transform.Position.Z != gameObject.transform.position.z) 
        {
            Transform.Position = gameObject.transform.position;
        }

        if (Transform.Rotation.X != gameObject.transform.rotation.x ||
            Transform.Rotation.Y != gameObject.transform.rotation.y ||
            Transform.Rotation.Z != gameObject.transform.rotation.z ||
            Transform.Rotation.W != gameObject.transform.rotation.w) 
        {
            Transform.Rotation = (SerializableVector)gameObject.transform.rotation;
        }

        if (Transform.Scale.X != gameObject.transform.lossyScale.x ||
            Transform.Scale.Y != gameObject.transform.lossyScale.y ||
            Transform.Scale.Z != gameObject.transform.lossyScale.z) 
        {
            Transform.Scale = gameObject.transform.lossyScale;
        }
    }

    // TODO ha senso mettere questi metodi quando basta mettere il set pubblico dei campi in alto?? boh
    public void SetPrefabName(string prefabName) {
        PrefabName = prefabName;
    }

    public void SetMaterialName(string materialName) {
        MaterialName = materialName;
    }

    public void SetPendingObjectUserType(UserType pendingObjectUserType)
    {
        PendingObjectUserType = pendingObjectUserType;
    }

    public void SetGuid(Guid guid)
    {
        Guid oldGuid = Guid;
        Guid = guid;

        if (GUIDKeeper.ContainsGuid(oldGuid))
        { //era guid 
            GUIDKeeper.RemoveFromList(oldGuid);
        }

        GUIDKeeper.AddToList(guid, gameObject);
    }

    // todo forse questo metodo va in Caretaker?

    // todo questa funzione è da vedere meglio perchè fa un po' le bizze
    // (soprattutto sulla posizione, sul colore sembra andare bene?)
    public void CopyObjectInLocalAndChangeToLocal()
    {
        // Call the update object and not the create object, because if I have the obj it means it is already in the 'existing' obj
        // in the guid list. So just update to make it local it and make it subscribe to the local scene
        PrefabManager.Instance.PutExistingObjectInLocal(Guid, Transform, MaterialName);

        //CaretakerScene.Instance.ChangeSceneToLocal();
    }

    // fromScene: tells in which scene we want to delete the object
    // userType: tells if the user is the sender of the deletion message or the receiver
    public void DeleteObject(ObjectLocation fromScene, UserType userType)
    {
        switch (fromScene)
        {
            case ObjectLocation.Local: // Delete it from the Local scene
                {
                    // Always run it 
                    UnsubscribeFromLocalScene();

                    // If the object is only in the local scene - Completely delete it!
                    if (ContainsOnlyFlag(ObjectLocation.Local))
                    {
                        GUIDKeeper.RemoveFromList(Guid);
                        Destroy(gameObject); //also destroy its children, e.g.: menus/buttons
                    }

                    break;
                }

            case ObjectLocation.Global: // Delete it from the Global scene
                {
                    // Always run these 
                    UnsubscribeFromGlobalScene();

                    // If the object is only in the global scene - Make it easier for the other user: copy it in the local scene
                    if (ContainsOnlyFlag(ObjectLocation.Global))
                    {
                        if (userType.Equals(UserType.Receiver)) // prima era il sender che lo faceva, ora è il receiver
                        {
                            CopyObjectInLocalAndChangeToLocal(); // todo to review!
                        }
                        else if (userType.Equals(UserType.Sender))
                        {
                            GUIDKeeper.RemoveFromList(Guid);
                            Destroy(gameObject); //also destroy its children, e.g.: menus/buttons    
                        }
                    }

                    if (userType.Equals(UserType.Sender))
                        MessagesManager.Instance.SendGlobalDeletionMessage(this); // Send deletion message

                    break;
                }

            case ObjectLocation.None:
                break;

            default: break;
        }

        HideObject();
    }

    // Duplicate obj with a slight movement of 0.1f on axis X and Y
    public void DuplicateObj()
    {
        PrefabManager.Instance.CreateNewObjectInLocal(PrefabName, MaterialName);
    }

    public void DeclineCommit()
    { 

    }

    public void DeclineCommit()
    {
        UnsubscribeFromPendingList();
        PendingObjectUserType = UserType.None;

        HideObject();
    }

    // ---------------------- BEGIN METHODS FOR 'MEMENTO' PATTERN ----------------------

    public Memento Save() {
        return new Memento(Guid, PrefabName, MaterialName, Transform, ObjectLocation);
    }

    public void Restore(Memento memento) 
    {
        // These 3 are commented because a memento should not touch or restore the object guid, its prefab name and location
        // It just needs to change the propreties, like position/rot/scale, or colors etc...

        /*
        Guid = memento.GetGuid();
        PrefabName = memento.GetPrefabName();
        ObjectLocation = memento.GetObjectLocation();
         */

        // Assign transform
        Transform = memento.GetTransform();
        gameObject.transform.AssignDeserTransformToOriginalTransform(Transform);

        // Assign material
        MaterialName = memento.GetMaterialName();
        PrefabManager.Instance.ChangeMaterial(gameObject, MaterialName);
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
        RestoreGlobalStateAction += () => SetActivePendingMenu(false); // untoggle pending menu
    }

    private void SetLocalUnityActions()
    {
        // Just set them, not attach to a listener yet
        SaveLocalStateAction += () => CaretakerScene.Instance.SaveLocalState(this);

        RestoreLocalStateAction += () => CaretakerScene.Instance.RestoreLocalState(this);
        RestoreLocalStateAction += () => SetActiveManipulation(true);

        RestoreLocalStateAction += () => SetActiveGlobalMenu(false); // untoggle global menu
        RestoreLocalStateAction += () => SetActivePendingMenu(false); // untoggle pending menu
    }

    private void SetPendingUnityActions()
    {
        // Just set them, not attach to a listener yet
        SavePendingListAction += () => CaretakerScene.Instance.SavePendingState(this);

        RestorePendingListAction += () => CaretakerScene.Instance.RestorePendingState(this);
        RestorePendingListAction += () => SetActiveManipulation(false);

        RestorePendingListAction += () => SetActiveLocalMenu(false); // Untoggle local menu
        RestorePendingListAction += () => SetActiveGlobalMenu(false); // untoggle global menu
    }

    // Adding multiple identical listeners results in only a single call being made.
    public void SubscribeToGlobalScene()
    {
        CaretakerScene.Instance.SaveGlobalStateEvent.AddListener(SaveGlobalStateAction);
        CaretakerScene.Instance.RestoreGlobalStateEvent.AddListener(RestoreGlobalStateAction);

        // Add global location to object location
        AddFlagLocation(ObjectLocation.Global);
    }

    // Adding multiple identical listeners results in only a single call being made.
    public void SubscribeToLocalScene()
    {
        CaretakerScene.Instance.SaveLocalStateEvent.AddListener(SaveLocalStateAction);
        CaretakerScene.Instance.RestoreLocalStateEvent.AddListener(RestoreLocalStateAction);

        // Add local location to object location
        AddFlagLocation(ObjectLocation.Local);
    }

    // Adding multiple identical listeners results in only a single call being made.
    public void SubscribeToPendingList()
    {
        CaretakerScene.Instance.SavePendingListEvent.AddListener(SavePendingListAction);
        CaretakerScene.Instance.RestorePendingListEvent.AddListener(RestorePendingListAction);

        AddFlagLocation(ObjectLocation.Pending);
    }

    public void UnsubscribeFromGlobalScene()
    {
        CaretakerScene.Instance.SaveGlobalStateEvent.RemoveListener(SaveGlobalStateAction);
        CaretakerScene.Instance.RestoreGlobalStateEvent.RemoveListener(RestoreGlobalStateAction);

        CaretakerScene.Instance.RemoveFromGlobalList(Guid);

        // Remove global location from object location
        RemoveFlagLocation(ObjectLocation.Global);
    }

    public void UnsubscribeFromLocalScene()
    {
        CaretakerScene.Instance.SaveLocalStateEvent.RemoveListener(SaveLocalStateAction);
        CaretakerScene.Instance.RestoreLocalStateEvent.RemoveListener(RestoreLocalStateAction);

        CaretakerScene.Instance.RemoveFromLocalList(Guid);

        // Remove local location from object location
        RemoveFlagLocation(ObjectLocation.Local);
    }

    public void UnsubscribeFromPendingList()
    {
        CaretakerScene.Instance.SavePendingListEvent.RemoveListener(SavePendingListAction);
        CaretakerScene.Instance.RestorePendingListEvent.RemoveListener(RestorePendingListAction);

        CaretakerScene.Instance.RemoveFromPendingList(Guid);

        RemoveFlagLocation(ObjectLocation.Pending);
    }

    // Always hide the object on scene change!
    public void HideObject()
    {
        gameObject.SetActive(false);
    }

    // ---------------------- END UN/SUB METHODS ----------------------

    // ---------------------- PRIVATE ----------------------

    private void OnSelect(ManipulationEventData data)
    {
        // Toggle global or local menu on object selection

        if (CaretakerScene.Instance.IsGlobalScene())
        {
            // If the object is ONLY in the global scene and NOT in the pending list
            if (ObjectLocation.HasFlag(ObjectLocation.Global) && !ObjectLocation.HasFlag(ObjectLocation.Pending))
            {
                SetActiveGlobalMenu(true);
            }
            // If the object is ALSO in the pending list
            else if (ObjectLocation.HasFlag(ObjectLocation.Pending))
            {
                // Show pending menu only if the pending obj was RECEIVED
                if (PendingObjectUserType.Equals(UserType.Receiver))
                    SetActivePendingMenu(true);
            }
        }
        else if (CaretakerScene.Instance.IsLocalScene())
        {
            SetActiveLocalMenu(true);
        }
    }

    private void CreateNearLocalFollowingMenu()
    {
        NearLocalFollowingMenu = UIManager.Instance.SetNearLocalFollowingMenu(this);
    }

    private void CreateNearGlobalFollowingMenu()
    {
        NearGlobalFollowingMenu = UIManager.Instance.SetNearGlobalFollowingMenu(this);
    }

    private void CreateNearPendingFollowingMenu()
    {
        NearPendingFollowingMenu = UIManager.Instance.SetNearPendingFollowingMenu(this);
    }

    // todo check this function because sometimes i can move objects in the global scene!
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

    private void SetActivePendingMenu(bool active)
    {
        NearPendingFollowingMenu.SetActive(active);
        NearPendingFollowingMenu.GetComponent<RadialView>().enabled = active;
    }

    // ------------------ FLAGS ------------------

    private void AddFlagLocation(ObjectLocation flagToAdd)
    {
        ObjectLocation |= flagToAdd;
    }

    private void RemoveFlagLocation(ObjectLocation flagToRemove)
    {
        ObjectLocation &= ~flagToRemove;
    }

    public bool ContainsOnlyFlag(ObjectLocation flag)
    {
        return ObjectLocation == flag;
    }
}
