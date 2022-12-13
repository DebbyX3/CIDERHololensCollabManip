using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// Script is attached to SlateUGUI Colors

public class SlateColorsManager : MonoBehaviour
{
    private GameObject UGUIButtons;
    PrefabSpecs PrefabSpecs;

    //-------------------- PRIVATE --------------------
    private void Awake()
    {
        // Need to to this operation here because it is used in one of the other called method from outside!
        // (so I need to make sure that is already assigned)
        UGUIButtons = gameObject.transform.Find("UGUIScrollViewContent/Scroll View/Viewport/Content/GridLayout1/Column1/UGUIButtons").gameObject;
    }

    private void OnEnable()
    {
        // Delete all the buttons in the SlateColor before repopulating the colors (because of the previous color display)
        // This operation is well suited in the OnEnable because I have to do it at each 'respawn'
        foreach (Transform child in UGUIButtons.transform)
            Destroy(child.gameObject);
    }

    private void AddButton(string prefabName, string imageName, Texture2D image, Guid guid = new Guid())
    {
        // The parent of the button is the gameobject UGUIButtons - important: set false as argument
        GameObject button = Instantiate(Resources.Load<GameObject>("SlateButton"), UGUIButtons.transform, false);

        // Get Button component and add listeners to the onClick event
        Button buttonComponent = button.GetComponent<Button>();

        // If the guid is not specified (it's empty), then the object is a new one, so add a new object
        if (guid.Equals(Guid.Empty))
        {
            // Create object specifying the material
            // Remember: image name and material name is the same!
            buttonComponent.onClick.AddListener(() => PrefabManager.Instance.CreateNewObjectInLocal(prefabName, imageName));
        }
        else // if the guid is specified, then the object exists! Just modify the material
        {
            // Edit object's material
            // Remember: image name and material name is the same!
            buttonComponent.onClick.AddListener(() => PrefabManager.Instance.PutExistingObjectInLocal(guid, imageName));
        }

        // Hide this slate
        buttonComponent.onClick.AddListener(() => gameObject.SetActive(false));

        // Get RawImage gameObject and component
        GameObject rawImage = button.transform.Find("RawImage").gameObject;
        RawImage rawImageComponent = rawImage.GetComponent<RawImage>();

        // Assign the image to the rawImage component to display the image in the button
        rawImageComponent.texture = image;

        // Get Text gameObject and TextMeshPro
        GameObject text = button.transform.Find("Text").gameObject;
        TMP_Text textTMP = text.GetComponent<TMP_Text>();

        // Set color name by trimming everything before the dash and making the first letter uppercase
        string imageNameTrimmed = imageName.Substring(imageName.IndexOf('-') + 1); // +1 to exclude the dash

        textTMP.SetText(char.ToUpper(imageNameTrimmed[0]) + imageNameTrimmed.Substring(1));
    }

    //-------------------- PUBLIC --------------------

    public void PopulateSlate(string prefabName, Guid guid = new Guid())
    {        
        PrefabSpecs = PrefabSpecs.FindByPrefabName(prefabName, PrefabManager.Instance.PrefabCollection);

        foreach (KeyValuePair<string, Texture2D> images in PrefabSpecs.Images)
            AddButton(prefabName, images.Key, images.Value, guid);
    }
}
