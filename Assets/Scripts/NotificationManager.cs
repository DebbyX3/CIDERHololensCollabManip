using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;

// Script is attached to SlateUGUI - Notifications
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
    }

    public NotificationID(string objectChangeReceived, string deletionReceived, string commitRequestReceived, string deletionRequestReceived, string declineCommitReceived)
    {
        ObjectChangeReceived = new NotificationID(objectChangeReceived);
        DeletionReceived = new NotificationID(deletionReceived);
        CommitRequestReceived = new NotificationID(commitRequestReceived);
        DeletionRequestReceived = new NotificationID(deletionRequestReceived);
        DeclineCommitReceived = new NotificationID(declineCommitReceived);
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

    private void Awake()
    {
        DeserializeNotificationTypes();
        PopulateNotificationIDs();        

        UGUIButtons = gameObject.transform.Find("UGUIScrollViewContent/Scroll View/Viewport/Content/GridLayout1/Column1/UGUIButtons").gameObject;
    }

    private void DeserializeNotificationTypes()
    {
        IDeserializer deserializer = new DeserializerBuilder().Build();
        NotificationTypesList = deserializer.Deserialize<List<NotificationType>>(StringsToDisplayFile.text);

        foreach(NotificationType nt in NotificationTypesList)
            Debug.Log(nt);
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
        // (that holds all the specification took from the config file) 
        NotificationType notificationType = NotificationTypesList.Find(x => x.NotificationID.Equals(notificationIDToFind.ToString()));

        // Change texts
        title.text = notificationType.NotificationTitle;
        description.text = notificationType.NotificationDesc;
        objReceivedText.text = notificationType.NotificationObjSpecs + colorName + " " + objectName;

        // Assign the image to the rawImage component to display the image in the button
        rawImage.texture = image;

        // ------------------ On Click events

        // Get Button component and add listeners to the onClick event
        Button buttonNotificationComponent = buttonNotification.GetComponent<Button>();

        // Bring the user to the global scene to show the modification
        buttonNotificationComponent.onClick.AddListener(() => CaretakerScene.Instance.ChangeSceneToGlobal());

        // Hide this slate
        buttonNotificationComponent.onClick.AddListener(() => gameObject.SetActive(false));

        // Destroy the button when clicked 
        buttonNotificationComponent.onClick.AddListener(() => Destroy(buttonNotification));

        // Disable the menu notification button on click if the clicked button was the last one
        buttonNotificationComponent.onClick.AddListener(() => DisableMenuNotifButtonIfLastNotifButton());

        gameObject.SetActive(false);

        UIManager.Instance.SetNotificationButtonActive(true);
    }

    private void DisableMenuNotifButtonIfLastNotifButton()
    {
        if (UGUIButtons.transform.childCount == 0)
            UIManager.Instance.SetNotificationButtonActive(false);
    }
}
