/*using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using TMPro;

public class SocketServer : MonoBehaviour {
    System.Threading.Thread SocketThread;
    volatile bool keepReading = false;
    public GameObject cube;

    public TMP_Text logText;
    private string log = "";


    // Use this for initialization
    void Start() {
        Application.runInBackground = true;
        startServer();
    }

    void startServer() {
        SocketThread = new System.Threading.Thread(networkCode);
        SocketThread.IsBackground = true;
        SocketThread.Start();
    }



    private string getIPAddress() {
        IPHostEntry host;
        string localIP = "127.0.0.1";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList) {
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                localIP = ip.ToString();
            }

        }
        return localIP;
    }


    Socket listener;
    Socket handler;

    void networkCode() {
        string data;

        // Data buffer for incoming data.
        byte[] bytes = new Byte[1024];

        // host running the application.
        //Debug.Log("Ip " + getIPAddress().ToString());
        //IPAddress[] ipArray = Dns.GetHostAddresses(getIPAddress());
        //IPEndPoint localEndPoint = new IPEndPoint(ipArray[0], 9025);

        //IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
        IPAddress ipAddress = IPAddress.Any;
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 60000);

        Debug.Log(ipAddress);
        log += ipAddress + "\n";

        // Create a TCP/IP socket.
        listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and 
        // listen for incoming connections.

        try {
            listener.Bind(localEndPoint);
            listener.Listen(10);

            // Start listening for connections.
            while (true) {
                keepReading = true;

                // Program is suspended while waiting for an incoming connection.
                Debug.Log("Waiting for Connection");     //It works
                log += "Waiting for connection \n";

                handler = listener.Accept();
                Debug.Log("Client Connected");     //It doesn't work
                log += "Client Connected \n";

                data = null;

                // An incoming connection needs to be processed.
                while (keepReading) {
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    Debug.Log("Received from Server");
                    log += "Received from Server \n";

                    if (bytesRec <= 0) {
                        keepReading = false;
                        handler.Disconnect(true);
                        break;
                    }

                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1) {
                        break;
                    }

                    System.Threading.Thread.Sleep(1);
                }

                System.Threading.Thread.Sleep(1);
            }
        } catch (Exception e) {
            Debug.Log(e.ToString());
        }
    }

    private void Update() 
    {
        if (!log.Equals("")) 
        {
            logText.text += log;
            log = "";
        }
    }

    void stopServer() {
        keepReading = false;

        //stop thread
        if (SocketThread != null) {
            SocketThread.Abort();
        }

        if (handler != null && handler.Connected) {
            handler.Disconnect(false);
            Debug.Log("Disconnected!");
            log += "Disconnected \n";
        }
    }

    void OnDisable() {
        stopServer();
    }
}
*/

using System;
using System.Collections;
using System.Collections.Generic;
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

    // can be changed in unity inspector
    public int portToListen = 60000;

    private Socket listener;
    private Socket handler;

    private IPEndPoint localEndPoint;

    public TMP_Text logText;
    private string log = "";

    protected void StartServer() 
    {
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

        Debug.Log("Groda");
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
                bytes = new byte[1024];
                bytesRec = handler.Receive(bytes);

                Debug.Log("Received from Client");
                log += "Received from client \n";

                Debug.Log("Received bytes from client: " + bytesRec);
                Debug.Log("Message from client: " + Encoding.UTF8.GetString(bytes, 0, bytes.Length));

                if (bytesRec <= 0) {
                    keepReading = false;
                    handler.Disconnect(true);
                    Debug.Log("Disconnected while waiting for more messages");
                    break;
                }

                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                if (data.IndexOf("<EOF>") > -1) {
                    break;
                }
            }
        } catch (Exception e) {
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
