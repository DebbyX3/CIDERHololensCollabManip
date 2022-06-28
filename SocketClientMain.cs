using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft;
using UnityEngine.UI;

public class SocketClientMain : SocketClient 
{
    public float[] array = new float[4];
    public bool connectionSucceeded = false;
    public GameObject cube;

    public float[] hand_right = new float[60];

    void Start() 
    {
        //apro connessione
        Debug.Log("Eseguo conn a server " + gameObject.name);
        connectionSucceeded = ConnectToServer();

        if(connectionSucceeded)
            cube.GetComponent<Renderer>().material.color = new Color(0, 204, 102);
    }

    // Update is called once per frame
    void Update() 
    {
        //ogni tot chiamo server request 
        if (connectionSucceeded) {
            StartCoroutine(SendToServer());
        }
    }

    IEnumerator SendToServer() 
    {
        float[] prediction = SendToPython(hand_right);
        int a = (int)prediction[0];

        Debug.Log("inviati i dati a server " + gameObject.name);

        yield return null;
    }
    
}
