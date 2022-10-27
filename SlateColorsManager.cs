using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Script is attached to SlateUGUI Colors

public class SlateColorsManager : MonoBehaviour
{
    private GameObject UGUIButtons;
    PrefabSpecs prefabSpecs;

    private void Awake()
    {
        // Need to to this operation at awake, because it is used in one of the other called method
        // (so I need to make sure that is already assigned)
        UGUIButtons = gameObject.transform.Find("UGUIScrollViewContent/Scroll View/Viewport/Content/GridLayout1/Column1/UGUIButtons").gameObject;
    }

    public void PopulateSlate(string prefabName)
    {
        prefabSpecs = PrefabSpecs.FindByPrefabName(prefabName, PrefabManager.Instance.PrefabCollection);

        foreach (KeyValuePair<string, Texture2D> images in prefabSpecs.Images)
            AddButton(prefabName, images.Key, images.Value);
    }

    public void AddButton(string prefabName, string imageName, Texture2D image)
    {
        // The parent of the button is the gameobject UGUIButtons - important: set false as argument
        GameObject button = Instantiate(Resources.Load<GameObject>("SlateButton"), UGUIButtons.transform, false);

        // Get Button component and add listener to the onClick event
        Button buttonComponent = button.GetComponent<Button>();

        // Add object specifying the material
        buttonComponent.onClick.AddListener(() => AddObjectToScene(prefabName, imageName));
        //Hide this slate
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

    // Delete all buttons before repopulating the colors
    public void DestroyButtons()
    {
        foreach (Transform child in UGUIButtons.transform)
            Destroy(child.gameObject);
    }

    // dovrei passare direttamente il materiale oppure il nome? da capire, per ora passo il nome
    // posso anche fare che ci sono vari overload del metodo e così accontento tutti? boh
    private void AddObjectToScene(string prefabName, string imageName)
    {
        //Material material = prefabSpecs.GetMaterialByName(imageName);

        // Remember: image name and material name is the same!
        PrefabManager.Instance.CreateNewObjectLocal(prefabName, imageName);
    }

}
