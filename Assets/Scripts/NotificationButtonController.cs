using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

public class NotificationButtonController : MonoBehaviour
{
    private TextMeshPro TextMeshPro;
    private Color TextOriginalColor;
    private Color IconOriginalColor;
    private Renderer IconRenderer;
    private List<MonoBehaviour> ButtonBehaviours;
    private Transform ButtonHighLightComponent;

    private Renderer BackPlateRenderer;

    private Transform BackPlate;

    private bool IsInitialized = false;

    private void Awake()
    {
        if (!IsInitialized)
        {
            IsInitialized = true;

            // Components
            Transform iconParent = transform.Find("IconAndText");
            TextMeshPro = iconParent.GetComponentInChildren<TextMeshPro>();
            IconRenderer = iconParent.Find("UIButtonSquareIcon").gameObject.GetComponent<Renderer>();
            ButtonHighLightComponent = transform.Find("CompressableButtonVisuals");
            ButtonBehaviours = GetComponents<MonoBehaviour>().ToList();

            // Material Changes
            TextOriginalColor = TextMeshPro.color;
            IconOriginalColor = IconRenderer.material.color;

            GetComponent<ButtonConfigHelper>().SetQuadIconByName("IconDot");

            BackPlate = transform.Find("BackPlate");
        }

        SetButtonStatus(true);
        // TODO: rimetti false!!!!
    }

    public void SetButtonStatus(bool active)
    {
        foreach (var b in ButtonBehaviours.Where(p => p != this))
        {
            b.enabled = active;
        }

        ButtonHighLightComponent.gameObject.SetActive(active);
        TextMeshPro.color = active ? TextOriginalColor : Color.gray;
        IconRenderer.material.color = active ? IconOriginalColor : Color.gray;

        BackPlate.gameObject.SetActive(active); // show red backplate bg when button is active
    }
}
