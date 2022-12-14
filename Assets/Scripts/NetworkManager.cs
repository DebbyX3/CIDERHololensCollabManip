using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;
using UnityEngine;

public class NetworkManager : MonoBehaviour 
{
    public static NetworkManager Instance { get; private set; }
    public static ConcurrentQueue<Message> Messages = new ConcurrentQueue<Message>();

    // Socket to send/receive - inherited by subclasses
    protected Socket ConnectionHandler;
    protected volatile bool ConnectionEstablished = false; // volatile: field might be changed by multiple threads

    protected void SetInstance(NetworkManager instance)
    {
        Instance = instance;
    }

    protected void Update()         
    {
        if (!Messages.IsEmpty) 
        {
            Messages.TryDequeue(out Message item);
            item.ExecuteMessage();
        }
    }

    public void Send(byte[] message) 
    {
        if (ConnectionEstablished)
        {
            try
            {
                ConnectionHandler.Send(message);
            }
            catch (SocketException se)
            {
                Debug.Log("An error occurred when attempting to access the socket.\n\n" + se.ToString());
                UIManager.Instance.PrintMessages("An error occurred when attempting to access the socket.\n\n" + se.ToString());
            }
            catch (ObjectDisposedException ode)
            {
                Debug.Log("The Socket has been closed.\n\n" + ode.ToString());
                UIManager.Instance.PrintMessages("The Socket has been closed.\n\n" + ode.ToString());
            }
            catch (ArgumentNullException ane)
            {
                Debug.Log("Data to be sent is null.\n\n" + ane.ToString());
                UIManager.Instance.PrintMessages("Data to be sent is null.\n\n" + ane.ToString());
            }
        }
        else
        {
            Debug.Log("Connection is not established yet");
            UIManager.Instance.PrintMessages("Connection is not established yet");
        }
    }

    public void Receive(Socket handler, bool connectionEstablished) 
    {
        bool keepReading = true; // todo not sure, in SocketServer era attributo della classe, non so bene come mai, da rivedere!
        byte[] bytes; // Data buffer for incoming data.
        int bytesRec = 0;

        // An incoming connection needs to be processed.
        while (keepReading && connectionEstablished) 
        {
            try 
            {
                bytes = new byte[100000];
                bytesRec = handler.Receive(bytes);

                Debug.Log("Received " + bytesRec + "bytes");
                UIManager.Instance.PrintMessages("Received " + bytesRec + "bytes");

                if (bytesRec <= 0) 
                {
                    keepReading = false;
                    handler.Disconnect(true);

                    Debug.Log("Disconnected because 0 bytes were received");
                    UIManager.Instance.PrintMessages("Disconnected because 0 bytes were received");
                    break;
                }

                // put new message in the concurrent queue, to be fetched later
                try {
                    Message newMsg = Message.Deserialize(bytes);
                    Messages.Enqueue(newMsg);
                } 
                catch (SerializationException sere) {
                    Debug.Log("The input stream is not a valid binary format.\n\n" + sere.ToString());
                    UIManager.Instance.PrintMessages("The input stream is not a valid binary format.\n\n" + sere.ToString());

                    //DO NOT assign false to keepReading, because we WANT to keep reading here!
                }
            } 
            catch (SocketException se) {
                Debug.Log("An error occurred when attempting to access the socket.\n\n" + se.ToString());
                UIManager.Instance.PrintMessages("An error occurred when attempting to access the socket.\n\n" + se.ToString());

                keepReading = false;
            } 
            catch (ObjectDisposedException ode) {
                Debug.Log("The Socket has been closed.\n\n" + ode.ToString());
                UIManager.Instance.PrintMessages("The Socket has been closed.\n\n" + ode.ToString());

                keepReading = false;
            } 
            catch (SecurityException see) {
                Debug.Log("A caller in the call stack does not have the required permissions.\n\n" + see.ToString());
                UIManager.Instance.PrintMessages("A caller in the call stack does not have the required permissions.\n\n" + see.ToString());

                keepReading = false;
            } 
            catch (ThreadAbortException tae) {
                Debug.Log("Receiving thread aborted.\n\n" + tae.ToString());
                UIManager.Instance.PrintMessages("Receiving thread aborted.\n\n" + tae.ToString());

                keepReading = false;
            }            
            catch (Exception e) {
                Debug.Log(e.ToString());
                UIManager.Instance.PrintMessages(e.ToString());

                keepReading = false;
            }
        }
        /*
        handler.Shutdown(SocketShutdown.Both);
        handler.Close();

        keepReading = false;
        connectionEstablished = false;*/
    }
}
