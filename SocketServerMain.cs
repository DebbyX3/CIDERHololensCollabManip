using System.Collections;
using UnityEngine;

public class SocketServerMain : SocketServer {

    bool avviato = false;

    void Start() 
    {
        //open connection
        Debug.Log("Server: open connection");

        SetInstance(this);

        StartServer();
    }

    new void Update() 
    {
        if (connectionEstablished && avviato == false) {
            avviato = true;
            //StartCoroutine(Cor());
        }

        base.Update();
    }

    IEnumerator Cor() {
        while (true) {
            SendObject(cube);
            yield return new WaitForSeconds(0.5f);
        }
    }
}