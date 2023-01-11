using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections.Generic;
using UnityEngine;

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

    // Collection that keeps prefabs, images and materials all together
    public List<PrefabSpecs> PrefabCollection { get; private set; } = new List<PrefabSpecs>();

    public List<GameObject> Prefabs;
    public List<Texture2D> Images;
    public List<Material> Materials;

    public Material CommitPendingStateMaterial;
    public Material DeletionPendingStateMaterial;

    // Dictionary that keeps all the materials indexed by material name.
    // Please note that this 'list' is the best way I found to access all the materials without having to also provide
    // the prefab name in the PrefabCollection. Infact, sometimes I need to access the material to assign just by its material name. 
    // If the convention to name the materials for the prefabCollection is followed (also described in a txt in the resources folder)
    // then there will not be overlapping/duplicate material names, and even if this is the case, as for the nature of a dictionary,
    // it will just take the first one (a duplicate key is not permitted!)
    private Dictionary<string, Material> AllMaterialsDict { get; set; } = new Dictionary<string, Material>();

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

        // Populate the dictionary that keeps all the materials indexed by material name
        AllMaterialsDict = PrefabSpecs.GetAllMaterials(PrefabCollection);
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
                                          prefabImages, materialsPathResources, prefabMaterials);

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

    // I need this method because sometimes I want to spawn an object with a certain GUID
    private GameObject CreateNewObject(Guid guid, string prefabName, string materialName, SerializableTransform transform)
    {
        GameObject newObj = CreateNewObject(prefabName, materialName, transform);
        newObj.GetComponent<GameObjController>().SetGuid(guid);

        return newObj;
    }

    // If the transform is not provided, use a shifted positions from the user head direction
    private GameObject CreateNewObject(string prefabName, string materialName)
    {
        /*
        // When creating an obj from scratch, shift it from the user POV
        SerializableTransform st = Camera.main.transform;

        SerializableVector sv = new SerializableVector(
            st.Position.X + 0.5f,
            st.Position.Y - 1.0f,
            st.Position.Z + 0.5f);

        // Assign new position
        st.Position = sv;

        // Keep 0,0,0,1 quaternion as rotation
        st.Rotation = (SerializableVector)Quaternion.identity;
        */

        return CreateNewObject(prefabName, materialName, Camera.main.transform.forward);
    }

    private GameObject CreateNewObject(string prefabName, string materialName, Vector3 position)
    {
        // The Scale doesn't matter because isn't used
        return CreateNewObject(prefabName, materialName, new SerializableTransform(position, Quaternion.identity, Vector3.one));
    }

    private GameObject CreateNewObject(string prefabName, string materialName, SerializableTransform transform)
    {
        // DO NOT modify or touch the scale property!
        // We want to keep the same scale as the original prefab! Otherwise the real world scale would not be correct!

        SerializableVector position = transform.Position;
        SerializableVector rotation = transform.Rotation;

        // Set position and rotation
        Vector3 pos = new Vector3(position.X, position.Y, position.Z);
        Quaternion rot = new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);

        // Find prefab specifications by prefab name
        PrefabSpecs prefabSpecs = PrefabSpecs.FindByPrefabName(prefabName, PrefabCollection);

        // Get prefab and instantiate it
        GameObject newObj = Instantiate(prefabSpecs.PrefabFile, pos, rot);

        // Change material of the object
        ChangeMaterial(newObj, materialName);

        // Set prefab and material name
        newObj.GetComponent<GameObjController>().SetPrefabName(prefabName);
        newObj.GetComponent<GameObjController>().SetMaterialName(materialName);

        return newObj;
    }

    // -------------- PUBLIC --------------

    public GameObject CreateNewObjectInLocal(string prefabName, string materialName)
    {
        bool wasGlobalScene = false;

        // Want to spawn object in the local scene, so check and switch to it. Then re-switch to the global one if that's the case
        if (CaretakerScene.Instance.IsGlobalScene())
        {
            CaretakerScene.Instance.ChangeSceneToLocal();
            wasGlobalScene = true;
        }

        GameObject gObj = CreateNewObject(prefabName, materialName);

        GameObjController gObjContr = gObj.GetComponent<GameObjController>();
        gObjContr.SubscribeToLocalScene();
        CaretakerScene.Instance.SaveLocalState(gObjContr);

        // If the previous scene was the global one, reswitch to the global
        if (wasGlobalScene)
            CaretakerScene.Instance.ChangeSceneToGlobal();

        return gObj;
    }

    public GameObject CreateNewObjectInGlobal(Guid guid, string prefabName, string materialName, SerializableTransform transform)
    {
        bool wasLocalScene = false;

        // Want to spawn object in the global scene, so check and switch to it. Then re-switch to the local one if that's the case
        if (CaretakerScene.Instance.IsLocalScene())
        {
            CaretakerScene.Instance.ChangeSceneToGlobal();
            wasLocalScene = true;
        }

        GameObject gObj = CreateNewObject(guid, prefabName, materialName, transform);

        GameObjController gObjContr = gObj.GetComponent<GameObjController>();
        gObjContr.SubscribeToGlobalScene();
        CaretakerScene.Instance.SaveGlobalState(gObjContr);

        // If the previous scene was the local one, reswitch to the local
        if (wasLocalScene)
            CaretakerScene.Instance.ChangeSceneToLocal();

        return gObj;
    }

    public GameObject CreateNewObjectInCommitPending(Guid guid, string prefabName, string materialName, SerializableTransform transform)
    {
        bool wasLocalScene = false;

        // Want to spawn object in the global scene, so check and switch to it. Then re-switch to the local one if that's the case
        if (CaretakerScene.Instance.IsLocalScene())
        {
            CaretakerScene.Instance.ChangeSceneToGlobal();
            wasLocalScene = true;
        }

        GameObject gObj = CreateNewObject(guid, prefabName, materialName, transform);

        GameObjController gObjContr = gObj.GetComponent<GameObjController>();
        gObjContr.SubscribeToCommitPendingList();
        CaretakerScene.Instance.SaveCommitPendingState(gObjContr);

        ChangeMaterialCommitPendingState(gObj);

        // If the previous scene was the local one, reswitch to the local
        if (wasLocalScene)
            CaretakerScene.Instance.ChangeSceneToLocal();

        return gObj;
    }

    public GameObject PutExistingObjectInLocal(Guid guid, string materialName)
    {
        return PutExistingObjectInLocal(guid, SerializableTransform.Default(), materialName);
    }

    public GameObject PutExistingObjectInLocal(Guid guid, SerializableTransform transform, string materialName = "") 
    {
        bool wasGlobalScene = false;

        // Want to spawn object in the local scene, so check and switch to it. Then re-switch to the global one if that's the case
        if (CaretakerScene.Instance.IsGlobalScene())
        {
            CaretakerScene.Instance.ChangeSceneToLocal();
            wasGlobalScene = true;
        }

        GameObject gObj = GUIDKeeper.GetGObjFromGuid(guid);
        GameObjController gObjContr = gObj.GetComponent<GameObjController>();

        // Update transform - only if there is actually something to change - if it is a default value do not change
        if (!transform.Equals(SerializableTransform.Default()))
        {
            gObjContr.Transform.AssignDeserTransformToOriginalTransform(transform);
        }

        // Change material of the object - only if there is actually something to change - if it is a default value do not change
        if (!materialName.Equals(""))
        {
            ChangeMaterial(gObj, materialName);
            gObj.GetComponent<GameObjController>().SetMaterialName(materialName); // Do not move this into ChangeMaterial
        }

        gObjContr.SubscribeToLocalScene(); //always
        CaretakerScene.Instance.SaveLocalState(gObjContr);

        gObj.SetActive(true);

        // If the previous scene was the global one, reswitch to the global
        if (wasGlobalScene)
            CaretakerScene.Instance.ChangeSceneToGlobal();

        return gObj;
    }

    public GameObject PutExistingObjectInGlobal(Guid guid, SerializableTransform transform, string materialName)
    {
        bool wasLocalScene = false;

        // Want to spawn object in the global scene, so check and switch to it. Then re-switch to the local one if that's the case
        if (CaretakerScene.Instance.IsLocalScene())
        {
            CaretakerScene.Instance.ChangeSceneToGlobal();
            wasLocalScene = true;
        }

        GameObject gObj = GUIDKeeper.GetGObjFromGuid(guid);
        GameObjController gObjContr = gObj.GetComponent<GameObjController>();

        // Update transform
        gObjContr.Transform.AssignDeserTransformToOriginalTransform(transform);  

        // Change material of the object
        ChangeMaterial(gObj, materialName);
        gObjContr.SetMaterialName(materialName); // Do not move this into ChangeMaterial

        gObjContr.SubscribeToGlobalScene(); //always
        CaretakerScene.Instance.SaveGlobalState(gObjContr);

        // If the previous scene was the local one, reswitch to the local
        if (wasLocalScene)
            CaretakerScene.Instance.ChangeSceneToLocal();

        return gObj;
    }

    public GameObject PutExistingObjectInCommitPending(Guid guid, SerializableTransform transform, string materialName)
    {
        bool wasLocalScene = false;

        // Want to spawn object in the global scene, so check and switch to it. Then re-switch to the local one if that's the case
        if (CaretakerScene.Instance.IsLocalScene())
        {
            CaretakerScene.Instance.ChangeSceneToGlobal();
            wasLocalScene = true;
        }

        GameObject gObj = GUIDKeeper.GetGObjFromGuid(guid);
        GameObjController gObjContr = gObj.GetComponent<GameObjController>();

        // Update transform
        gObjContr.Transform.AssignDeserTransformToOriginalTransform(transform);

        // Change material of the object
        ChangeMaterialCommitPendingState(gObj);
        gObjContr.SetMaterialName(materialName); // Do not move this into ChangeMaterial

        gObjContr.SubscribeToCommitPendingList(); //always
        CaretakerScene.Instance.SaveCommitPendingState(gObjContr);

        // If the previous scene was the local one, reswitch to the local
        if (wasLocalScene)
            CaretakerScene.Instance.ChangeSceneToLocal();

        return gObj;
    }

    public GameObject PutExistingObjectInDeletionPending(Guid guid, SerializableTransform transform, string materialName)
    {
        bool wasLocalScene = false;

        // Want to spawn object in the global scene, so check and switch to it. Then re-switch to the local one if that's the case
        if (CaretakerScene.Instance.IsLocalScene())
        {
            CaretakerScene.Instance.ChangeSceneToGlobal();
            wasLocalScene = true;
        }

        GameObject gObj = GUIDKeeper.GetGObjFromGuid(guid);
        GameObjController gObjContr = gObj.GetComponent<GameObjController>();

        // Update transform
        gObjContr.Transform.AssignDeserTransformToOriginalTransform(transform);

        // Change material of the object
        ChangeMaterialDeletionPendingState(gObj);
        gObjContr.SetMaterialName(materialName); // Do not move this into ChangeMaterial

        gObjContr.SubscribeToDeletionPendingList(); //always
        CaretakerScene.Instance.SaveDeletionPendingState(gObjContr);

        // If the previous scene was the local one, reswitch to the local
        if (wasLocalScene)
            CaretakerScene.Instance.ChangeSceneToLocal();

        return gObj;
    }

    // --------------- CHANGE MATERIAL METHODS SET --------------------------

    public void ChangeMaterial(Guid guid, string materialName)
    {
        GameObject gObj = GUIDKeeper.GetGObjFromGuid(guid);
        ChangeMaterial(gObj, materialName);
    }

    public void ChangeMaterial(Guid guid, Material material)
    {
        GameObject gObj = GUIDKeeper.GetGObjFromGuid(guid);
        ChangeMaterial(gObj, material);
    }

    public void ChangeMaterial(GameObject gObj, string materialName)
    {
        Material mat;

        if (AllMaterialsDict.TryGetValue(materialName, out mat)) // If the material exists
            ChangeMaterial(gObj, mat);
        else if (materialName.Equals(CommitPendingStateMaterial.name)) //todo probabilmente da sistemare?
            ChangeMaterialCommitPendingState(gObj);
        else
            ChangeMaterial(gObj, PrefabCollection[0].GetAMaterial()); // if the material does not exists, then take whatever material
                                                                      // (can also be randomized it but i don't think it will be useful)
    }

    public void ChangeMaterial(GameObject gObj, Material material)
    {
        // If the object has children, then loop on them and change each child
        if (gObj.transform.childCount > 0)
        {
            foreach (Transform child in gObj.transform)
            {
                ChangeOneMaterial(child.gameObject, material);
            }
        }

        // Sometimes, the object has its own meshRenderer to change
        ChangeOneMaterial(gObj, material);
    }

    public void ChangeMaterialCommitPendingState(GameObject gObj)
    {
        ChangeMaterial(gObj, CommitPendingStateMaterial);
    }

    public void ChangeMaterialCommitPendingState(Guid guid)
    {
        ChangeMaterial(guid, CommitPendingStateMaterial);
    }

    public void ChangeMaterialDeletionPendingState(GameObject gObj)
    {
        ChangeMaterial(gObj, DeletionPendingStateMaterial);
    }

    public void ChangeMaterialDeletionPendingState(Guid guid)
    {
        ChangeMaterial(guid, DeletionPendingStateMaterial);
    }

    private void ChangeOneMaterial(GameObject gObj, Material material)
    {
        MeshRenderer meshRenderer;

        // Get mesh renderer
        meshRenderer = gObj.GetComponent<MeshRenderer>();

        // If the gobj has a mesh, change the material
        // I need to do this because a prefab has also other children, like the manipulation MRTK ones,
        // and they don't have a Mesh
        if (meshRenderer != null)
        {
            // if the material is the Commit pending one, change every material
            if (material.Equals(CommitPendingStateMaterial))
            {
                Material[] mat;

                mat = meshRenderer.materials;

                for (int i = 0; i < meshRenderer.materials.Length; i++)
                    mat[i] = material; // Set the new material on the GameObject

                meshRenderer.materials = mat;         
            }
            else // if it is a common material, just change the first submesh material
            {
                meshRenderer.material = material;
            }
        }
    }

    // -----------------------------------------
}
