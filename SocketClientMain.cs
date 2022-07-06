using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft;
using UnityEngine.UI;
using System.Xml.Serialization;
using System.IO;

public class SocketClientMain : SocketClient 
{
    public float[] array = new float[4];
    public bool connectionSucceeded = false;
    

    public float[] hand_right = new float[3];

    void Start() 
    {
        //apro connessione
        Debug.Log("Eseguo conn a server " + gameObject.name);
        //connectionSucceeded = ConnectToServer();

        if(connectionSucceeded)
            cube.GetComponent<Renderer>().material.color = new Color(0, 204, 102);

        //if (connectionSucceeded) {
            StartCoroutine(SendToServer());
        //}
    }

    IEnumerator SendToServer() 
    {
        SendOnSocket(cube);

        Debug.Log("inviati i dati a server " + gameObject.name);

        yield return null;
    }
    
}
