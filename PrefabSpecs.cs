using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}

public class PrefabSpecs 
{
    public string PrefabName { get; private set; }
    public string PrefabPathResources { get; private set; }

    public List<ImageSpecs> Images = new List<ImageSpecs>();
    public List<MaterialSpecs> Materials = new List<MaterialSpecs>();

    public PrefabSpecs() { }

    // Mai usato questo costruttore, sicuro
    public PrefabSpecs(string prefabName, string prefabPathResources, List<ImageSpecs> images, List<MaterialSpecs> materials)
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
            Images.Add(new ImageSpecs(tex.name, tex));

        foreach (Material mat in materials)
            Materials.Add(new MaterialSpecs(mat.name, mat));
    }
}
