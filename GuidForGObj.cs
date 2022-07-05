using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GuidForGObj : MonoBehaviour
{
    public Guid Guid { get; private set; }

    private void Awake() 
    {
        // Generate new guid
        Guid = Guid.NewGuid();

        // Add guid and attached gameobject in list
        GUIDList.AddToList(Guid, gameObject);
    }

    public string getGuidString() 
    {
        return Guid.ToString();
    }
}
