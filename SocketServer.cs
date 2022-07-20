using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using TMPro;

public class SocketServer : MonoBehaviour 
{
    protected volatile bool connectionEstablished = false;

    private volatile bool keepReading = false;

    private Thread waitingForConnectionsThread;
    private Thread handleIncomingRequestThread;

    private Socket listener;
    private Socket handler;

    private IPEndPoint localEndPoint;

    public GameObject cube;

    // can be changed in unity inspector
    public int portToListen = 60000;

    public TMP_Text logText;

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

                keepReading = true;

                Debug.Log("Waiting for Connection");

                // Program is suspended while waiting for an incoming connection
                // (not a problem because we are in a different thread than the main one)
                handler = listener.Accept();

                Debug.Log("Client Connected");

                connectionEstablished = true;

                //create thread to handle request
                handleIncomingRequestThread = new Thread(() => NetworkHandler.Receive(handler, connectionEstablished));
                handleIncomingRequestThread.IsBackground = true;
                handleIncomingRequestThread.Start();
            }
        } catch (Exception e) {
            Debug.Log(e.ToString());
        }
    }

    public void SendObject(GameObject gameObject) {
        GameObjController controller = gameObject.GetComponent<GameObjController>();

        GameObjMessage msg = new GameObjMessage(new GameObjMessageInfo(controller.Guid, gameObject.transform, controller.PrefabName));
        byte[] serializedMsg = msg.Serialize();

        NetworkHandler.Send(handler, serializedMsg, connectionEstablished);
    }

    protected void CreateServerListener() 
    {
        // IP on where the server should listen to incoming connections/requests
        IPAddress ipAddress = IPAddress.Any;
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 60000);

        Debug.Log(ipAddress);

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
        } catch (ObjectDisposedException ode) {
            Debug.Log("The Socket has been closed.\n\n" + ode.ToString());
        } catch (ArgumentNullException ane) {
            Debug.Log("The local endpoint is null.\n\n" + ane.ToString());
        }
    }

    // listen does NOT block!
    private void ListenToClient() 
    {
        try {
            listener.Listen(10); //max 10 connections
        } catch (SocketException se) {
            Debug.Log("An error occurred when attempting to access the socket.\n\n" + se.ToString());
        } catch (ObjectDisposedException ode) {
            Debug.Log("The Socket has been closed.\n\n" + ode.ToString());
        } catch (ArgumentNullException ane) {
            Debug.Log("The local endpoint is null.\n\n" + ane.ToString());
        }
    }

    protected void StopServer() 
    {
        keepReading = false;

        if (handler != null && handler.Connected) {
            // there's no need of shutdown and disconnect a listener socket
            listener.Close();

            handler.Shutdown(SocketShutdown.Both);
            handler.Disconnect(false);
            handler.Close();

            Debug.Log("Disconnected!");
        }

        //stop thread
        if (waitingForConnectionsThread != null) {
            Debug.Log("Abort threads");

            waitingForConnectionsThread.Abort();
            handleIncomingRequestThread.Abort();
        }
    }

    void OnDisable() 
    {
        StopServer();
    }
}