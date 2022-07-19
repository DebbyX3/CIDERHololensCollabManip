using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameObjController : MonoBehaviour
{
    public Guid Guid { get; private set; }
    public string PrefabName { get; private set; } = "cube"; // solo per debug, poi l'assegnamento si toglie. TODO magari ricordati di toglierlo polla!!!

    private void Awake() {
        // Generate new guid
        Guid = Guid.NewGuid();

        // Add guid and attached gameobject in list
        GUIDList.AddToList(Guid, gameObject);
    }

    public string getGuidString() {
        return Guid.ToString();
    }

    public void setPrefabName(string prefabName) {
        this.PrefabName = prefabName;
    }

    public void setGuid(Guid guid) {
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

    public void UpdateObj() {

        SerializableTransform tr = new SerializableTransform(gameObject.transform);
        SerializebleVector pos = tr.Position;
        pos.x = pos.x + 0.3f;
        tr.Position = pos;

        PrefabHandler.UpdateObject(Guid, tr);
    }
}
