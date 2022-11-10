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
    Global = 2
}

public class GameObjController : MonoBehaviour
{
    public Guid Guid { get; private set; }
    public string PrefabName { get; private set; }
    public string MaterialName { get; private set; }
    public SerializableTransform Transform;
    public ObjectLocation ObjectLocation = ObjectLocation.None;

    // Need to keep the references to the unity actions in order to disable them
    private UnityAction saveGlobalStateAction;
    private UnityAction restoreGlobalStateAction;

    private UnityAction saveLocalStateAction;
    private UnityAction restoreLocalStateAction;

    private GameObject nearLocalFollowingMenu;
    private GameObject nearGlobalFollowingMenu;

    private GameObject SlateColor;

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

        // Set global and local UnityActions
        saveGlobalStateAction = () => CaretakerScene.Instance.SaveGlobalState(this);
        restoreGlobalStateAction = () => CaretakerScene.Instance.RestoreGlobalState(this);

        saveLocalStateAction = () => CaretakerScene.Instance.SaveLocalState(this);
        restoreLocalStateAction = () => CaretakerScene.Instance.RestoreLocalState(this);

        // Subscribe to event to hide the object at each scene change
        // no need to use an unityaction (I think) because i don't need to unsubscribe from this event! it's fixed
        CaretakerScene.Instance.HideObjectEvent.AddListener(() => HideObject());

        // Add manipulation event/s
        ObjectManipulator objManip = gameObject.GetComponent<ObjectManipulator>();
        objManip.OnManipulationStarted.AddListener(OnSelect);

        // I can't attach it from the inspector, because the controlled is created at runtime! So I need to reference it using UIManager
        // NOTE: set SlateColor BEFORE calling SetNearLocalFollowingMenu!
        // Alternatively, I can just call UIManager.Instance.SlateColor when I need it, and not reference it using a field (but whatever)
        SlateColor = UIManager.Instance.SlateColor;

        /* Create the menus
           Note: The menus need to stay in Awake because they are referred even when the gameobject is not active,
           for example in SetActiveLocalMenu. The said function will throw an error if the creations of the menu is done in the Start method,
           because Start is called only when the gameobject is active, but not before
       
           Long story short: keep these 2 methods here
        */
        SetNearLocalFollowingMenu();
        SetNearGlobalFollowingMenu();
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

    public string GetGuidString() {
        return Guid.ToString();
    }

    public void SetPrefabName(string prefabName) {
        PrefabName = prefabName;
    }

    public void SetMaterialName(string materialName) {
        MaterialName = materialName;
    }

    public void SetGuid(Guid guid) {
        Guid oldGuid = Guid;
        Guid = guid;

        if (GUIDKeeper.ContainsGuid(guid)) {
            GUIDKeeper.RemoveFromList(oldGuid);
        }

        GUIDKeeper.AddToList(Guid, gameObject);
    }

    // ---------------------- BEGIN METHODS FOR 'MEMENTO' PATTERN ----------------------

    public Memento Save() {
        return new Memento(Guid, PrefabName, MaterialName, Transform);
    }

    public void Restore(Memento memento) 
    {
        // These 2 are commented because a memento should not touch or restore the object guid and its prefab name
        // It just needs to change the propreties, like position/rot/scale, or colors etc...

        /*
        Guid = memento.GetGuid();
        PrefabName = memento.GetPrefabName();
         */

        // Assign transform
        Transform = memento.GetTransform();
        gameObject.transform.AssignDeserTransformToOriginalTransform(transform);

        // Assign material
        MaterialName = memento.GetMaterialName();
        PrefabManager.Instance.ChangeMaterial(gameObject, MaterialName);
    }

    // ---------------------- END MEMENTO METHODS ----------------------

    // ---------------------- BEGIN UN/SUB METHODS ----------------------

    // Adding multiple identical listeners results in only a single call being made.
    public void SubscribeToGlobalScene()
    {
        CaretakerScene.Instance.SaveGlobalStateEvent.AddListener(saveGlobalStateAction);
       
        CaretakerScene.Instance.RestoreGlobalStateEvent.AddListener(restoreGlobalStateAction);
        CaretakerScene.Instance.RestoreGlobalStateEvent.AddListener(() => SetActiveManipulation(false));

        // Untoggle local menu
        CaretakerScene.Instance.RestoreGlobalStateEvent.AddListener(() => SetActiveLocalMenu(false));

        // Add global location to object location
        AddFlagLocation(ObjectLocation.Global);
    }

    // Adding multiple identical listeners results in only a single call being made.
    public void SubscribeToLocalScene()
    {
        CaretakerScene.Instance.SaveLocalStateEvent.AddListener(saveLocalStateAction);
        
        CaretakerScene.Instance.RestoreLocalStateEvent.AddListener(restoreLocalStateAction);
        CaretakerScene.Instance.RestoreLocalStateEvent.AddListener(() => SetActiveManipulation(true));

        // untoggle global menu
        CaretakerScene.Instance.RestoreLocalStateEvent.AddListener(() => SetActiveGlobalMenu(false));

        // Add local location to object location
        AddFlagLocation(ObjectLocation.Local);
    }
    public void UnsubscribeFromGlobalScene()
    {
        CaretakerScene.Instance.SaveGlobalStateEvent.RemoveListener(saveGlobalStateAction);
        CaretakerScene.Instance.RestoreGlobalStateEvent.RemoveListener(restoreGlobalStateAction);

        // Remove local location from object global
        RemoveFlagLocation(ObjectLocation.Global);
    }

    public void UnsubscribeFromLocalScene()
    {
        CaretakerScene.Instance.SaveLocalStateEvent.RemoveListener(saveLocalStateAction);
        CaretakerScene.Instance.RestoreLocalStateEvent.RemoveListener(restoreLocalStateAction);

        // Remove local location from object location
        RemoveFlagLocation(ObjectLocation.Local);
    }

    // Always hide the object on change scene!
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
            nearGlobalFollowingMenu.SetActive(true);
            nearGlobalFollowingMenu.GetComponent<RadialView>().enabled = true;
        }
        else if (CaretakerScene.Instance.IsLocalScene())
        {
            nearLocalFollowingMenu.SetActive(true);
            nearLocalFollowingMenu.GetComponent<RadialView>().enabled = true;
        }
    }

    private void SetNearLocalFollowingMenu()
    {
        nearLocalFollowingMenu = Instantiate(Resources.Load<GameObject>("NearMenu3x2"), Vector3.zero, Quaternion.identity);

        GameObject buttonCollection = nearLocalFollowingMenu.transform.Find("ButtonCollection").gameObject;

        // Button 1 - Forced Commit
        GameObject buttonOne = buttonCollection.transform.Find("ButtonOne").gameObject;
        Interactable interactableOne = buttonOne.GetComponent<Interactable>();
        interactableOne.OnClick.AddListener(() => MessagesManager.Instance.SendForcedCommit(this));

        // Button 2 - Voting Commit
        GameObject buttonTwo = buttonCollection.transform.Find("ButtonTwo").gameObject;

        // Button 3 - Close Menu
        GameObject buttonThree = buttonCollection.transform.Find("ButtonThree").gameObject;
        Interactable interactableThree = buttonThree.GetComponent<Interactable>();
        interactableThree.OnClick.AddListener(() => CloseMenu());

        // Button 4 - Remove from local
        GameObject buttonFour = buttonCollection.transform.Find("ButtonFour").gameObject;
        Interactable interactableFour = buttonFour.GetComponent<Interactable>();
        interactableFour.OnClick.AddListener(() => DeleteObject(this, ObjectLocation.Local, UserType.Sender));

        // Button 5 - Duplicate
        GameObject buttonFive = buttonCollection.transform.Find("ButtonFive").gameObject;
        Interactable interactableFive = buttonFive.GetComponent<Interactable>();
        interactableFive.OnClick.AddListener(() => DuplicateObj());

        // Button 6 - Change color
        GameObject buttonSix = buttonCollection.transform.Find("ButtonSix").gameObject;
        Interactable interactableSix = buttonSix.GetComponent<Interactable>();
        // Important: set slate active before populating - so onEnable & Awake (1st time) are called
        interactableSix.OnClick.AddListener(() => SlateColor.SetActive(true));
        interactableSix.OnClick.AddListener(() => SlateColor.GetComponent<SlateColorsManager>().PopulateSlate(PrefabName, Guid));

        //----------------------

        SolverHandler sh = nearLocalFollowingMenu.GetComponent<SolverHandler>();
        sh.TrackedTargetType = MSUtilities.TrackedObjectType.CustomOverride;
        sh.TransformOverride = gameObject.transform;

        // Hide it
        nearLocalFollowingMenu.SetActive(false);

        // The parent of the menu is the gameobject
        nearLocalFollowingMenu.transform.SetParent(gameObject.transform);

        // todo Maybe:  set scale to the same for every menu (so it doesn't become too small or too big)
    }

    private void SetNearGlobalFollowingMenu()
    {
        nearGlobalFollowingMenu = Instantiate(Resources.Load<GameObject>("NearMenu3x2 - Global obj"), Vector3.zero, Quaternion.identity);

        GameObject buttonCollection = nearGlobalFollowingMenu.transform.Find("ButtonCollection").gameObject;

        // Button 1 - Copy object in local scene
        GameObject buttonOne = buttonCollection.transform.Find("ButtonOne").gameObject;
        Interactable interactableOne = buttonOne.GetComponent<Interactable>();
        interactableOne.OnClick.AddListener(() => CopyObjectInLocalAndChangeToLocal(this));

        // Button 2 -
        GameObject buttonTwo = buttonCollection.transform.Find("ButtonTwo").gameObject;

        // Button 3 - 
        GameObject buttonThree = buttonCollection.transform.Find("ButtonThree").gameObject;

        //----------------------

        SolverHandler sh = nearGlobalFollowingMenu.GetComponent<SolverHandler>();
        sh.TrackedTargetType = MSUtilities.TrackedObjectType.CustomOverride;
        sh.TransformOverride = gameObject.transform;

        // Hide it
        nearGlobalFollowingMenu.SetActive(false);

        // The parent of the menu is the gameobject
        nearGlobalFollowingMenu.transform.SetParent(gameObject.transform);

        //todo: set scale to the same for every menu (so it doesn't become too small or too big)
    }

    // todo forse questo metodo va in Caretaker?

    // todo questa funzione è da vedere meglio perchè fa un po' le bizze
    // (soprattutto sulla posizione, sul colore sembra andare bene?)
    private void CopyObjectInLocalAndChangeToLocal(GameObjController gobj)
    {
        // Call the update object and not the create object, because if I have the obj it means it is already in the 'existing' obj
        // in the guid list. So just update to make it local it and make it subscribe to the local scene
        PrefabManager.Instance.UpdateObjectLocal(gobj.Guid, gobj.Transform, gobj.MaterialName);

        //CaretakerScene.Instance.ChangeSceneToLocal();
    }

    // fromScene: tells in which scene we want to delete the object
    // userType: tells if the user is the sender of the deletion message or the receiver
    public void DeleteObject(GameObjController gObj, ObjectLocation fromScene, UserType userType)
    {
        switch (fromScene)
        {
            case ObjectLocation.Local: // Delete it from the Local scene
            {
                // If the object is only in the local scene - Completely delete it!
                if (ContainsOnlyFlag(ObjectLocation.Local))
                {
                    UnsubscribeFromLocalScene();
                    CaretakerScene.Instance.RemoveFromLocalList(gObj.Guid);
                    GUIDKeeper.RemoveFromList(gObj.Guid);
                    Destroy(gObj.gameObject); //also destroy its children, e.g.: menus/buttons
                }
                // If the object is both in the local and global scene
                else if (ObjectLocation.HasFlag(ObjectLocation.Local | ObjectLocation.Global))
                {
                    // lo tolgo dal locale e basta, non cancello niente dal guid keeper, nè dal globale, no?
                    // ma allora posso unire i casi sopra!!
                    UnsubscribeFromLocalScene();
                    CaretakerScene.Instance.RemoveFromLocalList(gObj.Guid);
                }

                break;
            }

            case ObjectLocation.Global: // Delete it from the Global scene
            {
                // If the object is only in the global scene - Make it easier for the user: copy it in the local scene
                if (ContainsOnlyFlag(ObjectLocation.Global))
                {
                    UnsubscribeFromGlobalScene();
                    CaretakerScene.Instance.RemoveFromGlobalList(gObj.Guid);

                    // todo: copy it in the local scene to make it easier for the user
                    if (userType.Equals(UserType.Sender))
                        CopyObjectInLocalAndChangeToLocal(this); // todo to review!

                    if (userType.Equals(UserType.Receiver))
                    {
                        GUIDKeeper.RemoveFromList(gObj.Guid);
                        Destroy(gObj.gameObject); //also destroy its children, e.g.: menus/buttons    
                    }

                }
                // If the object is both in the local and global scene
                else if (ObjectLocation.HasFlag(ObjectLocation.Local | ObjectLocation.Global))
                {
                    // ma allora posso unire i casi sopra!!
                    UnsubscribeFromGlobalScene();
                    CaretakerScene.Instance.RemoveFromGlobalList(gObj.Guid);
                }

                if (userType.Equals(UserType.Sender))
                    MessagesManager.Instance.SendGlobalDeletionMessage(this);

                break;
            }

            case ObjectLocation.None:
                break;

            default: break;
        }

        HideObject();
    }



    // Duplicate obj with a slight movement of 0.1f on axis X and Y
    private void DuplicateObj()
    {
        // I think I don't need to do all of this when the CreateNewObject method does kinda the same thing
        /*
        SerializableTransform st = Transform; 
        SerializableVector sv = new SerializableVector(
            st.Position.x + 0.1f,
            st.Position.y,
            st.Position.z);

        st.Position = sv;
        */

        PrefabManager.Instance.CreateNewObjectLocal(PrefabName, MaterialName);
    }

    private void CloseMenu()
    {
        nearLocalFollowingMenu.SetActive(false);
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
        nearLocalFollowingMenu.SetActive(active);
    }

    private void SetActiveGlobalMenu(bool active)
    {
        nearGlobalFollowingMenu.SetActive(active);
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
