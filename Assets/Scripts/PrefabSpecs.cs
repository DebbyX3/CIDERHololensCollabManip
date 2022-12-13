using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 Store the paths to the folders even if I will not plan to use them
 
Example of a PrefabSpecs instance:

- prefabName: cube
- prefabPathResources: cube/cube
- prefabFile: the actual cube.prefab file linked in the inspector
- imagesPathResources: cube/images
- images: [cube_red, Texture2D object linked in the inspector], [cube_blue, Texture2D object linked in the inspector]...
- materialsPathResources: cube/materials
- materials: [cube_red, Material object linked in the inspector], [cube_blue, Material object linked in the inspector]...
*/

public class PrefabSpecs 
{
    public string PrefabName { get; private set; }
    public string PrefabPathResources { get; private set; }
    public GameObject PrefabFile { get; private set; }
    public string ImagesPathResources { get; private set; }
    public Dictionary<string, Texture2D> Images = new Dictionary<string, Texture2D>();
    public string MaterialsPathResources { get; private set; }
    public Dictionary<string, Material> Materials = new Dictionary<string, Material>();

    public PrefabSpecs() { }

    // Mai usato questo costruttore, sicuro
    public PrefabSpecs(string prefabName, string prefabPathResources, GameObject prefabFile, string imagesPathResources, 
                       Dictionary<string, Texture2D> images, string materialsPathResources, Dictionary<string, Material> materials)
    {
        PrefabName = prefabName;
        PrefabPathResources = prefabPathResources;
        PrefabFile = prefabFile;
        ImagesPathResources = imagesPathResources;
        Images = images;
        MaterialsPathResources = materialsPathResources;
        Materials = materials;
    }

    public PrefabSpecs(string prefabName, string prefabPathResources, GameObject prefabFile, string imagesPathResources,
                       List<Texture2D> images, string materialsPathResources, List<Material> materials)
    {
        PrefabName = prefabName;
        PrefabPathResources = prefabPathResources;
        PrefabFile = prefabFile;

        ImagesPathResources = imagesPathResources;
        MaterialsPathResources = materialsPathResources;

        foreach (Texture2D tex in images)
            Images.Add(tex.name, tex);

        foreach (Material mat in materials)
            Materials.Add(mat.name, mat);
    }

    // Returns whatever image from the dictionary of images. It is not important what image returns, could be random
    public Texture2D GetAnImage()
    {
        IDictionaryEnumerator e = Images.GetEnumerator();
        e.MoveNext();
        KeyValuePair<string, Texture2D> anElement = (KeyValuePair<string, Texture2D>) e.Current;

        return anElement.Value;
    }

    // Returns whatever material from the dictionary of materials. It is not important what material returns, could be random
    public Material GetAMaterial()
    {
        IDictionaryEnumerator e = Materials.GetEnumerator();
        e.MoveNext();
        KeyValuePair<string, Material> anElement = (KeyValuePair<string, Material>)e.Current;

        return anElement.Value;
    }

    public Material GetMaterialByName(string name)
    {
        if (Materials.TryGetValue(name, out Material mat)) // If the material exists
            return mat;

        return GetAMaterial(); //else, return a 'default' material 
    }

    public Texture2D GetImageByName(string name)
    {
        if (Images.TryGetValue(name, out Texture2D image)) // If the image exists
            return image;

        return GetAnImage(); //else, return a 'default' image 
    }

    /* Returns the first element that matches the Equals, if found;
       otherwise, returns the default value for PrefabSpecs (null) */
    public static PrefabSpecs FindByPrefabName(string prefabName, List<PrefabSpecs> prefabCollection)
    {
        return prefabCollection.Find(x => x.PrefabName.Equals(prefabName));
    }

    /* Returns the list of prefabs in the collection as GameObjects*/
    public static List<GameObject> GetPrefabs(List<PrefabSpecs> prefabCollection)
    {
        List<GameObject> prefabs = new List<GameObject>();

        foreach(PrefabSpecs specs in prefabCollection)
            prefabs.Add(specs.PrefabFile);

        return prefabs;
    }

    /* Returns the names of prefabs in the collection as GameObjects*/
    public static List<string> GetPrefabsNames(List<PrefabSpecs> prefabCollection)
    {
        List<string> prefabsNames = new List<string>();

        foreach (PrefabSpecs specs in prefabCollection)
            prefabsNames.Add(specs.PrefabName);

        return prefabsNames;
    }

    // Note: in case of a duplicate key (aka: same name for different material), the method adds only the first material, since adding a 
    // duplicate key is not permitted, and also the case is not really an issue here, assuming the convention used to name the materials
    // is followed (also described in a txt in the resources folder)
    public static Dictionary<string, Material> GetAllMaterials(List<PrefabSpecs> prefabCollection)
    {
        Dictionary<string, Material> allMaterials = new Dictionary<string, Material>();

        foreach (PrefabSpecs prefabSpec in prefabCollection)
        {
            foreach (KeyValuePair<string, Material> materialPair in prefabSpec.Materials)
            {
                if (!allMaterials.ContainsKey(materialPair.Key))
                    allMaterials.Add(materialPair.Key, materialPair.Value);
            }
        }

        return allMaterials;
    }
}
