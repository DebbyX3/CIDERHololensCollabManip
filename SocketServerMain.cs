using System.Collections;
using UnityEngine;

public class SocketServerMain : SocketServer {

    bool avviato = false;

    void Start() 
    {
        //open connection
        Debug.Log("Server: open connection");
        StartServer();
    }

    void Update() 
    {
        if (connectionEstablished && avviato == false) {
            avviato = true;
            //StartCoroutine(Cor());
        }
    }

    IEnumerator Cor() {
        while (true) {
            SendObject(cube);
            yield return new WaitForSeconds(0.5f);
        }
    }
}