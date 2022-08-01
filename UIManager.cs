using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public TMP_Text sceneStateText;
    public TMP_Text logText;

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
    }

    public void PrintMessages(string message) {
        logText.text = logText.text + message + "\n";
    }

    public void ChangeSceneState(Location info) {
        sceneStateText.text = "Current scene: " + info;
    }
}