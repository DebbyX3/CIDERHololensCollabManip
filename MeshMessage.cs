using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshMessage : Message 
{
    public MeshMessage(int id, Mesh mesh) : base(id, mesh) { }

    public override void ExecuteMessage() 
    {
        //something!
    }

    public Mesh getObj() {
        return (Mesh) message.Value;
    }

    public int getId() {
        return message.Key;
    }
}
