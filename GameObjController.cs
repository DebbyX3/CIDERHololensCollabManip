using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameObjController : MonoBehaviour
{
    public Guid Guid { get; private set; }
    public string PrefabName { get; private set; }

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

    public void SendGObj() 
    {
        //TODO: controlla che il client sia connesso prima?
        //SocketClient.Instance.SendNewObject(gameObject);
    }
}
