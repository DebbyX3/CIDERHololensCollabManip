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

    private GameObject InstantiateAndSetPrefabName(string prefabName, Vector3 pos, Quaternion rot)
    {
        GameObject newObj = Instantiate(Resources.Load<GameObject>(prefabName), pos, rot);
        newObj.GetComponent<GameObjController>().SetPrefabName(prefabName);

        return newObj;
    }

    // need this method because sometimes i want to spawn an object with a certain GUID
    private GameObject CreateNewObject(Guid guid, string prefabName, SerializableTransform transform)
    {
        GameObject newObj = CreateNewObject(prefabName, transform);
        newObj.GetComponent<GameObjController>().SetGuid(guid);

        return newObj;
    }

    private GameObject CreateNewObject(string prefabName, SerializableTransform transform)
    {
        // NO need to modify or touch the scale property
        // We want to keep the same scale as the original prefab! Otherwise the real world scale would not be correct!

        SerializableVector position = transform.Position;
        SerializableVector rotation = transform.Rotation;

        Vector3 pos = new Vector3(position.x, position.y, position.z);
        Quaternion rot = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

        return InstantiateAndSetPrefabName(prefabName, pos, rot);
    }

    private GameObject CreateNewObjectShiftPos(string prefabName)
    {
        // When creating an obj from scratch, shift it
        SerializableTransform st = Camera.main.transform;
        SerializableVector sv = new SerializableVector(
            st.Position.x + 0.5f,
            st.Position.y,
            st.Position.z + 0.5f);

        // Assign new position
        st.Position = sv;

        // Keep 0,0,0,1 quaternion as rotation
        st.Rotation = (SerializableVector)Quaternion.identity;

        return CreateNewObject(prefabName, st);
    }

    public void CreateNewObjectLocal(string prefabName)
    {
        GameObject gobj = CreateNewObjectShiftPos(prefabName);
        gobj.GetComponent<GameObjController>().SubscribeToLocalScene();
    }

    // not sure about this, maybe WIP or TODO better
    public void CreateNewObjectGlobal(Guid guid, string prefabName, SerializableTransform transform)
    {
        GameObject gobj = CreateNewObject(guid, prefabName, transform);
        gobj.GetComponent<GameObjController>().SubscribeToGlobalScene();
    }

    public void UpdateObject(Guid guid, SerializableTransform transform) 
    {        
        GameObject obj = GUIDKeeper.GetGObjFromGuid(guid);
        TransformSerializer.LoadTransform(obj.transform, transform);
    }
}
