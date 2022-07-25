using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class GameObjController : MonoBehaviour
{
    public Guid Guid { get; private set; }
    public string PrefabName { get; private set; } = "cube"; // solo per debug, poi l'assegnamento si toglie. TODO magari ricordati di toglierlo polla!!!
    public SerializableTransform Transform;

    private void Awake() {
        // Generate new guid
        Guid = Guid.NewGuid();

        // Add guid and attached gameobject in list
        GUIDList.AddToList(Guid, gameObject);
    }

    void Update() 
    {
        //can be done better but not really urgent
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

        if(GUIDList.ContainsGuid(guid)) {
            GUIDList.RemoveFromList(oldGuid);
        }

        GUIDList.AddToList(Guid, gameObject);
    }

    public void SendGObj() 
    {
        
    }

    public void UpdateObj() 
    {
        SerializableTransform tr = new SerializableTransform(gameObject.transform);
        SerializebleVector pos = tr.Position;
        pos.x = pos.x + 0.3f;
        tr.Position = pos;

        PrefabHandler.UpdateObject(Guid, tr);
    }

    /* PROVA MEMENTO DA QUA IN GIù! */
    public MementoProva Save() {
        return new MementoProva(this.Guid.ToString());
    }

    // Restores the Originator's state from a memento object.
    public void Restore(MementoProva memento) {
        if (!(memento is MementoProva)) {
            throw new Exception("Unknown memento class " + memento.ToString());
        }

        this.Guid = memento.GetState();
        Console.Write($"Originator: My state has changed to: {Guid}");
    }
}
