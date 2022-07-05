using UnityEngine;
using System.Net.Sockets;
using System;
using System.Text;
using System.Collections.Generic;


public class SocketClient : MonoBehaviour 
{
    public string ipToSend = "0.0.0.0"; //to be changed in the Unity Inspector
    public int portToSend = 60000;
    public GameObject cube;
    public GameObject sphere;

    private Socket client;

    //private MeshSerializer meshSerializer;

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

        MeshFilter viewedModelFilter = (MeshFilter)cube.GetComponent("MeshFilter");
        Mesh viewedModel = viewedModelFilter.mesh;

        byte[] data = SimpleMeshSerializer.Serialize(viewedModel);

        //client.Send(data);

        byte[] datatrans = TransformSerializer.Serialize((Transform)cube.GetComponent("Transform"));

        Debug.Log(datatrans);
    }

    /// <summary> 
    /// Send data to port, receive data from port.
    /// </summary>
    /// <param name="dataOut">Data to send</param>
    /// <returns></returns>
    /*
    private float[] SendAndReceive(float[] dataOut) {
        //initialize socket
        float[] floatsReceived;
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(ipToSend, portToSend);
        if (!client.Connected) {
            Debug.LogError("Connection Failed");
            return null;
        }

        //convert floats to bytes, send to port
        var byteArray = new byte[dataOut.Length * 4];
        Buffer.BlockCopy(dataOut, 0, byteArray, 0, byteArray.Length);
        client.Send(byteArray);

        //allocate and receive bytes
        byte[] bytes = new byte[4000];
        int idxUsedBytes = client.Receive(bytes);
        //print(idxUsedBytes + " new bytes received.");

        //convert bytes to floats
        floatsReceived = new float[idxUsedBytes / 4];
        Buffer.BlockCopy(bytes, 0, floatsReceived, 0, idxUsedBytes);

        client.Close();
        return floatsReceived;
    }
    */
}