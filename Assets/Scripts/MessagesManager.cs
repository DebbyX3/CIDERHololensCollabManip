using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UserType
{ 
    None,
    Sender,
    Receiver
}

public enum CommitType : int
{
    None,
    ForcedCommit,
    VotingCommit
}

public enum DeclineType : int 
{
    None,
    DeclineCommit,
    DeclineDeletion
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
    private void CreateAndSendMessage(GameObjController gObjCont, MessageType messageType, CommitType commitType = CommitType.None,
                                      DeclineType declineType = DeclineType.None)
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

            case MessageType.DeclineMessage:
            {
                if (declineType != DeclineType.None)
                {
                    msg = new DeclineMessage(new DeclineMessageInfo(gObjCont.Guid, declineType));
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

    // ------------ SEND COMMIT ------------

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
        // non servono mi sa?
        /*
        switch (commitType)
        {
            case CommitType.ForcedCommit:
            {
                CaretakerScene.Instance.ExecuteForcedCommit(gObjCont);
                break;
            }
                
            case CommitType.VotingCommit:
            {
                CaretakerScene.Instance.ExecuteVotingCommit(gObjCont);
                break;
            }
                
            default:
                break;
        }
        */

        CreateAndSendMessage(gObjCont, MessageType.GameObjMessage, commitType: commitType);

        // Play sound on commit
        UIManager.Instance.CommitSentSound.Play();

        // Todo maybe display a dialog/confirmation box? forse uno che va via da solo dopo tot tempo?

        switch (commitType)
        {
            case CommitType.ForcedCommit:
            {
                OnForcedCommitSent(gObjCont);
                break;
            }

            case CommitType.VotingCommit:
            {
                OnVotingCommitSent(gObjCont);
                break;
            }

            default:
                break;
        }
    }

    private void OnForcedCommitSent(GameObjController gObjCont)
    {
        gObjCont.RemovePending();
        PrefabManager.Instance.PutExistingObjectInGlobal(gObjCont.Guid, gObjCont.Transform, gObjCont.MaterialName);
    }

    private void OnVotingCommitSent(GameObjController gObjCont)
    {
        PrefabManager.Instance.PutExistingObjectInPending(gObjCont.Guid, gObjCont.Transform, gObjCont.MaterialName);
        // todo da fare qualcosa che boh
    }

    public void AcceptCommit(GameObjController gObjCont)
    {
        SendForcedCommit(gObjCont);
    }

    // ------------ RECEIVE COMMIT ------------

    // todo forse questo metodo è da spostare in una classe più appropriata?
    // cioè non dovrebbe essere la classe di commit che controlla se l'oggetto esiste già porca l'oca!!
    // dovrebbe mica essere il caretaker? o il prefab manager? forse il caretaker?
    public void OnCommitReceived(GameObjMessage gObjMsg)
    {
        GameObjMessageInfo gObjMsgInfo = gObjMsg.GetMsgInfo();

        if (gObjMsgInfo.CommitType.Equals(CommitType.ForcedCommit))
            OnForcedCommitReceived(gObjMsgInfo);
        else if (gObjMsgInfo.CommitType.Equals(CommitType.VotingCommit))
            OnVotingCommitReceived(gObjMsgInfo);
    }

    private void OnForcedCommitReceived(GameObjMessageInfo gObjMsgInfo)
    {
        GameObject gObj;

        // If the receiver already has the object in one or both scenes
        if (GUIDKeeper.ContainsGuid(gObjMsgInfo.GameObjectGuid))
        {
            gObj = PrefabManager.Instance.PutExistingObjectInGlobal(gObjMsgInfo.GameObjectGuid, 
                gObjMsgInfo.Transform, gObjMsgInfo.MaterialName);
        }
        // If it does NOT have the object
        else
        {
            // Spawn the obj in a specific pos & rot and make it subscribe to the global scene events
            gObj = PrefabManager.Instance.CreateNewObjectInGlobal(gObjMsgInfo.GameObjectGuid,
                gObjMsgInfo.PrefabName, gObjMsgInfo.MaterialName, gObjMsgInfo.Transform);

            // Note: the instance is added in the GUIDKeeper.List in the Awake directly at object creation!
        }

        gObj.GetComponent<GameObjController>().OnForcedCommitReceived();

        // Notify the user that a new commit has arrived

        // Play notification sound
        UIManager.Instance.NotificationSound.Play();

        // Send commit notification to this device
        UIManager.Instance.SetNotificationButtonActive(true);
    }

    private void OnVotingCommitReceived(GameObjMessageInfo gObjMsgInfo)
    {
        GameObject gObj;

        if (GUIDKeeper.ContainsGuid(gObjMsgInfo.GameObjectGuid))
        {
            gObj = PrefabManager.Instance.PutExistingObjectInPending(gObjMsgInfo.GameObjectGuid, 
                gObjMsgInfo.Transform, gObjMsgInfo.MaterialName);
        }
        else
        {
            gObj = PrefabManager.Instance.CreateNewObjectInPending(gObjMsgInfo.GameObjectGuid,
                gObjMsgInfo.PrefabName, gObjMsgInfo.MaterialName, gObjMsgInfo.Transform);
        }

        gObj.GetComponent<GameObjController>().OnVotingCommitReceived();

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

        GameObjController gObjController = GUIDKeeper.GetGObjFromGuid(deletionMsgInfo.GameObjectGuid)
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

    // -------------------------- DECLINE --------------------------

    public void SendDeclineCommit(GameObjController gObjCont)
    {
        SendDecline(gObjCont, DeclineType.DeclineCommit);
    }

    public void SendDeclineDeletion(GameObjController gObjCont)
    {
        SendDecline(gObjCont, DeclineType.DeclineDeletion);
    }

    private void SendDecline(GameObjController gObjCont, DeclineType declineType)
    {
        CreateAndSendMessage(gObjCont, MessageType.DeclineMessage, declineType: declineType);

        // Play sound
        UIManager.Instance.CommitSentSound.Play();

        // todo maybe display a dialog/confirmation box?
    }

    public void OnDeclineReceived(DeclineMessage declineMessage)
    {
        DeclineMessageInfo declineMsgInfo = declineMessage.GetMsgInfo();

        if (declineMsgInfo.DeclineType.Equals(DeclineType.DeclineCommit))
            OnDeclineCommitReceived(declineMsgInfo);
        else if (declineMsgInfo.DeclineType.Equals(DeclineType.DeclineDeletion))
            OnDeclineDeletionReceived(declineMsgInfo);
    }

    private void OnDeclineCommitReceived(DeclineMessageInfo declineMsgInfo)
    {
        GameObjController gObjController = GUIDKeeper.GetGObjFromGuid(declineMsgInfo.GameObjectGuid)
                                            .GetComponent<GameObjController>();

        gObjController.DeclineCommit(UserType.Receiver);

        // Notify the user that a new thing has arrived

        // Play notification sound
        UIManager.Instance.NotificationSound.Play();

        // Send commit notification to this device
        UIManager.Instance.SetNotificationButtonActive(true);

        //todo: show the user a notification in case the global deletion generates a new local obj}
    }

    private void OnDeclineDeletionReceived(DeclineMessageInfo declineMsgInfo)
    { }
}
