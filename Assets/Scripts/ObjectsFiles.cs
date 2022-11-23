using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Web.Script.Serialization;
using System.IO;

public class ObjectsFiles : MonoBehaviour
{
    public void SaveDictionaryToFile(Dictionary<object, object> dictionary)
    {
       // File.WriteAllText("SomeFile.Txt", new JavaScriptSerializer().Serialize(dictionary));
    }

    /*
    public Dictionary<object, object> ReadDictionaryFromFile()
    {
        return new JavaScriptSerializer().Deserialize<Dictionary<object, object>>(File.ReadAllText("SomeFile.txt"));
    }*/
}
