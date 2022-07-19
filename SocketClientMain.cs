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
    public bool connectionEstablished = false;

    void Start() 
    {
        //apro connessione
        Debug.Log("Eseguo conn a server");
        connectionEstablished = ConnectToServer();            

        if (connectionEstablished) 
        {


            cube.GetComponent<Renderer>().material.color = new Color(0, 204, 102);
            SendToServer();
            StartCoroutine(cor());
        }        
    }

    IEnumerator cor() {
        while (true) {
            SendObject(cube);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
