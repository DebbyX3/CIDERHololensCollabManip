using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.UI;

public class PrefabManager : MonoBehaviour
{
    public static PrefabManager Instance { get; private set; }

    public List<GameObject> prefabs;
    public List<Texture2D> images;
    public List<Material> materials;

    private List<PrefabSpecs> prefabCollection = new List<PrefabSpecs>();


    // -------------- PRIVATE --------------

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

        // Populate 
        PopulatePrefabCollection();
    }

    private void PopulatePrefabCollection()
    {
        PrefabSpecs prefabSpecs;
        List<Texture2D> prefabImages;
        List<Material> prefabMaterials;

        string prefabName;

        foreach (GameObject gObj in prefabs)
        {
            prefabName = gObj.name;

            prefabImages = getRelevantImagesByPrefab(prefabName);
        }

        /*
        ImagesMaterialsStruct item;

        foreach (Texture2D tex in images)
        {
            item = new ImagesMaterialsStruct
            {
                name = tex.name,
                image = tex
            };

            if (materials.Find(x => x.name == tex.name) is Material mat)
                item.material = mat;

            imagesMaterialsStruct.Add(item);
        }*/
    }

    // Given a list of images and the prefab name, the function returns a list of images of that prefab, following this logic:
    //      the current image is a relevant image of that prefab if the prefab name is at the beginning of the image name, before the dash
    //      for example:
    //          cube is the prefab name
    //          cube-red and cube-green are the images associated with the 'cube' prefab.
    //          Please note that if the image is called 'cubeoid-red', it does not belong to the cube prebab, because we want to match
    //          the entire word before the dash, not just the substring
    private List<Texture2D> getRelevantImagesByPrefab(string prefabName)
    {
        string stringToMatch;
        List<Texture2D> relevantImages = new List<Texture2D>();

        foreach (Texture2D tex in images)
        {
            stringToMatch = tex.name.Substring(0, prefabName.IndexOf('-'));

            if (stringToMatch.Equals(prefabName))
                relevantImages.Add(tex);
        }

        return relevantImages;
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

    // -------------- PUBLIC --------------

    public void CreateNewObjectLocal(string prefabName)
    {
        bool wasGlobalScene = false;

        // Want to spawn object in the local scene, so check and switch to it. Then re-switch to the global one if that's the case
        if (CaretakerScene.Instance.IsGlobalScene())
        {
            CaretakerScene.Instance.ChangeSceneToLocal();
            wasGlobalScene = true;
        }

        GameObject gobj = CreateNewObjectShiftPos(prefabName);
        gobj.GetComponent<GameObjController>().SubscribeToLocalScene();

        // If the previous scene was the global one, reswitch to the global
        if (wasGlobalScene)
            CaretakerScene.Instance.ChangeSceneToGlobal();
    }

    public GameObject CreateNewObjectGlobal(Guid guid, string prefabName, SerializableTransform transform)
    {
        bool wasLocalScene = false;

        // Want to spawn object in the global scene, so check and switch to it. Then re-switch to the local one if that's the case
        if (CaretakerScene.Instance.IsLocalScene())
        {
            CaretakerScene.Instance.ChangeSceneToGlobal();
            wasLocalScene = true;
        }

        GameObject gobj = CreateNewObject(guid, prefabName, transform);
        gobj.GetComponent<GameObjController>().SubscribeToGlobalScene();

        // If the previous scene was the local one, reswitch to the local
        if (wasLocalScene)
            CaretakerScene.Instance.ChangeSceneToLocal();

        return gobj;
    }

    public void UpdateObjectLocal(Guid guid, SerializableTransform transform) 
    {
        bool wasGlobalScene = false;

        // Want to spawn object in the local scene, so check and switch to it. Then re-switch to the global one if that's the case
        if (CaretakerScene.Instance.IsGlobalScene())
        {
            CaretakerScene.Instance.ChangeSceneToLocal();
            wasGlobalScene = true;
        }

        GameObject gobj = GUIDKeeper.GetGObjFromGuid(guid);
        TransformSerializer.AssignDeserTransformToOriginalTransform(gobj.transform, transform);

        gobj.GetComponent<GameObjController>().SubscribeToLocalScene();

        // If the previous scene was the global one, reswitch to the global
        if (wasGlobalScene)
            CaretakerScene.Instance.ChangeSceneToGlobal();        
    }

    public void UpdateObjectGlobal(Guid guid, SerializableTransform transform)
    {
        bool wasLocalScene = false;

        // Want to spawn object in the global scene, so check and switch to it. Then re-switch to the local one if that's the case
        if (CaretakerScene.Instance.IsLocalScene())
        {
            CaretakerScene.Instance.ChangeSceneToGlobal();
            wasLocalScene = true;
        }

        GameObject gobj = GUIDKeeper.GetGObjFromGuid(guid);
        TransformSerializer.AssignDeserTransformToOriginalTransform(gobj.transform, transform);

        gobj.GetComponent<GameObjController>().SubscribeToGlobalScene();

        // If the previous scene was the local one, reswitch to the local
        if (wasLocalScene)
            CaretakerScene.Instance.ChangeSceneToLocal();       
    }

    /* public ImagesMaterialsStruct FindElementByName(string name)
     {
         if (imagesMaterialsStruct.Find(x => x.name == name) is ImagesMaterialsStruct ims)
             return ims;

         return null;
     }*/
}
