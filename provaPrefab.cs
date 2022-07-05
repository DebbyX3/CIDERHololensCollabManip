using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Microsoft.MixedReality.Toolkit.UI;

public class provaPrefab : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject cube = Instantiate(Resources.Load<GameObject>("Cube"), new Vector3(-0.2f, 0.1f, 0.5f), new Quaternion(9, 14, 0, 1));
        GameObject cubetwo = Instantiate(Resources.Load<GameObject>("Cube"), new Vector3(1f, 1f, 0.5f), new Quaternion(9, 14, 0, 1));

        Debug.Log("primo "+ cube.GetComponent<GuidForGObj>().Guid);
        Debug.Log("secondo "+ cubetwo.GetComponent<GuidForGObj>().Guid);

        Debug.Log("stampa lista");

        foreach (KeyValuePair<Guid, GameObject> kvp in GUIDList.List) {
            Debug.Log(string.Format("Key = {0}, Value = {1}", kvp.Key, kvp.Value));
        }



        //GUIDList.GetGObjFromGuid()

    }




    // Update is called once per frame
    void Update()
    {
        
    }
}
