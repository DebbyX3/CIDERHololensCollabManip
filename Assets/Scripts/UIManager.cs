using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using TMPro;
using UnityEngine;
using MSUtilities = Microsoft.MixedReality.Toolkit.Utilities;

public class UIManager : MonoBehaviour
{
    /*
      PLEASE NOTE!
        UIManager needs to be run BEFORE GameObjController, because GameObjController needs 
        references to the object that UIManager has
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

    public GameObject SetNearLocalFollowingMenu(GameObjController gObjContr)
    {
        // The parent of the menu is the gameobject -  important: set false as argument
        GameObject nearLocalFollowingMenu = Instantiate(Resources.Load<GameObject>("NearMenu3x2 - Local obj"), gObjContr.Transform, false);
        GameObject buttonCollection = nearLocalFollowingMenu.transform.Find("ButtonCollection").gameObject;

        // Button 1 - Forced Commit
        GameObject buttonOne = buttonCollection.transform.Find("ButtonOne").gameObject;
        Interactable interactableOne = buttonOne.GetComponent<Interactable>();
        interactableOne.OnClick.AddListener(() => gObjContr.PrepareForcedCommit());

        // Button 2 - Voting Commit
        GameObject buttonTwo = buttonCollection.transform.Find("ButtonTwo").gameObject;
        Interactable interactableTwo = buttonTwo.GetComponent<Interactable>();
        interactableTwo.OnClick.AddListener(() => gObjContr.PrepareVotingCommit());

        // Button 3 - Close Menu
        GameObject buttonThree = buttonCollection.transform.Find("ButtonThree").gameObject;
        Interactable interactableThree = buttonThree.GetComponent<Interactable>();
        interactableThree.OnClick.AddListener(() => CloseMenu(nearLocalFollowingMenu));

        // Button 4 - Remove from local
        GameObject buttonFour = buttonCollection.transform.Find("ButtonFour").gameObject;
        Interactable interactableFour = buttonFour.GetComponent<Interactable>();
        interactableFour.OnClick.AddListener(() => gObjContr.DeleteObject(ObjectLocation.Local));

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
        sh.TransformOverride = gObjContr.Transform;

        // Hide it
        nearLocalFollowingMenu.SetActive(false);

        return nearLocalFollowingMenu;

        // todo: set scale to the same for every menu (so it doesn't become too small or too big)
    }

    public GameObject SetNearGlobalFollowingMenu(GameObjController gObjContr)
    {
        // The parent of the menu is the gameobject -  important: set false as argument
        GameObject nearGlobalFollowingMenu = Instantiate(Resources.Load<GameObject>("NearMenu3x1 - Global obj"), gObjContr.Transform, false);
        GameObject buttonCollection = nearGlobalFollowingMenu.transform.Find("ButtonCollection").gameObject;

        // Button 1 - Copy object in local scene
        GameObject buttonOne = buttonCollection.transform.Find("ButtonOne").gameObject;
        Interactable interactableOne = buttonOne.GetComponent<Interactable>();
        interactableOne.OnClick.AddListener(() => gObjContr.CopyObjectInLocal());

        // Button 2 - Delete object from global scene
        GameObject buttonTwo = buttonCollection.transform.Find("ButtonTwo").gameObject;
        Interactable interactableTwo = buttonTwo.GetComponent<Interactable>();
        interactableTwo.OnClick.AddListener(() => gObjContr.DeleteObject(ObjectLocation.Global));

        // Button 6 - Close menu
        GameObject buttonSix = buttonCollection.transform.Find("ButtonSix").gameObject;
        Interactable interactableSix = buttonSix.GetComponent<Interactable>();
        interactableSix.OnClick.AddListener(() => CloseMenu(nearGlobalFollowingMenu));

        //----------------------

        SolverHandler sh = nearGlobalFollowingMenu.GetComponent<SolverHandler>();
        sh.TrackedTargetType = MSUtilities.TrackedObjectType.CustomOverride;
        sh.TransformOverride = gObjContr.Transform;

        // Hide it
        nearGlobalFollowingMenu.SetActive(false);

        //todo: set scale to the same for every menu (so it doesn't become too small or too big)}

        return nearGlobalFollowingMenu;
    }

    public GameObject SetNearPendingFollowingMenu(GameObjController gObjContr)
    {
        // The parent of the menu is the gameobject -  important: set false as argument
        GameObject nearPendingFollowingMenu = Instantiate(Resources.Load<GameObject>("NearMenu3x2 - Pending obj"), gObjContr.Transform, false);

        GameObject buttonCollection = nearPendingFollowingMenu.transform.Find("ButtonCollection").gameObject;

        // todo! Accept or decline commit

        // Button 2 - Accept commit
        GameObject buttonTwo = buttonCollection.transform.Find("ButtonTwo").gameObject;
        Interactable interactableTwo = buttonTwo.GetComponent<Interactable>();
        interactableTwo.OnClick.AddListener(() => gObjContr.AcceptCommit());

        // Button 3 - Decline commit
        GameObject buttonThree = buttonCollection.transform.Find("ButtonThree").gameObject;
        Interactable interactableThree = buttonThree.GetComponent<Interactable>();
        interactableThree.OnClick.AddListener(() => gObjContr.DeclineCommit());

        // Button 6 - Close menu
        GameObject buttonSix = buttonCollection.transform.Find("ButtonSix").gameObject;
        Interactable interactableSix = buttonSix.GetComponent<Interactable>();
        interactableSix.OnClick.AddListener(() => CloseMenu(nearPendingFollowingMenu));

        //----------------------

        SolverHandler sh = nearPendingFollowingMenu.GetComponent<SolverHandler>();
        sh.TrackedTargetType = MSUtilities.TrackedObjectType.CustomOverride;
        sh.TransformOverride = gObjContr.Transform;

        // Hide it
        nearPendingFollowingMenu.SetActive(false);

        //todo: set scale to the same for every menu (so it doesn't become too small or too big)}

        return nearPendingFollowingMenu;
    }
}
