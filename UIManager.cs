using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    /*
      PLEASE NOTE!
        UIManager needs to be run BEFORE GameObjController, because GameObjController needs references to the object that UIManager has
     */

    public static UIManager Instance { get; private set; }

    public TMP_Text SceneStateText;
    public TMP_Text LogText;

    public GameObject NotificationButton;

    public AudioSource NotificationSound;
    public AudioSource CommitSentSound;

    public GameObject SlateColor;
    public GameObject SlatePrefab;

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

    // TODO huge! rifare una schemata notifiche decente plis
    public void SetNotificationButtonActive(bool active)
    {
        NotificationButton.GetComponent<NotificationButtonController>().SetButtonStatus(active);
    }

    public void ShowDialogOnGlobalScene(Dialog dialog)
    {
        if (CaretakerScene.Instance.IsGlobalScene())
        {
            dialog.gameObject.SetActive(true);
            dialog.GetComponent<SolverHandler>().enabled = true;
        }
    }

    public void ShowSlateOnLocalScene(GameObject slate)
    {
        if (CaretakerScene.Instance.IsLocalScene())
        {
            slate.gameObject.SetActive(true);
            slate.GetComponent<RadialView>().enabled = true;
        }
    }

    public void HideSlateOnGlobalScene(GameObject slate)
    {
        if (CaretakerScene.Instance.IsGlobalScene())
        {
            slate.gameObject.SetActive(false);
        }
    }

}
