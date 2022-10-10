using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public TMP_Text sceneStateText;
    public TMP_Text logText;

    public GameObject notificationButton;

    private string logString = "";

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
        if (!logString.Equals(""))
        {
            logText.text = logText.text + logString;
            logString = "";
        }
    }

    public void PrintMessages(string message) {
        logString += message + "\n";
    }

    public void ChangeSceneStateText(Location info) {
        sceneStateText.text = "Current scene: " + info;
    }

    public void SetNotificationButtonActive()
    {
        notificationButton.SetActive(true);
    }
}
