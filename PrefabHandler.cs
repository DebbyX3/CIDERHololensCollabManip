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
        SerializebleVector position = transform.Position;
        SerializebleVector rotation = transform.Rotation;
        SerializebleVector scale = transform.Scale;

        Vector3 pos = new Vector3(position.x, position.y, position.z);
        Quaternion rot = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

        GameObject newObj = Instantiate(Resources.Load<GameObject>(prefabName), pos, rot);
        newObj.GetComponent<GameObjController>().SetPrefabName(prefabName);

        GameObject button1 = Instantiate(Resources.Load<GameObject>("PressableButtonHoloLens2"), Vector3.zero, Quaternion.identity);
        Interactable interactable = button1.GetComponent<Interactable>();
        interactable.OnClick.AddListener(() => newObj.GetComponent<GameObjController>().UpdateObj());

        return newObj;
    }

    public static void UpdateObject(Guid guid, SerializableTransform transform) 
    {        
        GameObject obj = GUIDKeeper.GetGObjFromGuid(guid);
        TransformSerializer.LoadTransform(obj.transform, transform);
    }
}
