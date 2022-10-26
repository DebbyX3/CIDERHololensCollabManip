using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

// Script is attached to the SlateUGUI Colors

public class SlateColorsManager : MonoBehaviour
{
    private GameObject UGUIButtons;

    private void Start()
    {
        UGUIButtons = gameObject.transform.Find("UGUIScrollViewContent/Scroll View/Viewport/Content/GridLayout1/Column1/UGUIButtons").gameObject;
       
        PopulateSlateColors("couch\\couch");
    }

    public void PopulateSlateColors(string prefabPathame)
    {
        string prefabName = Path.GetFileName(prefabPathame);



        // sta roba non serve a niente! non voglio leggere il file system perchè hololens è una app uwp
        // e per sta roba bisogna usare delle schifezze
        /*
        //Load a Texture (Assets/Resources/Textures/texture01.png)
        Texture2D texture = Resources.Load<Texture2D>("Textures/texture01");

        // Get the path to the prefab
        string directoryName = Path.GetDirectoryName(prefabPathame);
        Debug.Log(directoryName);
        UIManager.Instance.PrintMessages(directoryName);

        // Build the path to the image folder of this prefab
        string imagesDirectoryName = "Assets\\Resources\\" + directoryName + "\\images";
        Debug.Log(imagesDirectoryName);
        UIManager.Instance.PrintMessages(imagesDirectoryName);

        // Get all the images in the directory
        string[] fileEntries = Directory.GetFiles(imagesDirectoryName, "*.png");

        foreach (string fileName in fileEntries)
        {
            UIManager.Instance.PrintMessages(fileName);
            AddButton(fileName);
        }

        //-------------------------------------------

        DirectoryInfo dir = new DirectoryInfo(imagesDirectoryName);
        FileInfo[] info = dir.GetFiles("*.*");
        foreach (FileInfo f in info)
        {
            Debug.Log(f);
            UIManager.Instance.PrintMessages(f.ToString());
        }*/


    }

    // questa funzione si può sistemare e semplificare, perchè ora ho già una texture2d quando carico l'immagine!
    private void AddButton(string fileName)
    {
        // The parent of the button is the gameobject UGUIButtons - important: set false as argument
        GameObject button = Instantiate(Resources.Load<GameObject>("ColorButton"), UGUIButtons.transform, false);

        // Get Button component and add listener to the onClick event
        Button buttonComponent = button.GetComponent<Button>();
        buttonComponent.onClick.AddListener(() => AddObject(fileName));

        // Get RawImage gameObject and component
        GameObject rawImage = button.transform.Find("RawImage").gameObject;
        RawImage rawImageComponent = rawImage.GetComponent<RawImage>();

        // Convert the image to bytes
        byte[] imageData = File.ReadAllBytes(fileName);

        // Create a new 2D Texture, load the image as bytes and assign it to the rawImage component to display the image in the button
        Texture2D tex = new Texture2D(512, 512);
        tex.LoadImage(imageData);
        rawImageComponent.texture = tex;
    }

    private void AddObject(string fileName)
    {

    }
}
