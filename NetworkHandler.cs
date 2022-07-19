using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
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
 * */


public class NetworkHandler : MonoBehaviour
{
    public static ConcurrentQueue<Message> messages = new ConcurrentQueue<Message>();

    void Update()         
    { 
        Message item;

        if (!messages.IsEmpty) 
        {
            messages.TryDequeue(out item);
            item.ExecuteMessage();

            if ((item is GameObjMessage message) && message.MessageType.Equals(MessageType.GameObjMessage)) {
                Debug.Log("guid " + message.getMsgInfo().GameObjectGuid);
                Debug.Log("pos " + message.getMsgInfo().Transform.Position.ToString());
                Debug.Log("rot " + message.getMsgInfo().Transform.Rotation.ToString());
                Debug.Log("scale " + message.getMsgInfo().Transform.Scale.ToString());

                //message.ExecuteMessage();
            }
        }
    }

    // non sono sicura se passare lo stato della connessione, magari è un attributo che si tiene questa classe? non so
    // sicuramente, voglio passare l'handler!
    public static void Send(Socket handler, byte[] message, bool connectionEstablished) 
    {
        if (connectionEstablished) {
            try {
                handler.Send(message);
            } 
            catch (SocketException se) {
                Debug.Log("An error occurred when attempting to access the socket.\n\n" + se.ToString());
            } 
            catch (ObjectDisposedException ode) {
                Debug.Log("The Socket has been closed.\n\n" + ode.ToString());
            } 
            catch (ArgumentNullException ane) {
                Debug.Log("Data to be sent is null.\n\n" + ane.ToString());
            }
        }
    }

    //receive per ora è esattamente il metodo RequestHandler di SocketServer.
    public static void Receive(Socket handler, bool connectionEstablished) 
    {
        bool keepReading = true; // not sure, in SocketServer era attributo della classe, non so bene come mai, da rivedere!
        byte[] bytes = new Byte[1024]; // Data buffer for incoming data.
        int bytesRec = 0;

        try {
            // An incoming connection needs to be processed.
            while (keepReading && connectionEstablished) 
            {
                bytes = new byte[100000];
                bytesRec = handler.Receive(bytes);

                Debug.Log("Received" + bytesRec + "bytes");

                if (bytesRec <= 0) 
                {
                    keepReading = false;
                    handler.Disconnect(true);
                    Debug.Log("Disconnected because 0 bytes were received");
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
        catch (Exception e) 
        {
            Debug.Log(e.ToString());
        }
    }
}
