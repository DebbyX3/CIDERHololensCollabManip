using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;

// Script is attached to SlateUGUI - Notifications

/*
    NotificationManager MUST be executed BEFORE MessagesManager, 
    in order to correctly initialize the NotificationID class appropriately
 */
public class NotificationType
{
    public string NotificationID { get; set; }
    public string NotificationTitle { get; set; }
    public string NotificationDesc { get; set; }
    public string NotificationObjSpecs { get; set; }

    public override string ToString() =>    $"NotificationID={NotificationID}, " +
                                            $"NotificationTitle={NotificationTitle}, " +
                                            $"NotificationDesc={NotificationDesc}" + 
                                            $"NotificationObjSpecs={NotificationObjSpecs}";
}

// Please, keep the order of these fields the same as the configuration file!
public class NotificationID
{
    private string TypeKeyWord;

    public static NotificationID ObjectChangeReceived;
    public static NotificationID DeletionReceived;
    public static NotificationID CommitRequestReceived;
    public static NotificationID DeletionRequestReceived;
    public static NotificationID DeclineCommitReceived;
    public static NotificationID DeclineDeletionReceived;

    private NotificationID(string typeKeyWord)
    {
        TypeKeyWord = typeKeyWord;
    }

    public NotificationID(List<string> notificationIDs)
    {
        ObjectChangeReceived = new NotificationID(notificationIDs[0]);
        DeletionReceived = new NotificationID(notificationIDs[1]);
        CommitRequestReceived = new NotificationID(notificationIDs[2]);
        DeletionRequestReceived = new NotificationID(notificationIDs[3]);
        DeclineCommitReceived = new NotificationID(notificationIDs[4]);
        DeclineDeletionReceived = new NotificationID(notificationIDs[5]);
    }

    public NotificationID(string objectChangeReceived, string deletionReceived, 
        string commitRequestReceived, string deletionRequestReceived, 
        string declineCommitReceived, string declineDeletionReceived)
    {
        ObjectChangeReceived = new NotificationID(objectChangeReceived);
        DeletionReceived = new NotificationID(deletionReceived);
        CommitRequestReceived = new NotificationID(commitRequestReceived);
        DeletionRequestReceived = new NotificationID(deletionRequestReceived);
        DeclineCommitReceived = new NotificationID(declineCommitReceived);
        DeclineDeletionReceived = new NotificationID(declineDeletionReceived);
    }

    public override string ToString()
    {
        return TypeKeyWord;
    }    
}

public class NotificationManager : MonoBehaviour
{
    public TextAsset StringsToDisplayFile;

    private GameObject UGUIButtons;
    private List<NotificationType> NotificationTypesList;
    private static NotificationID NotificationIDs;

    private void Start()
    {
        DeserializeNotificationTypes();
        PopulateNotificationIDs();        

        UGUIButtons = gameObject.transform.Find("UGUIScrollViewContent/Scroll View/Viewport/Content/GridLayout1/Column1/UGUIButtons").gameObject;
        gameObject.SetActive(false);
    }

    private void DeserializeNotificationTypes()
    {
        IDeserializer deserializer = new DeserializerBuilder().Build();
        NotificationTypesList = deserializer.Deserialize<List<NotificationType>>(StringsToDisplayFile.text);
    }

    private void PopulateNotificationIDs()
    {
        List<string> notificationIDS = new List<string>();

        foreach (NotificationType nt in NotificationTypesList)
            notificationIDS.Add(nt.NotificationID);

        NotificationIDs = new NotificationID(notificationIDS);
    }

    public void AddNotification(NotificationID notificationIDToFind, string objectName, string colorName, Texture2D image)
    {
        bool wasAlreadyActive = false;

        if (gameObject.activeSelf)      // If the slate is already visible, then set the variable
            wasAlreadyActive = true;
        else                            // Else, activate it
            gameObject.SetActive(true);

        // The parent of the button is the gameobject UGUIButtons - important: set false as argument
        GameObject buttonNotification = Instantiate(Resources.Load<GameObject>("SlateNotificationButton"), UGUIButtons.transform, false);

        // Get TextMeshPro object
        TMP_Text title = buttonNotification.transform.Find("Title").GetComponent<TMP_Text>();
        TMP_Text description = buttonNotification.transform.Find("Description").GetComponent<TMP_Text>();
        TMP_Text objReceivedText = buttonNotification.transform.Find("ObjSpecs").GetComponent<TMP_Text>();

        // Get RawImage gameObject and component
        RawImage rawImage = buttonNotification.transform.Find("RawImage").GetComponent<RawImage>();

        // Based on the notification ID received, find the corresponding info in the notification types list
        // (that holds all the specifications took from the config file) 
        NotificationType notificationType = NotificationTypesList.Find(x => x.NotificationID.Equals(notificationIDToFind.ToString()));

        // Set color name by trimming everything before the dash and making the first letter uppercase
        string colorNameTrimmed = colorName.Substring(colorName.IndexOf('-') + 1); // +1 to exclude the dash

        // Change texts
        title.text = notificationType.NotificationTitle;
        description.text = notificationType.NotificationDesc;
        objReceivedText.text =  notificationType.NotificationObjSpecs + 
                                char.ToUpper(colorNameTrimmed[0]) + colorNameTrimmed.Substring(1) + " " + 
                                objectName;

        // Assign the image to the rawImage component to display the image in the button
        rawImage.texture = image;

        // ------------------ On Click events

        // Get Button component and add listeners to the onClick event
        Button buttonNotificationComponent = buttonNotification.GetComponent<Button>();

        // Bring the user to the global scene to show the modification
        buttonNotificationComponent.onClick.AddListener(() => CaretakerScene.Instance.ChangeSceneToGlobal());

        // Hide this slate
        buttonNotificationComponent.onClick.AddListener(() => gameObject.SetActive(false));

        // Remove child (that is because otherwise childCount is not = 0) (Destroy is deferred at the and of current frame)
        buttonNotificationComponent.onClick.AddListener(() => RemoveChild(buttonNotification));

        // Destroy the button when clicked 
        buttonNotificationComponent.onClick.AddListener(() => Destroy(buttonNotification));

        // Disable the menu notification button on click if the clicked notif was the last one
        buttonNotificationComponent.onClick.AddListener(() => DisableMenuNotifButtonIfLastNotifButton());

        if (!wasAlreadyActive)               // If the slate was NOT already visible before, deactivate it
            gameObject.SetActive(false);

        // Enable notif button on following menu
        UIManager.Instance.SetNotificationButtonActive(true);
    }

    private void RemoveChild(GameObject child)
    {
        child.transform.parent = null;
    }

    private void DisableMenuNotifButtonIfLastNotifButton()
    {
        if (UGUIButtons.transform.childCount == 0)
            UIManager.Instance.SetNotificationButtonActive(false);
    }

    public void DestroyAllChildren(GameObject parent)
    {
        foreach (Transform child in parent.transform)
            Destroy(child.gameObject);
    }
}
