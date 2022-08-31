using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.UI;

public class PrefabHandler : MonoBehaviour
{
    public static PrefabHandler Instance { get; private set; }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    // need this method because sometimes i want to spawn an object with a certain GUID
    public GameObject CreateNewObject(Guid guid, string prefabName, SerializableTransform transform)
    {
        GameObject newObj = CreateNewObject(prefabName, transform);
        newObj.GetComponent<GameObjController>().SetGuid(guid);

        return newObj;
    }

    public GameObject CreateNewObject(string prefabName, SerializableTransform transform) {
        SerializableVector position = transform.Position;
        SerializableVector rotation = transform.Rotation;
        SerializableVector scale = transform.Scale;

        Vector3 pos = new Vector3(position.x, position.y, position.z);
        Quaternion rot = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

        GameObject newObj = Instantiate(Resources.Load<GameObject>(prefabName), pos, rot);
        newObj.GetComponent<GameObjController>().SetPrefabName(prefabName);

        return newObj;
    }

    public void CreateNewObject(string prefabName)
    {
        // When creating an obj from scratch, shift it

        SerializableTransform st = Camera.main.transform;
        SerializableVector sv = new SerializableVector(
            st.Position.x + 0.5f,
            st.Position.y,
            st.Position.z + 0.5f);

        st.Position = sv;

        CreateNewObject(prefabName, st);        
    }

    public void UpdateObject(Guid guid, SerializableTransform transform) 
    {        
        GameObject obj = GUIDKeeper.GetGObjFromGuid(guid);
        TransformSerializer.LoadTransform(obj.transform, transform);
    }
}
