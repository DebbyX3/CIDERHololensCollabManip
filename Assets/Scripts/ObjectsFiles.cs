using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.MixedReality.Toolkit;

// It works!

public static class ObjectsFiles
{
    public static void SaveData(Dictionary<Guid, Memento> obj)
    {
        string filename = "GlobalLayer-" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");
        string path = string.Format("{0}/GlobalLayers/{1}.bin", Application.persistentDataPath, filename);

        byte[] data = obj.Serialize();

        // da usare per quando si prova su unity
        path = "./" + filename + ".json";

        UnityEngine.Windows.File.WriteAllBytes(path, data);
    }

    // Record the user gaze
    /*public static IEnumerator SaveData(GameObjController gObjContr)
    {
        string filename = "UserData-" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");
        string path = string.Format("{0}/UserData/{1}.bin", Application.persistentDataPath, filename);

        // da usare per quando si prova su unity
        path = "./" + filename + ".json";

        UnityEngine.Windows.File.WriteAllBytes();
    }*/

    public static Dictionary<Guid, Memento> ReadData(TextAsset binaryFile)
    {
        byte[] binaryBytes = binaryFile.bytes;

        Dictionary<Guid, Memento> obj = Deserialize(binaryBytes);

        return obj;
    }

    //---------------------------------------

    public static byte[] Serialize(this Dictionary<Guid, Memento> dictionary)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        using (MemoryStream stream = new MemoryStream())
        {
            formatter.Serialize(stream, dictionary);
            return stream.ToArray();
        }
    }

    public static Dictionary<Guid, Memento> Deserialize(byte[] data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        Dictionary<Guid, Memento> dictionary;

        using (MemoryStream stream = new MemoryStream(data))
        {
            dictionary = (Dictionary<Guid, Memento>)formatter.Deserialize(stream);
        }

        return dictionary;
    }
}
