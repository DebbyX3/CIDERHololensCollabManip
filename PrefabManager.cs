using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.UI;

/*
  Some comments about the prefabs/materials/images loading:

    AVOID using Resources.Load, especially Resources.LoadAll!!!
    It is better to drag and drop assets in the inspector and remove them from the Resources folder, because Unity serializes the entire
    folder on build!
    You can use Resources.Load at sporadic time though, like to load little or less used files.
    Also, you can use Resources.Load at startup, but be careful that this will spike the application startup time. Once the assets are loaded 
    at startup, you should store them in a structure or something similar and refer to them by using the structure, not using
    Resources.Load anymore!

    Foy my project, I decided to drag and drop all the prefabs, materials and images, but I may change and load everything at startup by
    reading the folders using Resources.LoadAll. I want to do this just because the contents of the folders could change, 
    and dragging & dropping in the inspector in this case would be not the idea (what if i miss a file?)
 */

public class PrefabManager : MonoBehaviour
{
    public static PrefabManager Instance { get; private set; }
    public List<PrefabSpecs> PrefabCollection { get; private set; } = new List<PrefabSpecs>();

    public List<GameObject> Prefabs;
    public List<Texture2D> Images;
    public List<Material> Materials;

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

        // Populate the collection that keeps prefabs, images and materials all together
        PopulatePrefabCollection();
    }

    private void PopulatePrefabCollection()
    {
        PrefabSpecs prefabSpecs;
        List<Texture2D> prefabImages;
        List<Material> prefabMaterials;

        string prefabName;
        string prefabPathResources;
        string imagesPathResources;
        string materialsPathResources;

        foreach (GameObject currentPrefab in Prefabs)
        {
            prefabName = currentPrefab.name;
            prefabPathResources = prefabName + "\\" + prefabName;

            imagesPathResources = prefabPathResources + "\\images";
            materialsPathResources = prefabPathResources + "\\materials";

            prefabImages = GetRelevantImagesByPrefab(prefabName);
            prefabMaterials = GetRelevantMaterialsByPrefab(prefabName);

            prefabSpecs = new PrefabSpecs(prefabName, prefabPathResources, currentPrefab, imagesPathResources, 
                                          prefabImages, materialsPathResources,prefabMaterials);

            PrefabCollection.Add(prefabSpecs);
        }
    }

    // Given a list of images and the prefab name, the function returns a list of images of that prefab, following this logic:
    //      the current image is a relevant image of that prefab if the prefab name is at the beginning of the image name, before the dash
    //      for example:
    //          cube is the prefab name
    //          cube-red and cube-green are the images associated with the 'cube' prefab.
    //          Please note that if the image is called 'cubeoid-red', it does not belong to the cube prebab, because we want to match
    //          the entire word before the dash, not just the substring
    private List<Texture2D> GetRelevantImagesByPrefab(string prefabName)
    {
        string stringToMatch;
        List<Texture2D> relevantImages = new List<Texture2D>();

        foreach (Texture2D tex in Images)
        {
            stringToMatch = tex.name.Substring(0, tex.name.IndexOf('-'));

            if (stringToMatch.Equals(prefabName))
                relevantImages.Add(tex);
        }

        return relevantImages;
    }

    // Given a list of materials and the prefab name, the function returns a list of materials of that prefab, following this logic:
    //      the current material is a relevant material of that prefab if the prefab name is at the beginning of the material name, before the dash
    //      for example:
    //          cube is the prefab name
    //          cube-red and cube-green are the materials associated with the 'cube' prefab.
    //          Please note that if the material is called 'cubeoid-red', it does not belong to the cube prebab, because we want to match
    //          the entire word before the dash, not just the substring
    private List<Material> GetRelevantMaterialsByPrefab(string prefabName)
    {
        string stringToMatch;
        List<Material> relevantMaterials = new List<Material>();

        foreach (Material mat in Materials)
        {
            stringToMatch = mat.name.Substring(0, mat.name.IndexOf('-'));

            if (stringToMatch.Equals(prefabName))
                relevantMaterials.Add(mat);
        }

        return relevantMaterials;
    }

    // forse è da cambiare perchè per ora pesca solo un mat dalla lista, mentre io voglio generarne uno nuovo ogni volta? da capire,
    // forse devo solo copiarlo e ciaone- idem per la creazione in globale no? (anche se qua non mi intressa)
    private GameObject InstantiateAndSetPrefabNameMaterialName(string prefabName, string materialName, Vector3 pos, Quaternion rot)
    {
        PrefabSpecs prefabSpecs = PrefabSpecs.FindByPrefabName(prefabName, PrefabCollection);

        // Get prefab and instantiate it
        GameObject prefabToSpawn = prefabSpecs.PrefabFile;
        GameObject newObj = Instantiate(prefabToSpawn, pos, rot);

        // Find material to apply
        Material material = prefabSpecs.GetMaterialByName(materialName);

        // Change material of the object
        ChangeMaterial(ref newObj, material);

        // Set prefab and material name
        newObj.GetComponent<GameObjController>().SetPrefabName(prefabName);
        newObj.GetComponent<GameObjController>().SetMaterialName(materialName);

        return newObj;
    }

    private void ChangeMaterial(ref GameObject gObj, Material material)
    {
        MeshRenderer meshRenderer;

        foreach (Transform child in gObj.transform)
        {
            // Get mesh renderer
            meshRenderer = child.GetComponent<MeshRenderer>();
            // Set the new material on the GameObject
            meshRenderer.material = material;
        }
    }

    // need this method because sometimes i want to spawn an object with a certain GUID
    private GameObject CreateNewObject(Guid guid, string prefabName, string materialName, SerializableTransform transform)
    {
        GameObject newObj = CreateNewObject(prefabName, materialName, transform);
        newObj.GetComponent<GameObjController>().SetGuid(guid);

        return newObj;
    }

    private GameObject CreateNewObject(string prefabName, string materialName, SerializableTransform transform)
    {
        // NO need to modify or touch the scale property
        // We want to keep the same scale as the original prefab! Otherwise the real world scale would not be correct!

        SerializableVector position = transform.Position;
        SerializableVector rotation = transform.Rotation;

        Vector3 pos = new Vector3(position.X, position.Y, position.Z);
        Quaternion rot = new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);

        return InstantiateAndSetPrefabNameMaterialName(prefabName, materialName, pos, rot);
    }

    private GameObject CreateNewObjectShiftPos(string prefabName, string materialName)
    {
        // When creating an obj from scratch, shift it
        SerializableTransform st = Camera.main.transform;
        SerializableVector sv = new SerializableVector(
            st.Position.X + 0.5f,
            st.Position.Y,
            st.Position.Z + 0.5f);

        // Assign new position
        st.Position = sv;

        // Keep 0,0,0,1 quaternion as rotation
        st.Rotation = (SerializableVector)Quaternion.identity;

        return CreateNewObject(prefabName, materialName, st);
    }

    // -------------- PUBLIC --------------

    public void CreateNewObjectLocal(string prefabName, string materialName)
    {
        bool wasGlobalScene = false;

        // Want to spawn object in the local scene, so check and switch to it. Then re-switch to the global one if that's the case
        if (CaretakerScene.Instance.IsGlobalScene())
        {
            CaretakerScene.Instance.ChangeSceneToLocal();
            wasGlobalScene = true;
        }

        GameObject gobj = CreateNewObjectShiftPos(prefabName, materialName);
        gobj.GetComponent<GameObjController>().SubscribeToLocalScene();

        // If the previous scene was the global one, reswitch to the global
        if (wasGlobalScene)
            CaretakerScene.Instance.ChangeSceneToGlobal();
    }

    public GameObject CreateNewObjectGlobal(Guid guid, string prefabName, string materialName, SerializableTransform transform)
    {
        bool wasLocalScene = false;

        // Want to spawn object in the global scene, so check and switch to it. Then re-switch to the local one if that's the case
        if (CaretakerScene.Instance.IsLocalScene())
        {
            CaretakerScene.Instance.ChangeSceneToGlobal();
            wasLocalScene = true;
        }

        GameObject gobj = CreateNewObject(guid, prefabName, materialName, transform);
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
}
