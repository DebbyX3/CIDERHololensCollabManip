using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keeps a list of every pair <GUID, GameObject> in a static list, to be consulted at runtime
/// Each time an object is spawned in the scene, the corresponding GUID is created and stored in the static list
/// </summary>
public static class GUIDList
{
    /// <summary>
    /// A list that contains the pairs <GUID, GameObject>
    /// </summary>
    public static Dictionary<Guid, GameObject> List { get; } = new Dictionary<Guid, GameObject>();

    /// <summary>
    /// Add the pair <GUID, GameObject> to the static list - where GUID is the key, GameObject is the value
    /// </summary>
    /// <param name="guid">String GUID to add to the list - Key</param>
    /// <param name="gObj">Gameobject to add to the list - Value</param>
    public static void AddToList(string guid, GameObject gObj) {
        List.Add(Guid.Parse(guid), gObj);
    }

    /// <summary>
    /// Add the pair <GUID, GameObject> to the static list - where GUID is the key, GameObject is the value
    /// </summary>
    /// <param name="guid">GUID to add to the list - Key</param>
    /// <param name="gObj">Gameobject to add to the list - Value</param>
    public static void AddToList(Guid guid, GameObject gObj) {
        List.Add(guid, gObj);
    }

    /// <summary>
    /// Get the corresponding GameObject (value) associated to the GUID (key)
    /// </summary>
    /// <param name="guid">String GUID to search in the list - Key</param>
    /// <returns>Returns the corresponding value - GameObject</returns>
    public static GameObject GetGObjFromGuid(string guid) {
        GameObject gObj;
        List.TryGetValue(Guid.Parse(guid), out gObj);

        return gObj;
    }

    /// <summary>
    /// Get the corresponding GameObject (value) associated to the GUID (key)
    /// </summary>
    /// <param name="guid">GUID to search in the list - Key</param>
    /// <returns>Returns the corresponding value - GameObject</returns>
    public static GameObject GetGObjFromGuid(Guid guid) {
        GameObject gObj;
        List.TryGetValue(guid, out gObj);

        return gObj;
    }
}
