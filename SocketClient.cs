using UnityEngine;
using System.Net.Sockets;
using System;
using System.Text;
using System.Collections.Generic;

public class SocketClient // prima derivada da monobehiavour
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

    private SocketClient() {
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

    public void SendNewObject(Guid guid, Transform transform) 
    {
        byte[] guidByte = Encoding.UTF8.GetBytes(guid.ToString());
        byte[] transformByte = TransformSerializer.Serialize(transform);

        client.Send(guidByte);
        client.Send(transformByte);

        //todo: sistemare il fatto che faccio due send
        // ne voglio fare solo una! posso usare il formato MESSAGE già usato per la coda di msg, così se invio un msg lo posso subito mettere in coda senza troppi parsig o problemi alla ricezione
    }
}