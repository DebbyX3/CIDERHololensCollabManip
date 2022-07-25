using UnityEngine;
using System.Net.Sockets;
using System.Threading;

public class SocketClient: MonoBehaviour
{
    public string ipToSend = "0.0.0.0"; //to be changed in the Unity Inspector
    public int portToSend = 60000;
    public GameObject cube;

    private Socket client;
    private Thread handleIncomingRequestThread;

    protected volatile bool connectionEstablished = false;

    protected bool StartClient() 
    {
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(ipToSend, portToSend);

        //create thread to handle request
        handleIncomingRequestThread = new Thread(() => NetworkHandler.Receive(client, connectionEstablished));
        handleIncomingRequestThread.IsBackground = true;
        handleIncomingRequestThread.Start();

        if (!client.Connected) {
            Debug.LogError("Connection Failed");
            NetworkHandler.PrintMessages("Connection Failed");
            return false;
        }

        return true;
    }

    public void SendObject(GameObject gameObject) 
    {
        GameObjController controller = gameObject.GetComponent<GameObjController>();

        GameObjMessage msg = new GameObjMessage(new GameObjMessageInfo(controller.Guid, gameObject.transform, controller.PrefabName));
        byte[] serializedMsg = msg.Serialize();

        NetworkHandler.Send(client, serializedMsg, connectionEstablished);
    }

    protected void StopClient()
    {
        if (client != null) 
        {
            try 
            {
                client.Shutdown(SocketShutdown.Both);
                client.Disconnect(false);
            } 
            finally {
                client.Close();
            }                      

            Debug.Log("Disconnected!");
            NetworkHandler.PrintMessages("Disconnected!");
        }

        //stop thread
        if (handleIncomingRequestThread != null) {
            handleIncomingRequestThread.Abort();
        }

        Debug.Log("Abort threads");
        NetworkHandler.PrintMessages("Abort threads");
    }

    void OnDisable() {
        StopClient();
    }
}