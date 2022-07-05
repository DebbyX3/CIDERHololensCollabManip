using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class SocketServerMain : SocketServer {
    public bool connessioneRiuscita = false;

    void Start() {
        //open connection
        Debug.Log("Eseguo conn con " + gameObject.name);
        StartServer();
    }

    void Update() {
        
        Message item;

        if (!messages.IsEmpty) {

            messages.TryDequeue(out item);

            if (item is MeshMessage meshMessage) 
            {
                sphere.GetComponent<MeshFilter>().mesh = meshMessage.getObj();
            }

            
        }
    }
}