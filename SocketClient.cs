using UnityEngine;
using System.Net.Sockets;
using System;
using System.Text;
using System.Collections.Generic;

public class SocketClient: MonoBehaviour // prima derivava da monobehiavour
{
    public string ipToSend = "0.0.0.0"; //to be changed in the Unity Inspector
    public int portToSend = 60000;
    public GameObject cube;
    public GameObject sphere;

    private Socket client;
    //private static readonly SocketClient instance = new SocketClient();

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static SocketClient() {
    }

    protected SocketClient() {
    }

    /*public static SocketClient Instance {
        get {
            return instance;
        }
    }
    */

    protected bool ConnectToServer() 
    {
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //client.Connect(ipToSend, portToSend);

        if (!client.Connected) 
        {
            Debug.LogError("Connection Failed");
            return false;
        }

        return true;
    }

    public void SendObject(GameObject gameObject) 
    {
        GameObjController controller = gameObject.GetComponent<GameObjController>();

        GameObjMessage msg = new GameObjMessage(new GameObjMessageInfo(controller.Guid, gameObject.transform, controller.PrefabName));
        byte[] serializedMsg = msg.Serialize();

        client.Send(serializedMsg);
    }
}