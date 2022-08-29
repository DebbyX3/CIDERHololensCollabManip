using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
using UnityEngine;
using UnityEngine.Events;

public class GameObjController : MonoBehaviour {
    public Guid Guid { get; private set; }
    public string PrefabName { get; private set; } = "cube"; // solo per debug, poi l'assegnamento si toglie. TODO magari ricordati di toglierlo polla!!!
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
        Transform.Rotation = (SerializebleVector)gameObject.transform.rotation;
        Transform.Scale = gameObject.transform.lossyScale;

        // Set global and local UnityActions
        saveGlobalStateAction = () => CaretakerScene.Instance.SaveGlobalState(this);
        restoreGlobalStateAction = () => CaretakerScene.Instance.RestoreGlobalState(this);

        saveLocalStateAction = () => CaretakerScene.Instance.SaveLocalState(this);
        restoreLocalStateAction = () => CaretakerScene.Instance.RestoreLocalState(this);

        // Subscribe to change scene events
        SubscribeToGlobalScene();
        SubscribeToLocalScene();
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
            Transform.Rotation = (SerializebleVector)gameObject.transform.rotation;
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

    public void UpdateObj() {
        SerializableTransform tr = new SerializableTransform(gameObject.transform);
        SerializebleVector pos = tr.Position;
        pos.x = pos.x + 0.3f;
        tr.Position = pos;

        PrefabHandler.UpdateObject(Guid, tr);
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
        TransformSerializer.LoadTransform(gameObject.transform, Transform);
    }

    // Adding multiple identical listeners results in only a single call being made.
    public void SubscribeToGlobalScene()
    {
        CaretakerScene.Instance.saveGlobalState.AddListener(saveGlobalStateAction);
        CaretakerScene.Instance.restoreGlobalState.AddListener(restoreGlobalStateAction);
    }

    // Adding multiple identical listeners results in only a single call being made.
    public void SubscribeToLocalScene()
    {
        CaretakerScene.Instance.saveLocalState.AddListener(saveLocalStateAction);
        CaretakerScene.Instance.restoreLocalState.AddListener(restoreLocalStateAction);
    }
    public void UnsubscribeToGlobalScene()
    {
        CaretakerScene.Instance.saveGlobalState.RemoveListener(saveGlobalStateAction);
        CaretakerScene.Instance.restoreGlobalState.RemoveListener(restoreGlobalStateAction);
    }

    public void UnsubscribeToLocalScene()
    {
        CaretakerScene.Instance.saveLocalState.RemoveListener(saveLocalStateAction);
        CaretakerScene.Instance.restoreLocalState.RemoveListener(restoreLocalStateAction);
    }

    public void OnSelect(ManipulationEventData data)
    {
        nearFollowingMenu.SetActive(true);

        Debug.Log(data + "\nselect!");
        UIManager.Instance.PrintMessages(data + "\nSelect!");
    }

    private void SetNearFollowingMenu()
    {
        nearFollowingMenu = Instantiate(Resources.Load<GameObject>("NearMenu3x2"), Vector3.zero, Quaternion.identity);

        GameObject buttonCollection = nearFollowingMenu.transform.Find("ButtonCollection").gameObject;

        // Button 1
        GameObject buttonOne = buttonCollection.transform.Find("ButtonOne").gameObject;
        Interactable interactableOne = buttonOne.GetComponent<Interactable>();
        interactableOne.OnClick.AddListener(() => CommitManager.Instance.OnClickForcedCommit(this));

        // Button 2
        GameObject buttonTwo = buttonCollection.transform.Find("ButtonTwo").gameObject;

        // Button 3
        GameObject buttonThree = buttonCollection.transform.Find("ButtonThree").gameObject;

        // Button 4 - Remove
        GameObject buttonFour = buttonCollection.transform.Find("ButtonFour").gameObject;
        Interactable interactableFour = buttonFour.GetComponent<Interactable>();
        interactableFour.OnClick.AddListener(() => RemoveGObj());

        // Button 5
        GameObject buttonFive = buttonCollection.transform.Find("ButtonFive").gameObject;

        // Button 6
        GameObject buttonSix = buttonCollection.transform.Find("ButtonSix").gameObject;

        SolverHandler sh = nearFollowingMenu.GetComponent<SolverHandler>();
        sh.TrackedTargetType = Microsoft.MixedReality.Toolkit.Utilities.TrackedObjectType.CustomOverride;
        sh.TransformOverride = gameObject.transform;

        /*
        Vector3 forcedCommitButtonAddOffset = Vector3.zero;
        forcedCommitButtonAddOffset.z = -1f;
        sh.AdditionalOffset = forcedCommitButtonAddOffset;

        RadialView rv = destroyButton.AddComponent<RadialView>();
        rv.MinDistance = 0.1f;
        rv.MaxDistance = 0.3f;*/

        // 'Hide' button
        nearFollowingMenu.SetActive(false);

        // button parent is this gameobject
        nearFollowingMenu.transform.parent = gameObject.transform;
    }

    public void RemoveGObj()
    {
        GUIDKeeper.RemoveFromList(this.Guid);
        Destroy(gameObject); //also destroy its children, e.g.: buttons
    }
}
