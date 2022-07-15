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
    public bool connectionSucceeded = false;

    void Start() 
    {
        //apro connessione
        Debug.Log("Eseguo conn a server");
        connectionSucceeded = ConnectToServer();

        if(connectionSucceeded)
            cube.GetComponent<Renderer>().material.color = new Color(0, 204, 102);

        //if (connectionSucceeded) {
        //StartCoroutine(SendToServer());
        //SendToServer();
        //}
    }

    void SendToServer() 
    {
        //SendOnSocket(cube);
        SendObject(cube);

        Debug.Log("inviati i dati a server");

        //yield return null;
    }
    
}
