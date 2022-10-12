using UnityEngine;
using System.Net.Sockets;
using System.Threading;

public class SocketClient: NetworkHandler
{
    public string ipToSend = "0.0.0.0"; //to be changed in the Unity Inspector
    public int portToSend = 60000;
    public GameObject cube;

    private Thread handleIncomingRequestThread;

    protected bool StartClient() 
    {
        SetInstance(Instance);

        connectionHandler = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        connectionHandler.Connect(ipToSend, portToSend);

        //create thread to handle request
        handleIncomingRequestThread = new Thread(() => NetworkHandler.Instance.Receive(connectionHandler, connectionEstablished));
        handleIncomingRequestThread.IsBackground = true;
        handleIncomingRequestThread.Start();

        //attach this object to NetworkHandler
        //NetworkHandler.Instance.SetNetworkPeer(this);

        if (!connectionHandler.Connected) {
            Debug.LogError("Connection Failed");
            UIManager.Instance.PrintMessages("Connection Failed");
            return false;
        }

        return true;
    }

    //nota: questo metodo è identico a SendObject di SocketServer, cambia solo che passo l'handler di socket diverso (giustamente)!
    //Da capire se delegarlo a network? boh? (penso di no)

    // ****************** METODO DA RIVEDERE, PROBABILMENTE DA BUTTARE!
    public void SendObject(GameObject gameObject) 
    {
        GameObjController controller = gameObject.GetComponent<GameObjController>();

        GameObjMessage msg = new GameObjMessage(new GameObjMessageInfo(controller.Guid, gameObject.transform, controller.PrefabName, CommitType.ForcedCommit));
        byte[] serializedMsg = msg.Serialize();

        NetworkHandler.Instance.Send(serializedMsg);
    }

    protected void StopClient()
    {
        if (connectionHandler != null) 
        {
            try 
            {
                connectionHandler.Shutdown(SocketShutdown.Both);
                connectionHandler.Disconnect(false);
            } 
            finally {
                connectionHandler.Close();
            }                      

            Debug.Log("Disconnected!");
            UIManager.Instance.PrintMessages("Disconnected!");
        }

        //stop thread
        if (handleIncomingRequestThread != null) {
            handleIncomingRequestThread.Abort();
        }

        Debug.Log("Abort threads");
        UIManager.Instance.PrintMessages("Abort threads");
    }

    void OnDisable() {
        StopClient();
    }
}