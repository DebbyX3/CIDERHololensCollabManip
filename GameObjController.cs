using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Microsoft.MixedReality.Toolkit.UI;

public class GameObjController : MonoBehaviour {
    public Guid Guid { get; private set; }
    public string PrefabName { get; private set; } = "cube"; // solo per debug, poi l'assegnamento si toglie. TODO magari ricordati di toglierlo polla!!!
    public SerializableTransform Transform;

    // Need to keep the references to the unity actions in order to disable them
    private UnityAction saveGlobalStateAction;
    private UnityAction restoreGlobalStateAction;

    private UnityAction saveLocalStateAction;
    private UnityAction restoreLocalStateAction;

    private GameObject button;

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
        // Create button
        button = Instantiate(Resources.Load<GameObject>("PressableButtonHoloLens2"), Vector3.zero, Quaternion.identity);

        ButtonConfigHelper bch = button.GetComponent<ButtonConfigHelper>();
        bch.MainLabelText = "Force Commit";
        bch.SeeItSayItLabelEnabled = false;

        Interactable interactable = button.GetComponent<Interactable>();
        interactable.OnClick.AddListener(() => CommitManager.Instance.OnClickForcedCommit(this));

        // 'Hide' button
        button.SetActive(false);
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
        button.SetActive(true);
        button.transform.position = data.Pointer.Position;

        Debug.Log(data + "\nselect!");
        UIManager.Instance.PrintMessages(data + "\nSelect!");
    }
}
