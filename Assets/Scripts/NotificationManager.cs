using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script is attached to SlateUGUI - Notifications

public class NotificationManager : MonoBehaviour
{
    private GameObject UGUIButtons;

    // Start is called before the first frame update
    private void Start()
    {
        UGUIButtons = gameObject.transform.Find("UGUIScrollViewContent/Scroll View/Viewport/Content/GridLayout1/Column1/UGUIButtons").gameObject;
       // SlateColor = UIManager.Instance.SlateColor;

        // Put this call in Start because the slate needs to be populated only the first time! Otherwise, I would have put it in OnEnable
        //PopulateSlate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
