using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class SocketServerMain : SocketServer {

    void Start() {
        //open connection
        Debug.Log("Eseguo conn con " + gameObject.name);
        StartServer();
    }

    void Update() {
        
        Message item;

        if (!messages.IsEmpty) 
        {
            messages.TryDequeue(out item);
            item.ExecuteMessage();

            if ((item is GameObjMessage message) && message.MessageType.Equals(MessageType.GameObjMessage))
            {
                Debug.Log("guid " + message.getMsgInfo().GameObjectGuid);
                Debug.Log("pos " + message.getMsgInfo().Transform.Position.ToString());
                Debug.Log("rot " + message.getMsgInfo().Transform.Rotation.ToString());
                Debug.Log("scale " + message.getMsgInfo().Transform.Scale.ToString());

                //message.ExecuteMessage();
            }
        }
    }
}