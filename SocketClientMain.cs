using System.Collections;
using UnityEngine;

public class SocketClientMain : SocketClient 
{
    void Start() 
    {
        //open connection
        Debug.Log("Client: connect to server");
        connectionEstablished = StartClient();            

        if (connectionEstablished) 
        {
            StartCoroutine(Cor());
        }        
    }

    IEnumerator Cor() {
        while (true) {
            SendObject(cube);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
