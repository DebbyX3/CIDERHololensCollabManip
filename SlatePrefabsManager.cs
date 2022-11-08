using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Script is attached to SlateUGUI

public class SlatePrefabsManager : MonoBehaviour
{
    public GameObject SlateColor;
    private GameObject UGUIButtons;

    private void Start()
    {
        UGUIButtons = gameObject.transform.Find("UGUIScrollViewContent/Scroll View/Viewport/Content/GridLayout1/Column1/UGUIButtons").gameObject;
        
        // Put this call in Start because the slate needs to be populated only the first time! Otherwise, I would have put it in OnEnable
        PopulateSlate();
    }

    public void PopulateSlate()
    {
        foreach (PrefabSpecs prefabSpecs in PrefabManager.Instance.PrefabCollection)
            AddButton(prefabSpecs.PrefabName, prefabSpecs.GetAnImage());
    }

    public void AddButton(string prefabName, Texture2D image)
    {
        // The parent of the button is the gameobject UGUIButtons - important: set false as argument
        GameObject button = Instantiate(Resources.Load<GameObject>("SlateButton"), UGUIButtons.transform, false);

        // Get Button component and add listeners to the onClick event
        Button buttonComponent = button.GetComponent<Button>();

        // Display SlateColor
        buttonComponent.onClick.AddListener(() => SlateColor.SetActive(true));
        // Populate the SlateColor
        buttonComponent.onClick.AddListener(() => SlateColor.GetComponent<SlateColorsManager>().PopulateSlate(prefabName));
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

        // Set name, first letter uppercase
        textTMP.SetText(char.ToUpper(prefabName[0]) + prefabName.Substring(1));
    }
}
