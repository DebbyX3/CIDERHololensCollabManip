using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class SocketServerMain : SocketServer
{
    public bool connessioneRiuscita = false;

    // Start is called before the first frame update
    void Start() {
        //apro connessione con py
        Debug.Log("Eseguo conn " + gameObject.name);
        Application.runInBackground = true;
        StartServer();

        cube.GetComponent<Renderer>().material.color = new Color(0, 204, 200);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
