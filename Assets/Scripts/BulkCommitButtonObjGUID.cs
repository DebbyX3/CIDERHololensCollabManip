using System;
using System.Collections.Generic;
using UnityEngine;


// This class is very dumb: it just stores the Object GUID of the button that triggers the bulk commit
// I need this class because when I select multiple commit to accept/delete,
// I need to know what object is connected to the toggle button when the event is fired.

public class BulkCommitButtonObjGUID : MonoBehaviour
{
    public Guid ObjGuid { get; private set; } = new Guid();

    public void SetObjGuid(Guid guid)
    {
        ObjGuid = guid;
    }
}
