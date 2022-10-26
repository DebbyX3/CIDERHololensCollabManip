using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public TMP_Text SceneStateText;
    public TMP_Text LogText;

    public GameObject NotificationButton;

    public AudioSource NotificationSound;
    public AudioSource CommitSentSound;

    private string LogString = "";

    private void Awake() 
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this) {
            Destroy(this);
        } 
        else 
        {
            Instance = this;
        }
    }

    private void Update() 
    {
        if (!LogString.Equals(""))
        {
            LogText.text = LogText.text + LogString;
            LogString = "";
        }
    }

    public void PrintMessages(string message) {
        LogString += message + "\n";
    }

    public void ChangeSceneStateText(Location info) {
        SceneStateText.text = "Current scene: " + info;
    }

    public void SetNotificationButtonActive(bool active)
    {
        NotificationButton.GetComponent<NotificationButtonController>().SetButtonStatus(active);
    }
}
