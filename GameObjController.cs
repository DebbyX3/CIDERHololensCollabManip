using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
using UnityEngine;
using UnityEngine.Events;

public class GameObjController : MonoBehaviour {
    public Guid Guid { get; private set; }
    public string PrefabName { get; private set; } // solo per debug, poi l'assegnamento si toglie. TODO magari ricordati di toglierlo polla!!!
    public SerializableTransform Transform;

    // Need to keep the references to the unity actions in order to disable them
    private UnityAction saveGlobalStateAction;
    private UnityAction restoreGlobalStateAction;

    private UnityAction saveLocalStateAction;
    private UnityAction restoreLocalStateAction;

    private GameObject nearFollowingMenu;

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
        CaretakerScene.Instance.hideObject.AddListener(() => HideObject());

        // Add manipulation event/s
        ObjectManipulator objManip = gameObject.GetComponent<ObjectManipulator>();
        objManip.OnManipulationStarted.AddListener(OnSelect);
    }

    private void Start()
    {
        SetNearFollowingMenu();
    }

    void Update() {
        // Can be done better but not really urgent
        if (Transform.Position.x != gameObject.transform.position.x ||
            Transform.Position.y != gameObject.transform.position.y ||
            Transform.Position.z != gameObject.transform.position.z) 
        {
            Transform.Position = gameObject.transform.position;
        }

        if (Transform.Rotation.x != gameObject.transform.rotation.x ||
            Transform.Rotation.y != gameObject.transform.rotation.y ||
            Transform.Rotation.z != gameObject.transform.rotation.z ||
            Transform.Rotation.w != gameObject.transform.rotation.w) 
        {
            Transform.Rotation = (SerializableVector)gameObject.transform.rotation;
        }

        if (Transform.Scale.x != gameObject.transform.lossyScale.x ||
            Transform.Scale.y != gameObject.transform.lossyScale.y ||
            Transform.Scale.z != gameObject.transform.lossyScale.z) 
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
        CaretakerScene.Instance.saveGlobalState.AddListener(saveGlobalStateAction);
        CaretakerScene.Instance.saveGlobalState.AddListener(() => SetActiveManipulation(false));

        CaretakerScene.Instance.restoreGlobalState.AddListener(restoreGlobalStateAction);
    }

    // Adding multiple identical listeners results in only a single call being made.
    public void SubscribeToLocalScene()
    {
        CaretakerScene.Instance.saveLocalState.AddListener(saveLocalStateAction);
        CaretakerScene.Instance.saveGlobalState.AddListener(() => SetActiveManipulation(true));

        CaretakerScene.Instance.restoreLocalState.AddListener(restoreLocalStateAction);
    }
    public void UnsubscribeFromGlobalScene()
    {
        CaretakerScene.Instance.saveGlobalState.RemoveListener(saveGlobalStateAction);
        CaretakerScene.Instance.restoreGlobalState.RemoveListener(restoreGlobalStateAction);
    }

    public void UnsubscribeFromLocalScene()
    {
        CaretakerScene.Instance.saveLocalState.RemoveListener(saveLocalStateAction);
        CaretakerScene.Instance.restoreLocalState.RemoveListener(restoreLocalStateAction);
    }

    // Always hide the object on change scene!
    public void HideObject()
    {
        this.gameObject.SetActive(false);
    }

    private void SetNearFollowingMenu()
    {
        nearFollowingMenu = Instantiate(Resources.Load<GameObject>("NearMenu3x2"), Vector3.zero, Quaternion.identity);

        GameObject buttonCollection = nearFollowingMenu.transform.Find("ButtonCollection").gameObject;

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

        SolverHandler sh = nearFollowingMenu.GetComponent<SolverHandler>();
        sh.TrackedTargetType = Microsoft.MixedReality.Toolkit.Utilities.TrackedObjectType.CustomOverride;
        sh.TransformOverride = gameObject.transform;

        // Hide it
        nearFollowingMenu.SetActive(false);

        // The parent of the menu is the gameobject
        nearFollowingMenu.transform.parent = gameObject.transform;

        //maybe to do: set scale to the same for every menu (so it doesn't become too small or too big)
    }

    //bisogna toglierlo anche dalla global e local list mementos!!!!!!!!! WIP TODO
    public void RemoveGObj()
    {
        GUIDKeeper.RemoveFromList(this.Guid);

        //should remove from local scene or also global? TODO

        //CaretakerScene.Instance.RemoveFromExistingMementos(this);

        Destroy(gameObject); //also destroy its children, e.g.: buttons
    }

    // Duplicate obj with a slight movement of 0.1f on axis X and Y
    public void DuplicateObj()
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

        PrefabHandler.Instance.CreateNewObjectLocal(PrefabName);
    }

    public void CloseMenu()
    {
        nearFollowingMenu.SetActive(false);
    }

    public void SetActiveManipulation(bool active)
    {
        gameObject.GetComponent<ObjectManipulator>().enabled = active;
    }

    public void OnSelect(ManipulationEventData data)
    {
        nearFollowingMenu.SetActive(true);
        nearFollowingMenu.GetComponent<RadialView>().enabled = true;

        Debug.Log("Select!");
        UIManager.Instance.PrintMessages("Select!");
    }
}
