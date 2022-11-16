using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MeshMessage : Message 
{
    public MeshMessage(Guid id, Mesh mesh, MessageType messageType) : base(id, mesh, messageType) { }
    public MeshMessage(Mesh mesh, MessageType messageType) : base(mesh, messageType) { }
    public MeshMessage(string id, Mesh mesh, MessageType messageType) : base(id, mesh, messageType) { }

    public override void ExecuteMessage() 
    {
        //something!
    }

    public Mesh getMsgInfo() 
    {
        return (Mesh) Info;
    }
}
