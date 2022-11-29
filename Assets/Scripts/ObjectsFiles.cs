using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Text;

public class ObjectsFiles
{
    public static void SaveData(Dictionary<Guid, Memento> obj)
    {
        string filename = "GlobalLayer-" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");
        string path = string.Format("{0}/GlobalLayers/{1}.json", Application.persistentDataPath, filename);

        string json = JsonConvert.SerializeObject(obj);
        byte[] data = Encoding.ASCII.GetBytes(json);

        UnityEngine.Windows.File.WriteAllBytes(path, data);
    }

    public static Dictionary<Guid, Memento> ReadData(string filename)
    {
        string path = string.Format("{0}/GlobalLayers/{1}.json", Application.persistentDataPath, filename);

        byte[] data = UnityEngine.Windows.File.ReadAllBytes(path);
        string json = Encoding.ASCII.GetString(data);

        Dictionary<Guid, Memento> obj = JsonConvert.DeserializeObject<Dictionary<Guid, Memento>>(json);

        return obj;
    }
}
