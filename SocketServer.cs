using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class SocketServer : MonoBehaviour 
{
    private Thread waitingForConnectionsThread;
    private Thread handleConnectionThread;

    volatile bool keepReading = false;
    public GameObject cube;

    // can be changed in unity inspector
    public int portToListen = 60000;

    private Socket listener;
    private Socket handler;

    private IPEndPoint localEndPoint;

    protected void StartServer() 
    {
        CreateServerListener();
        BindToLocalEndPoint();

        waitingForConnectionsThread = new Thread(WaitingForConnections);
        waitingForConnectionsThread.IsBackground = true;
        waitingForConnectionsThread.Start();
    }

    private string GetIPAddress() 
    {
        IPHostEntry host;
        string localIP = "127.0.0.1";
        host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (IPAddress ip in host.AddressList) 
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork) 
            {
                localIP = ip.ToString();
            }
        }

        return localIP;
    }

    private void WaitingForConnections() 
    {
        try 
        {
            while (true) 
            {
                ListenToClient();

                keepReading = true;

                Debug.Log("Waiting for Connection");

                // Program is suspended while waiting for an incoming connection.
                handler = listener.Accept();

                Debug.Log("Client Connected");

                //create thread to handle request
                //SocketThread = new Thread(NetworkCode(handler));
                handleConnectionThread = new Thread(() => RequestHandler(handler));
                handleConnectionThread.IsBackground = true;
                handleConnectionThread.Start();
            }
        } 
        catch (Exception e) 
        {
            Debug.Log(e.ToString());
        }
    }

    void RequestHandler(Socket handler) 
    {  
        string data = null;        
        byte[] bytes = new Byte[1024]; // Data buffer for incoming data.

        try 
        {
            // An incoming connection needs to be processed.
            while (keepReading) {                    
                bytes = new byte[1024];
                int bytesRec = handler.Receive(bytes);
                Debug.Log("Received from Server");
                    

                if (bytesRec <= 0) {
                    keepReading = false;
                    handler.Disconnect(true);
                    break;
                }

                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                if (data.IndexOf("<EOF>") > -1) {
                    break;
                }
            }   
        } 
        catch (Exception e) 
        {
            Debug.Log(e.ToString());
        }
    }

    protected void CreateServerListener() 
    {
        // host running the application.
        Debug.Log("Ip " + GetIPAddress().ToString());
        IPAddress[] ipArray = Dns.GetHostAddresses(GetIPAddress());
        IPEndPoint localEndPoint = new IPEndPoint(ipArray[0], portToListen);

        // Create a TCP/IP socket.
        listener = new Socket(ipArray[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);

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


    protected void StopServer() 
    {
        keepReading = false;

        if (handler != null && handler.Connected) 
        {            
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
        if (waitingForConnectionsThread != null) 
        {
            Debug.Log("abort");
            waitingForConnectionsThread.Abort();
            handleConnectionThread.Abort();
        }        
    }

    void OnApplicationQuit() 
    {
        StopServer();
    }
}

