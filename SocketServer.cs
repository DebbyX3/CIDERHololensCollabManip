using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using TMPro;

public class SocketServer : MonoBehaviour 
{
    protected volatile bool connectionEstablished = false;

    private Thread waitingForConnectionsThread;
    private Thread handleIncomingRequestThread;

    private Socket listener;
    private Socket handler;

    private IPEndPoint localEndPoint;

    public GameObject cube;

    // can be changed in unity inspector
    public int portToListen = 60000;

    protected void StartServer() 
    {
        CreateServerListener();
        BindToLocalEndPoint();

        waitingForConnectionsThread = new Thread(WaitingForConnections);
        waitingForConnectionsThread.IsBackground = true;
        waitingForConnectionsThread.Start();
    }

    private void WaitingForConnections() 
    {
        try {
            while (true) {
                ListenToClient();

                Debug.Log("Waiting for Connection");
                NetworkHandler.PrintMessages("Waiting for Connection");

                // Program is suspended while waiting for an incoming connection
                // (not a problem because we are in a different thread than the main one)
                handler = listener.Accept();

                Debug.Log("Client Connected");
                NetworkHandler.PrintMessages("Client connected");

                connectionEstablished = true;

                //create thread to handle request
                handleIncomingRequestThread = new Thread(() => NetworkHandler.Receive(handler, connectionEstablished));
                handleIncomingRequestThread.IsBackground = true;
                handleIncomingRequestThread.Start();
            }
        } catch (Exception e) {
            Debug.Log(e.ToString());
            NetworkHandler.PrintMessages(e.ToString());
        }
    }

    //nota: questo metodo è identico a SendObject di SocketClient, cambia solo che passo l'handler di socket diverso (giustamente)!
    //Da capire se delegarlo a network? boh? (penso di no)
    public void SendObject(GameObject gameObject) 
    {
        GameObjController controller = gameObject.GetComponent<GameObjController>();

        GameObjMessage msg = new GameObjMessage(new GameObjMessageInfo(controller.Guid, gameObject.transform, controller.PrefabName, CommitType.ForcedCommit));
        byte[] serializedMsg = msg.Serialize();

        NetworkHandler.Send(handler, serializedMsg, connectionEstablished);
    }

    protected void CreateServerListener() 
    {
        // IP on where the server should listen to incoming connections/requests
        IPAddress ipAddress = IPAddress.Any;
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 60000);

        Debug.Log(ipAddress);
        NetworkHandler.PrintMessages(ipAddress.ToString());

        // Create a TCP/IP socket
        listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        this.localEndPoint = localEndPoint;
    }

    // not sure, but i think bind does not block
    private void BindToLocalEndPoint() 
    {
        try {
            listener.Bind(localEndPoint);
        } catch (SocketException se) {
            Debug.Log("An error occurred when attempting to access the socket.\n\n" + se.ToString());
            NetworkHandler.PrintMessages("An error occurred when attempting to access the socket.\n\n" + se.ToString());
        } catch (ObjectDisposedException ode) {
            Debug.Log("The Socket has been closed.\n\n" + ode.ToString());
            NetworkHandler.PrintMessages("The Socket has been closed.\n\n" + ode.ToString());
        } catch (ArgumentNullException ane) {
            Debug.Log("The local endpoint is null.\n\n" + ane.ToString());
            NetworkHandler.PrintMessages("The local endpoint is null.\n\n" + ane.ToString());
        }
    }

    // listen does NOT block!
    private void ListenToClient() 
    {
        try {
            listener.Listen(10); //max 10 connections
        } catch (SocketException se) {
            Debug.Log("An error occurred when attempting to access the socket.\n\n" + se.ToString());
            NetworkHandler.PrintMessages("An error occurred when attempting to access the socket.\n\n" + se.ToString());
        } catch (ObjectDisposedException ode) {
            Debug.Log("The Socket has been closed.\n\n" + ode.ToString());
            NetworkHandler.PrintMessages("The Socket has been closed.\n\n" + ode.ToString());
        } catch (ArgumentNullException ane) {
            Debug.Log("The local endpoint is null.\n\n" + ane.ToString());
            NetworkHandler.PrintMessages("The local endpoint is null.\n\n" + ane.ToString());
        }
    }

    protected void StopServer() 
    {
        if (handler != null && handler.Connected) {
            // there's no need of shutdown and disconnect a listener socket
            listener.Close();

            handler.Shutdown(SocketShutdown.Both);
            handler.Disconnect(false);
            handler.Close();

            Debug.Log("Disconnected!");
            NetworkHandler.PrintMessages("Disconnected!");
        }

        //stop thread
        if (waitingForConnectionsThread != null) {            
            waitingForConnectionsThread.Abort();
        }
        if (handleIncomingRequestThread != null) {
            handleIncomingRequestThread.Abort();
        }

        Debug.Log("Abort threads");
        NetworkHandler.PrintMessages("Abort threads");
    }

    void OnDisable() 
    {
        StopServer();
    }
}