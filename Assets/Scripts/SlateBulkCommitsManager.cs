using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEditor;
using Microsoft.MixedReality.Toolkit.UI;
using System.Security.AccessControl;

// Script is attached to SlateUGUI bulk commits
/*
 * This function manages the bulk commit slate, that shows the pending commits. 
 * Users can select/deselect multiple pending commits to accept/reject them all at once.
 * Altenratively, users can accept or reject them all using the buttons at the top of the slate.
 */

public class SlateBulkCommitsManager : MonoBehaviour
{
    private GameObject UGUIButtons;
    PrefabSpecs PrefabSpecs;

    //-------------------- PRIVATE --------------------
    private void Awake()
    {
        // Need to to this operation here because it is used in one of the other called method from outside!
        // (so I need to make sure that is already assigned)
        // UGUIButtons is the 'area' where the pending commits will be added and displayed
        UGUIButtons = gameObject.transform.Find("UGUIScrollViewContent/Scroll View/Viewport/Content/GridLayout1/Column1/UGUIButtons").gameObject;
    }

    private void OnEnable()
    {
        DeleteAllButtons();
        PopulateSlate();
    }

    public void DeleteAllButtons()
    {
        // Delete all the buttons in the SlateBulkCommits before repopulating the commits (because of the previous commits display)
        // This operation is well suited in the OnEnable because I have to do it at each 'respawn'
        foreach (Transform child in UGUIButtons.transform)
            Destroy(child.gameObject);
    }

    private void DeleteButtons(List<Transform> buttons)
    {
        // Delete the buttons that are in the list
        foreach (Transform child in buttons)
            Destroy(child.gameObject);
    }

    private void AddButton(Texture2D image, string imageName, string objectName, Guid ObjGuid)
    {
        UIManager.Instance.PrintMessages(
            image.ToString() + "\n" +
            imageName + "\n" +
            objectName + "\n" +
            ObjGuid.ToString()
            );

        // The parent of the button is the gameobject UGUIButtons - important: set false as argument
        GameObject button = Instantiate(Resources.Load<GameObject>("Menus and buttons/SlateBulkCommitButton"), UGUIButtons.transform, false);

        // Get RawImage gameObject and component
        GameObject rawImage = button.transform.Find("RawImage").gameObject;
        RawImage rawImageComponent = rawImage.GetComponent<RawImage>();

        // Assign the image to the rawImage component to display the image in the button
        rawImageComponent.texture = image;

        // Get Text gameObject and TextMeshPro
        GameObject text = button.transform.Find("ObjSpecs").gameObject;
        TMP_Text objReceivedTextTMP = text.GetComponent<TMP_Text>();

        // Set color name by trimming everything before the dash and making the first letter uppercase
        string colorNameTrimmed = imageName.Substring(imageName.IndexOf('-') + 1); // +1 to exclude the dash

        // Print received object's name and color
        objReceivedTextTMP.text = objReceivedTextTMP.text +
                                    char.ToUpper(colorNameTrimmed[0]) + colorNameTrimmed.Substring(1) + " " +
                                    objectName;

        // Set Object GUID to the button, so I can use it later when I fire the accept/reject event
        button.GetComponent<BulkCommitButtonObjGUID>().SetObjGuid(ObjGuid);
    }

    //-------------------- PUBLIC --------------------

    public void PopulateSlate()
    {
        foreach (KeyValuePair<Guid, Memento> element in CaretakerScene.Instance.GetCommitPendingListRequests())
        {
            PrefabSpecs = PrefabSpecs.FindByPrefabName(element.Value.GetPrefabName(), PrefabManager.Instance.PrefabCollection);

            AddButton(PrefabSpecs.GetImageByName(element.Value.GetMaterialName()), element.Value.GetMaterialName(), element.Value.GetPrefabName(), element.Value.GetGuid());
        }
    }

    public void AcceptSelected()
    {
        List<Transform> childernToDelete = new List<Transform>();

        foreach (Transform child in UGUIButtons.transform)
        {
            // Get CheckBox button GameObject and component
            GameObject checkbox = child.transform.Find("PressableButtonHoloLens2UIToggleCheckBox").gameObject;
            Interactable checkboxInteractable = checkbox.GetComponent<Interactable>();

            if (checkboxInteractable.IsToggled)
            {
                // Get the object GUID from the button
                Guid objGuid = child.GetComponent<BulkCommitButtonObjGUID>().ObjGuid;

                // Get the object controller from the GUID
                GameObjController gObjContr = GUIDKeeper.GetGObjFromGuid(objGuid).GetComponent<GameObjController>();

                // Accept the commit
                gObjContr.AcceptCommit();

                // Add the child to the list of children to delete
                childernToDelete.Add(child);
            }

            // Delete the buttons that corresponds to accepted commits
            DeleteButtons(childernToDelete);
        }
    }

    public void RejectSelected()
    {
        List<Transform> childernToDelete = new List<Transform>();

        foreach (Transform child in UGUIButtons.transform)
        {
            // Get CheckBox button GameObject and component
            GameObject checkbox = child.transform.Find("PressableButtonHoloLens2UIToggleCheckBox").gameObject;
            Interactable checkboxInteractable = checkbox.GetComponent<Interactable>();

            if (checkboxInteractable.IsToggled)
            {
                // Get the object GUID from the button
                Guid objGuid = child.GetComponent<BulkCommitButtonObjGUID>().ObjGuid;

                // Get the object controller from the GUID
                GameObjController gObjContr = GUIDKeeper.GetGObjFromGuid(objGuid).GetComponent<GameObjController>();

                // Accept the commit
                gObjContr.DeclineCommit();

                // Add the child to the list of children to delete
                childernToDelete.Add(child);
            }

            // Delete the buttons that corresponds to accepted commits
            DeleteButtons(childernToDelete);
        }
    }

    public void AcceptAll()
    {
        foreach (Transform child in UGUIButtons.transform)
        {
            // Get CheckBox button GameObject and component
            GameObject checkbox = child.transform.Find("PressableButtonHoloLens2UIToggleCheckBox").gameObject;
            Interactable checkboxInteractable = checkbox.GetComponent<Interactable>();

            // Get the object GUID from the button
            Guid objGuid = child.GetComponent<BulkCommitButtonObjGUID>().ObjGuid;

            // Get the object controller from the GUID
            GameObjController gObjContr = GUIDKeeper.GetGObjFromGuid(objGuid).GetComponent<GameObjController>();

            // Accept the commit
            gObjContr.AcceptCommit();

            // Delete all buttons
            DeleteAllButtons();
        }
    }

    public void RejectAll()
    {
        foreach (Transform child in UGUIButtons.transform)
        {
            // Get CheckBox button GameObject and component
            GameObject checkbox = child.transform.Find("PressableButtonHoloLens2UIToggleCheckBox").gameObject;
            Interactable checkboxInteractable = checkbox.GetComponent<Interactable>();

            // Get the object GUID from the button
            Guid objGuid = child.GetComponent<BulkCommitButtonObjGUID>().ObjGuid;

            // Get the object controller from the GUID
            GameObjController gObjContr = GUIDKeeper.GetGObjFromGuid(objGuid).GetComponent<GameObjController>();

            // Accept the commit
            gObjContr.DeclineCommit();

            // Delete all buttons
            DeleteAllButtons();
        }
    }
}
