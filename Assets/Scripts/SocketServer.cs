using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using TMPro;

public class SocketServer : NetworkManager 
{
    // can be changed in unity inspector
    public int PortToListen = 60000;

    private Thread WaitingForConnectionsThread;
    private Thread HandleIncomingRequestThread;
    private Socket Listener;
    private IPEndPoint LocalEndPoint;

    protected void StartServer() 
    {
        CreateServerListener();
        BindToLocalEndPoint();

        WaitingForConnectionsThread = new Thread(WaitingForConnections);
        WaitingForConnectionsThread.IsBackground = true;
        WaitingForConnectionsThread.Start();

        //attach this object to NetworkManager
        //NetworkManager.Instance.SetNetworkPeer(this);
    }

    private void WaitingForConnections() 
    {
        try {
            while (true) {
                ListenToClient();

                Debug.Log("Waiting for Connection");
                UIManager.Instance.PrintMessages("Waiting for Connection");

                // Program is suspended while waiting for an incoming connection
                // (not a problem because we are in a different thread than the main one)
                ConnectionHandler = Listener.Accept();

                Debug.Log("Client Connected");
                UIManager.Instance.PrintMessages("Client connected");

                ConnectionEstablished = true;

                //create thread to handle request
                HandleIncomingRequestThread = new Thread(() => NetworkManager.Instance.Receive(ConnectionHandler, ConnectionEstablished));
                HandleIncomingRequestThread.IsBackground = true;
                HandleIncomingRequestThread.Start();
            }
        } catch (Exception e) {
            Debug.Log(e.ToString());
            UIManager.Instance.PrintMessages(e.ToString());
        }
    }

    protected void CreateServerListener() 
    {
        // IP on where the server should listen to incoming connections/requests
        IPAddress ipAddress = IPAddress.Any;
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 60000);

        Debug.Log(ipAddress);
        UIManager.Instance.PrintMessages(ipAddress.ToString());

        // Create a TCP/IP socket
        Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        this.LocalEndPoint = localEndPoint;
    }

    // not sure, but i think bind does not block
    private void BindToLocalEndPoint() 
    {
        try {
            Listener.Bind(LocalEndPoint);
        } catch (SocketException se) {
            Debug.Log("An error occurred when attempting to access the socket.\n\n" + se.ToString());
            UIManager.Instance.PrintMessages("An error occurred when attempting to access the socket.\n\n" + se.ToString());
        } catch (ObjectDisposedException ode) {
            Debug.Log("The Socket has been closed.\n\n" + ode.ToString());
            UIManager.Instance.PrintMessages("The Socket has been closed.\n\n" + ode.ToString());
        } catch (ArgumentNullException ane) {
            Debug.Log("The local endpoint is null.\n\n" + ane.ToString());
            UIManager.Instance.PrintMessages("The local endpoint is null.\n\n" + ane.ToString());
        }
    }

    // listen does NOT block!
    private void ListenToClient() 
    {
        try {
            Listener.Listen(10); //max 10 connections
        } catch (SocketException se) {
            Debug.Log("An error occurred when attempting to access the socket.\n\n" + se.ToString());
            UIManager.Instance.PrintMessages("An error occurred when attempting to access the socket.\n\n" + se.ToString());
        } catch (ObjectDisposedException ode) {
            Debug.Log("The Socket has been closed.\n\n" + ode.ToString());
            UIManager.Instance.PrintMessages("The Socket has been closed.\n\n" + ode.ToString());
        } catch (ArgumentNullException ane) {
            Debug.Log("The local endpoint is null.\n\n" + ane.ToString());
            UIManager.Instance.PrintMessages("The local endpoint is null.\n\n" + ane.ToString());
        }
    }

    protected void StopServer() 
    {
        if (ConnectionHandler != null && ConnectionHandler.Connected) {
            // there's no need of shutdown and disconnect a listener socket, unlike SocketClient
            Listener.Close();

            ConnectionHandler.Shutdown(SocketShutdown.Both);
            ConnectionHandler.Disconnect(false);
            ConnectionHandler.Close();

            Debug.Log("Disconnected!");
            UIManager.Instance.PrintMessages("Disconnected!");
        }

        //stop thread
        if (WaitingForConnectionsThread != null) {            
            WaitingForConnectionsThread.Abort();
        }
        if (HandleIncomingRequestThread != null) {
            HandleIncomingRequestThread.Abort();
        }

        Debug.Log("Abort threads");
        UIManager.Instance.PrintMessages("Abort threads");
    }

    void OnDisable() 
    {
        StopServer();
    }
}