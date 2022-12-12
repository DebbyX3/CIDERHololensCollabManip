using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

// Script is attached to SlateUGUI - Notifications
public class NotificationType
{
    public string NotificationID { get; set; }
    public string NotificationTitle { get; set; }
    public string NotificationDesc { get; set; }

    public override string ToString() =>    $"NotificationID={NotificationID}, " +
                                            $"NotificationTitle={NotificationTitle}, " +
                                            $"NotificationDesc={NotificationDesc}";
}

// Please, keep the order of these fields the same as the configuration file!
public class NotificationID
{
    private string TypeKeyWord;

    public static NotificationID ObjectReceived;
    public static NotificationID DeletionReceived;

    private NotificationID(string typeKeyWord)
    {
        TypeKeyWord = typeKeyWord;
    }

    public NotificationID(List<string> notificationIDs)
    {
        ObjectReceived = new NotificationID(notificationIDs[0]);
        DeletionReceived = new NotificationID(notificationIDs[1]);
    }

    public NotificationID(string objectReceived, string deletionReceived)
    {
        ObjectReceived = new NotificationID(objectReceived);
        DeletionReceived = new NotificationID(deletionReceived);
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
        PopulateNotificationIDsStruct();        

        UGUIButtons = gameObject.transform.Find("UGUIScrollViewContent/Scroll View/Viewport/Content/GridLayout1/Column1/UGUIButtons").gameObject;

        NotificationID myGrain = NotificationID.ObjectReceived;
        DisplayNotification(NotificationID.ObjectReceived);
    }

    private void DeserializeNotificationTypes()
    {
        IDeserializer deserializer = new DeserializerBuilder().Build();
        NotificationTypesList = deserializer.Deserialize<List<NotificationType>>(StringsToDisplayFile.text);

        foreach(NotificationType nt in NotificationTypesList)
            Debug.Log(nt);
    }

    private void PopulateNotificationIDsStruct()
    {
        List<string> notificationIDS = new List<string>();

        foreach (NotificationType nt in NotificationTypesList)
            notificationIDS.Add(nt.NotificationID);

        NotificationIDs = new NotificationID(notificationIDS);
    }

    private void DisplayNotification(NotificationID notificationID)
    { 

    }
}
