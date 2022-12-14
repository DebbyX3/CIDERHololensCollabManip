using System.Collections;
using UnityEngine;
using System.Net;

public class SocketServerMain : SocketServer {

    bool Running = false;

    void Start() 
    {
        //open connection
        Debug.Log("Open connection. Listening on IP: " + IPAddress.Any + " Port: " + PortToListen);
        UIManager.Instance.PrintMessages("Open connection. Listening on IP: " + IPAddress.Any + " Port: " + PortToListen);

        SetInstance(this);

        StartServer();
    }

    new void Update() 
    {
        if (ConnectionEstablished && Running == false) {
            Running = true;
        }

        base.Update();
    }
}