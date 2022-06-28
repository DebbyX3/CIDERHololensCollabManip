using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

using MeshMessage = System.Collections.Generic.KeyValuePair<int, UnityEngine.Mesh>;
public class SocketServerMain : SocketServer
{
    public bool connessioneRiuscita = false;

    void Start()
    {
        //open connection
        Debug.Log("Eseguo conn con " + gameObject.name);
        StartServer();
    }

    void Update()
    {
        MeshMessage item;

        if (!meshMessages.IsEmpty)
        {
            meshMessages.TryDequeue(out item);
            SimpleMeshSerializer.CreateMesh(item);

            sphere.GetComponent<MeshFilter>().mesh = item.Value;
        }
    }
}