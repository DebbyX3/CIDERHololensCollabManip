using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
//using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using TMPro;

public class SocketServer : MonoBehaviour 
{
    private volatile bool connectionEstablished = false;
    private volatile bool keepReading = false;

    private Thread waitingForConnectionsThread;
    private Thread handleIncomingRequest;
    private Thread handleOutgoingRequest;

    private Socket listener;
    private Socket handler;

    private IPEndPoint localEndPoint;

    public GameObject cube;
    public GameObject sphere;

    // can be changed in unity inspector
    public int portToListen = 60000;    

    public TMP_Text logText;

    protected ConcurrentQueue<Message> messages = new ConcurrentQueue<Message>();

    void Awake() {
        // da togliere probabilmente, non lo uso 
        //UnityThread.initUnityThread();
    }

    protected void StartServer() 
    {
        CreateServerListener();
        BindToLocalEndPoint();

        waitingForConnectionsThread = new Thread(WaitingForConnections);
        waitingForConnectionsThread.IsBackground = true;
        waitingForConnectionsThread.Start();
    }

    private void Update() 
    {
        /*
        if (!log.Equals("")) {
            logText.text += log;
            log = "";
        }
        */
    }

    private void WaitingForConnections() {
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
                handleIncomingRequest = new Thread(() => RequestHandler());
                handleIncomingRequest.IsBackground = true;
                handleIncomingRequest.Start();

                //proviamo
                //crea thread per gestire i msg da mandare!
                /*handleOutgoingRequest = new Thread(() => SendHandler(handler));
                handleOutgoingRequest.IsBackground = true;
                handleOutgoingRequest.Start();*/
            }
        } catch (Exception e) {
            Debug.Log(e.ToString());
        }
    }

    void RequestHandler() 
    {
        byte[] bytes = new Byte[1024]; // Data buffer for incoming data.
        int bytesRec = 0;

        try {
            // An incoming connection needs to be processed.
            while (keepReading && connectionEstablished) {
                bytes = new byte[100000];
                bytesRec = handler.Receive(bytes);

                Debug.Log("Received from Client " + bytesRec + "bytes");

                if (bytesRec <= 0) {
                    keepReading = false;
                    handler.Disconnect(true);
                    Debug.Log("Disconnected because received 0 bytes");
                    break;
                }

                // put new message in the concurrent queue, to be fetched later
                Message newMsg = Message.Deserialize(bytes);
                messages.Enqueue(newMsg); 
            }

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
            keepReading = false;
            connectionEstablished = false;
        }
        catch (Exception e) {
            Debug.Log(e.ToString());
        }        
    }

    void SendHandler()
    {
        try
        {
            if (connectionEstablished)
            {
                GameObject newObj = PrefabHandler.CreateNewObject("cube", new SerializableTransform(Vector3.one, Quaternion.identity, Vector3.one));
                GameObjController controller = newObj.GetComponent<GameObjController>();
                GameObjMessage msg = new GameObjMessage(new GameObjMessageInfo(controller.Guid, newObj.transform, controller.PrefabName));

                byte[] serializedMsg = msg.Serialize();

                handler.Send(serializedMsg);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
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

    private void BindToLocalEndPoint() {
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

    private void ListenToClient() {
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


    protected void StopServer() {
        keepReading = false;

        if (handler != null && handler.Connected) {
            // there's no need of shutdown and disconnect a listener socket!
            //listener.Shutdown(SocketShutdown.Both);
            //listener.Disconnect(false);
            listener.Close();

            handler.Shutdown(SocketShutdown.Both);
            handler.Disconnect(false);
            handler.Close();

            Debug.Log("Disconnected!");
        }

        //stop thread
        if (waitingForConnectionsThread != null) {
            Debug.Log("abort");

            waitingForConnectionsThread.Abort();
            handleIncomingRequest.Abort();
        }
    }

    void OnDisable() {
        StopServer();
    }
}