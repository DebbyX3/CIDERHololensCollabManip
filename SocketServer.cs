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

public class SocketServer : MonoBehaviour {
    private Thread waitingForConnectionsThread;
    private Thread handleConnectionThread;

    volatile bool keepReading = false;
    public GameObject cube;
    public GameObject sphere;

    // can be changed in unity inspector
    public int portToListen = 60000;

    private Socket listener;
    private Socket handler;

    private IPEndPoint localEndPoint;

    public TMP_Text logText;
    private string log = "";

    public ConcurrentQueue<Message> messages = new ConcurrentQueue<Message>();

    void Awake() {
        UnityThread.initUnityThread();
    }

    protected void StartServer() {
        CreateServerListener();
        BindToLocalEndPoint();

        waitingForConnectionsThread = new Thread(WaitingForConnections);
        waitingForConnectionsThread.IsBackground = true;
        waitingForConnectionsThread.Start();
    }

    private void Update() {
        if (!log.Equals("")) {
            logText.text += log;
            log = "";
        }
    }


    private void WaitingForConnections() {
        try {
            while (true) {
                ListenToClient();

                keepReading = true;

                Debug.Log("Waiting for Connection");
                log += "Waiting for Connection \n";

                // Program is suspended while waiting for an incoming connection.
                handler = listener.Accept();

                Debug.Log("Client Connected");
                log += "Client Connected \n";

                //create thread to handle request
                //SocketThread = new Thread(NetworkCode(handler));
                handleConnectionThread = new Thread(() => RequestHandler(handler));
                handleConnectionThread.IsBackground = true;
                handleConnectionThread.Start();
            }
        } catch (Exception e) {
            Debug.Log(e.ToString());
        }
    }

    void RequestHandler(Socket handler) {
        string data = null;
        byte[] bytes = new Byte[1024]; // Data buffer for incoming data.
        int bytesRec = 0;

        try {
            // An incoming connection needs to be processed.
            while (keepReading) {
                bytes = new byte[100000];
                bytesRec = handler.Receive(bytes);

                Debug.Log("Received from Client " + bytesRec + "bytes");

                if (bytesRec <= 0) {
                    keepReading = false;
                    handler.Disconnect(true);
                    Debug.Log("Disconnected because received 0 bytes");
                    break;
                }

                Message newMsg = Message.Deserialize(bytes);
                messages.Enqueue(newMsg);

                //UnityThread.executeInUpdate(() =>
                //{

                /// solo per prova
                /*MeshFilter viewedModelFilter = (MeshFilter)cube.GetComponent("MeshFilter");
                Mesh viewedModel = viewedModelFilter.mesh;

                byte[] groda = SimpleMeshSerializer.Serialize(viewedModel);
                ///

                deserializedMesh = SimpleMeshSerializer.Deserialize(groda);*/


                //});                

                // Echo the data back to the client.  
                //byte[] msg = Encoding.ASCII.GetBytes(data);

                //handler.Send(msg);                
            }

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
            keepReading = false;
        }
        catch (Exception e) {
            Debug.Log(e.ToString());
        }        
    }

    protected void CreateServerListener() {
        // host running the application.
        IPAddress ipAddress = IPAddress.Any;
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 60000);

        Debug.Log(ipAddress);
        log += ipAddress + "\n";

        // Create a TCP/IP socket.
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
            listener.Listen(10);
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
            log += "Disconnected! \n";
        }

        //stop thread
        if (waitingForConnectionsThread != null) {
            Debug.Log("abort");
            log += "abort \n";

            waitingForConnectionsThread.Abort();
            handleConnectionThread.Abort();
        }
    }

    void OnDisable() {
        StopServer();
    }
}