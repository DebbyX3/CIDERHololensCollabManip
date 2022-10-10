using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommitManager : MonoBehaviour
{
    public static CommitManager Instance { get; private set; }

    // i metodi oncommitreceived e oncommitsent sono quelli di 'base' che probabilmente
    // poi forced commit e voting commit useranno? pu� darsi? boh

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

    public void ForcedCommit(GameObjController gObjCont)
    {
        CaretakerScene.Instance.ExecuteForcedCommit(gObjCont);

        // Create message to send (serialize it)
        GameObjMessage msg = new GameObjMessage(new GameObjMessageInfo(gObjCont.Guid, gObjCont.Transform, gObjCont.PrefabName, CommitType.ForcedCommit));
        byte[] serializedMsg = msg.Serialize();

        // Send message
        NetworkHandler.Instance.Send(serializedMsg);

        // Play sound on commit
        UIManager.Instance.GetComponent<AudioSource>().Play();

        //send commit notification to this device
        UIManager.Instance.SetNotificationButtonActive();
    }

    public void OnCommitSent(GameObjController gObjCont)
    { 

    }

    // forse questo metodo � da spostare in una classe pi� appropriata?
    public void OnCommitReceived(GameObjMessage gObjMsg)
    {
        GameObjMessageInfo gObjMsgInfo = gObjMsg.GetMsgInfo();

        // If the receiver already has the object in one or both scenes
        if (GUIDKeeper.ContainsGuid(gObjMsgInfo.GameObjectGuid))
        {
            // To fix probably
            // cio� che potrebbe avere l'oggetto in locale e non globale? ma pu� capitare o no? Da capire?
            PrefabHandler.Instance.UpdateObjectGlobal(gObjMsgInfo.GameObjectGuid, gObjMsgInfo.Transform);
        }
        // If it does NOT have the object
        else
        {
            // Spawn the obj in a specific pos & rot and make it subscribe to the global scene events
            GameObject newGObj = PrefabHandler.Instance.CreateNewObjectGlobal(gObjMsgInfo.GameObjectGuid, gObjMsgInfo.PrefabName, gObjMsgInfo.Transform);

            // Create the instance in the GUIDKeeper list and add the obj
            GUIDKeeper.AddToList(gObjMsgInfo.GameObjectGuid, newGObj);
        }
    }
}
