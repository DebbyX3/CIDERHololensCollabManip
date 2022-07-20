using UnityEngine;

public class SocketServerMain : SocketServer 
{
    void Start() 
    {
        //open connection
        Debug.Log("Server: open connection");
        StartServer();

        SendObject(cube);
    }



    void Update() 
    {
        //ora tutto questo lo fa NetworkHandler!
        /*
        if (!NetworkHandler.messages.IsEmpty) {
            NetworkHandler.messages.TryDequeue(out Message item);
            item.ExecuteMessage();

            if ((item is GameObjMessage message) && message.MessageType.Equals(MessageType.GameObjMessage)) {
                Debug.Log("guid " + message.getMsgInfo().GameObjectGuid);
                Debug.Log("pos " + message.getMsgInfo().Transform.Position.ToString());
                Debug.Log("rot " + message.getMsgInfo().Transform.Rotation.ToString());
                Debug.Log("scale " + message.getMsgInfo().Transform.Scale.ToString());

                //message.ExecuteMessage();
            }
        }
        */
    }
}