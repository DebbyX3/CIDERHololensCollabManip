using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public struct ImageSpecs
{
    public string Name { get; private set; }
    public Texture2D Image { get; private set; }

    public ImageSpecs(string name, Texture2D image)
    {
        Name = name;
        Image = image;
    }
}

public struct MaterialSpecs
{
    public string Name { get; private set; }
    public Material Material { get; private set; }

    public MaterialSpecs(string name, Material material)
    {
        Name = name;
        Material = material;
    }
}*/

public class PrefabSpecs 
{
    public string PrefabName { get; private set; }
    public string PrefabPathResources { get; private set; }

    public Dictionary<string, Texture2D> Images = new Dictionary<string, Texture2D>();
    public Dictionary<string, Material> Materials = new Dictionary<string, Material>();

    public PrefabSpecs() { }

    // Mai usato questo costruttore, sicuro
    public PrefabSpecs(string prefabName, string prefabPathResources, 
                       Dictionary<string, Texture2D> images, Dictionary<string, Material> materials)
    {
        PrefabName = prefabName;
        PrefabPathResources = prefabPathResources;
        Images = images;
        Materials = materials;
    }

    public PrefabSpecs(string prefabName, string prefabPathResources, List<Texture2D> images, List<Material> materials)
    {
        PrefabName = prefabName;
        PrefabPathResources = prefabPathResources;

        foreach (Texture2D tex in images)
            Images.Add(tex.name, tex);

        foreach (Material mat in materials)
            Materials.Add(mat.name, mat);
    }
}
