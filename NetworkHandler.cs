using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;
using TMPro;
using UnityEngine;

//per inviare un roba con send, indipendentemente che sia un client o un server, potrei accendere un thread che
//sta sempre in ascolto per entrambi e poi usare gli stessi metodi delle stesse classi (che accomuno?) per inviare il messaggio,
//passsando a questa funzione in comune l'HANDLER, DOVE SE SONO CLIENT METTO L'HANDLER CLIENT, SE SONO SERVER QUELLO SERVER
//e poi l'oggetto da inviare tipo no? magari già serializzato, così il metodo COMUNE deve solo inviare e non fare altri casini.
//prima di inviare però controllo che la connessione sia già stabilita, con una var della classe che mi tengo magari, così non devo fare altro

/* 
 * non è complicato! una volta creato l'handler di classe Socket, basta passarlo sia per fare SEND che per fare RECEIVE sullo stesso!!! quindi il client non deve fare nulla di strano,
 * solo mandare e ricevere sullo stesso socket. idem il server, ma qua è + avvantaggiato perchè entrambi sono fatti praticamente.
 * Nota! il client per accettare le incoming request, così come il server, deve creare un thread apposito!!!! aiutox
 * 
 * funziona? incredible
 * */


public class NetworkHandler : MonoBehaviour
{
    public static NetworkHandler Instance { get; private set; }
    public static ConcurrentQueue<Message> messages = new ConcurrentQueue<Message>();

    // Socket to send/receive - inherited by subclasses
    protected Socket connectionHandler;
    protected volatile bool connectionEstablished = false;

    private void Awake() {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }

    }

    void Update()         
    {        
        if (!messages.IsEmpty) 
        {
            messages.TryDequeue(out Message item);
            item.ExecuteMessage();

            /*
            if ((item is GameObjMessage message) && message.MessageType.Equals(MessageType.GameObjMessage)) {
                Debug.Log("guid " + message.getMsgInfo().GameObjectGuid);
                Debug.Log("pos " + message.getMsgInfo().Transform.Position.ToString());
                Debug.Log("rot " + message.getMsgInfo().Transform.Rotation.ToString());
                Debug.Log("scale " + message.getMsgInfo().Transform.Scale.ToString());

                //message.ExecuteMessage();
            }
            */
        }
    }

    public void Send(byte[] message) 
    {
        if (connectionEstablished)
        {
            try
            {
                connectionHandler.Send(message);
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
        bool keepReading = true; // not sure, in SocketServer era attributo della classe, non so bene come mai, da rivedere!
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
                    messages.Enqueue(newMsg);
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
            }
        }
        /*
        handler.Shutdown(SocketShutdown.Both);
        handler.Close();

        keepReading = false;
        connectionEstablished = false;*/
    }
}
