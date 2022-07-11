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
    private static readonly SocketClient instance = new SocketClient();

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static SocketClient() {
    }

    protected SocketClient() {
    }

    public static SocketClient Instance {
        get {
            return instance;
        }
    }

    protected bool ConnectToServer() 
    {
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        client.Connect(ipToSend, portToSend);

        if (!client.Connected) 
        {
            Debug.LogError("Connection Failed");
            return false;
        }

        return true;
    }

    protected void SendOnSocket(GameObject gObj) 
    {
        //if (!client.Connected) 
        //{
            //Debug.LogError("Connection Failed");
        //}        


        byte[] datatrans = TransformSerializer.Serialize((Transform)cube.GetComponent("Transform"));
        client.Send(datatrans);

        Debug.Log(datatrans);
    }

    public void SendNewObject(GameObject gameObject) {
        
        GameObjMessage msg = new GameObjMessage(new MessageInfo(gameObject.GetComponent<GuidForGObj>().Guid, gameObject.transform));
        byte[] groda = msg.Serialize();

        //client.Send(groda);

        GameObjMessage newMsg = GameObjMessage.Deserialize(groda);
        Debug.Log("guid " + newMsg.getObj().GameObjectGuid);
        Debug.Log("pos " + newMsg.getObj().Transform.Position);
        Debug.Log("rot " + newMsg.getObj().Transform.Rotation);
        Debug.Log("scale " + newMsg.getObj().Transform.Scale);

        //todo: sistemare il fatto che faccio due send
        // ne voglio fare solo una! posso usare il formato MESSAGE già usato per la coda di msg, così se invio un msg lo posso subito mettere in coda senza troppi parsig o problemi alla ricezione
    }
}