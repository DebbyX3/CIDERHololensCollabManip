using System.Collections;
using UnityEngine;

public class SocketClientMain : SocketClient 
{
    void Start() 
    {
        //open connection
        Debug.Log("Searching for a server having IP: " + IpToSend + " Port: " + PortToSend);
        UIManager.Instance.PrintMessages("Searching for a server having IP: " + IpToSend + " Port: " + PortToSend);

        SetInstance(this);

        ConnectionEstablished = StartClient();            

        if (ConnectionEstablished) 
        {
            Debug.Log("Connection to server established");
            UIManager.Instance.PrintMessages("Connection to server established");
        }        
    }

    new void Update()
    {
        base.Update();
    }
}
