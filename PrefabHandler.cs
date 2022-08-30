using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.UI;

public class PrefabHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //GameObject nuovo = CreateNewObject("cube", new SerializableTransform(Vector3.one, Quaternion.identity, Vector3.one));        
    }

    // need this method because sometimes i want to spawn an object with a certain GUID
    public static GameObject CreateNewObject(Guid guid, string prefabName, SerializableTransform transform)
    {
        GameObject newObj = CreateNewObject(prefabName, transform);
        newObj.GetComponent<GameObjController>().SetGuid(guid);

        return newObj;
    }

    public static GameObject CreateNewObject(string prefabName, SerializableTransform transform) {
        SerializableVector position = transform.Position;
        SerializableVector rotation = transform.Rotation;
        SerializableVector scale = transform.Scale;

        Vector3 pos = new Vector3(position.x, position.y, position.z);
        Quaternion rot = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

        GameObject newObj = Instantiate(Resources.Load<GameObject>(prefabName), pos, rot);
        newObj.GetComponent<GameObjController>().SetPrefabName(prefabName);

        return newObj;
    }

    public static void UpdateObject(Guid guid, SerializableTransform transform) 
    {        
        GameObject obj = GUIDKeeper.GetGObjFromGuid(guid);
        TransformSerializer.LoadTransform(obj.transform, transform);
    }
}
