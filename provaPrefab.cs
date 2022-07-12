using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.UI;

public class provaPrefab : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        GameObject cubeone = Instantiate(Resources.Load<GameObject>("Cube"), new Vector3(-0.2f, 0.1f, 0.5f), new Quaternion(9, 14, 0, 1));
        GameObject cubetwo = Instantiate(Resources.Load<GameObject>("Cube"), new Vector3(1f, 1f, 0.5f), new Quaternion(9, 14, 0, 1));

        Debug.Log("primo " + cubeone.GetComponent<GuidForGObj>().Guid);
        Debug.Log("secondo " + cubetwo.GetComponent<GuidForGObj>().Guid);

        Debug.Log("stampa lista");

        foreach (KeyValuePair<Guid, GameObject> kvp in GUIDList.List) {
            Debug.Log(string.Format("Key = {0}, Value = {1}", kvp.Key, kvp.Value));
        }

        GameObject button1 = Instantiate(Resources.Load<GameObject>("PressableButtonHoloLens2"), new Vector3(-0.0576f, 0f, 0.1739f), Quaternion.identity);
        Interactable interactable = button1.GetComponent<Interactable>();
        interactable.OnClick.AddListener(() => cubeone.GetComponent<GameObjController>().SendGObj());


        //GUIDList.GetGObjFromGuid()

    }

    public static void groda() { 
    }
}
