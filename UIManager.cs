using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using TMPro;
using UnityEngine;
using MSUtilities = Microsoft.MixedReality.Toolkit.Utilities;

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

        if (Instance != null && Instance != this)
        {
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

    public void PrintMessages(string message)
    {
        LogString += message + "\n";
    }

    public void ChangeSceneStateText(Location info)
    {
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

    public void CloseMenu(GameObject menu)
    {
        menu.SetActive(false);
    }

    public void SetNearLocalFollowingMenu(GameObject nearLocalFollowingMenu, GameObjController gObjContr)
    {
        GameObject buttonCollection = nearLocalFollowingMenu.transform.Find("ButtonCollection").gameObject;

        // Button 1 - Forced Commit
        GameObject buttonOne = buttonCollection.transform.Find("ButtonOne").gameObject;
        Interactable interactableOne = buttonOne.GetComponent<Interactable>();
        interactableOne.OnClick.AddListener(() => MessagesManager.Instance.SendForcedCommit(gObjContr));

        // Button 2 - Voting Commit
        GameObject buttonTwo = buttonCollection.transform.Find("ButtonTwo").gameObject;
        Interactable interactableTwo = buttonTwo.GetComponent<Interactable>();
        interactableTwo.OnClick.AddListener(() => MessagesManager.Instance.SendVotingCommit(gObjContr));

        // Button 3 - Close Menu
        GameObject buttonThree = buttonCollection.transform.Find("ButtonThree").gameObject;
        Interactable interactableThree = buttonThree.GetComponent<Interactable>();
        interactableThree.OnClick.AddListener(() => CloseMenu(nearLocalFollowingMenu));

        // Button 4 - Remove from local
        GameObject buttonFour = buttonCollection.transform.Find("ButtonFour").gameObject;
        Interactable interactableFour = buttonFour.GetComponent<Interactable>();
        interactableFour.OnClick.AddListener(() => gObjContr.DeleteObject(ObjectLocation.Local, UserType.Sender));

        // Button 5 - Duplicate
        GameObject buttonFive = buttonCollection.transform.Find("ButtonFive").gameObject;
        Interactable interactableFive = buttonFive.GetComponent<Interactable>();
        interactableFive.OnClick.AddListener(() => gObjContr.DuplicateObj());

        // Button 6 - Change color
        GameObject buttonSix = buttonCollection.transform.Find("ButtonSix").gameObject;
        Interactable interactableSix = buttonSix.GetComponent<Interactable>();
        // Important: set slate active before populating - so onEnable & Awake (1st time) are called
        interactableSix.OnClick.AddListener(() => SlateColor.SetActive(true));
        interactableSix.OnClick.AddListener(() => SlateColor.GetComponent<SlateColorsManager>().
            PopulateSlate(gObjContr.PrefabName, gObjContr.Guid));

        //----------------------

        SolverHandler sh = nearLocalFollowingMenu.GetComponent<SolverHandler>();
        sh.TrackedTargetType = MSUtilities.TrackedObjectType.CustomOverride;
        sh.TransformOverride = gameObject.transform;

        // Hide it
        nearLocalFollowingMenu.SetActive(false);

        // The parent of the menu is the gameobject
        nearLocalFollowingMenu.transform.SetParent(gameObject.transform);

        // todo Maybe:  set scale to the same for every menu (so it doesn't become too small or too big)
    }

    public void SetNearGlobalFollowingMenu(GameObject nearGlobalFollowingMenu, GameObjController gObjContr)
    {
        GameObject buttonCollection = nearGlobalFollowingMenu.transform.Find("ButtonCollection").gameObject;

        // Button 1 - Copy object in local scene
        GameObject buttonOne = buttonCollection.transform.Find("ButtonOne").gameObject;
        Interactable interactableOne = buttonOne.GetComponent<Interactable>();
        interactableOne.OnClick.AddListener(() => gObjContr.CopyObjectInLocalAndChangeToLocal());

        // Button 2 - Delete object from global scene
        GameObject buttonTwo = buttonCollection.transform.Find("ButtonTwo").gameObject;
        Interactable interactableTwo = buttonTwo.GetComponent<Interactable>();
        interactableTwo.OnClick.AddListener(() => gObjContr.DeleteObject(ObjectLocation.Global, UserType.Sender));

        // todo! Accept or decline commit
        // Button 3 - Accept or decline commit
        GameObject buttonThree = buttonCollection.transform.Find("ButtonThree").gameObject;
        Interactable interactableThree = buttonThree.GetComponent<Interactable>();
        //interactableThree.OnClick.AddListener(() => DeleteObject(ObjectLocation.Global, UserType.Sender));

        // Button 6 - Close menu
        GameObject buttonSix = buttonCollection.transform.Find("ButtonSix").gameObject;
        Interactable interactableSix = buttonSix.GetComponent<Interactable>();
        interactableSix.OnClick.AddListener(() => CloseMenu(nearGlobalFollowingMenu));

        //----------------------

        SolverHandler sh = nearGlobalFollowingMenu.GetComponent<SolverHandler>();
        sh.TrackedTargetType = MSUtilities.TrackedObjectType.CustomOverride;
        sh.TransformOverride = gameObject.transform;

        // Hide it
        nearGlobalFollowingMenu.SetActive(false);

        // The parent of the menu is the gameobject
        nearGlobalFollowingMenu.transform.SetParent(gameObject.transform);

        //todo: set scale to the same for every menu (so it doesn't become too small or too big)}
    }
}
