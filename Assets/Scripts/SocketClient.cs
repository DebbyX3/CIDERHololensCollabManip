using UnityEngine;
using System.Net.Sockets;
using System.Threading;

public class SocketClient: NetworkManager
{
    public string IpToSend = "0.0.0.0"; //to be changed in the Unity Inspector
    public int PortToSend = 60000;

    private Thread HandleIncomingRequestThread;

    protected bool StartClient() 
    {
        SetInstance(Instance);

        ConnectionHandler = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ConnectionHandler.Connect(IpToSend, PortToSend);

        //create thread to handle request
        HandleIncomingRequestThread = new Thread(() => NetworkManager.Instance.Receive(ConnectionHandler, ConnectionEstablished));
        HandleIncomingRequestThread.IsBackground = true;
        HandleIncomingRequestThread.Start();

        //attach this object to NetworkManager
        //NetworkManager.Instance.SetNetworkPeer(this);

        if (!ConnectionHandler.Connected) {
            Debug.LogError("Connection Failed");
            UIManager.Instance.PrintMessages("Connection Failed");
            return false;
        }

        return true;
    }

    protected void StopClient()
    {
        if (ConnectionHandler != null) 
        {
            try 
            {
                ConnectionHandler.Shutdown(SocketShutdown.Both);
                ConnectionHandler.Disconnect(false);
            } 
            finally {
                ConnectionHandler.Close();
            }                      

            Debug.Log("Disconnected!");
            UIManager.Instance.PrintMessages("Disconnected!");
        }

        //stop thread
        if (HandleIncomingRequestThread != null) {
            HandleIncomingRequestThread.Abort();
        }

        Debug.Log("Abort threads");
        UIManager.Instance.PrintMessages("Abort threads");
    }

    void OnDisable() {
        StopClient();
    }
}