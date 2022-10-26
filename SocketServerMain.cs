using System.Collections;
using UnityEngine;

public class SocketServerMain : SocketServer {

    bool Running = false;

    void Start() 
    {
        //open connection
        Debug.Log("Server: open connection");

        SetInstance(this);

        StartServer();
    }

    new void Update() 
    {
        if (ConnectionEstablished && Running == false) {
            Running = true;
            //StartCoroutine(Cor());
        }

        base.Update();
    }

    /*
    IEnumerator Cor() {
        while (true) {
            SendObject(cube);
            yield return new WaitForSeconds(0.5f);
        }
    }*/
}