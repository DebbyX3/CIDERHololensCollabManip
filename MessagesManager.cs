using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UserType
{ 
    Sender,
    Receiver
}

public enum CommitType : int
{
    None,
    ForcedCommit,
    VotingCommit
}

public class MessagesManager : MonoBehaviour
{
    public static MessagesManager Instance { get; private set; }

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

    // Based on the type of message, compose the object to send and then send it
    private void CreateAndSendMessage(GameObjController gObjCont, MessageType messageType, CommitType commitType = CommitType.None)
    {
        Message msg = null;

        switch (messageType)
        {
            case MessageType.DeletionMessage:
            {
                // Create message
                msg = new DeletionMessage(new DeletionMessageInfo(gObjCont.Guid));
                break;
            }

            case MessageType.GameObjMessage:
            {
                // Create message
                if (commitType != CommitType.None)
                {
                    msg = new GameObjMessage(new GameObjMessageInfo
                        (gObjCont.Guid, gObjCont.Transform, gObjCont.PrefabName, gObjCont.MaterialName, commitType));
                }
                break;
            }

            default:
                break;
        }

        if (msg != null)
        {
            // Serialize
            byte[] serializedMsg = msg.Serialize();

            // Send message
            NetworkManager.Instance.Send(serializedMsg);
        }
        else
        {
            Debug.Log("Message is null: nothing was sent");
            UIManager.Instance.PrintMessages("Message is null: nothing was sent");
        }
    }

    // -------------------------- COMMITS --------------------------

    public void SendForcedCommit(GameObjController gObjCont)
    {
        SendCommit(gObjCont, CommitType.ForcedCommit);
    }

    public void SendVotingCommit(GameObjController gObjCont)
    {
        SendCommit(gObjCont, CommitType.VotingCommit);
    }

    private void SendCommit(GameObjController gObjCont, CommitType commitType)
    {
        switch (commitType)
        {
            case CommitType.ForcedCommit:
                CaretakerScene.Instance.ExecuteForcedCommit(gObjCont);
                break;
                
            case CommitType.VotingCommit:
                CaretakerScene.Instance.ExecuteVotingCommit(gObjCont);
                break;
                
            default:
                break;
        }

        CreateAndSendMessage(gObjCont, MessageType.GameObjMessage, commitType);

        // Play sound on commit
        UIManager.Instance.CommitSentSound.Play();

        // Todo maybe display a dialog/confirmation box? forse uno che va via da solo dopo tot tempo?

        // Do things
        OnForcedCommitSent(gObjCont);
    }

    // per ora questo metodo funziona bene solo con i commit forzati. con quelli di voting � pi� complicato
    // visto che serve prima la risposta dell'altro utente che accetta o meno la modifica (sempre che si facciano insomma) ???
    public void OnForcedCommitSent(GameObjController gObjCont)
    {
        PrefabManager.Instance.UpdateObjectGlobal(gObjCont.Guid, gObjCont.Transform, gObjCont.MaterialName);
    }

    public void OnVotingCommitSent(GameObjController gObjCont)
    {
        // da fare qualcosa che boh
    }

    // todo forse questo metodo � da spostare in una classe pi� appropriata?
    // cio� non dovrebbe essere la classe di commit che controlla se l'oggetto esiste gi� porca l'oca!!
    // dovrebbe mica essere il caretaker? o il prefab manager? forse il caretaker?
    public void OnCommitReceived(GameObjMessage gObjMsg)
    {
        GameObjMessageInfo gObjMsgInfo = gObjMsg.GetMsgInfo();

        // If the receiver already has the object in one or both scenes
        if (GUIDKeeper.ContainsGuid(gObjMsgInfo.GameObjectGuid))
        {
            // todo To fix probably
            // cio� che potrebbe avere l'oggetto in locale e non globale? ma pu� capitare o no? Da capire?
            PrefabManager.Instance.UpdateObjectGlobal(gObjMsgInfo.GameObjectGuid, gObjMsgInfo.Transform, gObjMsgInfo.MaterialName);
        }
        // If it does NOT have the object
        else
        {
            // Spawn the obj in a specific pos & rot and make it subscribe to the global scene events
            PrefabManager.Instance.CreateNewObjectGlobal(gObjMsgInfo.GameObjectGuid, gObjMsgInfo.PrefabName, gObjMsgInfo.MaterialName, gObjMsgInfo.Transform);

            // Note: the instance is added in the GUIDKeeper.List in the Awake directly at object creation!
        }

        // Notify the user that a new commit has arrived

        // Play notification sound
        UIManager.Instance.NotificationSound.Play();

        // Send commit notification to this device
        UIManager.Instance.SetNotificationButtonActive(true);
    }

    // -------------------------- DELETION --------------------------

    public void SendGlobalDeletionMessage(GameObjController gObjCont)
    {
        CreateAndSendMessage(gObjCont, MessageType.DeletionMessage);

        // Play sound
        UIManager.Instance.CommitSentSound.Play();

        // todo maybe display a dialog/confirmation box?
    }

    /*
     * Note: 
     * I don't have to check if the object exists in the global scene to delete it, because the object is shared, and if one user
     * deletes it, the other necessarily has to have it. To make it clear, let's cover the cases:
     * 
     * Deletion:
     * 1) User1 (U1) wants to delete a global obj. It issues the related command and sends the message to User2 (U2)
     * 2) U2 receives the message, and deletes the object from its global scene
     * 
     * So now both have a deleted object. But what assures me that both have the same obj in the global scene to begin off?
     * 
     * Addition:
     * 1) U1 wants to commit an object to the global scene. U2 does not have it
     * 2) U1 issues a commit
     * 3) U2 receives the commit and adds the object to its global scene
     * 
     * Now both have a new shared object
     * 
     * I proved that if the obj exists in the global scene for U1, it also exists in the global scene for U2
     * So in the deletion I don't have to check if the object exists in the global scene  
     */
    public void OnDeletionReceived(DeletionMessage deletionMsg)
    {
        DeletionMessageInfo deletionMsgInfo = deletionMsg.GetMsgInfo();

        GameObjController gObjController =  GUIDKeeper.GetGObjFromGuid(deletionMsgInfo.GameObjectGuid)
                                            .GetComponent<GameObjController>();

        gObjController.DeleteObject(ObjectLocation.Global, UserType.Receiver);

        // Notify the user that a new thing has arrived

        // Play notification sound
        UIManager.Instance.NotificationSound.Play();

        // Send commit notification to this device
        UIManager.Instance.SetNotificationButtonActive(true);

        //todo: show the user a notification in case the global deletion generates a new local obj
        // potrei farlo che il delete obj mi torna un valore X, dove se X corrisponde a 'ho cancellato
        // da globale ma messo in locale' allora mostro la notifica
        // NON MOSTRARE NOTIFICA IN CONTROLLER!!! MA FARLO QUA EH!!!
    }
}
