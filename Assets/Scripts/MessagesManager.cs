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
    RequestCommit
}

public enum DeletionType : int
{
    None,
    ForcedGlobalDeletion,
    RequestGlobalDeletion
}

public enum DeclineType : int 
{
    None,
    DeclineCommit,
    DeclineDeletion
}

// To be used in the SaveData script
public enum AcceptType : int
{
    None,
    AcceptCommit,
    AcceptDeletion,
}

/*
    MessagesManager MUST be executed AFTER NotificationManager, 
    in order to correctly initialize the NotificationID class appropriately
 */
public class MessagesManager : MonoBehaviour
{
    public static MessagesManager Instance { get; private set; }
    private NotificationManager NotificationManager;

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

        NotificationManager = UIManager.Instance.SlateNotifications.GetComponent<NotificationManager>();
    }

    // Based on the type of message, compose the object to send and then send it
    // todo: forse fare diverse funzioni col polimorfismo ma boh, tanto alla fine devo comunque inviare, da capire ecco
    private void CreateAndSendMessage(GameObjController gObjCont, MessageType messageType, CommitType commitType = CommitType.None,
                                      DeclineType declineType = DeclineType.None, DeletionType deletionType = DeletionType.None)
    {
        Message msg = null;

        switch (messageType)
        {
            case MessageType.DeletionMessage:
            {
                if (deletionType != DeletionType.None)
                {
                    msg = new DeletionMessage(new DeletionMessageInfo(gObjCont.Guid, deletionType));
                }

                break;
            }

            case MessageType.GameObjMessage:
            {
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
        SaveData.Instance.LogUserOperations(gObjCont, commitType: CommitType.ForcedCommit);
    }

    public void SendRequestCommit(GameObjController gObjCont)
    {
        SendCommit(gObjCont, CommitType.RequestCommit);
        SaveData.Instance.LogUserOperations(gObjCont, commitType: CommitType.RequestCommit);
    }

    private void SendCommit(GameObjController gObjCont, CommitType commitType)
    {
        CreateAndSendMessage(gObjCont, MessageType.GameObjMessage, commitType: commitType);

        // Play sound on commit
        UIManager.Instance.MessageSentSound.Play();

        switch (commitType)
        {
            case CommitType.ForcedCommit:
            {
                OnForcedCommitSent(gObjCont);
                break;
            }

            case CommitType.RequestCommit:
            {
                OnRequestCommitSent(gObjCont);
                break;
            }

            default:
                break;
        }
    }

    private void OnForcedCommitSent(GameObjController gObjCont)
    {
        gObjCont.RemoveCommitPending();
        PrefabManager.Instance.PutExistingObjectInGlobal(gObjCont.Guid, gObjCont.Transform, gObjCont.MaterialName);
    }

    private void OnRequestCommitSent(GameObjController gObjCont)
    {
        PrefabManager.Instance.PutExistingObjectInCommitPending(gObjCont.Guid, gObjCont.Transform, gObjCont.MaterialName);
    }

    public void AcceptCommit(GameObjController gObjCont)
    {
        SendForcedCommit(gObjCont);
        SaveData.Instance.LogUserOperations(gObjCont, acceptType: AcceptType.AcceptCommit);
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
        else if (gObjMsgInfo.CommitType.Equals(CommitType.RequestCommit))
            OnRequestCommitReceived(gObjMsgInfo);
    }

    private void OnForcedCommitReceived(GameObjMessageInfo gObjMsgInfo)
    {
        GameObject gObj;
        GameObjController gObjCont;

        // If the receiver already has the object in one or both scenes
        if (GUIDKeeper.ContainsGuid(gObjMsgInfo.GameObjectGuid))
        {
            gObj = GUIDKeeper.GetGObjFromGuid(gObjMsgInfo.GameObjectGuid);
            gObjCont = gObj.GetComponent<GameObjController>();

            // Important: remove Commit pending before putting object in global scene
            gObjCont.RemoveCommitPending();

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
        UIManager.Instance.MessageReceivedSound.Play();

        // Send commit notification to this device
        PrefabSpecs prefabSpecs = PrefabSpecs.FindByPrefabName(gObjMsgInfo.PrefabName, PrefabManager.Instance.PrefabCollection);

        NotificationManager.AddNotification(
            NotificationID.ObjectChangeReceived,
            gObjMsgInfo.PrefabName,
            gObjMsgInfo.MaterialName,
            prefabSpecs.GetImageByName(gObjMsgInfo.MaterialName));
    }

    private void OnRequestCommitReceived(GameObjMessageInfo gObjMsgInfo)
    {
        GameObject gObj;

        if (GUIDKeeper.ContainsGuid(gObjMsgInfo.GameObjectGuid))
        {
            gObj = PrefabManager.Instance.PutExistingObjectInCommitPending(gObjMsgInfo.GameObjectGuid, 
                gObjMsgInfo.Transform, gObjMsgInfo.MaterialName);
        }
        else
        {
            gObj = PrefabManager.Instance.CreateNewObjectInCommitPending(gObjMsgInfo.GameObjectGuid,
                gObjMsgInfo.PrefabName, gObjMsgInfo.MaterialName, gObjMsgInfo.Transform);
        }

        gObj.GetComponent<GameObjController>().OnRequestCommitReceived();

        // Notify the user that a new commit has arrived

        // Play notification sound
        UIManager.Instance.MessageReceivedSound.Play();

        // Send commit notification to this device
        PrefabSpecs prefabSpecs = PrefabSpecs.FindByPrefabName(gObjMsgInfo.PrefabName, PrefabManager.Instance.PrefabCollection);

        NotificationManager.AddNotification(
            NotificationID.CommitRequestReceived,
            gObjMsgInfo.PrefabName,
            gObjMsgInfo.MaterialName,
            prefabSpecs.GetImageByName(gObjMsgInfo.MaterialName));
    }

    // -------------------------- DELETION --------------------------

    // ------------ SEND DELETION ------------

    public void SendGlobalForcedDeleteMessage(GameObjController gObjCont)
    {
        SendGlobalDeletionMessage(gObjCont, DeletionType.ForcedGlobalDeletion);
        SaveData.Instance.LogUserOperations(gObjCont, deletionType: DeletionType.ForcedGlobalDeletion);
    }

    public void SendGlobalRequestDeletionMessage(GameObjController gObjCont)
    {
        SendGlobalDeletionMessage(gObjCont, DeletionType.RequestGlobalDeletion);
        SaveData.Instance.LogUserOperations(gObjCont, deletionType: DeletionType.RequestGlobalDeletion);
    }

    private void SendGlobalDeletionMessage(GameObjController gObjCont, DeletionType deletionType)
    {
        CreateAndSendMessage(gObjCont, MessageType.DeletionMessage, deletionType: deletionType);

        // Play sound on sent
        UIManager.Instance.MessageSentSound.Play();

        switch (deletionType)
        {
            case DeletionType.ForcedGlobalDeletion:
            {
                OnGlobalForcedDeletionSent(gObjCont);
                break;
            }

            case DeletionType.RequestGlobalDeletion:
            {
                OnGlobalRequestDeletionSent(gObjCont);
                break;
            }

            default:
                break;
        }
    }   

    private void OnGlobalForcedDeletionSent(GameObjController gObjCont)
    {
        gObjCont.RemoveDeletionPending();

        gObjCont.DeleteObject(ObjectLocation.Global, UserType.Receiver);
    }

    private void OnGlobalRequestDeletionSent(GameObjController gObjCont)
    {
        PrefabManager.Instance.PutExistingObjectInDeletionPending(gObjCont.Guid, gObjCont.Transform, gObjCont.MaterialName);
    }

    public void AcceptDeletion(GameObjController gObjCont)
    {
        SendGlobalForcedDeleteMessage(gObjCont);
        SaveData.Instance.LogUserOperations(gObjCont, acceptType: AcceptType.AcceptDeletion);
    }

    // ------------ RECEIVE DELETION ------------

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

        if (deletionMsgInfo.DeletionType.Equals(DeletionType.ForcedGlobalDeletion))
            OnForcedDeletionReceived(deletionMsgInfo);
        else if (deletionMsgInfo.DeletionType.Equals(DeletionType.RequestGlobalDeletion))
            OnRequestDeletionReceived(deletionMsgInfo);
    }

    public void OnForcedDeletionReceived(DeletionMessageInfo deletionMsgInfo)
    {
        GameObjController gObjCont = GUIDKeeper.GetGObjFromGuid(deletionMsgInfo.GameObjectGuid)
                                                .GetComponent<GameObjController>();

        gObjCont.RemoveDeletionPending();
        gObjCont.DeleteObject(ObjectLocation.Global, UserType.Receiver);

        //forse è da chiamare?
        //gObjCont.OnForcedDeletionReceived();

        // Notify the user that a new thing has arrived

        // Play notification sound
        UIManager.Instance.MessageReceivedSound.Play();

        // Send notification to this device
        PrefabSpecs prefabSpecs = PrefabSpecs.FindByPrefabName(gObjCont.PrefabName, PrefabManager.Instance.PrefabCollection);

        NotificationManager.AddNotification(
            NotificationID.DeletionReceived,
            gObjCont.PrefabName,
            gObjCont.MaterialName,
            prefabSpecs.GetImageByName(gObjCont.MaterialName));

        //todo: show the user a notification in case the global deletion generates a new local obj
        // potrei farlo che il delete obj mi torna un valore X, dove se X corrisponde a 'ho cancellato
        // da globale ma messo in locale' allora mostro la notifica
    }

    private void OnRequestDeletionReceived(DeletionMessageInfo deletionMsgInfo)
    {
        GameObjController gObjCont = GUIDKeeper.GetGObjFromGuid(deletionMsgInfo.GameObjectGuid)
                                                .GetComponent<GameObjController>();

        PrefabManager.Instance.PutExistingObjectInDeletionPending(gObjCont.Guid, gObjCont.Transform, gObjCont.MaterialName);

        gObjCont.OnRequestDeletionReceived();

        // Notify the user that a new thing has arrived

        // Play notification sound
        UIManager.Instance.MessageReceivedSound.Play();

        // Send commit notification to this device
        PrefabSpecs prefabSpecs = PrefabSpecs.FindByPrefabName(gObjCont.PrefabName, PrefabManager.Instance.PrefabCollection);

        NotificationManager.AddNotification(
            NotificationID.DeletionRequestReceived,
            gObjCont.PrefabName,
            gObjCont.MaterialName,
            prefabSpecs.GetImageByName(gObjCont.MaterialName));

        //todo: show the user a notification in case the global deletion generates a new local obj
        // potrei farlo che il delete obj mi torna un valore X, dove se X corrisponde a 'ho cancellato
        // da globale ma messo in locale' allora mostro la notifica
    }

    // -------------------------- DECLINE --------------------------

    // ------------ SEND DECLINE ------------

    public void SendDeclineCommit(GameObjController gObjCont)
    {
        SendDecline(gObjCont, DeclineType.DeclineCommit);
        SaveData.Instance.LogUserOperations(gObjCont, declineType: DeclineType.DeclineCommit);
    }

    public void SendDeclineDeletion(GameObjController gObjCont)
    {
        SendDecline(gObjCont, DeclineType.DeclineDeletion);
        SaveData.Instance.LogUserOperations(gObjCont, declineType: DeclineType.DeclineDeletion);
    }

    private void SendDecline(GameObjController gObjCont, DeclineType declineType)
    {
        CreateAndSendMessage(gObjCont, MessageType.DeclineMessage, declineType: declineType);

        // Play sound
        UIManager.Instance.MessageSentSound.Play();
    }

    // ------------ RECEIVE DECLINE  ------------

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
        GameObjController gObjCont = GUIDKeeper.GetGObjFromGuid(declineMsgInfo.GameObjectGuid)
                                            .GetComponent<GameObjController>();

        gObjCont.DeclineCommit(UserType.Receiver);

        // Notify the user that a new thing has arrived

        // Play notification sound
        UIManager.Instance.MessageReceivedSound.Play();

        // Send commit notification to this device
        PrefabSpecs prefabSpecs = PrefabSpecs.FindByPrefabName(gObjCont.PrefabName, PrefabManager.Instance.PrefabCollection);

        NotificationManager.AddNotification(
            NotificationID.DeclineCommitReceived,
            gObjCont.PrefabName,
            gObjCont.MaterialName,
            prefabSpecs.GetImageByName(gObjCont.MaterialName));
    }

    private void OnDeclineDeletionReceived(DeclineMessageInfo declineMsgInfo)
    {
        GameObjController gObjCont = GUIDKeeper.GetGObjFromGuid(declineMsgInfo.GameObjectGuid)
                                            .GetComponent<GameObjController>();

        gObjCont.DeclineDeletion(UserType.Receiver);

        UIManager.Instance.MessageReceivedSound.Play();

        // Send commit notification to this device
        PrefabSpecs prefabSpecs = PrefabSpecs.FindByPrefabName(gObjCont.PrefabName, PrefabManager.Instance.PrefabCollection);

        NotificationManager.AddNotification(
            NotificationID.DeclineDeletionReceived,
            gObjCont.PrefabName,
            gObjCont.MaterialName,
            prefabSpecs.GetImageByName(gObjCont.MaterialName));
    }
}
