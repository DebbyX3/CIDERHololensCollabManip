using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 Store the paths to the folders even if I will not plan to use them
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
        Material mat;

        if (Materials.TryGetValue(name, out mat)) // If the material exists
            return mat;

        return GetAMaterial(); //else, return a 'default' material 
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
}
