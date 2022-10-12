using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

public class NotificationButtonController : MonoBehaviour
{
    private TextMeshPro textMeshPro;
    private Color textOriginalColor;
    private Color iconOriginalColor;
    private Renderer iconRenderer;
    private List<MonoBehaviour> buttonBehaviours;
    private Transform buttonHighLightComponent;

    private Renderer backPlateRenderer;

    private Transform backPlate;

    private bool isInitialized = false;

    private void Awake()
    {
        if (!isInitialized)
        {
            isInitialized = true;

            // Components
            Transform iconParent = transform.Find("IconAndText");
            textMeshPro = iconParent.GetComponentInChildren<TextMeshPro>();
            iconRenderer = iconParent.Find("UIButtonSquareIcon").gameObject.GetComponent<Renderer>();
            buttonHighLightComponent = transform.Find("CompressableButtonVisuals");
            buttonBehaviours = GetComponents<MonoBehaviour>().ToList();

            // Material Changes
            textOriginalColor = textMeshPro.color;
            iconOriginalColor = iconRenderer.material.color;

            this.GetComponent<ButtonConfigHelper>().SetQuadIconByName("IconDot");

            // non serve che tanto basta mostrare la backplate 
            //backPlateRenderer = transform.Find("BackPlate").Find("Quad").gameObject.GetComponent<Renderer>();
            backPlate = transform.Find("BackPlate");
        }

        SetButtonStatus(false);
    }

    public void SetButtonStatus(bool active)
    {
        foreach (var b in buttonBehaviours.Where(p => p != this))
        {
            b.enabled = active;
        }

        buttonHighLightComponent.gameObject.SetActive(active);
        textMeshPro.color = active ? textOriginalColor : Color.gray;
        iconRenderer.material.color = active ? iconOriginalColor : Color.gray;

        backPlate.gameObject.SetActive(active); // show red backplate bg when button is active
    }

    // non serve probabilmente, basta mostrare backplate
    public void SetButtonColor(Color color)
    {
        backPlateRenderer.material.color = color;
    }
}
