using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// è un singleton? boh forse
public class CommitManager : MonoBehaviour
{
    public static CommitManager Instance { get; private set; }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnClickForcedCommit(GameObjController gObj)
    {
        CaretakerScene.Instance.ExecuteForcedCommit(gObj);
        //send commit message

        //send commit notification to this device
    }
}
