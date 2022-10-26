using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void SendForcedCommit(GameObjController gObjCont)
    {
        CaretakerScene.Instance.ExecuteForcedCommit(gObjCont);

        // Create message to send (serialize it)
        GameObjMessage msg = new GameObjMessage(new GameObjMessageInfo(gObjCont.Guid, gObjCont.Transform, gObjCont.PrefabName, CommitType.ForcedCommit));
        byte[] serializedMsg = msg.Serialize();

        // Send message
        NetworkHandler.Instance.Send(serializedMsg);

        // Play sound on commit
        UIManager.Instance.CommitSentSound.Play();

        // maybe display a dialog/confirmation box?

        // Do things
        OnCommitSent(gObjCont);
    }

    // per ora questo metodo funziona bene solo con i commit forzati. con quelli di voting è più complicato
    // visto che serve prima la risposta dell'altro utente che accetta o meno la modifica (sempre che si facciano insomma) ???
    public void OnCommitSent(GameObjController gObjCont)
    {
        PrefabManager.Instance.UpdateObjectGlobal(gObjCont.Guid, gObjCont.Transform);
    }

    // forse questo metodo è da spostare in una classe più appropriata?
    public void OnCommitReceived(GameObjMessage gObjMsg)
    {
        GameObjMessageInfo gObjMsgInfo = gObjMsg.GetMsgInfo();

        // If the receiver already has the object in one or both scenes
        if (GUIDKeeper.ContainsGuid(gObjMsgInfo.GameObjectGuid))
        {
            // To fix probably
            // cioè che potrebbe avere l'oggetto in locale e non globale? ma può capitare o no? Da capire?
            PrefabManager.Instance.UpdateObjectGlobal(gObjMsgInfo.GameObjectGuid, gObjMsgInfo.Transform);
        }
        // If it does NOT have the object
        else
        {
            // Spawn the obj in a specific pos & rot and make it subscribe to the global scene events
            PrefabManager.Instance.CreateNewObjectGlobal(gObjMsgInfo.GameObjectGuid, gObjMsgInfo.PrefabName, gObjMsgInfo.Transform);

            // Note: the instance is added in the GUIDKeeper.List in the Awake directly at object creation!
        }

        // Notify the user that a new commit has arrived

        // Play notification sound
        UIManager.Instance.NotificationSound.Play();

        // Send commit notification to this device
        UIManager.Instance.SetNotificationButtonActive(true);
    }
}
