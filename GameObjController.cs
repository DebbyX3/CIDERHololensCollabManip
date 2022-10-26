using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
using UnityEngine;
using UnityEngine.Events;
using MSUtilities = Microsoft.MixedReality.Toolkit.Utilities;

public class GameObjController : MonoBehaviour {
    public Guid Guid { get; private set; }
    public string PrefabName { get; private set; }
    public SerializableTransform Transform;

    // Need to keep the references to the unity actions in order to disable them
    private UnityAction saveGlobalStateAction;
    private UnityAction restoreGlobalStateAction;

    private UnityAction saveLocalStateAction;
    private UnityAction restoreLocalStateAction;

    private GameObject nearLocalFollowingMenu;
    private GameObject nearGlobalFollowingMenu;

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
        //CaretakerScene.Instance.hideObject.AddListener(() => CaretakerScene.Instance.HideObject(this)); prima
        CaretakerScene.Instance.HideObjectEvent.AddListener(() => HideObject());

        // Add manipulation event/s
        ObjectManipulator objManip = gameObject.GetComponent<ObjectManipulator>();
        objManip.OnManipulationStarted.AddListener(OnSelect);
    }

    private void Start()
    {
        SetNearLocalFollowingMenu();
        SetNearGlobalFollowingMenu();
    }

    void Update() {
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
        this.PrefabName = prefabName;
    }

    public void SetGuid(Guid guid) {
        Guid oldGuid = this.Guid;
        this.Guid = guid;

        if (GUIDKeeper.ContainsGuid(guid)) {
            GUIDKeeper.RemoveFromList(oldGuid);
        }

        GUIDKeeper.AddToList(Guid, gameObject);
    }

    public void SendGObj() {

    }

    /* METHODS FOR 'MEMENTO' PATTERN */

    public Memento Save() {
        return new Memento(Guid, PrefabName, Transform);
    }

    public void Restore(Memento memento) 
    {
        // These 2 are commented because a memento should not touch or restore the object guid and its prefab name
        // It just needs to change the propreties, like position/rot/scale, or colors etc...

        /*
        Guid = memento.GetGuid();
        PrefabName = memento.GetPrefabName();*/

        Transform = memento.GetTransform();
        TransformSerializer.AssignDeserTransformToOriginalTransform(gameObject.transform, Transform);
    }

    // Adding multiple identical listeners results in only a single call being made.
    public void SubscribeToGlobalScene()
    {
        CaretakerScene.Instance.SaveGlobalStateEvent.AddListener(saveGlobalStateAction);
       
        CaretakerScene.Instance.RestoreGlobalStateEvent.AddListener(restoreGlobalStateAction);
        CaretakerScene.Instance.RestoreGlobalStateEvent.AddListener(() => SetActiveManipulation(false));

        // untoggle local menu
        CaretakerScene.Instance.RestoreGlobalStateEvent.AddListener(() => SetActiveLocalMenu(false));
    }

    // Adding multiple identical listeners results in only a single call being made.
    public void SubscribeToLocalScene()
    {
        CaretakerScene.Instance.SaveLocalStateEvent.AddListener(saveLocalStateAction);
        
        CaretakerScene.Instance.RestoreLocalStateEvent.AddListener(restoreLocalStateAction);
        CaretakerScene.Instance.RestoreLocalStateEvent.AddListener(() => SetActiveManipulation(true));

        // untoggle global menu
        CaretakerScene.Instance.RestoreLocalStateEvent.AddListener(() => SetActiveGlobalMenu(false));
    }
    public void UnsubscribeFromGlobalScene()
    {
        CaretakerScene.Instance.SaveGlobalStateEvent.RemoveListener(saveGlobalStateAction);
        CaretakerScene.Instance.RestoreGlobalStateEvent.RemoveListener(restoreGlobalStateAction);
    }

    public void UnsubscribeFromLocalScene()
    {
        CaretakerScene.Instance.SaveLocalStateEvent.RemoveListener(saveLocalStateAction);
        CaretakerScene.Instance.RestoreLocalStateEvent.RemoveListener(restoreLocalStateAction);
    }

    // Always hide the object on change scene!
    public void HideObject()
    {
        this.gameObject.SetActive(false);
    }

    private void SetNearLocalFollowingMenu()
    {
        nearLocalFollowingMenu = Instantiate(Resources.Load<GameObject>("NearMenu3x2"), Vector3.zero, Quaternion.identity);

        GameObject buttonCollection = nearLocalFollowingMenu.transform.Find("ButtonCollection").gameObject;

        // Button 1 - Forced Commit
        GameObject buttonOne = buttonCollection.transform.Find("ButtonOne").gameObject;
        Interactable interactableOne = buttonOne.GetComponent<Interactable>();
        interactableOne.OnClick.AddListener(() => CommitManager.Instance.SendForcedCommit(this));

        // Button 2 - Voting Commit
        GameObject buttonTwo = buttonCollection.transform.Find("ButtonTwo").gameObject;

        // Button 3 - Close Menu
        GameObject buttonThree = buttonCollection.transform.Find("ButtonThree").gameObject;
        Interactable interactableThree = buttonThree.GetComponent<Interactable>();
        interactableThree.OnClick.AddListener(() => CloseMenu());

        // Button 4 - Remove
        GameObject buttonFour = buttonCollection.transform.Find("ButtonFour").gameObject;
        Interactable interactableFour = buttonFour.GetComponent<Interactable>();
        interactableFour.OnClick.AddListener(() => RemoveGObj());

        // Button 5 - Duplicate
        GameObject buttonFive = buttonCollection.transform.Find("ButtonFive").gameObject;
        Interactable interactableFive = buttonFive.GetComponent<Interactable>();
        interactableFive.OnClick.AddListener(() => DuplicateObj());

        // Button 6
        GameObject buttonSix = buttonCollection.transform.Find("ButtonSix").gameObject;

        //----------------------

        SolverHandler sh = nearLocalFollowingMenu.GetComponent<SolverHandler>();
        sh.TrackedTargetType = MSUtilities.TrackedObjectType.CustomOverride;
        sh.TransformOverride = gameObject.transform;

        // Hide it
        nearLocalFollowingMenu.SetActive(false);

        // The parent of the menu is the gameobject
        nearLocalFollowingMenu.transform.SetParent(gameObject.transform);
        //nearLocalFollowingMenu.transform.parent = gameObject.transform;

        //maybe to do: set scale to the same for every menu (so it doesn't become too small or too big)
    }

    private void SetNearGlobalFollowingMenu()
    {
        nearGlobalFollowingMenu = Instantiate(Resources.Load<GameObject>("NearMenu3x2 - Global obj"), Vector3.zero, Quaternion.identity);

        GameObject buttonCollection = nearGlobalFollowingMenu.transform.Find("ButtonCollection").gameObject;

        // Button 1 - Forced Commit
        GameObject buttonOne = buttonCollection.transform.Find("ButtonOne").gameObject;
        Interactable interactableOne = buttonOne.GetComponent<Interactable>();
        interactableOne.OnClick.AddListener(() => CopyObjectInLocalAndChangeToLocal(this));

        // Button 2 - Voting Commit
        GameObject buttonTwo = buttonCollection.transform.Find("ButtonTwo").gameObject;

        // Button 3 - Close Menu
        GameObject buttonThree = buttonCollection.transform.Find("ButtonThree").gameObject;

        //----------------------

        SolverHandler sh = nearGlobalFollowingMenu.GetComponent<SolverHandler>();
        sh.TrackedTargetType = MSUtilities.TrackedObjectType.CustomOverride;
        sh.TransformOverride = gameObject.transform;

        // Hide it
        nearGlobalFollowingMenu.SetActive(false);

        // The parent of the menu is the gameobject
        nearGlobalFollowingMenu.transform.SetParent(gameObject.transform);
        //nearGlobalFollowingMenu.transform.parent = gameObject.transform;

        //maybe to do: set scale to the same for every menu (so it doesn't become too small or too big)
    }

    // forse questo metodo va in Caretaker?
    private void CopyObjectInLocalAndChangeToLocal(GameObjController gobj)
    {
        //se lo sto copiando, allora ce l'ho già nella lista completa dei guid!
        PrefabManager.Instance.UpdateObjectLocal(gobj.Guid, gobj.Transform);
        //CaretakerScene.Instance.ChangeSceneToLocal();
    }

    //bisogna toglierlo anche dalla global e local list mementos!!!!!!!!! WIP TODO
    private void RemoveGObj()
    {
        GUIDKeeper.RemoveFromList(this.Guid);

        //should remove from local scene or also global? TODO

        //CaretakerScene.Instance.RemoveFromExistingMementos(this);

        Destroy(gameObject); //also destroy its children, e.g.: buttons
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

        PrefabManager.Instance.CreateNewObjectLocal(PrefabName);
    }

    private void CloseMenu()
    {
        nearLocalFollowingMenu.SetActive(false);
    }

    private void SetActiveManipulation(bool active)
    {      
        // Want to just spawn the object menu on manipulation, so lock rotations and movements

        // if manip is active (true), then do not lock movements (make 'enable' false)
        gameObject.GetComponent<MoveAxisConstraint>().enabled = !active; // not - because i want to lock on false

        // if manip is not active (false), then also lock y axis (so lock every axis)
        if (!active)
            gameObject.GetComponent<RotationAxisConstraint>().ConstraintOnRotation = MSUtilities.AxisFlags.YAxis | MSUtilities.AxisFlags.XAxis | MSUtilities.AxisFlags.ZAxis;
        else // if manip is active (true), then do not lock y axis (only lock axis x and z) 
            gameObject.GetComponent<RotationAxisConstraint>().ConstraintOnRotation = MSUtilities.AxisFlags.XAxis | MSUtilities.AxisFlags.ZAxis;
    }

    public void OnSelect(ManipulationEventData data)
    {
        // toggle global or local menu on object selection

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

    private void SetActiveLocalMenu(bool active)
    {
        nearLocalFollowingMenu.SetActive(active);
    }

    private void SetActiveGlobalMenu(bool active)
    {
        nearGlobalFollowingMenu.SetActive(active);
    }
}
