using System.Collections;
using UnityEngine;

public class SocketClientMain : SocketClient 
{
    void Start() 
    {
        //open connection
        Debug.Log("Client: connect to server");
        UIManager.Instance.PrintMessages("Client: connect to server");

        SetInstance(this);
        Debug.Log("istanza settata");

        connectionEstablished = StartClient();            

        if (connectionEstablished) 
        {
            //StartCoroutine(Cor());
        }        
    }

    new void Update()
    {
        base.Update();
    }

    IEnumerator Cor() {
        while (true) {
            //SendObject(cube);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
