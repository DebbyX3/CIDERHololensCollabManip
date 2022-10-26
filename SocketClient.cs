using UnityEngine;
using System.Net.Sockets;
using System.Threading;

public class SocketClient: NetworkHandler
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
        HandleIncomingRequestThread = new Thread(() => NetworkHandler.Instance.Receive(ConnectionHandler, ConnectionEstablished));
        HandleIncomingRequestThread.IsBackground = true;
        HandleIncomingRequestThread.Start();

        //attach this object to NetworkHandler
        //NetworkHandler.Instance.SetNetworkPeer(this);

        if (!ConnectionHandler.Connected) {
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