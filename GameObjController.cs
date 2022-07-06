using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameObjController : MonoBehaviour
{
    public void SendGObj() 
    {
        Guid guid = gameObject.GetComponent<GuidForGObj>().Guid;

        //TODO: groda
        //controlla che il client sia connesso prima tipo
        SocketClient.Instance.SendNewObject(guid, gameObject.transform)
    }
}
