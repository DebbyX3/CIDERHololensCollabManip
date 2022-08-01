using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class GameObjController : MonoBehaviour {
    public Guid Guid { get; private set; }
    public string PrefabName { get; private set; } = "cube"; // solo per debug, poi l'assegnamento si toglie. TODO magari ricordati di toglierlo polla!!!
    public SerializableTransform Transform;

    private void Awake() {
        // Generate new guid
        Guid = Guid.NewGuid();

        // Add guid and attached gameobject in list
        GUIDKeeper.AddToList(Guid, gameObject);

        //CaretakerScene.saveGlobalState.AddListener(() => CaretakerScene.SaveGlobalState(this));
        //CaretakerScene.saveLocalState.AddListener(() => CaretakerScene.SaveLocalState(this));

        CaretakerScene.saveState.AddListener(() => CaretakerScene.SaveState(this));
    }

    void Update() {
        //can be done better but not really urgent
        if (Transform.Position.x != gameObject.transform.position.x ||
            Transform.Position.y != gameObject.transform.position.y ||
            Transform.Position.z != gameObject.transform.position.z) {
            Transform.Position = gameObject.transform.position;
        }

        if (Transform.Rotation.x != gameObject.transform.rotation.x ||
            Transform.Rotation.y != gameObject.transform.rotation.y ||
            Transform.Rotation.z != gameObject.transform.rotation.z ||
            Transform.Rotation.w != gameObject.transform.rotation.w) {
            Transform.Rotation = (SerializebleVector)gameObject.transform.rotation;
        }

        if (Transform.Scale.x != gameObject.transform.lossyScale.x ||
            Transform.Scale.y != gameObject.transform.lossyScale.y ||
            Transform.Scale.z != gameObject.transform.lossyScale.z) {
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

    /* PROVA MEMENTO DA QUA IN GIù! */

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
    }
}
