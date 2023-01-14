using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Text;
using TMPro;
using UnityEngine;

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

    public AudioSource MessageReceivedSound;
    public AudioSource MessageSentSound;
    public AudioSource ButtonPressed;

    public GameObject SlateColor;
    public GameObject SlatePrefab;
    public GameObject SlateNotifications;

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

    public GameObject SetNearLocalFollowingMenu(GameObjController gObjContr, 
        out GameObject localForcedCommitButton, out GameObject localRequestCommitButton)
    {
        // The parent of the menu is the gameobject - important: set true as argument
        GameObject nearLocalFollowingMenu = Instantiate(Resources.Load<GameObject>("Menus and buttons/NearMenu3x2 - Local obj"), gObjContr.Transform, true);
        GameObject buttonCollection = nearLocalFollowingMenu.transform.Find("ButtonCollection").gameObject;

        // Button 1 - Forced Commit
        GameObject buttonOne = buttonCollection.transform.Find("ButtonOne").gameObject;
        Interactable interactableOne = buttonOne.GetComponent<Interactable>();
        interactableOne.OnClick.AddListener(() => gObjContr.PrepareForcedCommit());

        localForcedCommitButton = buttonOne;

        // Button 2 - Request Commit
        GameObject buttonTwo = buttonCollection.transform.Find("ButtonTwo").gameObject;
        Interactable interactableTwo = buttonTwo.GetComponent<Interactable>();
        interactableTwo.OnClick.AddListener(() => gObjContr.PrepareRequestCommit());

        localRequestCommitButton = buttonTwo;

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
            DeleteAllButtons());
        interactableSix.OnClick.AddListener(() => SlateColor.GetComponent<SlateColorsManager>().
            PopulateSlate(gObjContr.PrefabName, gObjContr.Guid));

        //----------------------

        TMP_Text objectText = nearLocalFollowingMenu.transform.Find("Backplate/Quad/ObjectText").GetComponent<TMP_Text>();
        objectText.text += gObjContr.ToString();

        // Hide it
        nearLocalFollowingMenu.SetActive(false);

        return nearLocalFollowingMenu;
    }

    public GameObject SetNearGlobalFollowingMenu(GameObjController gObjContr)
    {
        // The parent of the menu is the gameobject - important: set true as argument
        GameObject nearGlobalFollowingMenu = Instantiate(Resources.Load<GameObject>("Menus and buttons/NearMenu3x1 - Global obj"), gObjContr.Transform, true);
        GameObject buttonCollection = nearGlobalFollowingMenu.transform.Find("ButtonCollection").gameObject;

        // Button 1 - Copy object in local scene
        GameObject buttonOne = buttonCollection.transform.Find("ButtonOne").gameObject;
        Interactable interactableOne = buttonOne.GetComponent<Interactable>();
        interactableOne.OnClick.AddListener(() => gObjContr.CopyObjectInLocal());

        // Button 2 - Force delete object from global scene
        GameObject buttonTwo = buttonCollection.transform.Find("ButtonTwo").gameObject;
        Interactable interactableTwo = buttonTwo.GetComponent<Interactable>();
        interactableTwo.OnClick.AddListener(() => gObjContr.PrepareGlobalForcedDeletion());

        // Button 3 - Request deletion of an object from global scene
        GameObject buttonThree = buttonCollection.transform.Find("ButtonThree").gameObject;
        Interactable interactableThree = buttonThree.GetComponent<Interactable>();
        interactableThree.OnClick.AddListener(() => gObjContr.PrepareGlobalRequestDeletion());

        // Button 6 - Close menu
        GameObject buttonSix = buttonCollection.transform.Find("ButtonSix").gameObject;
        Interactable interactableSix = buttonSix.GetComponent<Interactable>();
        interactableSix.OnClick.AddListener(() => CloseMenu(nearGlobalFollowingMenu));

        //----------------------

        TMP_Text objectText = nearGlobalFollowingMenu.transform.Find("Backplate/Quad/ObjectText").GetComponent<TMP_Text>();
        objectText.text += gObjContr.ToString();

        // Hide it
        nearGlobalFollowingMenu.SetActive(false);

        return nearGlobalFollowingMenu;
    }

    public GameObject SetNearCommitPendingFollowingMenu(GameObjController gObjContr)
    {
        // The parent of the menu is the gameobject - important: set true as argument
        GameObject nearCommitPendingFollowingMenu = Instantiate(Resources.Load<GameObject>("Menus and buttons/NearMenu3x2 - Commit Pending obj"), gObjContr.Transform, true);

        GameObject buttonCollection = nearCommitPendingFollowingMenu.transform.Find("ButtonCollection").gameObject;

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
        interactableSix.OnClick.AddListener(() => CloseMenu(nearCommitPendingFollowingMenu));

        //----------------------

        TMP_Text objectText = nearCommitPendingFollowingMenu.transform.Find("Backplate/Quad/ObjectText").GetComponent<TMP_Text>();
        objectText.text += gObjContr.ToString();


        // Hide it
        nearCommitPendingFollowingMenu.SetActive(false);

        return nearCommitPendingFollowingMenu;
    }
    
    public GameObject SetNearDeletionPendingFollowingMenu(GameObjController gObjContr)
    {
        // The parent of the menu is the gameobject - important: set true as argument
        GameObject nearDeletionPendingFollowingMenu = Instantiate(Resources.Load<GameObject>("Menus and buttons/NearMenu3x2 - Deletion Pending obj"), gObjContr.Transform, true);

        GameObject buttonCollection = nearDeletionPendingFollowingMenu.transform.Find("ButtonCollection").gameObject;

        // Button 2 - Accept deletion
        GameObject buttonTwo = buttonCollection.transform.Find("ButtonTwo").gameObject;
        Interactable interactableTwo = buttonTwo.GetComponent<Interactable>();
        interactableTwo.OnClick.AddListener(() => gObjContr.AcceptDeletion());

        // Button 3 - Decline deletion
        GameObject buttonThree = buttonCollection.transform.Find("ButtonThree").gameObject;
        Interactable interactableThree = buttonThree.GetComponent<Interactable>();
        interactableThree.OnClick.AddListener(() => gObjContr.DeclineDeletion());

        // Button 6 - Close menu
        GameObject buttonSix = buttonCollection.transform.Find("ButtonSix").gameObject;
        Interactable interactableSix = buttonSix.GetComponent<Interactable>();
        interactableSix.OnClick.AddListener(() => CloseMenu(nearDeletionPendingFollowingMenu));

        //----------------------

        TMP_Text objectText = nearDeletionPendingFollowingMenu.transform.Find("Backplate/Quad/ObjectText").GetComponent<TMP_Text>();
        objectText.text += gObjContr.ToString();

        // Hide it
        nearDeletionPendingFollowingMenu.SetActive(false);

        return nearDeletionPendingFollowingMenu;
    }
}
