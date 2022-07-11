using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MeshMessage : Message 
{
    public MeshMessage(Guid id, Mesh mesh) : base(id, mesh) { }
    public MeshMessage(Mesh mesh) : base(mesh) { }
    public MeshMessage(String id, Mesh mesh) : base(id, mesh) { }

    public override void ExecuteMessage() 
    {
        //something!
    }

    public new Mesh getObj() 
    {
        return (Mesh) message.Value;
    }
}
